#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial class SEUResources
{
    protected Object m_Asset = null;
    protected Object asset
    {
        get
        {
            return m_Asset;
        }
    }
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

    private SEUResourcesPool m_Pool = null;

    protected List<SEUResources> m_DependenceResources = new List<SEUResources>();

    protected SEUResources(string path,System.Type type)
    {
        m_LoadPath = path;
        m_Type = type;
    }

    private string GUID()
    {
        return  ToResGUID(m_LoadPath, m_Type);
    }

    internal void Use()
    {
        m_RefCount++;
#if SEU_DEBUG
        Debug_StackInfo.Add("[Use]" + StackTraceUtility.ExtractStackTrace());
#endif
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
#if SEU_DEBUG
            Debug_StackInfo.Add("[UnUse]" + StackTraceUtility.ExtractStackTrace());
#endif
            if (m_RefCount == 0)
            {
                if(m_Pool != null)
                {
                    m_Pool.PopResource(this);
                    for (int i = 0; i < m_DependenceResources.Count; i++)
                    {
                        SEUResources.UnLoadResource(m_DependenceResources[i]);
                    }
                }
                else
                {
                    Debug.LogError(GetType()+ " pool is NULL");
                }            
            }
        }    
    }

    private void AttachPool(SEUResourcesPool pool)
    {
        m_Pool = pool;
    }

    protected virtual void ReleaseResource()
    {
    
    }

    protected virtual void LoadAsset() 
    {
    }
    protected virtual IEnumerator LoadAssetAsync()
    {
        yield break;
    }

    protected Request SendLoadAsyncRequest(System.Action<SEUResources> callback =null)
    {
        if(m_LoadAysncRequest == null)
        {
            m_LoadAysncRequest = new Request(this,callback);
        }
        return m_LoadAysncRequest;
    }

    protected void AddDependenceResources(SEUResources resource)
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

    private static string ToResGUID(string path, System.Type type)
    {
        return path + type.ToString();
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

    static private void UnLoadResource(SEUResources resource)
    {
        if (resource != null)
        {
            resource.UnUsed();
        }
    }
 
    static private SEUResources _Load(string path, System.Type type)
    {
        path = path.ToLower();
        SEUResources result = m_ResourcePool.Load(path, type);
        return result;
    }

    static private Request _LoadAsync(string path, System.Type type, System.Action<SEUResources> callback = null)
    {
        path = path.ToLower();
        return m_ResourcePool.LoadAsyn(path, type,callback);
    }

#if SEU_DEBUG||UNITY_EDITOR
    public int refCount
    {
        get
        {
            return m_RefCount;
        }
    }
    public List<SEUResources> dependenceResources
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
        SEUResourcesDebug debugObject = DebugObject.AddComponent<SEUResourcesDebug>();
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
    private class SEUNormalResource : SEUResources
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
    private class SEUResourcesLoadedFromBundle : SEUResources
    { 
        public SEUResourcesLoadedFromBundle(string path, System.Type type) : base(path, type)
        {
        }
        protected override void LoadAsset()
        {
            SEUResources bundleRes = m_Pool.LoadAssetBundle(m_LoadPath,true);
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
            SEUResources bundleRes = request.resource;
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
    private class SEUMenifestBundleResource : SEUResourcesLoadedFromBundle
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
    private class SEUABResource : SEUResourcesLoadedFromBundle
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

            SEUResources manifestRes = m_Pool.LoadBundleManifest(m_LoadPath);          
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
                            SEUResources depRes = m_Pool.LoadAssetBundle(dependenciesPaths[i]);
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
            SEUResources manifestRes = m_Pool.LoadBundleManifest(m_LoadPath);          
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
                            SEUResources depRes = request.resource;
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

    private class SEUResourcesPool
    {
        class SEULoaderResourceGroupPooolRegister
        {
            class PathNode
            {
                internal string name;
                internal List<PathNode> m_ChildNode = new List<PathNode>();
                internal int poolCode = -1;
                internal SEUResourcesPool Register(string[] folders, int index = 0)
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
                            SEUResourcesPool pool = new SEUResourcesPool();
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
            internal SEUResourcesPool ResisterGroupPath(string path)
            {

                string[] folders = path.Split(new string[] { "/" }, System.StringSplitOptions.None);
                SEUResourcesPool pool = Root.Register(folders);
                return pool;

            }
            internal int GetGroupPoolCode(string path)
            {
                string[] folders = path.Split(new string[] { "/" }, System.StringSplitOptions.None);
                return Root.GetPoolCode(folders);
            }
        }

        private Dictionary<int, SEUResourcesPool> m_ResourceGroupPool = new Dictionary<int, SEUResourcesPool>();
        private Dictionary<string, SEUResources> m_Resources = new Dictionary<string, SEUResources>();
        private static Dictionary<string, SEUResources> m_AssetBundles = new Dictionary<string, SEUResources>();
        private SEULoaderResourceGroupPooolRegister m_GroupPoolRegister;
        private SEULoaderType m_LoaderType;
        private SEUResourcesUnLoadType m_UnloadType;
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



        static SEUResourcesPool()
        {
            Debug_SEUPoolObject = new GameObject("_[SEUAssetPool]_");
            GameObject.DontDestroyOnLoad(Debug_SEUPoolObject);

            Debug_ResourcesLoadObject = new GameObject("_[ResourcesLoad]_");
            Debug_ResourcesLoadObject.transform.SetParent(Debug_SEUPoolObject.transform);

            Debug_AssetsObject = new GameObject("_[Assets]");
            Debug_AssetsObject.transform.SetParent(Debug_ResourcesLoadObject.transform);

            Debug_AssetBundleLoadObject = new GameObject("_[AssetBundleLoad]_");
            Debug_AssetBundleLoadObject.transform.SetParent(Debug_SEUPoolObject.transform);

            Debug_AssetBundlesObject = new GameObject("_[AssetBundles]_");
            Debug_AssetBundlesObject.transform.SetParent(Debug_AssetBundleLoadObject.transform);

            Debug_AssetsObjectLoadByBundles = new GameObject("_[Assets]_");
            Debug_AssetsObjectLoadByBundles.transform.SetParent(Debug_AssetBundleLoadObject.transform);

        }
#endif
        public SEUResourcesPool()
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
            SEUResourcesUnLoadType unLoadType = SEUResourcesUnLoadType.REFCOUNT_ZERO,
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

        private void PushResource(SEUResources resource)
        {
            string resGUID = resource.GUID();
            Dictionary<string, SEUResources> container = null;
            if (resource is SEUABResource || resource is SEUMenifestBundleResource)
            {
                container = m_AssetBundles;
            }
            else
            {
                container = m_Resources;
            }
            if (!container.ContainsKey(resGUID))
            {
                container.Add(resGUID, resource);
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
                Debug.Log(resGUID);
                Debug.LogError("Error");
            }
        }

        internal void PopResource(SEUResources resource)
        {
            string resGUI = resource.GUID();
            Dictionary<string, SEUResources> container = null;
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
                if (m_UnloadType == SEUResourcesUnLoadType.REFCOUNT_ZERO)
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

        private SEUResourcesPool GetGroupPool(string path)
        {
            int id = m_GroupPoolRegister.GetGroupPoolCode(path);
            SEUResourcesPool pool = null;
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

        internal SEUResources Load(string path, System.Type type)
        {
            SEUResourcesPool pool = GetGroupPool(path);
            if (pool != null)
            {
                return pool.LoadInternal(path,type);
            }
            return null;
        }

        internal Request LoadAsyn(string path, System.Type type ,System.Action<SEUResources> callback =null)
        {
            SEUResourcesPool pool = GetGroupPool(path);
            if (pool != null)
            {
                return pool.LoadAsynInternal(path, type,callback);
            }
            return null; ;
        }

        private SEUResources LoadInternal(string path,System.Type type)
        {
            SEUResources resource = null;
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
                        resource = new SEUResourcesLoadedFromBundle(path,type);
                        break;
                }
                PushResource(resource);

            }
            if (resource.asset == null)
            {
                resource.LoadAsset();
            }
            resource.Use();

            return resource;
        }

        private Request LoadAsynInternal(string path,System.Type type,System.Action<SEUResources> callback)
        {
            SEUResources resource = null;
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
                        resource = new SEUResourcesLoadedFromBundle(path,type);
                        break;
                }
                PushResource(resource);
            }
            resource.Use();

            return resource.SendLoadAsyncRequest(callback);
        }

        internal SEUResources LoadAssetBundle(string path, bool isNeedConvertBundlePath = false)
        {
            string bundlePath = path;
            if (isNeedConvertBundlePath)
            {
                bundlePath = m_ResourceToBundlePathConverter.HandlePath(path);
            }
            System.Type type = typeof(AssetBundle);
            string resGUID = ToResGUID(bundlePath ,type);
            SEUResources resource = null;
            if (m_AssetBundles.ContainsKey(resGUID))
            {
                resource = m_AssetBundles[resGUID];
                /// 这样处理 为了同步和异步并存
                if (resource.asset == null)
                {
                    resource.LoadAsset();
                }
            }
            else
            {
                resource = new SEUABResource(bundlePath,type);
                PushResource(resource);
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
            System.Type type = typeof(AssetBundle);
            string resGUID = ToResGUID(bundlePath, type);
            SEUResources resource = null;
            if (m_AssetBundles.ContainsKey(resGUID))
            {
                resource = m_AssetBundles[resGUID];
            }
            else
            {
                resource = new SEUABResource(bundlePath,type);
                PushResource(resource);
            }
            return resource.SendLoadAsyncRequest();
        }

        internal SEUResources LoadBundleManifest(string path)
        {
            string manifestPath = m_ManifestPathProvider.GetPath();
            System.Type type = typeof(AssetBundleManifest);
            string resGUID = ToResGUID(manifestPath, type);
            SEUResources resource = null;
            if (m_AssetBundles.ContainsKey(resGUID))
            {
                resource = m_AssetBundles[resGUID];
                if (resource.asset == null)
                {
                    resource.LoadAsset();
                }
            }
            else
            {
                resource = new SEUMenifestBundleResource(manifestPath, type);
                PushResource(resource);
                resource.LoadAsset();
            }
            return resource;
        }

        internal Request LoadBundleManifestAsync(string path)
        {
            string manifestPath = m_ManifestPathProvider.GetPath();
            System.Type type = typeof(UnityEngine.Object);
            string resGUID = ToResGUID(manifestPath,type);
            SEUResources resource = null;
            if (m_AssetBundles.ContainsKey(resGUID))
            {
                resource = m_AssetBundles[resGUID];
            }
            else
            {
                resource = new SEUMenifestBundleResource(manifestPath,type);
                PushResource(resource);
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
            SEUResourcesUnLoadType unLoadType = SEUResourcesUnLoadType.REFCOUNT_ZERO,
            IPathProvider manifestBunderPathProvider = null,
            IPathConverter resToBundlerPathConverter = null
            )
        {
            SEUResourcesPool pool = m_GroupPoolRegister.ResisterGroupPath(groupPath);
            if (pool != null)
            {
                pool.InitPool(groupPath, loaderType, unLoadType, manifestBunderPathProvider, resToBundlerPathConverter);
            }
            AddGroupPool(pool);
        }

        private void AddGroupPool(SEUResourcesPool pool)
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

    private class SEUObjectPool
    {
        private Dictionary<int, SEUResources> m_AssetRefSEUResources = new Dictionary<int, SEUResources>();
        private Dictionary<int, SEUResources> m_InstantiateRefSEUResources = new Dictionary<int, SEUResources>();

#if SEU_DEBUG
        GameObject Debug_SEUObjectPoolObject;

        internal SEUObjectPool()
        {
            Debug_SEUObjectPoolObject = new GameObject("_[SEUObjectPool]_");
            GameObject.DontDestroyOnLoad(Debug_SEUObjectPoolObject);
        }
#endif
        internal void  AttachAssetToInstance(int assetCode, int code)
        {
            SEUResources refResource = null;

            m_AssetRefSEUResources.TryGetValue(assetCode, out refResource);
            if (refResource == null)
            {
                m_InstantiateRefSEUResources.TryGetValue(assetCode, out refResource);
            }
            if (refResource != null)
            {
                if (!m_InstantiateRefSEUResources.ContainsKey(code))
                {
                    m_InstantiateRefSEUResources.Add(code, refResource);

                }
                refResource.Use();
            }
            else
            {
                Debug.LogError("SEUResources Instantiate Object ,But the Object  is not in ref system " + StackTraceUtility.ExtractStackTrace());
            }
        }

        internal void DestoryObject(Object asset)
        {
            if ((TryDestoryObject(asset, true) ||TryDestoryObject(asset, false)) == false)
            {
                Debug.LogError("[SEUResources] Try Destory Object ,But it not in Ref System " + StackTraceUtility.ExtractStackTrace());
            }
        }

        internal  bool TryDestoryObject(Object asset, bool isAsset)
        {
            int code = asset.GetInstanceID();
            Dictionary<int, SEUResources> record = null;
            if (isAsset)
            {
                record = m_AssetRefSEUResources;
            }
            else
            {
                record = m_InstantiateRefSEUResources;
            }
            SEUResources refResource = null;
            record.TryGetValue(code, out refResource);
            if (refResource != null)
            {       
                if (refResource.m_RefCount == 0)
                {
                    record.Remove(code);
                }
                if (isAsset == false)
                {
                    Object.Destroy(asset);
                }
                UnLoadResource(refResource);
                return true;
            }
            return false;
        }

        internal void PopResource(bool isAsset)
        {

        }

        internal Object PushResource(SEUResources resource, bool isAsset)
        {
            Object asset = null;
            if (resource != null)
            {
                Dictionary<int, SEUResources> record = null;
                if (isAsset)
                {
                    record = m_AssetRefSEUResources;
                }
                else
                {
                    record = m_InstantiateRefSEUResources;
                }
                if (resource.asset != null)
                {
                    int code = resource.asset.GetInstanceID();
                    if (!record.ContainsKey(code))
                    {
                        record.Add(code, resource);
                    }
                    asset = resource.asset;
                }
                else
                {
                    UnLoadResource(resource);
                }
            }
            return asset;
        }

    }

    static SEUResourcesPool m_ResourcePool;
    static SEUObjectPool   m_ObjectPool;

    static SEUResources()
    {
        m_ResourcePool = new SEUResourcesPool();
        m_ResourcePool.InitPool("defualt", SEULoaderType.RESOURCE);
        m_ObjectPool = new SEUObjectPool();
    }
}


