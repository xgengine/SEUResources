using UnityEngine;
using System.Collections;
using MoleMole;
public class UISample1 : MonoBehaviour
{

    class MyBundlePathConverter : IPathConverter
    {
        public string HandlePath(string path)
        {
            return "assets/resources/" + path;
        }
    }
    class MyManifestPathProvider : IPathProvider
    {
        public string GetPath()
        {
            return "assetbundles";
        }
    }

    private void Awake()
    {
        SEUResource.ResisterGroupPath("view", SEULoaderType.AB, SEUResourceUnLoadType.REFCOUNT_ZERO, new MyManifestPathProvider(), new MyBundlePathConverter());
        Singleton<UIManager>.Create();
        Singleton<ContextManager>.Create();
    }
    void Start ()
    {
     
	}	
	// Update is called once per frame
	void Update () {
	
	}
}
