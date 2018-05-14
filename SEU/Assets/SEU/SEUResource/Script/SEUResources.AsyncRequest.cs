using UnityEngine;
using System.Collections;

public partial class SEUResources{

    public class AsyncRequest : CustomYieldInstruction
    {
        public Object asset
        {
            get
            {
                if (m_Resource != null)
                {
                    return m_Resource.asset;
                }
                return null;
            }
        }
        private SEUResources m_Resource;
        internal SEUResources resource
        {
            get
            {
                return m_Resource;
            }
        }
        
        internal AsyncRequest(SEUResources resource, System.Action<SEUResources> callback = null)
        {
            m_Resource = resource;
            if (resource.asset == null)
            {
                AsyncOperator.SendReqest(MainLoop(callback));
            }
            else
            {
                if (callback != null)
                {
                    callback(resource);
                }
                m_KepWaiting = false;
            }
        }
        private IEnumerator MainLoop(System.Action<SEUResources> callback = null)
        {
            yield return resource.LoadAssetAsync();
            if (callback != null)
            {
                callback(resource);
            }
            m_KepWaiting = false;
        }
        private bool m_KepWaiting = true;
        public override bool keepWaiting
        {
            get
            {
                return m_KepWaiting;
            }
        }
    }
}
