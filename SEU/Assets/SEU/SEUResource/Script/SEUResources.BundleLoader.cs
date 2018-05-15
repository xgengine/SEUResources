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
public class SEUBundlePathConverter : IPathConverter
{
    public string HandlePath(string path)
    {
        return "assets/resources/" + path;
    }
}

public abstract class SEUBundleLoader
{
    SEUFileLoader m_FileLoader;
    protected AssetBundle m_Bundle;
    public AssetBundle assetBundle
    {
        get
        {
            return m_Bundle;
        }
    }
    protected string m_BundleName;
    public void SetBundleName(string bundleName)
    {
        m_BundleName = bundleName;
    }
    public abstract void  Load();
    public abstract IEnumerator LoadAsync();
}

public class SEUBundleLoaderFromFile : SEUBundleLoader
{
    public override void Load()
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + m_BundleName;
        m_Bundle = AssetBundle.LoadFromFile(bundlePath);
       
    }
    public override IEnumerator LoadAsync()
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + m_BundleName;
        AssetBundleCreateRequest request =  AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;
        m_Bundle = request.assetBundle;
    }
}

public class SEUBundleLoaderFromMemory : SEUBundleLoader
{
    public override void  Load()
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + m_BundleName;
        byte[] buffer = SEUFileLoader.ReadAllBytes(bundlePath);
        m_Bundle =  AssetBundle.LoadFromMemory(buffer);
     
    }
    public override IEnumerator LoadAsync()
    {
        string bundlePath = System.IO.Path.GetDirectoryName(Application.dataPath) + "/assetbundles/" + m_BundleName;
        byte[] buffer = SEUFileLoader.ReadAllBytes(bundlePath);
        AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(buffer);
        yield return request;
        m_Bundle = request.assetBundle; ;
    }
}


