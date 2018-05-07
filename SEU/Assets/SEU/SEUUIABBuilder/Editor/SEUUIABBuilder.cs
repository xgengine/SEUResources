using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
public class UIABBuilder {
    static readonly string UIViewPath = "assets/resources/view";
    [MenuItem("SEU/Build UI Assetbundle")]
	static void Build()
    {       
        BuildUI();
        string assetbundlesPath =Path.Combine(  System.IO.Path.GetDirectoryName(Application.dataPath),"assetbundles");
        if (!Directory.Exists(assetbundlesPath))
        {
            Directory.CreateDirectory(assetbundlesPath);
        }      
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(assetbundlesPath, BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
    }

    static void BuildUI()
    {
        string[] resultGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { UIViewPath });
        for(int i = 0; i < resultGUIDs.Length; i++)
        {
            var deps = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(resultGUIDs[i]));
            foreach (var item in deps)
            {
                string extension = System.IO.Path.GetExtension(item);
                if (extension.Equals(".cs") == false)
                {
                    //string bundleName = item.Substring(0, item.Length - extension.Length).Replace("Assets/Resources/","").Replace("Assets/StaticResources/","").Replace("Assets/","");
                    string bundleName = item.Substring(0, item.Length - extension.Length);
                    AssetImporter importer = AssetImporter.GetAtPath(item);
                    importer.assetBundleName = bundleName.ToLower();
                }
            }
        }       
    }
}
