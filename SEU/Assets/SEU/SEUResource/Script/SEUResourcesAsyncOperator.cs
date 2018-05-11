﻿using UnityEngine;
using System.Collections;
/// <summary>
/// 执行异步加载操作
/// </summary>
public class SEUResourcesAsyncOperator : MonoBehaviour {

    static SEUResourcesAsyncOperator _operator = null;
    static bool _destory = false;
    static SEUResourcesAsyncOperator asyncOperator
    {
        get
        {
            if(_operator == null&&_destory ==false)
            {
                GameObject asyncRequestRunner = new GameObject("_[SEUResourcessAsyncOperator]_");
                _operator = asyncRequestRunner.AddComponent<SEUResourcesAsyncOperator>();
                asyncOperator.hideFlags = HideFlags.HideInHierarchy;
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
