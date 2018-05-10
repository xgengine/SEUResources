using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
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
        BuildPipeline.BuildAssetBundles(assetbundlesPath, BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
    }

    static void BuildUI()
    {
        string[] resultGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { UIViewPath });
        for(int i = 0; i <resultGUIDs.Length; i++)
        {
            string resultPath = AssetDatabase.GUIDToAssetPath(resultGUIDs[i]);
         
            if(RingRefCheck(resultPath,new string[] { resultPath }))
            {
                Debug.LogError("有环状引用 " +resultPath);
            }
            else
            {
                var deps = AssetDatabase.GetDependencies(resultPath);
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
    static bool RingRefCheck(string path,string[] paths)
    {
        
        var deps = AssetDatabase.GetDependencies(path);
        if (deps.Length == 1)
        {
            return false;
        }

        foreach(var item in deps)
        {
            if(item == path)
            {
                continue;
            }
            string extension = System.IO.Path.GetExtension(item);
            if (extension.Equals(".cs") == false)
            {
                var cDeps = AssetDatabase.GetDependencies(item);
                if(cDeps.Length == 1)
                {
                    continue;
                }
                List<string> cDepsList = new List<string>(cDeps);

                List<string> prePaths = new List<string>(paths);
                if (IsIntersection(cDepsList, prePaths))
                {
                   
                    return true;
                }
                else
                {

                    prePaths.Add(item);
                    if(RingRefCheck(item, prePaths.ToArray()))
                    {
                        return true;
                    }
                }
            }      
        }

        return false;
    }

    static bool IsIntersection(List<string> A,List<string> B)
    {
        foreach(var itemA in A)
        {
            if (B.Contains(itemA))
            {
                return true;
            }
        }
        return false;
    }
}
