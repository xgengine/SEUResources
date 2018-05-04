#define SEU_DEITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
[CustomEditor(typeof(SEUResourceDebug))]
public class SEUResouceDebugEditor : Editor{

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SEUResourceDebug debugObject = target as SEUResourceDebug;
            EditorGUILayout.LabelField("SEUResource", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSEUResource(debugObject.resource);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Dependence", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
       
            for (int i = 0; i < debugObject.resource.dependenceResources.Count; i++)
            {
                DrawSEUResource(debugObject.resource.dependenceResources[i],true);
            }
            EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }

    void DrawSEUResource(SEUResource res,bool showDeps =false)
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
            EditorGUILayout.LabelField("Load /UnLoad Stack Info", EditorStyles.boldLabel);
            var loadList = res.Debug_StackInfo.FindAll((p) => p.StartsWith("[Load]"));
           
            EditorGUILayout.LabelField(string.Format( "Load [{0}] UnLoad[{1}]",loadList.Count,res.Debug_StackInfo.Count-loadList.Count));

            Color old = GUI.color;
            for (int i = 0; i < res.Debug_StackInfo.Count; i++)
            {
                string info = res.Debug_StackInfo[i];
                if (info.StartsWith("[Load]"))
                {
                    GUI.color = Color.green;
                }
                else
                {
                    GUI.color = Color.red;
                }
               EditorGUILayout.LabelField(res.Debug_StackInfo[i],EditorStyles.textArea);
            }
            GUI.color = old;
            EditorGUILayout.EndVertical();

        }
       
        EditorGUILayout.EndVertical();
    }
}
