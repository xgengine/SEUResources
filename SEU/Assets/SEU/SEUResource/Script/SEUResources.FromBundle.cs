using UnityEngine;
using System.Collections;

public partial class SEUResources{

	  /// <summary>
    /// 采用资源从 AssetBundle中加载方式的资源类
    /// </summary>
    private class SEUResourcesFromBundle : SEUResources
    {
        public SEUResourcesFromBundle(string path, System.Type type) : base(path, type)
        {
        }
        protected override void LoadAsset()
        {
            SEUResources bundleRes = m_Pool.LoadAssetBundle(m_LoadPath, true);
            AssetBundle bundle = bundleRes.asset as AssetBundle;
            if (bundle != null)
            {
                AddDependenceResources(bundleRes);
                Object asset = bundle.LoadAsset(System.IO.Path.GetFileName(m_LoadPath), m_Type);
                if (m_Asset == null)
                {
                    m_Asset = asset;
                }
            }
            LogResult();
        }
        protected override IEnumerator LoadAssetAsync()
        {
            Request request = m_Pool.LoadAssetBundleAsyn(m_LoadPath, true);
            yield return request;
            SEUResources bundleRes = request.resource;
            AssetBundle bundle = bundleRes.asset as AssetBundle;
            if (bundle != null)
            {
                AddDependenceResources(bundleRes);
                AssetBundleRequest bdRequest = bundle.LoadAssetAsync(System.IO.Path.GetFileName(m_LoadPath), m_Type);
                yield return bdRequest;
                if (m_Asset == null)
                {
                    m_Asset = bdRequest.asset;
                }
            }
            LogResult();
        }
    }
}
