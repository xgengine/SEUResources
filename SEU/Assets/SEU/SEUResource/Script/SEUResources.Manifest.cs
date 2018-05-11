using UnityEngine;
using System.Collections;

public partial class SEUResources{

    /// <summary>
    /// Manifest AssetBundle 资源对象 从该对象中可以获取AB包的依赖
    /// </summary>
    private class SEUResourceMenifest : SEUResources
    {
        AssetBundle bundle;
        public SEUResourceMenifest(string path, System.Type type) : base(path, type)
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
            AssetBundleCreateRequest createReuest = m_Pool.LoadAssetBundlAsynInternal(m_LoadPath);
            yield return createReuest;
            if (bundle == null)
            {
                bundle = createReuest.assetBundle;
            }
            if (bundle != null)
            {
                AssetBundleRequest request = bundle.LoadAssetAsync("assetbundlemanifest");
                yield return request;
                if (m_Asset == null)
                {
                    m_Asset = request.asset;
                }
            }
            //LogResult();
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
}
