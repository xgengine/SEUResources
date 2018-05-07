#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// SEUResource 对Unity资源加载进行封装、保持和 Resources 资源加载接口形式，并对资源进行引用计数的管理
/// </summary>
public partial class SEUResource
{
    protected Object m_Asset = null;

    protected string m_LoadPath;
    public string loadPath
    {
        get
        {
            return m_LoadPath;
        }
    }

    protected Request m_LoadAysncRequest;

    private int m_RefCount = 0;

    private SEUResourcePool m_Pool = null;

    protected List<SEUResource> m_DependenceResources = new List<SEUResource>();

    protected SEUResource(string path)
    {
        m_LoadPath = path;
    }

    private void Use()
    {
        m_RefCount++;
      
    }

    private void UnUsed()
    {
        if (m_RefCount == 0)
        {
            Debug.LogError("多次调用的 UnLoadUsedResource "+ StackTraceUtility.ExtractStackTrace());
        }
        else
        {
            m_RefCount--;
            if (m_RefCount == 0)
            {
                m_Pool.PopResource(this);
                for (int i = 0; i < m_DependenceResources.Count; i++)
                {
                    SEUResource.UnLoadResource(m_DependenceResources[i]);
                }
            }
        }
       
    }

    private void AttachPool(SEUResourcePool pool)
    {
        m_Pool = pool;
    }

    protected virtual void ReleaseResource()
    {
        Debug.Log(GetType()+" unload ");
   
    }

    protected virtual void LoadAsset()
    {

    }

   
    protected virtual IEnumerator LoadAssetAsync()
    {
        yield break;
    }

    protected Request SendLoadAsyncRequest()
    {
        if(m_LoadAysncRequest == null)
        {
            m_LoadAysncRequest = new Request(this);
        }
        return m_LoadAysncRequest;
    }

    protected void AddDependenceResources(SEUResource resource)
    {
        if(m_DependenceResources.Contains(resource) == false)
        {
            m_DependenceResources.Add(resource);
            resource.Use();
#if SEU_DEBUG
            resource.Debug_MarkStackInfo();
#endif
        }
    }

     
    protected void LogResult()
    {
        if (m_Asset == null)
        {
            Debug.LogError(GetType()+" Load failed :" + m_LoadPath);
        }
        else
        {
            Debug.Log(GetType()+ " Load Succeed :"+m_LoadPath );
        }
        
    }

#if SEU_DEBUG||UNITY_EDITOR
    public int refCount
    {
        get
        {
            return m_RefCount;
        }
    }
    public List<SEUResource> dependenceResources
    {
        get
        {
            return m_DependenceResources;
        }
    }
    public GameObject DebugObject;
    public void DebugCreateObject()
    {
        DebugObject = new GameObject(m_LoadPath);
        SEUResourceDebug debugObject = DebugObject.AddComponent<SEUResourceDebug>();
        debugObject.resource = this;
    }

    public List<string> Debug_StackInfo=new List<string>();

    public void Debug_MarkStackInfo()
    {
        Debug_StackInfo.Add("[Load]" + StackTraceUtility.ExtractStackTrace());
    }
#endif

    /// <summary>
    /// 采用资源从中 Resources 加载方式的资源类
    /// </summary>
    private class SEUNormalResource : SEUResource
    {
        public SEUNormalResource(string path) : base(path)
        {
        }
        protected override void LoadAsset()
        {
            m_Asset = Resources.Load(m_LoadPath);
        }
        protected override IEnumerator LoadAssetAsync()
        {
            ResourceRequest request = Resources.LoadAsync(m_LoadPath);
            yield return request;
            m_Asset = request.asset;
        }
    }

    /// <summary>
    /// 采用资源从 AssetBundle中加载方式的资源类
    /// </summary>
    private class SEUResourceLoadedFromBundle : SEUResource
    { 
        public SEUResourceLoadedFromBundle(string path) : base(path)
        {
        }
        protected override void LoadAsset()
        {
            SEUResource bundleRes = m_Pool.LoadAssetBundle(m_LoadPath,true);
            AssetBundle bundle = bundleRes.asset as AssetBundle;
            if (bundle != null)
            {
                AddDependenceResources(bundleRes);
                Object asset = bundle.LoadAsset(System.IO.Path.GetFileName(m_LoadPath));
                if(m_Asset == null)
                {
                    m_Asset = asset;
                }
            }
            LogResult();
        }

