using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelStudentPracticeSession : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    Main MainCmp;
    MenuManager m_menuManager;
    GameObject m_headerName;
    Button RepeatButton;
    Button NextButton;
    Text NextButtonText;
    string m_nextBtn_start = "Ask First Question";
    string m_nextBtn_next = "Next Question";
    int m_currentQuestionIndex = 0;

    AGUtteranceDataFilter m_chrUttCmp;
    List<AGUtteranceData> PrimaryContent;

    AGUtteranceData CurrentUtterance;

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.StudentPracticeSession);
        //GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");

        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");

        RepeatButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_RepeatQuestion").GetComponent<Button>();
        NextButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_NextQuestion").GetComponent<Button>();
        NextButtonText = VHUtils.FindChildRecursive(NextButton.gameObject, "TextMain_NoStatus").GetComponent<Text>();
        StartCoroutine(WaitForCutSceneReady());
    }

    public void OnMenuEnter()
    {
        string loginName = "DEMO";
        string loginOrganization = "";
        if (VitaGlobals.m_loggedInProfile != null)
        {
            loginName = VitaGlobals.m_loggedInProfile.name;
            loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        }

        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;
        RepeatButton.interactable = false;
        NextButton.interactable = true;
        NextButtonText.text = m_nextBtn_start;
    }

    public void OnMenuExit()
    {
    }

    IEnumerator WaitForCutSceneReady()
    {
        //if (!VHUtils.SceneManagerActiveSceneName().StartsWith("Env"))
        //    yield break;

        if (GameObject.FindObjectOfType<Main>() == null)
        {
            yield break;
        }

        while (GameObject.FindObjectOfType<Main>().m_vitaCurrentCharacter == -1)
        {
            yield return new WaitForEndOfFrame();
        }

        MainCmp = GameObject.FindObjectOfType<Main>();

        //Debug.Log(MainCmp.m_vitaCurrentCharacter);

        MenuManager menuManager = GameObject.FindObjectOfType<MenuManager>();
        while (menuManager.m_cutscenes.Length == 0)
        {
            yield return new WaitForEndOfFrame();
        }

        while (GameObject.FindObjectOfType<AGUtteranceDataFilter>() == null)
        {
            yield return new WaitForEndOfFrame();
        }

        m_chrUttCmp = GameObject.FindObjectOfType<AGUtteranceDataFilter>();

        PrimaryContent = m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_chrUttCmp.GetScenarioBoolArrayByEnum((VitaGlobals.VitaScenarios)MainCmp.m_vitaCurrentMood), m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Primary));
    }

    public void PlayUtt(string utt, string uttText)
    {
        Debug.LogFormat("PlayUtt({0}, {1})", utt, uttText);
        NetworkRelay.SendNetworkMessage("playcutscene " + utt);
    }

    #region UI Hooks
    public void BtnHome()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnBack()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnRepeatQuestion()
    {
        Debug.LogWarning("BtnRepeatQuestion()");
        PlayUtt(CurrentUtterance.name, CurrentUtterance.GetComponent<AudioSpeechFile>().UtteranceText);
    }

    public void BtnNextQuestion()
    {
        Debug.LogWarning("BtnNextQuestion()");
        //if (m_currentQuestionIndex + 1 <= PrimaryContent.Count)
        //{
        AGUtteranceData utterance = PrimaryContent[m_currentQuestionIndex];
        PlayUtt(utterance.name, utterance.GetComponent<AudioSpeechFile>().UtteranceText);
        NextButtonText.text = m_nextBtn_next;
        CurrentUtterance = utterance;
        RepeatButton.interactable = true;
        m_currentQuestionIndex += 1;
        //}

        if (m_currentQuestionIndex >= PrimaryContent.Count)
        {
            NextButton.interactable = false;
        }
    }

    public void BtnCompleteSession()
    {
        StartCoroutine(BtnCompleteSessionInternal());
    }

    IEnumerator BtnCompleteSessionInternal()
    {
        VitaGlobals.m_setReturnMenu = true;
        if (VitaGlobals.m_interviewType == VitaGlobals.InterviewType.Demo)
            VitaGlobals.m_returnMenu = MenuManager.Menu.DemoConfigure;
        else
            VitaGlobals.m_returnMenu = MenuManager.Menu.StudentSessionConfiguration;

        // check badges
        if (VitaGlobals.m_interviewType != VitaGlobals.InterviewType.Demo)
        {
            if (VitaGlobals.m_loggedInProfile != null)
            {
                bool waitForDB = true;

                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                if (!dbUser.DoesUserHaveBadge(VitaGlobals.m_loggedInProfile, "Learner's Permit"))
                {
                    Debug.LogFormat("AddBadge() - {0}", "Learner's Permit");

                    dbUser.AddBadge(VitaGlobals.m_loggedInProfile.username, DBUser.BadgeData.NewBadge("Learner's Permit"), (profile, error) => 
                    {
                        waitForDB = false;

                        if (!string.IsNullOrEmpty(error))
                        {
                            PopUpDisplay.Instance.Display("Error", error);
                            return;
                        }

                        VitaGlobals.m_loggedInProfile = profile;
                    });

                    while (waitForDB)
                        yield return new WaitForEndOfFrame();
                }
            }
        }

        NetworkRelay.SendNetworkMessage("endinterview");
    }
    #endregion
}
