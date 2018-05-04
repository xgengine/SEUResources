using UnityEngine;
using System.Collections;

public class SEUResourceTest : MonoBehaviour {

    class testABPathBuilder: SEUABPathBuilder
    {
        public override string BundlePathHandle(string path)
        {

            return path.Replace("1","");
        }
        public override string ManifestBundlePathHandle(string path)
        {
            return "a/test_group";
        }
    }

   // SEUResource resource;
    void Start () {

        //SEUResource.Request requst = SEUResource.LoadAsyn("a/cube");
        //yield return requst;

        //Instantiate(requst.resource.asset);

        //assetbundle test
        SEUResource.ResisterGroupPath("a", SEULoaderType.AB,SEUResourceUnLoadType.REFCOUNT_ZERO, new testABPathBuilder());


        //SEUResource.Request requst = SEUResource.LoadAsyn("a/cube");
        //yield return requst;
        //Instantiate(requst.resource.asset);

        //for(int i = 0; i < 11;i++)
        //{

        //    StartCoroutine(RUN());
        //    var resource = SEUResource.Load("a/cube");
        //    GameObject obj = Instantiate(resource.asset) as GameObject;
        //    SEUResource.UnLoadResource(resource);
        //}

        var resource1 = SEUResource.Load("a/cube");
        var resource2 = SEUResource.Load("a/cube1");
        GameObject obj1 = Instantiate(resource1.asset) as GameObject;

        GameObject obj2 = Instantiate(resource2.asset) as GameObject;

        var resource3 = SEUResource.Load("a/roll");
        GameObject obj3 = Instantiate(resource3.asset) as GameObject;

       

        var resource4 = SEUResource.Load("a/sphere");
        GameObject obj4 = Instantiate(resource4.asset) as GameObject;

        //SEUResource.UnLoadResource(resource1);
        //SEUResource.UnLoadResource(resource2);
        //SEUResource.UnLoadResource(resource3);
        //SEUResource.UnLoadResource(resource4);

    }
    int loadCount = 0;
    int unloadCount = 0;
    Vector3 p = Vector3.zero;
    private void OnGUI()
    {
        if (GUILayout.Button("Load cube"))
        {
            loadCount++;
            var resource = SEUResource.Load("a/cube");
            GameObject obj = Instantiate(resource.asset) as GameObject;
            p += Vector3.right;
            obj.transform.position =p;
            
        }
        if(GUILayout.Button("Load cube async"))
        {
            StartCoroutine(RUN());
        }

        GUILayout.Label(loadCount.ToString());
        if(GUILayout.Button("unload cube"))
        {
        
            
        }
     
        GUILayout.Label(unloadCount.ToString());
    }

    IEnumerator RUN()
    {
        loadCount++;
        int a = loadCount;
        Debug.Log("vvvvvvvvvvvvvvvvvvvvvvvvvv" +a.ToString());
    
        SEUResource.Request requst = SEUResource.LoadAsyn("a/cube");
        yield return requst;


        var resource = requst.resource;
        GameObject obj = Instantiate(requst.resource.asset) as GameObject;
        obj.name = "xxx";
        SEUResource.UnLoadResource(resource);

        Debug.Log("cccccccccccccccccccccccccccccc" + a.ToString());
    }
    void Update () {


	
	}
}
