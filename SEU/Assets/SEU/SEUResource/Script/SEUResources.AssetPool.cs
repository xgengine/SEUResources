using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public partial class SEUResources { 

    private class SEUResourcesPool
    {
        class SEUGroupPooolRegister
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
            internal SEUGroupPooolRegister()
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
        private SEUGroupPooolRegister m_GroupPoolRegister;
        private SEULoaderType m_LoaderType;
        private SEUUnLoadType m_UnloadType;
        private IPathConverter m_ResourceToBundlePathConverter;
        private string m_ManifestPath;
        private IPathConverter m_BundleFilePathConverter;
        private SEUBundleLoaderType m_BundleLoaderType = SEUBundleLoaderType.Defualt_Memory_BundleLoader;

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
                Debug_SEUPoolObject = new GameObject("[SEUAssetPool]");
                GameObject.DontDestroyOnLoad(Debug_SEUPoolObject);

                Debug_ResourcesLoadObject = new GameObject("[ResourcesLoad]");
                Debug_ResourcesLoadObject.transform.SetParent(Debug_SEUPoolObject.transform);

                Debug_AssetsObject = new GameObject("[Assets]");
                Debug_AssetsObject.transform.SetParent(Debug_ResourcesLoadObject.transform);

                Debug_AssetBundleLoadObject = new GameObject("[AssetBundleLoad]");
                Debug_AssetBundleLoadObject.transform.SetParent(Debug_SEUPoolObject.transform);

                Debug_AssetBundlesObject = new GameObject("[AssetBundles]");
                Debug_AssetBundlesObject.transform.SetParent(Debug_AssetBundleLoadObject.transform);

                Debug_AssetsObjectLoadByBundles = new GameObject("[Assets]");
                Debug_AssetsObjectLoadByBundles.transform.SetParent(Debug_AssetBundleLoadObject.transform);

            }
    #endif
        public SEUResourcesPool()
        {
            m_GroupPoolRegister = new SEUGroupPooolRegister();
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
            SEUUnLoadType unLoadType = SEUUnLoadType.REFCOUNT_ZERO,
            IPathConverter resToBundlerPathConverter = null,
            SEUBundleLoaderType bundleLoaderType = SEUBundleLoaderType.Defualt_Memory_BundleLoader,
            string manifestBundlePath = null
            )
        {
            m_GroupPath = groupPath;
            m_LoaderType = loaderType;
            m_UnloadType = unLoadType;
            m_ResourceToBundlePathConverter = resToBundlerPathConverter;
            m_ManifestPath = manifestBundlePath;
            m_BundleLoaderType = bundleLoaderType;

            if (m_ResourceToBundlePathConverter == null)
            {
                m_ResourceToBundlePathConverter = new SEUBundlePathConverter();
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
            if (resource is SEUResourcesBundle || resource is SEUResourceMenifest)
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
                    if (resource is SEUResourcesBundle || resource is SEUResourceMenifest)
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
            if (resource is SEUResourcesBundle || resource is SEUResourceMenifest)
            {
                container = m_AssetBundles;
            }
            else
            {
                container = m_Resources;
            }

            if (container.ContainsKey(resGUI))
            {
                if (m_UnloadType == SEUUnLoadType.REFCOUNT_ZERO)
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
                return pool.LoadInternal(path, type);
            }
            return null;
        }

        internal AsyncRequest LoadAsyn(string path, System.Type type, System.Action<SEUResources> callback = null)
        {
            SEUResourcesPool pool = GetGroupPool(path);
            if (pool != null)
            {
                return pool.LoadAsynInternal(path, type, callback);
            }
            return null; ;
        }

        private SEUResources LoadInternal(string path, System.Type type)
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
                        resource = new SEUResourceNormal(path, type);
                        break;
                    case SEULoaderType.AB:
                        resource = new SEUResourcesFromBundle(path, type);
                        break;
                }
                PushResource(resource);

            }
            /// 这样处理 为了同步和异步并存
            if (resource.asset == null)
            {
                resource.LoadAsset();
            }
            resource.Use();

            return resource;
        }

        private AsyncRequest LoadAsynInternal(string path, System.Type type, System.Action<SEUResources> callback)
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
                        resource = new SEUResourceNormal(path, type);
                        break;
                    case SEULoaderType.AB:
                        resource = new SEUResourcesFromBundle(path, type);
                        break;
                }
                PushResource(resource);
            }
            resource.Use();

            return resource.SendLoadAsyncRequest(callback);
        }

        internal SEUResources LoadBundle(string path, bool isNeedConvertBundlePath = false)
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
                /// 这样处理 为了同步和异步并存
                if (resource.asset == null)
                {
                    resource.LoadAsset();
                }
            }
            else
            {
                resource = new SEUResourcesBundle(bundlePath, type);
                PushResource(resource);
                resource.LoadAsset();
            }
            return resource;
        }

        internal AsyncRequest LoadBundleAsyn(string path, bool isNeedConvertBundlePath = false)
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
                resource = new SEUResourcesBundle(bundlePath, type);
                PushResource(resource);
            }
            return resource.SendLoadAsyncRequest();
        }

        internal SEUResources LoadManifest(string path)
        {
            if (m_ManifestPath != null)
            {
                string manifestPath = m_ManifestPath;
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
                    resource = new SEUResourceMenifest(manifestPath, type);
                    PushResource(resource);
                    resource.LoadAsset();
                }
                return resource;

            }
            return null;
        }

        internal AsyncRequest LoadManifestAsync(string path)
        {
            if (m_ManifestPath != null)
            {
                string manifestPath = m_ManifestPath;
                System.Type type = typeof(UnityEngine.Object);
                string resGUID = ToResGUID(manifestPath, type);
                SEUResources resource = null;
                if (m_AssetBundles.ContainsKey(resGUID))
                {
                    resource = m_AssetBundles[resGUID];
                }
                else
                {
                    resource = new SEUResourceMenifest(manifestPath, type);
                    PushResource(resource);
                }
                return resource.SendLoadAsyncRequest();
            }
            return null;
        }

        internal SEUBundleLoader GetBundleLoader(string bundleName)
        {
            SEUBundleLoader bundleLoader = SEUResources.GetBundleLoader(m_BundleLoaderType);
            if(bundleLoader != null)
            {
                bundleLoader.SetBundleName(bundleName);
            }
            return bundleLoader;
        }

        internal void ResisterGroupPath(
            string groupPath,
            SEULoaderType loaderType,
            SEUUnLoadType unLoadType = SEUUnLoadType.REFCOUNT_ZERO,
            IPathConverter resToBundlerPathConverter = null,
            SEUBundleLoaderType bundleLoaderType = SEUBundleLoaderType.Defualt_Memory_BundleLoader,
            string manifestBundlePath = null
            )
        {
            SEUResourcesPool pool = m_GroupPoolRegister.ResisterGroupPath(groupPath);
            if (pool != null)
            {
                pool.InitPool(groupPath, loaderType, unLoadType, resToBundlerPathConverter, bundleLoaderType, manifestBundlePath);
                AddGroupPool(pool);
            }
           
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
}
