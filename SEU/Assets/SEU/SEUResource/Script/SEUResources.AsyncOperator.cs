using UnityEngine;
using System.Collections;
/// <summary>
/// 执行异步加载操作
/// </summary>
public class AsyncOperator : MonoBehaviour {

    static AsyncOperator _operator = null;
    static bool _destory = false;
    static AsyncOperator asyncOperator
    {
        get
        {
            if(_operator == null&&_destory ==false)
            {
                GameObject asyncRequestRunner = new GameObject("[AsyncOperator]");
                _operator = asyncRequestRunner.AddComponent<AsyncOperator>();
                asyncRequestRunner.hideFlags = HideFlags.HideInHierarchy;
                DontDestroyOnLoad(asyncRequestRunner);
            }
            return _operator;
        }
    }
    static public void SendReqest(IEnumerator request)
    {
        if(asyncOperator != null)
        {
            asyncOperator.StartCoroutine(request);
        }
    }
    private void OnDestroy()
    {
        _destory = true;
    }

}
