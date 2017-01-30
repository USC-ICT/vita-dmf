using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AGUtteranceData))]
[CanEditMultipleObjects]
public class AGUtteranceDataEditor : Editor
{
    Object m_scriptAsset = null;
    List<string> m_displayList;

    public override void OnInspectorGUI()
    {
        //EditorGUI.BeginChangeCheck();
        #region Variables
        AGUtteranceData m_uttDataCmp = (AGUtteranceData)target;
        #endregion

        #region UI
        //Draw script component for niceness
        if (m_scriptAsset == null)
        {
            string[] m_scriptGuids = AssetDatabase.FindAssets("AGUtteranceData");
            if (m_scriptGuids.Length > 0)
            {
                string m_scriptAssetPath = AssetDatabase.GUIDToAssetPath(m_scriptGuids[0]);
                m_scriptAsset = AssetDatabase.LoadAssetAtPath(m_scriptAssetPath, typeof(Object));
            }
        }
        if (m_scriptAsset != null)
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", m_scriptAsset, typeof(Object), false);
            GUI.enabled = true;
        }

        //Character
        EditorGUI.BeginChangeCheck();

        if (m_displayList == null)
        {
            m_displayList = new List<string>();
            foreach (var i in VitaGlobals.m_vitaCharacterInfo)
                m_displayList.Add(i.displayName);
        }

        m_uttDataCmp.m_character = EditorGUILayout.Popup("Character", m_uttDataCmp.m_character, m_displayList.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            foreach (Object obj in targets)
            {
                //Debug.Log(obj.name);
                ((AGUtteranceData)obj).m_character = m_uttDataCmp.m_character;
                EditorUtility.SetDirty(obj);
            }
        }

        //Disposition
        EditorGUI.BeginChangeCheck();
        m_uttDataCmp.m_disposition = EditorGUILayout.Popup("Disposition", m_uttDataCmp.m_disposition, VitaGlobals.m_vitaMoods);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (Object obj in targets)
            {
                //Debug.Log(obj.name);
                ((AGUtteranceData)obj).m_disposition = m_uttDataCmp.m_disposition;
                EditorUtility.SetDirty(obj);
            }
        }

        //Scenario
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scenario");
        for (int i = 0; i < VitaGlobals.m_vitaScenarios.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            m_uttDataCmp.m_scenario[i] = EditorGUILayout.Toggle(VitaGlobals.m_vitaScenarios[i], m_uttDataCmp.m_scenario[i]);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    //Debug.Log(obj.name);
                    ((AGUtteranceData)obj).m_scenario[i] = m_uttDataCmp.m_scenario[i];
                    EditorUtility.SetDirty(obj);
                }
            }
        }


        //Reponse Type
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Response Type");
        for (int i = 0; i < VitaGlobals.m_vitaResponseTypes.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            m_uttDataCmp.m_responseType[i] = EditorGUILayout.Toggle(VitaGlobals.m_vitaResponseTypes[i], m_uttDataCmp.m_responseType[i]);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    //Debug.Log(obj.name);
                    ((AGUtteranceData)obj).m_responseType[i] = m_uttDataCmp.m_responseType[i];
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        #endregion
    }
}
