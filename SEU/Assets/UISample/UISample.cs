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
    IEnumerator Start ()
    {
        SEUResource.ResisterGroupPath("view", SEULoaderType.AB);
        StartCoroutine(vvv());
        yield return null;
        float t = Time.realtimeSinceStartup;
        var res1 = SEUResource.Load(UIType.MainView.Path);

        var res2 = SEUResource.Load(UIType.NewTestView.Path);

        Debug.Log(Time.realtimeSinceStartup - t);
        SEUResource.UnLoadResource(res1);
        SEUResource.UnLoadResource(res2);


        yield return null;
	}	

    IEnumerator vvv()
    {
       var t = Time.realtimeSinceStartup;
        var res3 = SEUResource.LoadAsyn(UIType.MainView.Path);
        yield return res3;
        var res4 = SEUResource.LoadAsyn(UIType.NewTestView.Path);

        yield return res4;

        SEUResource.UnLoadResource(res3.resource);
        SEUResource.UnLoadResource(res4.resource);
        Debug.Log(Time.realtimeSinceStartup - t);
    }

	void Update () {
	
	}
}
