﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 *	
 *  Manage View's Create And Destory
 *
 *	by Xuanyi
 *
 */

namespace MoleMole
{
    public class UIManager
    {
        public Dictionary<UIType, GameObject> _UIDict = new Dictionary<UIType,GameObject>();
        public Dictionary<UIType, SEUResource> _UIResDict = new Dictionary<UIType, SEUResource>();
        private Transform _canvas;

        private UIManager()
        {
            _canvas = GameObject.Find("UI Root").transform;
        }

        public GameObject GetSingleUI(UIType uiType)
        {
            if (_UIDict.ContainsKey(uiType) == false || _UIDict[uiType] == null)
            {
                SEUResource res = SEUResource.Load(uiType.Path);
                GameObject go = GameObject.Instantiate(res.asset) as GameObject;
                go.transform.SetParent(_canvas, false);
                go.name = uiType.Name;

                _UIDict.AddOrReplace(uiType, go);
                _UIResDict.AddOrReplace(uiType, res);
                return go;
            }
            return _UIDict[uiType];
        }

        public void DestroySingleUI(UIType uiType)
        {
            if (!_UIDict.ContainsKey(uiType))
            {
                return;
            }

            if (_UIDict[uiType] == null)
            {
                _UIDict.Remove(uiType);
                _UIResDict.Remove(uiType);
                return;
            }

            GameObject.Destroy(_UIDict[uiType]);
            SEUResource.UnLoadResource(_UIResDict[uiType]);

            _UIDict.Remove(uiType);
            _UIResDict.Remove(uiType);
        }
	}
}
