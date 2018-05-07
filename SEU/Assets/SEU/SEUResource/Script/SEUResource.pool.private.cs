#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class SEUResource
{
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
            if (!container.ContainsKey(path))
            {
                container.Add(path, resource);
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
            string path = resource.loadPath;
            Dictionary<string, SEUResource> container = null;
            if (resource is SEUABResource|| resource is SEUMenifestBundleResource)
            {
                container = m_AssetBundles;
            }
            else
            {
                container = m_Resources;
            }

            if (container.ContainsKey(path))
            {
                if (m_UnloadType == SEUResourceUnLoadType.REFCOUNT_ZERO)
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

        internal SEUResource LoadAssetBundle(string path,bool isNeedConvertBundlePath = false)
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
                resource = new SEUABResource(bundlePath);
                PushResource(bundlePath, resource);
                resource.LoadAsset();
            }
            return resource;
        }

        internal Request LoadAssetBundleAsyn(string path,bool isNeedConvertBundlePath = false)
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
                resource = new SEUABResource(bundlePath);
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
                resource = new SEUMenifestBundleResource(manifestPath);
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
                resource = new SEUMenifestBundleResource(manifestPath);
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
                pool.InitPool(groupPath, loaderType, unLoadType, manifestBunderPathProvider,resToBundlerPathConverter);
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

    static SEUResourcePool m_ResourcePool;

    static SEUResource()
    {
        m_ResourcePool = new SEUResourcePool();
        m_ResourcePool.InitPool("defualt", SEULoaderType.RESOURCE);
    }

}
