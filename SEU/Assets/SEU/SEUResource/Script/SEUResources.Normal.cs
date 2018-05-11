using UnityEngine;
using System.Collections;

public partial class SEUResources{  

	/// <summary>
    /// 采用资源从中 Resources 加载方式的资源类
    /// </summary>
    private class SEUResourceNormal : SEUResources
    {
        public SEUResourceNormal(string path, System.Type type) : base(path, type)
        {
        }
        protected override void LoadAsset()
        {
            m_Asset = Resources.Load(m_LoadPath, m_Type);
        }
        protected override IEnumerator LoadAssetAsync()
        {
            ResourceRequest request = Resources.LoadAsync(m_LoadPath, m_Type);
            yield return request;
            m_Asset = request.asset;
        }
        protected override void ReleaseResource()
        {
            base.ReleaseResource();
        }
    }
}
