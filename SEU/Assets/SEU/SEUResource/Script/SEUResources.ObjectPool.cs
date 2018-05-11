using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public partial class SEUResources
{
    private class SEUObjectPool
    {
        private Dictionary<int, SEUResources> m_AssetRefSEUResources = new Dictionary<int, SEUResources>();
        private Dictionary<int, SEUResources> m_InstantiateRefSEUResources = new Dictionary<int, SEUResources>();

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

            m_AssetRefSEUResources.TryGetValue(assetCode, out refResource);
            if (refResource == null)
            {
                m_InstantiateRefSEUResources.TryGetValue(assetCode, out refResource);
            }
            if (refResource != null)
            {
                if (!m_InstantiateRefSEUResources.ContainsKey(instanceCode))
                {
                    m_InstantiateRefSEUResources.Add(instanceCode, refResource);
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

        internal void DestoryObject(Object asset)
        {
            if ((TryDestoryObject(asset, true) || TryDestoryObject(asset, false)) == false)
            {
                Debug.LogError("[SEUResources] Try Destory Object ,But it not in Ref System " + StackTraceUtility.ExtractStackTrace());
            }
        }

        internal bool TryDestoryObject(Object asset, bool isAsset)
        {
            return RemoveResource(asset, isAsset);
        }

        internal bool RemoveResource(Object asset, bool isAsset)
        {
            int assetCode = asset.GetInstanceID();
            Dictionary<int, SEUResources> record = null;
            if (isAsset)
            {
                record = m_AssetRefSEUResources;
            }
            else
            {
                record = m_InstantiateRefSEUResources;
            }
            SEUResources refResource = null;
            record.TryGetValue(assetCode, out refResource);
            if (refResource != null)
            {
                if (isAsset == false)
                {
                    Object.Destroy(asset);
                }
                if (refResource.m_RefCount == 0)
                {
                    record.Remove(assetCode);
                }
                UnLoadResource(refResource);
                return true;
            }
            return false;
        }

        internal Object PushResource(SEUResources resource)
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
