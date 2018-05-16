using UnityEngine;
using System.Collections;
using System.IO;
public class DGMBundleLoader : SEUBundleLoader
{
    public override void Load()
    {
        //先尝试从SDCard加载
        //string bundleFilePath = "";
        //if (ResourceManagerUtil.ConstsStreamingLatest == false)
        //{
        //    bundleFilePath = ToBundlePathSDCardPath(m_BundleName);
        //    if (DGMFile.Exist(bundleFilePath))
        //    {
        //        using (DGMFile file = DGMFile.Open(bundleFilePath))
        //        {
        //            byte[] data = file.Read();
        //            Encrypt.Hanlde(ref data);
        //            m_Bundle = AssetBundle.LoadFromMemory(data);
        //        }
        //        if (m_Bundle != null)
        //        {
        //            return;
        //        }
        //    }
        //}
        //bundleFilePath = ToStreamingPath(m_BundleName);
        //if (DGMFile.ExistStreamingFile(bundleFilePath))
        //{
        //    using (DGMFile file = DGMFile.OpenStreamingFile(bundleFilePath))
        //    {
        //        byte[] data = file.Read();
        //        Encrypt.Hanlde(ref data);
        //        m_Bundle = AssetBundle.LoadFromMemory(data);
        //    }
        //}
    }
    public override IEnumerator LoadAsync()
    {
        //string bundleFilePath = "";
        //if (ResourceManagerUtil.ConstsStreamingLatest == false)
        //{
        //    bundleFilePath = ToBundlePathSDCardPath(m_BundleName);
        //    if (DGMFile.Exist(bundleFilePath))
        //    {
        //        using (DGMFile file = DGMFile.Open(bundleFilePath))
        //        {
        //            byte[] data = file.Read();
        //            Encrypt.Hanlde(ref data);
        //            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(data);
        //            yield return request;
        //            m_Bundle = request.assetBundle;
        //        }
        //        if (m_Bundle != null)
        //        {
        //            yield break;
        //        }
        //    }
        //}
        //bundleFilePath = ToStreamingPath(m_BundleName);
        //if (DGMFile.ExistStreamingFile(bundleFilePath))
        //{
        //    using (DGMFile file = DGMFile.OpenStreamingFile(bundleFilePath))
        //    {
        //        byte[] data = file.Read();
        //        Encrypt.Hanlde(ref data);
        //        AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(data);
        //        yield return request;
        //        m_Bundle = request.assetBundle;
        //    }
        //}
        yield break;
    }
    public static string ToStreamingPath(string bundleName)
    {
        string streamPath = "";
#if UNITY_EDITOR || UNITY_IPHONE
        streamPath = Path.Combine(Application.streamingAssetsPath, bundleName);
#elif UNITY_ANDROID
        streamPath ="assetbundles/" + bundleName;
#endif
        return streamPath;
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

public class DGMLinkBundlePathConverter : IPathConverter
{
    public string HandlePath(string path)
    {
        string folderPath = Path.GetDirectoryName(path);
        return folderPath;
    }
}



