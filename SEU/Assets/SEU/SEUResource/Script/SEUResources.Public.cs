//#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// SEUResources 对Unity资源加载进行封装、
/// 1 保持和 Resources 资源加载等同的接口形式
/// 2 支持同步、异步加载方式，
/// 3 对资源进行引用计数的管理
/// 4 支持Resource 和 AssetBundle 两种加载方式
/// </summary>
public partial class SEUResources{
    static public void ResisterGroupPath( 
        string groupPath,
        SEULoaderType loaderType,
        SEUUnLoadType unLoadType = SEUUnLoadType.REFCOUNT_ZERO,
        string manifestBundlePath=null,
        IPathConverter resToBundlerPathConverter = null,
        SEUBundleLoaderType bundleLoaderType = SEUBundleLoaderType.Defualt_Memory_BundleLoader    
        )
    {
        if (groupPath.EndsWith("/"))
        {
            groupPath = groupPath.Substring(0, groupPath.Length - 1);
        }
        m_ResourcePool.ResisterGroupPath(groupPath, loaderType, unLoadType, resToBundlerPathConverter,bundleLoaderType,manifestBundlePath);
    }

    static public T Instantiate<T>(Object asset) where T : Object
    {
        T instance = null;
        if(asset != null)
        {
            instance = Object.Instantiate<T>(asset as T);
            m_ObjectPool.AttachAssetToInstance(asset, instance);
        }
        else
        {
            Debug.LogError("[SEUResources] Instantiate Object But The Object is NULL");
        }
        return instance;
    }
    static public Object Instantiate(Object asset)
    {
        Object obj = null;
        if (asset != null)
        {
            obj = Object.Instantiate(asset);
            m_ObjectPool.AttachAssetToInstance(asset, obj);
        }
        else
        {
            Debug.LogError("[SEUResources] Instantiate Object But The Object is NULL");
        }
        return obj;
    }
    static public Object Instantiate(Object asset,Vector3 postion ,Quaternion quaternion)
    {
        Object obj = null;
        if (asset != null)
        {
            obj = Object.Instantiate(asset,postion,quaternion);
            m_ObjectPool.AttachAssetToInstance(asset, obj);
        }
        else
        {
            Debug.LogError("[SEUResources] Instantiate Object But The Object is NULL");
        }
        return obj;
    }
    static public T LoadAndInstantiate<T>(string path) where T:Object
    {
        Object asset = Load<T>(path);
        T insObj = Instantiate<T>(asset);
        Destory(asset);
        return insObj;
    }
    static public T Load<T>(string path) where T : Object
    {
        return Load(path, typeof(T)) as T;
    }
    static public Object Load(string path)
    {
        return Load(path, typeof(UnityEngine.Object));
    }
    static public Object Load(string path, System.Type type)
    {
        SEUResources resource = _Load(path, type);
        return m_ObjectPool.GetObject(resource);
    }
    static public AsyncRequest LoadAsync<T>(string path) where T : Object
    {
        return LoadAsync(path, typeof(T));
    }

    static public bool PreLoadBundle(string path)
    {
        m_ResourcePool.LoadBundle(path);
        return true;
    }

    static public void Destory(Object asset)
    {
        if (asset != null)
        {
            m_ObjectPool.DesotoryObject(asset);       
        }      
    }   
}



