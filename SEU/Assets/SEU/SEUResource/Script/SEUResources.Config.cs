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

    static void InitSEUResources()
    {

        PreLoadBundle("assets/staticresources/art/font/方正正准黑简体");

        ResisterGroupPath("view", SEULoaderType.AB,SEUUnLoadType.REFCOUNT_ZERO,"assetbundles",new SEUDefulatResourceToBundlePathConverter());
    }
}
