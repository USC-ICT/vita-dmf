/* AGLightSettingsEditor.cs
 *
 * This extends AGLightSettings.cs for interactive UI in the editor.
 *
 * Joe Yip
 * 2015-Aug-14
 * yip@ict.usc.edu
 */
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AGLightSettings))]
public class AGLightSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        #region Variables
        AGLightSettings m_lightsManagerCmp = (AGLightSettings)target;
        List<string> editorLightSetNameList = m_lightsManagerCmp.GetAllSetNames();

        //Initialize dropdown menu value
        int m_enabledGOs = 0;
        for (int i = 0; i < m_lightsManagerCmp.GetAllSetNames().Count; i++)
        {
            bool m_goActive = m_lightsManagerCmp.m_goSets[i].m_active;

            if (m_goActive)
            {
                m_lightsManagerCmp.m_currentToggledGoSet = i;
                m_enabledGOs += 1;
            }
        }
        if (m_enabledGOs > 1) { m_lightsManagerCmp.m_currentToggledGoSet = -1; }
        #endregion

        #region GUI
        int editorConfigVarIndex = EditorGUILayout.Popup("Light Set Toggle", m_lightsManagerCmp.m_currentToggledGoSet, editorLightSetNameList.ToArray());
        if (editorConfigVarIndex != m_lightsManagerCmp.m_currentToggledGoSet)
        {
            m_lightsManagerCmp.Toggle(m_lightsManagerCmp.m_goSets[editorConfigVarIndex].m_goName);
            m_lightsManagerCmp.m_currentToggledGoSet = editorConfigVarIndex;
        }
        #endregion

    }
}
