using UnityEngine;
using System.Collections;
using MoleMole;
public class UISample : MonoBehaviour
{

    private void Awake()
    {
        
        //Singleton<UIManager>.Create();
        //Singleton<ContextManager>.Create();
    }
    IEnumerator  Start ()
    {
        SEUResource.ResisterGroupPath("view", SEULoaderType.AB);
        //StartCoroutine(vvv());

        yield return vvv();

        //yield return null;
        //float t = Time.realtimeSinceStartup;
        var res1 = SEUResource.Load(UIType.MainView.Path);
        Debug.LogError(res1.asset);
        Debug.LogError("res1 =====");
        //var res2 = SEUResource.Load(UIType.NewTestView.Path);

        //Debug.LogError(res2.asset);
        //Debug.LogError("res2 =====");

        //Debug.Log(Time.realtimeSinceStartup - t);
        SEUResource.UnLoadResource(res1);
        //SEUResource.UnLoadResource(res2);


        yield return null;
    }	

    IEnumerator vvv()
    {
        Debug.LogError("vvvvvvvvvv");
        var t = Time.realtimeSinceStartup;
        SEUResource.Request res3 = SEUResource.LoadAsyn(UIType.MainView.Path);
        Debug.LogError(res3);
        yield return res3;
        Debug.LogError(res3.resource.asset);
        Debug.LogError("res3 =====");
        var res4 = SEUResource.LoadAsyn(UIType.NewTestView.Path);

        yield return res4;
        Debug.LogError(res4.resource.asset);
        Debug.LogError("res 4=====");
        SEUResource.UnLoadResource(res3.resource);
        SEUResource.UnLoadResource(res4.resource);
        Debug.Log(Time.realtimeSinceStartup - t);
    }

	void Update () {
	
	}
}
