#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// SEUResource 对Unity资源加载进行封装、保持和 Resources 资源加载接口形式，并对资源进行引用计数的管理
/// </summary>
public class SEUResource
{
    protected Object m_Asset = null;

    public Object asset
    {
        get
        {
            return m_Asset;
        }
    }

    private int m_RefCount = 0;

   
    private SEUResourcePool m_Pool = null;

    protected List<SEUResource> m_DependenceResources = new List<SEUResource>();

    protected string m_LoadPath;

    public string loadPath
    {
        get
        {
            return m_LoadPath;
        }
    }

    public SEUResource(string path)
    {
        m_LoadPath = path;
    }

    private void Use()
    {
        m_RefCount++;
        for (int i = 0; i < m_DependenceResources.Count; i++)
        {
            m_DependenceResources[i].Use();
        }
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

            for (int i = 0; i < m_DependenceResources.Count; i++)
            {
                m_DependenceResources[i].UnUsed();
            }
            if (m_RefCount == 0)
            {
                m_Pool.PopResource(this);
            }
        }
       
    }

    private void AttachPool(SEUResourcePool pool)
    {
        m_Pool = pool;
    }

    internal virtual void UnloadResource()
    {
        Debug.Log(GetType()+" unload ");
   
    }

    internal virtual void LoadAsset()
    {

    }

    protected void AddDependenceResources(SEUResource resource)
    {
        if(m_DependenceResources.Contains(resource) == false)
        {
            m_DependenceResources.Add(resource);
        }
    }

    #region 对外接口
    static public void ResisterGroupPath(string path, SEULoaderType loaderType, SEUResourceUnLoadType unLoadType, SEUABPathBuilder ABPathBuilder = null)
    {
        m_ResourcePool.ResisterGroupPath(path, loaderType,unLoadType, ABPathBuilder);
    }

    static public SEUResource Load(string path)
    {
        SEUResource result = m_ResourcePool.Load(path);
#if SEU_DEBUG
        result.Debug_StackInfo.Add("[Load]"+StackTraceUtility.ExtractStackTrace());
#endif
        return result;
    }

    static public SEUResourceRequest LoadAsyn(string path)
    {
        return m_ResourcePool.LoadAsyn(path);
    }
    
    static public void UnLoadUsedResource(SEUResource resource)
    {
#if SEU_DEBUG
        resource.Debug_StackInfo.Add("[UnLoad]" + StackTraceUtility.ExtractStackTrace());
        resource.UnUsed();
#endif
    }

#if SEU_DEBUG
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
#endif

