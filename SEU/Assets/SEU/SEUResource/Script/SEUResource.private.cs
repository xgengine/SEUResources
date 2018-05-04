//#define SEU_DEBUG
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

    static SEUResourcePool m_ResourcePool;

    static SEUResource()
    {
        m_ResourcePool = new SEUResourcePool();
        m_ResourcePool.InitPool("Resource", SEULoaderType.RESOURCE);
    }

    private class SEUResourcePool
    {
        class SEULoaderResourceGroupPooolRegister
        {
            class PathNode
            {
                internal string name;
                internal List<PathNode> m_ChildNode = new List<PathNode>();
                internal int poolCode = -1;
                internal SEUResourcePool Register(string[] folders, int index = 0)
                {
                    string nodeName = folders[index];
                    PathNode cnode = null;
                    for (int i = 0; i < m_ChildNode.Count; i++)
                    {
                        if (m_ChildNode[i].name == nodeName)
                        {
                            cnode = m_ChildNode[i];
                        }
                    }
                    if (cnode == null)
                    {
                        if (folders.Length - 1 == index)
                        {
                            PathNode endNode = new PathNode();
                            SEUResourcePool pool = new SEUResourcePool();
                            endNode.poolCode = pool.GetHashCode();
                            endNode.name = folders[index];
                            m_ChildNode.Add(endNode);
                            return pool;
                        }
                        else
                        {
                            PathNode midNode = new PathNode();
                            midNode.name = folders[index];
                            m_ChildNode.Add(midNode);
                            index++;
                            return midNode.Register(folders, index);
                        }
                    }
                    else
                    {
                        if (folders.Length - 1 == index)
                        {
                            Debug.LogError("Already Resgister");
                            return null;
                        }
                        else
                        {
                            if (cnode.poolCode != -1)
                            {
                                Debug.LogError("Register Error 2");
                                return null;
                            }
                            index++;
                            return cnode.Register(folders, index);
                        }
                    }
                }
                internal int GetPoolCode(string[] folders, int index = 0)
                {
                    string nodeName = folders[index];
                    PathNode cnode = null;
                    for (int i = 0; i < m_ChildNode.Count; i++)
                    {
                        if (m_ChildNode[i].name == nodeName)
                        {
                            cnode = m_ChildNode[i];
                            break;
                        }
                    }
                    if (cnode != null)
                    {
                        if (cnode.poolCode != - 1 )
                        {
                            return cnode.poolCode;
                        }
                        else
                        {
                            index++;
                            return cnode.GetPoolCode(folders, index);
                        }
                    }
                    return -1;
                }
            }
            PathNode Root;
            internal SEULoaderResourceGroupPooolRegister()
            {
                Root = new PathNode();
            }
            internal SEUResourcePool ResisterGroupPath(string path)
            {
                string[] folders = path.Split(new string[] { "/" }, System.StringSplitOptions.None);
                SEUResourcePool pool = Root.Register(folders);
                return pool;

            }
            internal int GetGroupPoolCode(string path)
            {
                string[] folders = path.Split(new string[] { "/" }, System.StringSplitOptions.None);
                return Root.GetPoolCode(folders);
            }
        }

        private Dictionary<int, SEUResourcePool> m_ResourceGroupPool = new Dictionary<int, SEUResourcePool>();
        private Dictionary<string, SEUResource> m_Resources = new Dictionary<string, SEUResource>();
        private Dictionary<string, SEUResource> m_AssetBundles = new Dictionary<string, SEUResource>();
        private SEULoaderResourceGroupPooolRegister m_GroupPoolRegister;
        private SEULoaderType         m_LoaderType;
        private SEUResourceUnLoadType m_UnloadType;
        private SEUABPathBuilder      m_ABPathBuilder;

        private string m_GroupPath = "Resources";

#if SEU_DEBUG
        static GameObject Debug_SEUPoolObject;
        GameObject Debug_GroupPoolObject;
        GameObject Debug_AssetsObject;
        GameObject Debug_AssetBundlesObject;

        static SEUResourcePool()
        {
            Debug_SEUPoolObject = new GameObject("SEUPool");
            GameObject.DontDestroyOnLoad(Debug_SEUPoolObject);
        }
#endif
        public SEUResourcePool()
        {
            m_GroupPoolRegister = new SEULoaderResourceGroupPooolRegister();
        }

        internal string groupPath
        {
            get
            {
                return m_GroupPath;
            }
        }

        internal void InitPool(string groupPath, SEULoaderType loaderType, SEUResourceUnLoadType unLoadType =SEUResourceUnLoadType.REFCOUNT_ZERO, SEUABPathBuilder abPathBuilder=null)
        {
            m_GroupPath = groupPath;
            m_LoaderType = loaderType;
            m_ABPathBuilder = abPathBuilder;
            m_UnloadType = unLoadType;

#if SEU_DEBUG
            GameObject degbug_GroupPoolObject = new GameObject(m_GroupPath);
            degbug_GroupPoolObject.transform.SetParent(Debug_SEUPoolObject.transform);
            if (abPathBuilder != null)
            {
                Debug_AssetBundlesObject = new GameObject("AssetBundle");
                Debug_AssetBundlesObject.transform.SetParent(degbug_GroupPoolObject.transform); 
            }
            Debug_AssetsObject = new GameObject("Assets");
            Debug_AssetsObject.transform.SetParent(degbug_GroupPoolObject.transform);
#endif
        }

        private void PushResource(string path, SEUResource resource)
        {
            Dictionary<string, SEUResource> container = null;
            if (resource is SEUABResource)
            {
                container = m_AssetBundles;    
            }
            else
            {
                container = m_Resources;
            }
            if (!container.ContainsKey(path))
            {
                container.Add(path, resource);
                resource.AttachPool(this);
#if SEU_DEBUG
                if(container == m_AssetBundles)
                {
                    resource.DebugCreateObject();
                    resource.DebugObject.transform.SetParent(Debug_AssetBundlesObject.transform);
                }
                else
                {
                    resource.DebugCreateObject();
                    resource.DebugObject.transform.SetParent(Debug_AssetsObject.transform);
                }         
#endif
            }
            else
            {
                Debug.LogError("Error");
            }
        }

        internal void PopResource(SEUResource resource)
        {
            string path = resource.loadPath;
            Dictionary<string, SEUResource> container = null;
            if (resource is SEUABResource)
            {
                container = m_AssetBundles;
            }
            else
            {
                container = m_Resources;
            }

            if (container.ContainsKey(path))
            {
                if(m_UnloadType == SEUResourceUnLoadType.REFCOUNT_ZERO)
                {                   
                    container.Remove(path);
                    resource.ReleaseResource();
#if SEU_DEBUG
                    GameObject.Destroy(resource.DebugObject);
#endif
                }
            }
            else
            {
                Debug.LogError("Error");
            }
        }

        internal SEUResource Load(string path)
        {
            SEUResourcePool pool = GetGroupPool(path);
            if (pool != null)
            {
                return pool.LoadInternal(path);
            }
            return null;
        }

        private SEUResourcePool GetGroupPool(string path)
        {
            int id = m_GroupPoolRegister.GetGroupPoolCode(path);
            SEUResourcePool pool = null;
            if (id == -1)
            {
                pool = this;

            }
            else
            {
                if (m_ResourceGroupPool.ContainsKey(id))
                {
                    pool = m_ResourceGroupPool[id];
                }
            }
            return pool;
        }

        private SEUResource LoadInternal(string path)
        {
            SEUResource resource = null;
            if (m_Resources.ContainsKey(path))
            {
                resource = m_Resources[path];
            }
            else
            {
                switch (m_LoaderType)
                {
                    case SEULoaderType.RESOURCE:
                        resource = new SEUNormalResource(path);
                        break;
                    case SEULoaderType.AB:
                        resource = new SEUResourceLoadedFromBundle(path);
                        break;
                }
                PushResource(path, resource);
               
            }
            if(resource.asset == null)
            {
                resource.LoadAsset();
            }
            resource.Use();
#if SEU_EDITOR
            resource.Debug_MarkStackInfo();
#endif
            return resource;
        }

        private Request LoadAsynInternal(string path)
        {        
            SEUResource resource = null;
            if (m_Resources.ContainsKey(path))
            {
                resource = m_Resources[path];           
            }
            else
            {
                switch (m_LoaderType)
                {
                    case SEULoaderType.RESOURCE:
                        resource = new SEUNormalResource(path);
                        break;
                    case SEULoaderType.AB:
                        resource = new SEUResourceLoadedFromBundle(path);
                        break;
                }
                PushResource(path, resource); 
            }
            resource.Use();
#if SEU_DEBUG
            resource.Debug_MarkStackInfo();
#endif
            return resource.SendLoadAsyncRequest();
        }

        internal Request LoadAsyn(string path)
        {
            SEUResourcePool pool = GetGroupPool(path);
            if (pool != null)
            {
                return pool.LoadAsynInternal(path);
            }
            return null; ;
        }

        internal SEUResource LoadAssetBundle(string path)
        {
            string bundlePath = m_ABPathBuilder.BundlePathHandle(path);
            SEUResource resource = null;
            if (m_AssetBundles.ContainsKey(bundlePath))
            {
                resource = m_AssetBundles[bundlePath];
                if (resource.asset == null)
                {
                    resource.LoadAsset();
                }
                else
                {
                    resource.Use();
                }
            }
            else
            {
                resource = new SEUABResource(bundlePath);
                PushResource(bundlePath, resource);
                resource.Use();
                resource.LoadAsset();
            }
#if SEU_EDITOR
            resource.Debug_MarkStackInfo();
#endif
            return resource;
        }

        internal Request LoadAssetBundleAsyn(string path)
        {
            string bundlePath = m_ABPathBuilder.BundlePathHandle(path);
            SEUResource resource = null;
            if (m_AssetBundles.ContainsKey(bundlePath))
            {
                resource = m_AssetBundles[bundlePath]; 
            }
            else
            {
                resource = new SEUABResource(bundlePath);
                PushResource(bundlePath, resource); 
            }
            resource.Use();
#if SEU_EDITOR
            resource.Debug_MarkStackInfo();
#endif
            return resource.SendLoadAsyncRequest();
        }

        internal SEUResource LoadBundleManifest(string path)
        {
            string manifestPath = m_ABPathBuilder.ManifestBundlePathHandle(path);
            SEUResource resource = null;
            if (m_Resources.ContainsKey(manifestPath))
            {
                resource = m_Resources[manifestPath];
                if (resource.asset == null)
                {
                    resource.LoadAsset();

                }
                else
                {
                    resource.Use();
                }
            }
            else
            {
                resource = new SEUMenifestBundleResource(manifestPath);
                PushResource(manifestPath, resource);
                resource.Use();
                resource.LoadAsset();
            }
#if SEU_DEBUG
            resource.Debug_MarkStackInfo();
#endif
            return resource;
        }

        internal Request LoadBundleManifestAsync(string path)
        {

            string manifestPath = m_ABPathBuilder.ManifestBundlePathHandle(path);
            SEUResource resource = null;
            if (m_Resources.ContainsKey(manifestPath))
            {
                resource = m_Resources[manifestPath];
               
            }
            else
            {
                resource = new SEUMenifestBundleResource(manifestPath);
                PushResource(manifestPath, resource);    
            }

            resource.Use();
#if SEU_DEBUG
            resource.Debug_MarkStackInfo();
#endif
            return resource.SendLoadAsyncRequest();
        }

        internal void ResisterGroupPath(string path, SEULoaderType loaderType,SEUResourceUnLoadType unLoadType, SEUABPathBuilder ABPathBuilder)
        {
            SEUResourcePool pool = m_GroupPoolRegister.ResisterGroupPath(path);
            if (pool != null)
            {
                pool.InitPool(path, loaderType, unLoadType, ABPathBuilder);
            }
            AddGroupPool(pool);
        }

        private void AddGroupPool(SEUResourcePool pool)
        {
            if (pool != null)
            {
                int poolCode = pool.GetHashCode();
                if (!m_ResourceGroupPool.ContainsKey(poolCode))
                {
                    m_ResourceGroupPool.Add(poolCode, pool);
                }
            }
        }

    }

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
            SEUResource bundleRes = m_Pool.LoadAssetBundle(m_LoadPath);
            AddDependenceResources(bundleRes);
            AssetBundle bundle = bundleRes.asset as AssetBundle;
            if (bundle != null)
            {
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
            Request request = m_Pool.LoadAssetBundleAsyn(m_LoadPath);
            yield return request;
            SEUResource bundleRes = request.resource;
            AddDependenceResources(bundleRes);

            AssetBundle bundle = bundleRes.asset as AssetBundle;
            if (bundle != null)
            {
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
            byte[] buffer = SEUFileLoader.ReadAllBytes(m_LoadPath);
            bundle = AssetBundle.LoadFromMemory(buffer);
            if (bundle != null)
            {
                m_Asset = bundle.LoadAsset("assetbundlemanifest");
            }
            LogResult();
        }
        protected override IEnumerator LoadAssetAsync()
        {
            byte[] buffer = SEUFileLoader.ReadAllBytes(m_LoadPath);
            AssetBundleCreateRequest  createReuest= AssetBundle.LoadFromMemoryAsync(buffer);
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
            if (bundle != null)
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
            AddDependenceResources(manifestRes);
            if (manifestRes != null&& manifestRes.asset != null)
            {
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
            ///加载AB包
            byte[] buffer = SEUFileLoader.ReadAllBytes(m_LoadPath);
            if(m_Asset == null)
            {
                //string path = Application.dataPath + "/Bundles/test_group/" + m_LoadPath;
                //m_Asset = AssetBundle.LoadFromFile(path);
                m_Asset = AssetBundle.LoadFromMemory(buffer);
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
            AddDependenceResources(manifestRes);
            if (manifestRes != null && manifestRes.asset != null)
            {
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
            ///加载AB包
            byte[] buffer = SEUFileLoader.ReadAllBytes(m_LoadPath);
            AssetBundleCreateRequest createRequest  = AssetBundle.LoadFromMemoryAsync(buffer);
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


