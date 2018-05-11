#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// SEUResources 对Unity资源加载进行封装、保持和 Resources 资源加载接口形式，并对资源进行引用计数的管理
/// </summary>
public partial class SEUResources{

    static public void ResisterGroupPath(
        string groupPath,
        SEULoaderType loaderType,
        SEUResourcesUnLoadType unLoadType = SEUResourcesUnLoadType.REFCOUNT_ZERO,
        IPathProvider manifestBunderPathProvider = null,
        IPathConverter resToBundlerPathConverter = null
        )
    {
        if (groupPath.EndsWith("/"))
        {
            groupPath = groupPath.Substring(0, groupPath.Length - 1);
        }
        m_ResourcePool.ResisterGroupPath(groupPath, loaderType, unLoadType, manifestBunderPathProvider,resToBundlerPathConverter);
    }

    static public T Instantiate<T>(Object asset) where T : Object
    {
        T obj = null;
        if(asset != null)
        {
            obj = Object.Instantiate<T>(asset as T);
            m_ObjectPool.AttachAssetToInstance(asset, obj);
        }
        else
        {
            Debug.LogError("[SEUResources] Instantiate Object But The Object is NULL");
        }
        return obj;
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
        DestoryObject(asset);
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
        return m_ObjectPool.PushResource(resource);
    }

    static public Request LoadAsync<T>(string path) where T : Object
    {
        return LoadAsync(path, typeof(T));
    }

    static private Request LoadAsync(string path, System.Type type)
    {
        Request request = _LoadAsync(path,type,
            (resource) => {
                m_ObjectPool.PushResource(resource);
            }
        );
        return request;

    }

    static private Request LoadAsync(string path)
    {
        return LoadAsync(path, typeof(UnityEngine.Object));
    }

    static public void DestoryObject(Object asset)
    {
        if (asset != null)
        {
            m_ObjectPool.DestoryObject(asset);       
        }
        
    }

    public class Request : CustomYieldInstruction
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
        internal Request(SEUResources resource,System.Action<SEUResources> callback =null)
        {
            m_Resource = resource;
            if (resource.asset == null)
            {
                SEUResourcesAsyncOperator.SendReqest(MainLoop(callback));
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
        private IEnumerator MainLoop(System.Action<SEUResources> callback =null)
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

public enum SEULoaderType
{
    RESOURCE,
    AB,
}

public enum SEUResourcesUnLoadType
{
    REFCOUNT_ZERO,  //计数为零释放内存
    PERMANENT       //常驻内存
}

public interface IPathConverter
{
    string HandlePath(string path);

}

public interface IPathProvider
{
    string GetPath();
}

/// <summary>
/// 根据加载路径，生成AB包路径
/// </summary>
/// <param name="path"></param>
/// <returns></returns>
public class SEUDefulatResourceToBundlePathConverter:IPathConverter
{
    public string HandlePath(string path)
    {
        return "assets/resources/" + path;
    }
}

/// <summary>
/// 获取资源组AB包的Manifest，进而获取AB包的依赖
/// </summary>
/// <returns></returns>
public class SEUGroupManifestBundlePathProvider : IPathProvider
{

    public string GetPath()
    {
        return "assetbundles";
    }
}


public abstract class SEUBundleLoader
{
    public abstract AssetBundle LoadAssetBundle(string bundleName);
    public abstract AssetBundleCreateRequest LoadAssetBundlAsyn(string bundleName);
}

public class SEUBundleLoaderFromFile:SEUBundleLoader
{
    public override AssetBundle LoadAssetBundle(string bundleName)
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + bundleName;
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        return bundle;
    }
    public override AssetBundleCreateRequest LoadAssetBundlAsyn(string bundleName)
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + bundleName;
        return AssetBundle.LoadFromFileAsync(bundlePath);
    }
}

public class SEUBundleLoaderFromMemory:SEUBundleLoader
{ 
    public override AssetBundle LoadAssetBundle(string bundleName)
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + bundleName;
        byte[] buffer = SEUFileLoader.ReadAllBytes(bundlePath);
        AssetBundle bundle = AssetBundle.LoadFromMemory(buffer);
        return bundle;
    }
    public override AssetBundleCreateRequest LoadAssetBundlAsyn(string bundleName)
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + bundleName;
        byte[] buffer = SEUFileLoader.ReadAllBytes(bundlePath);
        return AssetBundle.LoadFromMemoryAsync(buffer);
    }
}

/// <summary>
/// 文件加载器
/// </summary>
public class SEUFileLoader
{
    static public byte[] ReadAllBytes(string path)
    {
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        else
        {
            return null;
        }
        
    }
}
