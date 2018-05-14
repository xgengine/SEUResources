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
            SEUResources bundleRes = m_Pool.LoadBundle(m_LoadPath, true);
            if (bundleRes != null)
            {
                AddDependenceResources(bundleRes);
                AssetBundle bundle = bundleRes.asset as AssetBundle;
                if (bundle != null)
                {

                    string assetName = System.IO.Path.GetFileName(m_LoadPath);
                    Object asset = bundle.LoadAsset(assetName, m_Type);
                    if (m_Asset == null)
                    {
                        m_Asset = asset;
                    }
                }
            }       
            LogResult();
        }
        protected override IEnumerator LoadAssetAsync()
        {
            AsyncRequest request = m_Pool.LoadBundleAsyn(m_LoadPath, true);
            yield return request;
            SEUResources bundleRes = request.resource;
            if (bundleRes != null)
            {
                AddDependenceResources(bundleRes);
                AssetBundle bundle = bundleRes.asset as AssetBundle;
                if (bundle != null)
                {
                    AssetBundleRequest bdRequest = bundle.LoadAssetAsync(System.IO.Path.GetFileName(m_LoadPath), m_Type);
                    yield return bdRequest;
                    if (m_Asset == null)
                    {
                        m_Asset = bdRequest.asset;
                    }
                }
            }           
            LogResult();
        }
    }
}
