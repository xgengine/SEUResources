using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public partial class SEUResources
{
    private class SEUObjectPool
    {
        public Dictionary<int, SEUResources> m_AssetRefSEUResources = new Dictionary<int, SEUResources>();
        public Dictionary<int, SEUResources> m_InstanceRefSEUResources = new Dictionary<int, SEUResources>();

#if SEU_DEBUG
        private Dictionary<int, GameObject> Debeg_InstantiateObjects = new Dictionary<int, GameObject>();
        GameObject Debug_SEUObjectPoolObject;

        internal SEUObjectPool()
        {
            //Debug_SEUObjectPoolObject = new GameObject("_[SEUObjectPool]_");
            //GameObject.DontDestroyOnLoad(Debug_SEUObjectPoolObject);
        }
        private void DeBug_AddInstanceObject(Object instance,SEUResources res)
        {
            GameObject debugObj = new GameObject(res.m_LoadPath);
            Debeg_InstantiateObjects.Add(instance.GetInstanceID(), debugObj);
            debugObj.transform.SetParent(Debug_SEUObjectPoolObject.transform);
        }
        private void DeBug_RemoveInstanceObject(Object instance)
        {
            GameObject debugObj =null ;
            Debeg_InstantiateObjects.TryGetValue(instance.GetInstanceID(), out debugObj);
            Object.Destroy(debugObj);

        }
#endif
        internal void AttachAssetToInstance(Object asset, Object instObj)
        {
            int assetCode = asset.GetInstanceID();
            int instanceCode = instObj.GetInstanceID();

            SEUResources refResource = null;

            if (m_AssetRefSEUResources.ContainsKey(assetCode))
            {
                refResource = m_AssetRefSEUResources[assetCode];
            }
            if (refResource == null)
            {
                m_InstanceRefSEUResources.TryGetValue(assetCode, out refResource);
            }
            if (refResource != null)
            {
                if (!m_InstanceRefSEUResources.ContainsKey(instanceCode))
                {
                    m_InstanceRefSEUResources.Add(instanceCode, refResource);
                    refResource.Use();
                }
                else
                {
                    Debug.LogError("SEUResources Instantiate Objec ,But this is a ref System Error");
                }
            }
            else
            {
                Debug.LogError("SEUResources Instantiate Object ,But the Object  is not in ref system " + StackTraceUtility.ExtractStackTrace());
            }
        }

        internal void  DesotoryObject(Object asset)
        {
            int assetCode = asset.GetInstanceID();
            SEUResources refRes = null;
            if (m_AssetRefSEUResources.ContainsKey(assetCode))
            {
                refRes = m_AssetRefSEUResources[assetCode];          
            }
            else if (m_InstanceRefSEUResources.ContainsKey(assetCode))
            {
                Object.Destroy(asset);
                refRes = m_InstanceRefSEUResources[assetCode];
                m_InstanceRefSEUResources.Remove(assetCode);               
            }
            if(refRes != null)
            {
                UnLoadResource(refRes);
                if (refRes.refCount == 0)
                {
                    m_AssetRefSEUResources.Remove(refRes.asset.GetInstanceID());
                }           
            }
            else
            {
                Debug.LogError("[SEUResources] Try Destory Object ,But it not in Ref System " + StackTraceUtility.ExtractStackTrace());
            }         
        }

        internal Object GetObject(SEUResources resource)
        {
            Object asset = null;
            if (resource != null)
            {
                Dictionary<int, SEUResources> record = null;

                record = m_AssetRefSEUResources;

                if (resource.asset != null)
                {
                    int code = resource.asset.GetInstanceID();
                    if (!record.ContainsKey(code))
                    {
                        record.Add(code, resource);
                    }
                    asset = resource.asset;
                }
                else
                {
                    //没有加载到资源，内部就把SEUResource 对象释放
                    UnLoadResource(resource);                   
                }
            }
            return asset;
        }

    }

}
