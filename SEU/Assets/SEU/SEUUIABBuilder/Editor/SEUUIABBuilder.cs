using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
public class UIABBuilder {
    [MenuItem("SEU/Build UI Assetbundle")]
	static void Build()
    {
        BuildUI();
        string assetbundlesPath =Path.Combine(  System.IO.Path.GetDirectoryName(Application.dataPath),"assetbundles");
        if (!Directory.Exists(assetbundlesPath))
        {
            Directory.CreateDirectory(assetbundlesPath);
        }
        Debug.Log(assetbundlesPath);
        BuildPipeline.BuildAssetBundles(assetbundlesPath, BuildAssetBundleOptions.UncompressedAssetBundle,BuildTarget.StandaloneWindows64);
    }

    static void BuildUI()
    {
        var deps = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(Selection.activeGameObject));
        foreach(var item in deps)
        {
            string extension = System.IO.Path.GetExtension(item);
            if (extension.Equals(".cs")==false)
            {
                //string bundleName = item.Substring(0, item.Length - extension.Length).Replace("Assets/Resources/","").Replace("Assets/StaticResources/","").Replace("Assets/","");
                string bundleName = item.Substring(0, item.Length - extension.Length);
                AssetImporter importer = AssetImporter.GetAtPath(item);
                importer.assetBundleName = bundleName.ToLower();
            }
        }
    }
}