#endregion

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
        static GameObject SEUPoolDebugObject;

        GameObject SEUGroupPoolDebugObject;
        GameObject SEUABResourceDebugObject;
        GameObject SEUResourceDebugObject;

        static SEUResourcePool()
        {
            SEUPoolDebugObject = new GameObject("SEUPool");
            GameObject.DontDestroyOnLoad(SEUPoolDebugObject);
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
            GameObject SEUGroupPoolDebugObject = new GameObject(m_GroupPath);
            SEUGroupPoolDebugObject.transform.SetParent(SEUPoolDebugObject.transform);
            if (abPathBuilder != null)
            {
                SEUABResourceDebugObject = new GameObject("AssetBundle");
                SEUABResourceDebugObject.transform.SetParent(SEUGroupPoolDebugObject.transform); 
            }
            SEUResourceDebugObject = new GameObject("Assets");
            SEUResourceDebugObject.transform.SetParent(SEUGroupPoolDebugObject.transform);
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
                resource.Use();
                container.Add(path, resource);
                resource.AttachPool(this);
#if SEU_DEBUG
                if(container == m_AssetBundles)
                {
                    resource.DebugCreateObject();
                    resource.DebugObject.transform.SetParent(SEUABResourceDebugObject.transform);
                }
                else
                {
                    resource.DebugCreateObject();
                    resource.DebugObject.transform.SetParent(SEUResourceDebugObject.transform);
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
                    resource.UnloadResource();
                    container.Remove(path);
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
                resource.Use();
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
                PushResource(path, resource); ;
                resource.LoadAsset();

                if (resource.asset == null)
                {
                    Debug.LogError("Load failed :" + path);
                }
            }
            return resource;
        }

        private SEUResourceRequest LoadAsynInternal(string path)
        {
            SEUResourceRequest request = new SEUResourceRequest();

            SEUResource resource = null;

            if (m_Resources.ContainsKey(path))
            {
                resource = m_Resources[path];
                resource.Use();
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
                resource.LoadAsset();

                if (resource.asset == null)
                {
                    Debug.LogError("Load failed :" + path);
                }
            }
            request
            return request;
        }

        internal SEUResourceRequest LoadAsyn(string path)
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
                resource.Use();
            }
            else
            {
                resource = new SEUABResource(bundlePath);
                PushResource(bundlePath, resource);
                resource.LoadAsset();
                if (resource.asset == null)
                {
                    Debug.LogError("Load failed :" + path);
                }
            }
            return resource;
        }

        internal SEUResource LoadBundleManifest(string path)
        {
            string manifestPath = m_ABPathBuilder.ManifestBundlePathHandle(path);
            SEUResource resource = null;
            if (m_Resources.ContainsKey(manifestPath))
            {
                resource = m_Resources[manifestPath];
                resource.Use();
            }
            else
            {
                resource = new SEUMenifestBundleResource(manifestPath);
                PushResource(manifestPath, resource);
                resource.LoadAsset();
                if (resource.asset == null)
                {
                    Debug.LogError("Load failed :" + path);
                }
            }
            return resource;
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
        internal override void LoadAsset()
        {
            m_Asset = Resources.Load(m_LoadPath);
        }

        internal override void UnloadResource()
        {
            Debug.Log("Resource");
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
        internal override void LoadAsset()
        {
            SEUResource bundleRes = m_Pool.LoadAssetBundle(m_LoadPath);
            AddDependenceResources(bundleRes);
            AssetBundle bundle = bundleRes.asset as AssetBundle;
            if (bundle != null)
            {
                m_Asset = bundle.LoadAsset(System.IO.Path.GetFileName(m_LoadPath));
            }
            else
            {
                Debug.LogError("Load Failed :" + m_LoadPath);
            }
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
        internal override void LoadAsset()
        {
            byte[] buffer = SEUFileLoader.ReadAllBytes(m_LoadPath);
            bundle = AssetBundle.LoadFromMemory(buffer);
            if (bundle != null)
            {
                m_Asset = bundle.LoadAsset("assetbundlemanifest");
            }
        }
        internal override void UnloadResource()
        {
            base.UnloadResource();
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

        internal override void LoadAsset()
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
            m_Asset = AssetBundle.LoadFromMemory(buffer);
        }

        internal override void UnloadResource()
        {
            base.UnloadResource();
            AssetBundle bundle = m_Asset as AssetBundle;
            if (bundle != null)
            {
                bundle.Unload(true);
            }
        }
    }

}
public class SEUResourceRequest
{
    private SEUResource m_Resource;

    public SEUResource resource
    {
        get
        {
            return m_Resource;
        }
    }
    internal void AttachSEUResource(SEUResource resource)
    {

    }
}

public enum SEULoaderType
{
    RESOURCE,
    AB,
}

public enum SEUResourceUnLoadType
{
    REFCOUNT_ZERO,  //计数为零释放内存
    PERMANENT       //常驻内存
}

public class SEUABPathBuilder
{
    /// <summary>
    /// 根据加载路径，生成AB包路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public virtual string BundlePathHandle(string path)
    {
        return path;
    }
    /// <summary>
    /// 获取资源组AB包的Manifest，进而获取AB包的依赖
    /// </summary>
    /// <returns></returns>
    public virtual string ManifestBundlePathHandle(string path)
    {
        return "test_group";
    }
}

public class SEUBundleLoader
{
    public virtual AssetBundle LoadAssetBundle(string path)
    {
        return null;
    } 
    public virtual AssetBundleCreateRequest LoadAssetBundlAsyn(string path)
    {
        return null;
    }

}

/// <summary>
/// 文件加载器
/// </summary>
internal class SEUFileLoader
{
    static public byte[] ReadAllBytes(string path)
    {
        path = Application.dataPath +"/Bundles/test_group/"+path;
        return System.IO.File.ReadAllBytes(path);
    }
}

