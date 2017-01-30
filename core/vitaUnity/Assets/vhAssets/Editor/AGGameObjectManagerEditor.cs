using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AGGameObjectManager))]
public class AGGameObjectManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        #region Variables
        AGGameObjectManager m_goManagerCmp = (AGGameObjectManager)target;

        List<string> editorGoSetNameList = m_goManagerCmp.GetAllSetNames();
        string[] editorGoSetVars = editorGoSetNameList.ToArray();
        List<bool> editorGoBoolVars = new List<bool>();

        //Initialize dropdown menu value
        int m_enabledGOs = 0;
        for (int i = 0; i < m_goManagerCmp.GetAllSetNames().Count; i++)
        {
            bool m_goActive = m_goManagerCmp.m_goSets[i].m_active;

            if (m_goActive)
            {
                m_goManagerCmp.m_currentToggledGoSet = i;
                m_enabledGOs += 1;
            }
        }
        if (m_enabledGOs > 1) { m_goManagerCmp.m_currentToggledGoSet = -1; }
        #endregion

        #region GUI
        int editorConfigVarIndex = EditorGUILayout.Popup("GameObject Toggle", m_goManagerCmp.m_currentToggledGoSet, editorGoSetVars);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("GameObjects");
        m_goManagerCmp.GetAllGameObjects();

        for (int i = 0; i < m_goManagerCmp.GetAllSetNames().Count; i++)
        {
            //Set bools' active state
            m_goManagerCmp.m_goSets[i].m_active = m_goManagerCmp.m_goSets[i].m_go.activeInHierarchy;
            string m_goName = m_goManagerCmp.m_goSets[i].m_goName;
            bool m_goActive = m_goManagerCmp.m_goSets[i].m_active;
            editorGoBoolVars.Add(EditorGUILayout.Toggle(m_goName, m_goActive));
        }
        #endregion

        #region Setup interactivity
        //Bool settings
        for (int i = 0; i < m_goManagerCmp.GetAllSetNames().Count; i++)
        {
            if (editorGoBoolVars[i] != m_goManagerCmp.m_goSets[i].m_active)
            {
                m_goManagerCmp.SetActive(m_goManagerCmp.m_goSets[i].m_goName, editorGoBoolVars[i]);
                m_goManagerCmp.m_currentToggledGoSet = -1;
                editorConfigVarIndex = -1;
            }
        }

        //Dropdown settings
        if (editorConfigVarIndex != m_goManagerCmp.m_currentToggledGoSet)
        {
            m_goManagerCmp.Toggle(m_goManagerCmp.m_goSets[editorConfigVarIndex].m_goName);
            m_goManagerCmp.m_currentToggledGoSet = editorConfigVarIndex;
        }


        #endregion

    }
}