        protected override IEnumerator LoadAssetAsync()
        {
            Request request = m_Pool.LoadAssetBundleAsyn(m_LoadPath,true);
            yield return request;
            SEUResource bundleRes = request.resource;
            AssetBundle bundle = bundleRes.asset as AssetBundle;
            if (bundle != null)
            {
                AddDependenceResources(bundleRes);
                AssetBundleRequest bdRequest = bundle.LoadAssetAsync(System.IO.Path.GetFileName(m_LoadPath));
                yield return bdRequest;
                if(m_Asset == null)
                {                    
                    m_Asset = bdRequest.asset;
                }
            }
            LogResult();
        }
    }

    /// <summary>
    /// Manifest AssetBundle 资源对象 从该对象中可以获取AB包的依赖
    /// </summary>
    private class SEUMenifestBundleResource : SEUResourceLoadedFromBundle
    {
        AssetBundle bundle;
        public SEUMenifestBundleResource(string path):base(path)
        {
        }
        protected override void LoadAsset()
        {
            bundle = m_Pool.LoadAssetBundleInternal(m_LoadPath);
            if (bundle != null)
            {
                m_Asset = bundle.LoadAsset("assetbundlemanifest");
            }
            LogResult();
        }
        protected override IEnumerator LoadAssetAsync()
        {
            AssetBundleCreateRequest  createReuest= m_Pool.LoadAssetBundlAsynInternal(m_LoadPath);
            yield return createReuest;
            if(bundle == null)
            {
                bundle = createReuest.assetBundle;
            }
            if (bundle != null)
            {
                AssetBundleRequest request = bundle.LoadAssetAsync("assetbundlemanifest");
                yield return request;
                if(m_Asset == null)
                {
                    m_Asset = request.asset;
                }
            }
            LogResult();
        }
        protected override void ReleaseResource()
        {
            base.ReleaseResource();
            if(bundle != null)
            {
                bundle.Unload(true);
            }

        }
    }

    /// <summary>
    /// AssetBundle 资源类
    /// </summary>
    private class SEUABResource : SEUResourceLoadedFromBundle
    {

        public SEUABResource(string path) : base(path)
        {

        }

        protected override void LoadAsset()
        {
            SEUResource manifestRes = m_Pool.LoadBundleManifest(m_LoadPath);          
            if (manifestRes != null&& manifestRes.asset != null)
            {
                AddDependenceResources(manifestRes);
                AssetBundleManifest manifest = manifestRes.asset as AssetBundleManifest;
                if (manifest != null)
                {
                    string[] dependenciesPaths = manifest.GetAllDependencies(m_LoadPath);
                    if (dependenciesPaths != null)
                    {
                        for (int i = 0; i < dependenciesPaths.Length; i++)
                        {
                            SEUResource depRes = m_Pool.LoadAssetBundle(dependenciesPaths[i]);
                            AddDependenceResources(depRes);
                        }
                    }
                } 
            } 
            if(m_Asset == null)
            {                 
                m_Asset = m_Pool.LoadAssetBundleInternal(m_LoadPath);
            }
            else
            {
                Debug.LogWarning("[异步冲突]");
            }
            LogResult();
           
        }

        protected override IEnumerator LoadAssetAsync()
        {
            SEUResource manifestRes = m_Pool.LoadBundleManifest(m_LoadPath);          
            if (manifestRes != null && manifestRes.asset != null)
            {
                AddDependenceResources(manifestRes);
                AssetBundleManifest manifest = manifestRes.asset as AssetBundleManifest;
                if (manifest != null)
                {
                    string[] dependenciesPaths = manifest.GetAllDependencies(m_LoadPath);
                    if (dependenciesPaths != null)
                    {
                        for (int i = 0; i < dependenciesPaths.Length; i++)
                        {
                            Request request = m_Pool.LoadAssetBundleAsyn(dependenciesPaths[i]);
                            yield return request;
                            SEUResource depRes = request.resource;
                            AddDependenceResources(depRes);
                        }
                    }
                }
            }
            AssetBundleCreateRequest createRequest  = m_Pool.LoadAssetBundlAsynInternal(m_LoadPath);
            yield return createRequest;
            if(m_Asset == null)//做一下判断 防止和同步冲突
            {
                m_Asset = createRequest.assetBundle;
            }
            else
            {
                Debug.LogWarning("[同步冲突]");
            }
            LogResult();
        }

        protected override void ReleaseResource()
        {
            base.ReleaseResource();
            AssetBundle bundle = m_Asset as AssetBundle;
            if (bundle != null)
            {
                bundle.Unload(true);
            }
        }
    }
}


