#define SEU_DEITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
[CustomEditor(typeof(SEUResourcesDebug))]
public class SEUResouceDebugEditor : Editor{

    public override void OnInspectorGUI()
    {
        SEUResourcesDebug debugObject = target as SEUResourcesDebug;
        if(debugObject ==null||debugObject.resource == null)
        {
            return;
        }
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
           

            var oc = GUI.color;
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("SEUResources ["+debugObject.resource.GetType()+"]", EditorStyles.boldLabel);
            GUI.color = oc;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSEUResources(debugObject.resource);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Dependence", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
       
            for (int i = 0; i < debugObject.resource.dependenceResources.Count; i++)
            {
                DrawSEUResources(debugObject.resource.dependenceResources[i],true);
            }
            EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }

    void DrawSEUResources(SEUResources res,bool showDeps =false)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(res.loadPath);
       
        if (showDeps)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(res.DebugObject, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("RefCount: " + res.refCount.ToString());
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Use /UnUse Stack Info", EditorStyles.boldLabel);
            var loadList = res.Debug_StackInfo.FindAll((p) => p.StartsWith("[Use]"));
           
            EditorGUILayout.LabelField(string.Format( "Use [{0}] UnUse[{1}]",loadList.Count,res.Debug_StackInfo.Count-loadList.Count));

            Color old = GUI.color;
            for (int i = 0; i < res.Debug_StackInfo.Count; i++)
            {
                string info = res.Debug_StackInfo[i];
                if (info.StartsWith("[Use]"))
                {
                    GUI.color = Color.green;
                }
                else
                {
                    GUI.color = Color.magenta;
                }
               EditorGUILayout.LabelField(res.Debug_StackInfo[i],EditorStyles.textArea);
            }
            GUI.color = old;
            EditorGUILayout.EndVertical();

        }
       
        EditorGUILayout.EndVertical();
    }
}
