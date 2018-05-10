#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial class SEUResource
{
    protected Object m_Asset = null;
    protected System.Type m_Type = typeof( UnityEngine.Object);
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

    protected SEUResource(string path,System.Type type)
    {
        m_LoadPath = path;
        m_Type = type;
    }

    private string GUID()
    {
        if(m_Pool == null)
        {
            throw new System.NullReferenceException(GetType()+" ResourcePool");
        }

        return m_Pool.ToResGUID(m_LoadPath, m_Type);
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
                if(m_Pool != null)
                {
                    m_Pool.PopResource(this);
                    for (int i = 0; i < m_DependenceResources.Count; i++)
                    {
                        SEUResource.UnLoadResource(m_DependenceResources[i]);
                    }
                }
                else
                {
                    Debug.LogError(GetType()+ " pool is NULL");
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
       // Debug.Log(GetType()+" unload ");
   
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
        public SEUNormalResource(string path,System.Type type) : base(path,type)
        {
        }
        protected override void LoadAsset()
        {
            m_Asset = Resources.Load(m_LoadPath,m_Type);
        }
        protected override IEnumerator LoadAssetAsync() 
        {
            ResourceRequest request = Resources.LoadAsync(m_LoadPath,m_Type);
            yield return request;
            m_Asset = request.asset;
        }
        protected override void ReleaseResource()
        {
            base.ReleaseResource();     
        }
    }

    /// <summary>
    /// 采用资源从 AssetBundle中加载方式的资源类
    /// </summary>
    private class SEUResourceLoadedFromBundle : SEUResource
    { 
        public SEUResourceLoadedFromBundle(string path, System.Type type) : base(path, type)
        {
        }
        protected override void LoadAsset()
        {
            SEUResource bundleRes = m_Pool.LoadAssetBundle(m_LoadPath,true);
            AssetBundle bundle = bundleRes.asset as AssetBundle;
            if (bundle != null)
            {
                AddDependenceResources(bundleRes);
                Object asset = bundle.LoadAsset(System.IO.Path.GetFileName(m_LoadPath),m_Type);
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
                AssetBundleRequest bdRequest = bundle.LoadAssetAsync(System.IO.Path.GetFileName(m_LoadPath),m_Type);
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
        public SEUMenifestBundleResource(string path, System.Type type) : base(path, type)
        {
        }
        protected override void LoadAsset()
        {
            bundle = m_Pool.LoadAssetBundleInternal(m_LoadPath);
            if (bundle != null)
            {
                m_Asset = bundle.LoadAsset("assetbundlemanifest");
            }
            //LogResult();
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
            //LogResult();
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

        public SEUABResource(string path, System.Type type) : base(path, type)
        {

        }

        protected override void LoadAsset()
        {
            if (m_Asset == null)
            {
                m_Asset = m_Pool.LoadAssetBundleInternal(m_LoadPath);
            }
            else
            {
                Debug.LogWarning("[异步冲突]");
            }

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
            //LogResult();

        }

        protected override IEnumerator LoadAssetAsync()
        {
            AssetBundleCreateRequest createRequest = m_Pool.LoadAssetBundlAsynInternal(m_LoadPath);
            yield return createRequest;
            if (m_Asset == null)//做一下判断 防止和同步冲突
            {
                m_Asset = createRequest.assetBundle;
            }
            else
            {
                Debug.LogWarning("[同步冲突] 已经处理");
            }
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
            //LogResult();
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
                        if (cnode.poolCode != -1)
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
        private static Dictionary<string, SEUResource> m_AssetBundles = new Dictionary<string, SEUResource>();
        private SEULoaderResourceGroupPooolRegister m_GroupPoolRegister;
        private SEULoaderType m_LoaderType;
        private SEUResourceUnLoadType m_UnloadType;
        private IPathConverter m_ResourceToBundlePathConverter;
        private IPathProvider m_ManifestPathProvider;
        private IPathConverter m_BundleFilePathConverter;
        private SEUBundleLoader m_BundleLoader;

        private string m_GroupPath = "defual";

#if SEU_DEBUG
        static GameObject Debug_ResourcesLoadObject;
        static GameObject Debug_AssetsObject;


        static GameObject Debug_SEUPoolObject;
        static GameObject Debug_AssetBundleLoadObject;
        static GameObject Debug_AssetBundlesObject;
        static GameObject Debug_AssetsObjectLoadByBundles;

        GameObject Debug_GroupPoolObject;



        static SEUResourcePool()
        {
            Debug_SEUPoolObject = new GameObject("SEUPool");
            GameObject.DontDestroyOnLoad(Debug_SEUPoolObject);

            Debug_ResourcesLoadObject = new GameObject("ResourcesLoad");
            Debug_ResourcesLoadObject.transform.SetParent(Debug_SEUPoolObject.transform);

            Debug_AssetsObject = new GameObject("Assets");
            Debug_AssetsObject.transform.SetParent(Debug_ResourcesLoadObject.transform);

            Debug_AssetBundleLoadObject = new GameObject("AssetBundleLoad");
            Debug_AssetBundleLoadObject.transform.SetParent(Debug_SEUPoolObject.transform);

            Debug_AssetBundlesObject = new GameObject("AssetBundles");
            Debug_AssetBundlesObject.transform.SetParent(Debug_AssetBundleLoadObject.transform);

            Debug_AssetsObjectLoadByBundles = new GameObject("Assets");
            Debug_AssetsObjectLoadByBundles.transform.SetParent(Debug_AssetBundleLoadObject.transform);

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

        internal void InitPool(
            string groupPath,
            SEULoaderType loaderType,
            SEUResourceUnLoadType unLoadType = SEUResourceUnLoadType.REFCOUNT_ZERO,
            IPathProvider manifestBunderPathProvider = null,
            IPathConverter resToBundlerPathConverter = null,
            SEUBundleLoader bundleLoader = null
            )
        {
            m_GroupPath = groupPath;
            m_LoaderType = loaderType;
            m_UnloadType = unLoadType;

            m_ResourceToBundlePathConverter = resToBundlerPathConverter;
            m_ManifestPathProvider = manifestBunderPathProvider;
            m_BundleLoader = bundleLoader;

            if (m_ResourceToBundlePathConverter == null)
            {
                m_ResourceToBundlePathConverter = new SEUDefulatResourceToBundlePathConverter();
            }
            if (m_ManifestPathProvider == null)
            {
                m_ManifestPathProvider = new SEUGroupManifestBundlePathProvider();
            }
            if (m_BundleLoader == null)
            {
                m_BundleLoader = new SEUBundleLoaderFromFile();
            }
            Debug_InitPool();
        }
        private void Debug_InitPool()
        {
#if SEU_DEBUG
            Debug_GroupPoolObject = new GameObject("[Assets Group] " + m_GroupPath);
            if (m_LoaderType == SEULoaderType.AB)
            {
                Debug_GroupPoolObject.transform.SetParent(Debug_AssetsObjectLoadByBundles.transform);
            }
            else
            {
                Debug_GroupPoolObject.transform.SetParent(Debug_AssetsObject.transform);
            }
#endif
        }

        private void PushResource(string path, SEUResource resource)
        {
            Dictionary<string, SEUResource> container = null;
            if (resource is SEUABResource || resource is SEUMenifestBundleResource)
            {
                container = m_AssetBundles;
            }
            else
            {
                container = m_Resources;
            }
            if (!container.ContainsKey(path+resource.m_Type))
            {
                container.Add(path+resource.m_Type, resource);
                resource.AttachPool(this);
#if SEU_DEBUG

                resource.DebugCreateObject();
                if (resource is SEUABResource || resource is SEUMenifestBundleResource)
                {
                    resource.DebugObject.transform.SetParent(Debug_AssetBundlesObject.transform);
                }
                else
                {
                    resource.DebugObject.transform.SetParent(Debug_GroupPoolObject.transform);
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
            string resGUI = resource.GUID();
            Dictionary<string, SEUResource> container = null;
            if (resource is SEUABResource || resource is SEUMenifestBundleResource)
            {
                container = m_AssetBundles;
            }
            else
            {
                container = m_Resources;
            }

            if (container.ContainsKey(resGUI))
            {
                if (m_UnloadType == SEUResourceUnLoadType.REFCOUNT_ZERO)
                {
                    container.Remove(resGUI);
                    resource.ReleaseResource();
#if SEU_DEBUG
                    GameObject.Destroy(resource.DebugObject);
#endif
                }
            }
            else
            {
                Debug.LogError("PopResource resource ,But can not find it in ResourcePool");
            }
        }

        internal SEUResource Load(string path, System.Type type)
        {
            SEUResourcePool pool = GetGroupPool(path);
            if (pool != null)
            {
                return pool.LoadInternal(path,type);
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

        private SEUResource LoadInternal(string path,System.Type type)
        {
            SEUResource resource = null;
            string resGUID = ToResGUID(path, type);
            if (m_Resources.ContainsKey(resGUID))
            {
                resource = m_Resources[resGUID];
            }
            else
            {
                switch (m_LoaderType)
                {
                    case SEULoaderType.RESOURCE:
                        resource = new SEUNormalResource(path,type);
                        break;
                    case SEULoaderType.AB:
                        resource = new SEUResourceLoadedFromBundle(path,type);
                        break;
                }
                PushResource(resGUID, resource);

            }
            if (resource.asset == null)
            {
                resource.LoadAsset();
            }
            resource.Use();
#if SEU_DEBUG
            resource.Debug_MarkStackInfo();
#endif
            return resource;
        }

        private Request LoadAsynInternal(string path,System.Type type)
        {
            SEUResource resource = null;
            string resGUID = ToResGUID(path, type);
            if (m_Resources.ContainsKey(resGUID))
            {
                resource = m_Resources[resGUID];
            }
            else
            {
                switch (m_LoaderType)
                {
                    case SEULoaderType.RESOURCE:
                        resource = new SEUNormalResource(path,type);
                        break;
                    case SEULoaderType.AB:
                        resource = new SEUResourceLoadedFromBundle(path,type);
                        break;
                }
                PushResource(resGUID, resource);
            }
            resource.Use();
#if SEU_DEBUG
            resource.Debug_MarkStackInfo();
#endif
            return resource.SendLoadAsyncRequest();
        }

        internal Request LoadAsyn(string path,System.Type type)
        {
            SEUResourcePool pool = GetGroupPool(path);
            if (pool != null)
            {
                return pool.LoadAsynInternal(path,type);
            }
            return null; ;
        }

        internal SEUResource LoadAssetBundle(string path, bool isNeedConvertBundlePath = false)
        {
            string bundlePath = path;
            if (isNeedConvertBundlePath)
            {
                bundlePath = m_ResourceToBundlePathConverter.HandlePath(path);
            }

            SEUResource resource = null;
            if (m_AssetBundles.ContainsKey(bundlePath))
            {
                resource = m_AssetBundles[bundlePath];
                /// 这样处理 为了同步和异步并存
                if (resource.asset == null)
                {
                    resource.LoadAsset();
                }
            }
            else
            {
                resource = new SEUABResource(bundlePath,typeof(UnityEngine.Object));
                PushResource(bundlePath, resource);
                resource.LoadAsset();
            }
            return resource;
        }

        internal Request LoadAssetBundleAsyn(string path, bool isNeedConvertBundlePath = false)
        {
            string bundlePath = path;
            if (isNeedConvertBundlePath)
            {
                bundlePath = m_ResourceToBundlePathConverter.HandlePath(path);
            }
            SEUResource resource = null;
            if (m_AssetBundles.ContainsKey(bundlePath))
            {
                resource = m_AssetBundles[bundlePath];
            }
            else
            {
                resource = new SEUABResource(bundlePath,typeof(UnityEngine.Object));
                PushResource(bundlePath, resource);
            }
            return resource.SendLoadAsyncRequest();
        }

        internal SEUResource LoadBundleManifest(string path)
        {
            string manifestPath = m_ManifestPathProvider.GetPath();
            SEUResource resource = null;
            if (m_AssetBundles.ContainsKey(manifestPath))
            {
                resource = m_AssetBundles[manifestPath];
                if (resource.asset == null)
                {
                    resource.LoadAsset();
                }
            }
            else
            {
                resource = new SEUMenifestBundleResource(manifestPath,typeof(UnityEngine.Object));
                PushResource(manifestPath, resource);
                resource.LoadAsset();
            }
            return resource;
        }

        internal Request LoadBundleManifestAsync(string path)
        {
            string manifestPath = m_ManifestPathProvider.GetPath();
            SEUResource resource = null;
            if (m_AssetBundles.ContainsKey(manifestPath))
            {
                resource = m_AssetBundles[manifestPath];
            }
            else
            {
                resource = new SEUMenifestBundleResource(manifestPath,typeof(UnityEngine.Object));
                PushResource(manifestPath, resource);
            }
            return resource.SendLoadAsyncRequest();
        }

        internal AssetBundle LoadAssetBundleInternal(string bundleName)
        {
            return m_BundleLoader.LoadAssetBundle(bundleName);
        }
        internal AssetBundleCreateRequest LoadAssetBundlAsynInternal(string bundleName)
        {
            return m_BundleLoader.LoadAssetBundlAsyn(bundleName);
        }
        internal void ResisterGroupPath(
            string groupPath,
            SEULoaderType loaderType,
            SEUResourceUnLoadType unLoadType = SEUResourceUnLoadType.REFCOUNT_ZERO,
            IPathProvider manifestBunderPathProvider = null,
            IPathConverter resToBundlerPathConverter = null
            )
        {
            SEUResourcePool pool = m_GroupPoolRegister.ResisterGroupPath(groupPath);
            if (pool != null)
            {
                pool.InitPool(groupPath, loaderType, unLoadType, manifestBunderPathProvider, resToBundlerPathConverter);
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

        internal string ToResGUID(string path,System.Type type)
        {
            return path + type.ToString();
        }
    }

    static SEUResourcePool m_ResourcePool;

    static SEUResource()
    {
        m_ResourcePool = new SEUResourcePool();
        m_ResourcePool.InitPool("defualt", SEULoaderType.RESOURCE);
    }
}


