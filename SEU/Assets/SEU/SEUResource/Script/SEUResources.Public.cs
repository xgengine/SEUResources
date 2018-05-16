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
    static public T Create<T>(string path) where T:Object
    {
        Object asset = Load<T>(path);
        T insObj = Instantiate<T>(asset);
        Destory(asset);
        return insObj;
    }
    static public T[] Create<T>(string path,int count) where T:Object
    {
        T[] result = null;
        Object asset = Load<T>(path);
        if(asset != null)
        {
            result = new T[count];
            for (int i = 0; i < count; i++)
            {
                T insObj = Instantiate<T>(asset);
                result[i] = insObj;
            }
            Destory(asset);
        }
        return result;
    }
    static public Object Create(string path, Vector3 postion, Quaternion quaternion)
    {
        Object insObj = null;
        Object asset = Load(path);
        if (asset != null)
        {
            insObj = Instantiate(asset,postion,quaternion);
            Destory(asset);
        }
        return insObj;
    }
    static public Object Create(string path, Vector3 postion, Quaternion rotation,Transform parent)
    {
        Object insObj = null;
        Object asset = Load(path);
        if (asset != null)
        {
            insObj = Instantiate(asset, postion, rotation,parent);
            Destory(asset);
        }
        return insObj;
    }
    static public Object Create(string path, Transform parent)
    {
        Object insObj = null;
        Object asset = Load(path);
        if (asset != null)
        {
            insObj = Instantiate(asset, parent);
            Destory(asset);
        }
        return insObj;
    }

    static public AsyncRequest CreateAsync<T>(string path) where T:Object
    {
        AsyncRequest request = LoadAsync<T>(path);
        return request;
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
    static private AsyncRequest LoadAsync(string path, System.Type type, System.Action<Object> callback = null)
    {
        AsyncRequest request = _LoadAsync(path, type,
            (resource) => {
                Object asset = m_ObjectPool.GetObject(resource);
                if(callback != null)
                {
                    callback(asset);
                }
            }
        );
        return request;

    }
    static public bool PreLoadBundle(string path)
    {
        SEUResources res = m_ResourcePool.LoadBundle(path);
        if(res.asset != null)
        {
            return true;
        }
        return false;
    }
   
    static public void Destory(Object asset)
    {
        if (asset != null)
        {
            m_ObjectPool.DesotoryObject(asset);       
        }      
    }   
}



