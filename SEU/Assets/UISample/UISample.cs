using UnityEngine;
using System.Collections;
using MoleMole;
public class UISample : MonoBehaviour
{

    private void Awake()
    {
        SEUResource.ResisterGroupPath("view", SEULoaderType.AB);
        //Singleton<UIManager>.Create();
        //Singleton<ContextManager>.Create();
    }
    IEnumerator Start ()
    {
        float t = Time.realtimeSinceStartup;
        var res1 = SEUResource.Load(UIType.MainView.Path);
        yield return res1;
        var res2 = SEUResource.Load(UIType.NewTestView.Path);
        yield return res2;
        Debug.Log(Time.realtimeSinceStartup -t);
        //SEUResource.UnLoadResource(res1);
        //SEUResource.UnLoadResource(res2);
	}	

	void Update () {
	
	}
}
