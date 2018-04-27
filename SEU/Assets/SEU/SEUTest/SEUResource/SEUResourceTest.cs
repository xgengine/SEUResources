using UnityEngine;
using System.Collections;

public class SEUResourceTest : MonoBehaviour {

    class testABPathBuilder: SEUABPathBuilder
    {
        public override string BundlePathHandle(string path)
        {
            return path;
        }
        public override string ManifestBundlePathHandle(string path)
        {
            return "a/test_group";
        }
    }

    SEUResource resource;
    void Start () {
        
        //assetbundle test
        SEUResource.ResisterGroupPath("a", SEULoaderType.AB,SEUResourceUnLoadType.REFCOUNT_ZERO, new testABPathBuilder());
   
    }
    int loadCount = 0;
    int unloadCount = 0;
    Vector3 p = Vector3.zero;
    private void OnGUI()
    {
        if (GUILayout.Button("Load cube"))
        {
            loadCount++;
            resource = SEUResource.Load("a/cube");
            GameObject obj = Instantiate(resource.asset) as GameObject;
            p += Vector3.right;
            obj.transform.position =p;
            
        }
        GUILayout.Label(loadCount.ToString());
        if(GUILayout.Button("unload cube"))
        {
            unloadCount++;
            SEUResource.UnLoadUsedResource(resource);
        }
        GUILayout.Label(unloadCount.ToString());
    }
    void Update () {


	
	}
}
