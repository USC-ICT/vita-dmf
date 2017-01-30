using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class AGGuiToggleScoreGraph : MonoBehaviour
{
    [Serializable]
    public class AGGuiToggleScore
    {
        public string ScoreName;
        public Toggle ScoreToggle;
    }

    [Range(-1, 4)]
    public int m_currentScore;
    public List<AGGuiToggleScore> m_scoreToggles;

    public int GetScore()
    {
        return m_currentScore;
    }

    public void SetScore(int score)
    {
        //Debug.Log("setting score " + score.ToString());
        m_currentScore = score;
        for (int i = 0; i < m_scoreToggles.Count; i++)
        {
            //Temporarily disable callback trigger
            m_scoreToggles[i].ScoreToggle.onValueChanged.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);

            //Set all to false, then only up to the current score set to true
            m_scoreToggles[i].ScoreToggle.isOn = false;
            if (i <= score)
            {
                m_scoreToggles[i].ScoreToggle.isOn = true;
            }

            //Re-enable callback trigger
            m_scoreToggles[i].ScoreToggle.onValueChanged.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
        }
    }
}
