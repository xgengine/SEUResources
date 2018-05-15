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
            SEUBundleLoader bundleLoader = m_Pool.GetBundleLoader(m_LoadPath);
            if(bundleLoader != null)
            {
                bundleLoader.Load();
                bundle = bundleLoader.assetBundle;
            }
            
            if (bundle != null)
            {
                m_Asset = bundle.LoadAsset("assetbundlemanifest");
            }
            //LogResult();
        }
        protected override IEnumerator LoadAssetAsync()
        {
            SEUBundleLoader bundleLoader = m_Pool.GetBundleLoader(m_LoadPath);
            if (bundleLoader != null)
            {
                yield return bundleLoader.LoadAsync();
                if (bundle == null)
                {
                    bundle = bundleLoader.assetBundle;
                }
                else
                {
                    Debug.LogWarning("[同步冲突] 已经处理");
                }
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
