#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.IO;
public partial class SEUResource{
    public Object asset
    {
        get
        {
            return m_Asset;
        }
    }
    static public void ResisterGroupPath(
        string groupPath,
        SEULoaderType loaderType,
        SEUResourceUnLoadType unLoadType = SEUResourceUnLoadType.REFCOUNT_ZERO,
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

    static public SEUResource Load(string path)
    {
        path = path.ToLower();
        SEUResource result = m_ResourcePool.Load(path);

        return result;
    }

    static public Request LoadAsyn(string path)
    {
        return m_ResourcePool.LoadAsyn(path);
    }

    static public void UnLoadResource(SEUResource resource)
    {
#if SEU_DEBUG
        resource.Debug_StackInfo.Add("[UnLoad]" + StackTraceUtility.ExtractStackTrace());

#endif
        resource.UnUsed();
    }

    public class Request : CustomYieldInstruction
    {
        private SEUResource m_Resource;
        public SEUResource resource
        {
            get
            {
                return m_Resource;
            }
        }
        internal Request(SEUResource resource)
        {
            m_Resource = resource;
            SEUResourceRequestRunner.SendReqest(MainLoop());
        }
        IEnumerator MainLoop()
        {
            yield return resource.LoadAssetAsync();
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

public enum SEUResourceUnLoadType
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
        return path;
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
    public override   AssetBundle LoadAssetBundle(string bundleName)
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
