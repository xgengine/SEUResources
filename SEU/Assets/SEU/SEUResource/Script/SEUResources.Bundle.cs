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
            if (m_Asset == null)
            {
                m_Asset = m_Pool.LoadAssetBundleInternal(m_LoadPath);
            }
            else
            {
                Debug.LogWarning("[异步冲突]");
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
}
