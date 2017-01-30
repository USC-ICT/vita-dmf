using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AGInterviewButtonStateManager : MonoBehaviour
{
    [Serializable]
    public class AGInterviewButtonState
    {
        public string ButtonName;
        public GameObject ButtonGO;
        public AGUtteranceData UtteranceDataCmp;
        public bool ButtonPressed;
    }

    public List<AGInterviewButtonState> m_interviewButtonStates = new List<AGInterviewButtonState>();

    public GameObject GetButton(string buttonName)
    {
        GameObject returnGO = null;
        foreach (AGInterviewButtonState m_interviewButtonState in m_interviewButtonStates)
        {
            if (m_interviewButtonState.ButtonName == buttonName)
            {
                returnGO = m_interviewButtonState.ButtonGO;
                break;
            }
        }
        return returnGO;
    }

    public AGInterviewButtonState GetLowestNumUnusedPrimaryResponseButton()
    {
        //Debug.Log("GetLowestNumUnusedPrimaryResponseButton()");

        AGInterviewButtonState returnButton = null;
        foreach (AGInterviewButtonState m_interviewButtonState in m_interviewButtonStates)
        {
            if (m_interviewButtonState.ButtonPressed == false)
            {
                returnButton = m_interviewButtonState;
                break;
            }
        }

        if (returnButton != null)   Debug.Log("GetLowestNumUnusedPrimaryResponseButton() - Returning: " + returnButton.ButtonName);
        else                        Debug.Log("GetLowestNumUnusedPrimaryResponseButton() - Returning: null");

        return returnButton;
    }

    /// <summary>
    /// Sets the button's pressed state
    /// </summary>
    /// <param name="button"></param>
    /// <param name="pressed"></param>
    public void SetButtonPressed(GameObject button, bool pressed)
    {
        //Debug.Log("Trying to set button pressed: " + button.name);
        foreach (AGInterviewButtonState m_interviewButtonState in m_interviewButtonStates)
        {
            if (m_interviewButtonState.ButtonGO == button)
            {
                //Debug.Log("Setting button pressed: " + button.name);
                m_interviewButtonState.ButtonPressed = pressed;
                break;
            }
        }
    }
}
