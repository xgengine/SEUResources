using UnityEngine;
using System.Collections;

public interface IPathConverter
{
    string HandlePath(string path);
}

/// <summary>
/// 根据加载路径，生成AB包路径
/// </summary>
/// <param name="path"></param>
/// <returns></returns>
public class SEUDefulatResourceToBundlePathConverter : IPathConverter
{
    public string HandlePath(string path)
    {
        return "assets/resources/" + path;
    }
}

public abstract class SEUBundleLoader
{
    public abstract AssetBundle LoadAssetBundle(string bundleName);
    public abstract AssetBundleCreateRequest LoadAssetBundlAsyn(string bundleName);
}

public class SEUBundleLoaderFromFile : SEUBundleLoader
{
    public override AssetBundle LoadAssetBundle(string bundleName)
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + bundleName;
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        return bundle;
    }
    public override AssetBundleCreateRequest LoadAssetBundlAsyn(string bundleName)
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + bundleName;
        return AssetBundle.LoadFromFileAsync(bundlePath);
    }
}

public class SEUBundleLoaderFromMemory : SEUBundleLoader
{
    public override AssetBundle LoadAssetBundle(string bundleName)
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + bundleName;
        byte[] buffer = SEUFileLoader.ReadAllBytes(bundlePath);
        AssetBundle bundle = AssetBundle.LoadFromMemory(buffer);
        return bundle;
    }
    public override AssetBundleCreateRequest LoadAssetBundlAsyn(string bundleName)
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + bundleName;
        byte[] buffer = SEUFileLoader.ReadAllBytes(bundlePath);
        return AssetBundle.LoadFromMemoryAsync(buffer);
    }
}


