using UnityEngine;
using System.Collections;
using UnityEditor;
public class UIABBuilder {
    [MenuItem("SEU/Build UI Assetbundle")]
	static void Build()
    {
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/Bundles/test_group",BuildAssetBundleOptions.UncompressedAssetBundle,BuildTarget.StandaloneWindows64);
    }
}
