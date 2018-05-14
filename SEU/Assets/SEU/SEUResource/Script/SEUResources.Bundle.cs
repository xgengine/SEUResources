using UnityEngine;
using System.Collections;

public partial class SEUResources{

    /// <summary>
    /// AssetBundle 资源类
    /// </summary>
    private class SEUResourcesBundle : SEUResources         
    {
        public SEUResourcesBundle(string path, System.Type type) : base(path, type)
        {
        }

        protected override void LoadAsset()
        {
            SEUBundleLoader bundleLoader = m_Pool.GetBundleLoader(m_LoadPath);
            if (bundleLoader != null)
            {
                bundleLoader.Load();
                if (m_Asset == null)
                {
                    m_Asset = bundleLoader.assetBundle;
                }
                else
                {
                    Debug.LogWarning("[异步冲突]");
                }
            }
            SEUResources manifestRes = m_Pool.LoadManifest(m_LoadPath);
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
                            SEUResources depRes = m_Pool.LoadBundle(dependenciesPaths[i]);
                            AddDependenceResources(depRes);
                        }
                    }
                }
            }
            //LogResult();
        }

        protected override IEnumerator LoadAssetAsync()
        {
            SEUBundleLoader bundleLoader = m_Pool.GetBundleLoader(m_LoadPath);
            if (bundleLoader != null)
            {
                yield return bundleLoader.LoadAsync();
                if (m_Asset == null)
                {
                    m_Asset = bundleLoader.assetBundle;
                }
                else
                {
                    Debug.LogWarning("[同步冲突] 已经处理");
                }
            }
            SEUResources manifestRes = m_Pool.LoadManifest(m_LoadPath);
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
                            AsyncRequest request = m_Pool.LoadBundleAsyn(dependenciesPaths[i]);
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
}
