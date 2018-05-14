using UnityEngine;
using System.Collections;


public partial class SEUResources
{
    public enum SEULoaderType
    {
        RESOURCE,
        AB,
    }

    public enum SEUUnLoadType
    {
        REFCOUNT_ZERO,  //计数为零释放内存
        PERMANENT       //常驻内存
    }

    public enum SEUBundleLoaderType
    {
        Defualt_Memory_BundleLoader,
        Defualt_FromFile_BundleLoader,
        DGM_BundleLoader,
    }

    public static SEUBundleLoader GetBundleLoader(SEUBundleLoaderType bundleLoaderType )
    {
        SEUBundleLoader bundleLoader = null;
        switch(bundleLoaderType)
        {
            case SEUBundleLoaderType.Defualt_Memory_BundleLoader:
                bundleLoader = new SEUBundleLoaderFromMemory();
                break;
            case SEUBundleLoaderType.Defualt_FromFile_BundleLoader:
                bundleLoader = new SEUBundleLoaderFromFile();
                break;
            case SEUBundleLoaderType.DGM_BundleLoader:
                bundleLoader = new DGMBundleLoader();
                break;
            default:
                Debug.LogError("Can not find BundleLoader");
                break;
        }
        return bundleLoader;    
    }

   

    static void InitSEUResources()
    {

        ResisterGroupPath("art/uiprefabs", SEULoaderType.AB,SEUUnLoadType.REFCOUNT_ZERO,"assetbundles",new SEUBundlePathConverter(),SEUBundleLoaderType.Defualt_Memory_BundleLoader);
        ResisterGroupPath("art/role/link", SEULoaderType.AB, SEUUnLoadType.REFCOUNT_ZERO, null, new DGMLinkBundlePathConverter(), SEUBundleLoaderType.DGM_BundleLoader);
        ResisterGroupPath("view", SEULoaderType.AB, SEUUnLoadType.REFCOUNT_ZERO);
    }
}
