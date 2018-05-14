using UnityEngine;
using System.Collections;
using System;
using System.IO;
public class DGMBundleLoader: SEUBundleLoader
{
    public override void Load()
    {
        //尝试从SDCard加载
        //if (!ResourceManagerUtil.ConstsStreamingLatest)
        //{
        //    string bundleFilePath =ToBundlePathSDCardPath(m_BundleName);
          
        //    if (File.Exists(bundleFilePath))
        //    {
        //        byte[] data = File.ReadAllBytes(bundleFilePath);
        //        Encrypt.Hanlde(ref data);
        //        m_Bundle = AssetBundle.LoadFromMemory(data);
        //        if(m_Bundle != null)
        //        {
        //            return;
        //        }
        //    }
        //}       
    }
    public override IEnumerator LoadAsync()
    {
        //if (!ResourceManagerUtil.ConstsStreamingLatest)
        //{
        //    string bundleFilePath = ToBundlePathSDCardPath(m_BundleName);

        //    if (File.Exists(bundleFilePath))
        //    {
        //        byte[] data = File.ReadAllBytes(bundleFilePath);
        //        Encrypt.Hanlde(ref data);
        //        AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(data);
        //        yield return request;
        //        m_Bundle = request.assetBundle;
        //        if (m_Bundle != null)
        //        {
        //            yield break;
        //        }
        //    }
        //}
        yield break;
    } 
    
    public static string ToStreamingPath(string bundleName)
    {
        return "assetbundles/" + bundleName;
    } 


//    public static string ToBundlePathSDCardPath(string bundleName)
//    {
//        string abFilePath = AssetBundlePathUtility.getSDCardABPath(bundleName);
//#if UNITY_EDITOR
//        if (bundleName == "global_dependeencies/shaders")
//        {
//            abFilePath = abFilePath.Replace("/pending/", "/editorCdn/");
//            abFilePath = abFilePath.Replace("/cdn/", "/editorCdn/");
//        }
//#endif
//        return abFilePath;
//    }
}


public class DGMLinkBundlePathConverter: IPathConverter
{
    public string HandlePath(string path)
    {
        string folderPath = Path.GetDirectoryName(path);
        return folderPath;
    }
}
