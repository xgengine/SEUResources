#define SEU_DEBUG
using UnityEngine;
using System.Collections;
public partial class SEUResource{
    public Object asset
    {
        get
        {
            return m_Asset;
        }
    }
    static public void ResisterGroupPath(string path, SEULoaderType loaderType, SEUResourceUnLoadType unLoadType, SEUABPathBuilder ABPathBuilder = null)
    {
        m_ResourcePool.ResisterGroupPath(path, loaderType, unLoadType, ABPathBuilder);
    }

    static public SEUResource Load(string path)
    {
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

public class SEUABPathBuilder
{
    /// <summary>
    /// 根据加载路径，生成AB包路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public virtual string BundlePathHandle(string path)
    {
        return path;
    }
    /// <summary>
    /// 获取资源组AB包的Manifest，进而获取AB包的依赖
    /// </summary>
    /// <returns></returns>
    public virtual string ManifestBundlePathHandle(string path)
    {
        return "test_group";
    }
}

public class SEUBundleLoader
{
    public virtual AssetBundle LoadAssetBundle(string path)
    {
        return null;
    }
    public virtual AssetBundleCreateRequest LoadAssetBundlAsyn(string path)
    {
        return null;
    }

}

/// <summary>
/// 文件加载器
/// </summary>
public class SEUFileLoader
{
    static public byte[] ReadAllBytes(string path)
    {
        path = Application.dataPath + "/Bundles/test_group/" + path;
        return System.IO.File.ReadAllBytes(path);
    }
}
