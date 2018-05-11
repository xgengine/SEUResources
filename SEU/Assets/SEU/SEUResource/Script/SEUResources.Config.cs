using UnityEngine;
using System.Collections;

public partial class SEUResources
{

    static void InitSEUResources()
    {
        ResisterGroupPath("view", SEULoaderType.AB,SEUUnLoadType.REFCOUNT_ZERO,"assetbundles");
    }
}
