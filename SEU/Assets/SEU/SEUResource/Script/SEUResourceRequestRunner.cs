using UnityEngine;
using System.Collections;

public class SEUResourceRequestRunner : MonoBehaviour {

    static SEUResourceRequestRunner _runer = null;
    static bool _destory = false;
    static SEUResourceRequestRunner runer
    {
        get
        {
            if(_runer == null&&_destory ==false)
            {
                GameObject asyncRequestRunner = new GameObject("SEUResource_AsyncRequestRuner");
                _runer = asyncRequestRunner.AddComponent<SEUResourceRequestRunner>();
                DontDestroyOnLoad(asyncRequestRunner);
            }
            return _runer;
        }
    }
    static public void SendReqest(IEnumerator request)
    {
        if(runer != null)
        {
            runer.StartCoroutine(request);
        }
    }
    private void OnDestroy()
    {
        _destory = true;
    }

}
