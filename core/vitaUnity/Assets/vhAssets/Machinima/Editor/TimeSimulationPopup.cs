using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TimeSimulationPopup : EditorWindow
{
    #region Variables
    bool m_ShouldBeClosed;
    EditorCutsceneManager m_EditorCutsceneManager;
    float m_TargetTime;
    int m_SelectedCutsceneIndex;
    List<string> m_CutsceneNames = new List<string>();
    float m_SecondsBeforeCompletion;
    #endregion

    #region Functions
    public static void Init(Rect windowPos, EditorCutsceneManager cutsceneManager, int selectedCutsceneIndex)
    {
        TimeSimulationPopup window = (TimeSimulationPopup)EditorWindow.GetWindow(typeof(TimeSimulationPopup));
        window.position = windowPos;
        window.Setup(cutsceneManager, selectedCutsceneIndex);
        window.ShowPopup();
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
        window.title = "Simulate Time";
#else
        window.titleContent.text = "Simulate Time";
#endif
        window.autoRepaintOnSceneChange = true;
    }

    void OnLostFocus()
    {
        m_ShouldBeClosed = true;
    }

    void Update()
    {
        if (m_ShouldBeClosed)
        {
            Close();
        }

        if (m_SecondsBeforeCompletion > 0)
        {
            m_SecondsBeforeCompletion -= Time.deltaTime;
            m_SecondsBeforeCompletion = Mathf.Max(0, m_SecondsBeforeCompletion);
        }
    }

    public void Setup(EditorCutsceneManager cutsceneManager, int selectedCutsceneIndex)
    {
        m_EditorCutsceneManager = cutsceneManager;
        m_SelectedCutsceneIndex = selectedCutsceneIndex;

        foreach (TimelineObject cutscene in cutsceneManager.GetAllTimelineObjects())
        {
            m_CutsceneNames.Add(cutscene.NameIdentifier);
        }
    }

    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            GUILayout.Label("The scene must be playing in order to use this");
            return;
        }

        m_SelectedCutsceneIndex = EditorGUILayout.Popup("Target Cutscene", m_SelectedCutsceneIndex, m_CutsceneNames.ToArray());
        m_TargetTime = EditorGUILayout.FloatField("Target Time", m_TargetTime);
        if (GUILayout.Button("Fast Forward"))
        {
            DoFastForward();
        }

        if (m_SecondsBeforeCompletion > 0)
        {
            GUILayout.Label(string.Format("Please wait: {0}", m_SecondsBeforeCompletion.ToString("f2")));
        }
    }

    void DoFastForward()
    {
        Cutscene targetCutscene = m_EditorCutsceneManager.GetTimelineObjectByIndex(m_SelectedCutsceneIndex) as Cutscene;
        m_SecondsBeforeCompletion = m_EditorCutsceneManager.CalculateTimeRequiredToFastForward(targetCutscene, m_TargetTime, Cutscene.MaxFastForwardSpeed)  + 5; // extra padding for the smartbody simulation. This is a hack;
        m_EditorCutsceneManager.FastForward(targetCutscene, m_TargetTime, Cutscene.MaxFastForwardSpeed);
    }
    #endregion
}
