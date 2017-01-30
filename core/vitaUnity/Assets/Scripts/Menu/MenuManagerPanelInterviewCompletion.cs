using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelInterviewCompletion : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
    GameObject m_headerName;
    GameObject m_eventLogContent;
    Text m_field_studentName;
    Text m_field_interviewer;
    Text m_field_evaluator;
    Text m_field_disposition;
    Text m_field_environment;
    Text m_field_sessionName;
    Text m_field_sessionLength;
    Text m_field_date;

    RectTransform m_suggestedScore_firstImpression;
    RectTransform m_suggestedScore_interviewResponses;
    RectTransform m_suggestedScore_selfPromoting;
    RectTransform m_suggestedScore_activeListening;
    RectTransform m_suggestedScore_closing;
    Text m_field_responseScore;

    AGGuiToggleScoreGraph m_finalScore_firstImpression;
    AGGuiToggleScoreGraph m_finalScore_interviewResponses;
    AGGuiToggleScoreGraph m_finalScore_selfPromoting;
    AGGuiToggleScoreGraph m_finalScore_activeListening;
    AGGuiToggleScoreGraph m_finalScore_closing;
    InputField m_field_scoringNote;
    Button m_submitScoreButton;
    bool m_submitButtonPressed = false;
    GameObject m_submitScoreLoading;

    GameObject m_dialogTeacherPrefab;
    GameObject m_dialogStudentPrefab;
    GameObject m_dialogDataPrefab;
    //GameObject m_dialogAlarmPrefab;

    public GameObject m_characterUtterancesPrefab;
    GameObject m_characterUtterancesPrefabInstance = null;
    AGUtteranceDataFilter m_chrUttCmp;

    EntityDBVitaProfile m_badgeCheckUserProfile;
    #endregion

    void Start()
    {
        MenuManager m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.InterviewCompletion);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");

        m_headerName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Names");
        m_eventLogContent = VHUtils.FindChildRecursive(menu, "Content_EventLog");
        m_field_studentName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_StudentName").GetComponent<Text>();
        m_field_interviewer = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Interviewer").GetComponent<Text>();
        m_field_evaluator = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Evaluator").GetComponent<Text>();
        m_field_disposition = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Disposition").GetComponent<Text>();
        m_field_environment = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Environment").GetComponent<Text>();
        m_field_sessionName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_SessionName").GetComponent<Text>();
        m_field_sessionLength = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_SessionLength").GetComponent<Text>();
        m_field_date = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Date").GetComponent<Text>();

        m_suggestedScore_firstImpression = VHUtils.FindChildRecursive(menu, "Image_FirstImpressions").GetComponent<RectTransform>();
        m_suggestedScore_interviewResponses = VHUtils.FindChildRecursive(menu, "Image_InterviewResponses").GetComponent<RectTransform>();
        m_suggestedScore_selfPromoting = VHUtils.FindChildRecursive(menu, "Image_SelfPromoting").GetComponent<RectTransform>();
        m_suggestedScore_activeListening = VHUtils.FindChildRecursive(menu, "Image_ActiveListening").GetComponent<RectTransform>();
        m_suggestedScore_closing = VHUtils.FindChildRecursive(menu, "Image_Closing").GetComponent<RectTransform>();
        m_field_responseScore = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_ResponseScore").GetComponent<Text>();
        m_submitScoreButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Submit").GetComponent<Button>();
        m_submitScoreLoading = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_Submit");

        m_finalScore_firstImpression = VHUtils.FindChildRecursive(menu, "GuiToggleScoreCirclesPrefab_FirstImpressions").GetComponent<AGGuiToggleScoreGraph>();
        m_finalScore_interviewResponses = VHUtils.FindChildRecursive(menu, "GuiToggleScoreCirclesPrefab_InterviewResponses").GetComponent<AGGuiToggleScoreGraph>();
        m_finalScore_selfPromoting = VHUtils.FindChildRecursive(menu, "GuiToggleScoreCirclesPrefab_SelfPromoting").GetComponent<AGGuiToggleScoreGraph>();
        m_finalScore_activeListening = VHUtils.FindChildRecursive(menu, "GuiToggleScoreCirclesPrefab_ActiveListening").GetComponent<AGGuiToggleScoreGraph>();
        m_finalScore_closing = VHUtils.FindChildRecursive(menu, "GuiToggleScoreCirclesPrefab_Closing").GetComponent<AGGuiToggleScoreGraph>();
        m_field_scoringNote = VHUtils.FindChildRecursive(menu, "GuiInputScrollFieldPrefab_Note").GetComponent<InputField>();

        //Event log prefabs
        m_dialogTeacherPrefab = VHUtils.FindChildRecursive(resources, "GuiDialogTeacherPrefab");
        m_dialogStudentPrefab = VHUtils.FindChildRecursive(resources, "GuiDialogStudentPrefab");
        m_dialogDataPrefab = VHUtils.FindChildRecursive(resources, "GuiDialogDataPrefab");
        //m_dialogAlarmPrefab = VHUtils.FindChildRecursive(resources, "GuiDialogAlarmPrefab");

    }

    void Update()
    {
        //Enable submit button only when all MIAS scores are given, and that submit button isn't pressed yet
        if (m_submitButtonPressed == true)
        {
            m_submitScoreButton.interactable = false;
        }
        else if (m_finalScore_firstImpression.GetScore() == -1 || m_finalScore_interviewResponses.GetScore() == -1 || m_finalScore_selfPromoting.GetScore() == -1 || m_finalScore_activeListening.GetScore() == -1 || m_finalScore_closing.GetScore() == -1)
        {
            m_submitScoreButton.interactable = false;
        }
        else
        {
            m_submitScoreButton.interactable = true;
        }
    }

    public void OnMenuEnter()
    {
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewEndSession());
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        if (m_characterUtterancesPrefabInstance == null)
        {
            m_characterUtterancesPrefabInstance = Instantiate(m_characterUtterancesPrefab);
        }
        m_chrUttCmp = m_characterUtterancesPrefabInstance.GetComponent<AGUtteranceDataFilter>();

        //Clear menu
        VHUtils.DeleteChildren(m_eventLogContent.transform);
        m_field_scoringNote.text = "";

        UpdateSessionInfo();
        m_submitScoreButton.interactable = false;
        m_submitScoreLoading.SetActive(false);
    }

    public void OnMenuExit()
    {
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

    public void BtnSubmit()
    {
        PopUpDisplay.Instance.DisplayYesNo("Submit", "Confirm score submission?", Submit, null);
    }

    public void Submit()
    {
        StartCoroutine(SubmitInternal());
    }
    #endregion

    #region Private Functions
    IEnumerator SubmitInternal()
    {
        m_submitButtonPressed = true; //To disable button avoiding double-pressing in Update()
        m_submitScoreLoading.SetActive(true);

        // Create one final event of teacher submitted score and note
        List<int> finalMiasScore = new List<int>();
        finalMiasScore.Add(m_finalScore_firstImpression.GetScore());
        finalMiasScore.Add(m_finalScore_interviewResponses.GetScore());
        finalMiasScore.Add(m_finalScore_selfPromoting.GetScore());
        finalMiasScore.Add(m_finalScore_activeListening.GetScore());
        finalMiasScore.Add(m_finalScore_closing.GetScore());
        string finalNote = m_field_scoringNote.text;
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewScoreNoteFinal(finalMiasScore, finalNote));

        yield return StartCoroutine(SaveSessionData());

        yield return StartCoroutine(CheckBadges(finalMiasScore));

        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherStudents);
    }

    IEnumerator SaveSessionData()
    {
        DBSession dbSession = GameObject.FindObjectOfType<DBSession>();

        Debug.LogFormat("Saving Session Data: {0}", VitaGlobals.m_localSessionInfo.sessionName);

        string session = VitaGlobals.m_localSessionInfo.sessionName;
        string student = VitaGlobals.m_interviewStudent;
        string teacher = VitaGlobals.m_loggedInProfile.username;
        string org = VitaGlobals.m_loggedInProfile.organization;
        bool waitForCreateSession = true;
        dbSession.CreateSession(session, student, teacher, org, error =>
        {
            waitForCreateSession = false;

            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("CreateSession() - {0} - error.", CreateNiceSessionName(VitaGlobals.m_localSessionInfo.sessionName.Split('_')[0]) ));
                Debug.LogError(string.Format("CreateSession() - {0} - error: {1}", VitaGlobals.m_localSessionInfo.sessionName, error));
            }
        });

        while (waitForCreateSession)
            yield return new WaitForEndOfFrame();

        bool waitForAddEvents = true;
        dbSession.AddEvents(VitaGlobals.m_localSessionInfo.sessionName, VitaGlobals.m_interviewStudent, VitaGlobals.m_localSessionInfo.eventData, error2 =>
        {
            waitForAddEvents = false;

            if (!string.IsNullOrEmpty(error2))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("AddEvent() - {0} - error.", CreateNiceSessionName(VitaGlobals.m_localSessionInfo.sessionName.Split('_')[0]) ));
                Debug.LogError(string.Format("AddEvent() - {0} - error: {1}", VitaGlobals.m_localSessionInfo.sessionName, error2));
            }
        });

        while (waitForAddEvents)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator CheckBadges(List<int> miasScore)
    {
        bool waitForGetUser = true;
        m_badgeCheckUserProfile = null;

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetUser(VitaGlobals.m_interviewStudent, (profile, error) =>
        {
            waitForGetUser = false;

            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetUser() - {0} - error: {1}", VitaGlobals.m_interviewStudent, error));
                return;
            }

            m_badgeCheckUserProfile = profile;
        });

        while (waitForGetUser)
            yield return new WaitForEndOfFrame();

        int totalMias = 0;
        miasScore.ForEach(i => totalMias += i);

        yield return StartCoroutine(CheckBadgesTotalMias(totalMias, 5, "Intern"));
        yield return StartCoroutine(CheckBadgesTotalMias(totalMias, 9, "Manager"));
        yield return StartCoroutine(CheckBadgesTotalMias(totalMias, 13, "Executive"));
        yield return StartCoroutine(CheckBadgesTotalMias(totalMias, 17, "Vice President"));
        yield return StartCoroutine(CheckBadgesTotalMias(totalMias, 20, "President"));
        yield return StartCoroutine(CheckBadgesAnyScore(miasScore, 3, "Great Job"));
        yield return StartCoroutine(CheckBadgesAnyScore(miasScore, 4, "Amazing Job"));
        yield return StartCoroutine(CheckBadgesHighestScore(miasScore, 0, "CEO Award First Impressions"));
        yield return StartCoroutine(CheckBadgesHighestScore(miasScore, 1, "CEO Award Interview Responses"));
        yield return StartCoroutine(CheckBadgesHighestScore(miasScore, 2, "CEO Award Self-Promoting"));
        yield return StartCoroutine(CheckBadgesHighestScore(miasScore, 3, "CEO Award Active Listening"));
        yield return StartCoroutine(CheckBadgesHighestScore(miasScore, 4, "CEO Award Closing"));
        yield return StartCoroutine(CheckBadgesDisposition(m_field_disposition.text, "Soft-Touch", "You Did It"));
        yield return StartCoroutine(CheckBadgesDisposition(m_field_disposition.text, "Neutral", "Road To Success"));
        yield return StartCoroutine(CheckBadgesDisposition(m_field_disposition.text, "Hostile", "Survivor"));
    }

    /// <summary>
    /// Function to instantiate widgets into a list's content object.
    /// </summary>
    /// <param name="m_widget"></param>
    /// <param name="m_listContent"></param>
    /// <param name="m_textObjName"></param>
    /// <param name="m_widgetDisplayString"></param>
    /// <returns></returns>
    GameObject AddWidgetToList(GameObject m_widget, GameObject m_listContent, string m_widgetName, string m_textObjName, string m_widgetDisplayString)
    {
        //Debug.LogFormat("AddWidgetToList({0})", m_widgetDisplayString);

        GameObject widgetGo = Instantiate(m_widget);
        GameObject widgetText = VHUtils.FindChildRecursive(widgetGo, m_textObjName);
        widgetGo.name = m_widgetName;
        widgetGo.SetActive(true);
        widgetGo.transform.SetParent(m_listContent.transform, false);
        widgetText.GetComponent<Text>().text = m_widgetDisplayString;
        return widgetGo;
    }

    List<int> CalculateMiasScore(List<List<int>> scores)
    {
        List<int> returnScore = new List<int>() { -1, -1, -1, -1, -1 };

        //Per MIAS category
        for (int i = 0; i < 5; i++)
        {
            int scoreTotal = 0;
            int numberOfScores = 0;

            //Add it all up
            foreach (List<int> score in scores)
            {
                if (score[i] != -1)
                {
                    scoreTotal += score[i];
                    numberOfScores += 1;
                }
            }

            //Average it out
            if (numberOfScores > 0)
            {
                returnScore[i] = (int)((float)scoreTotal / (float)numberOfScores);
            }
        }

        //Debug.LogFormat("Returning scores: {0}, {1}, {2}, {3}, {4}", returnScore[0], returnScore[1], returnScore[2], returnScore[3], returnScore[4]);
        return returnScore;
    }

    string CreateNiceSessionName(string ticksString)
    {
        string returnString = ticksString;
        try
        {
            DateTime sessionDateTime = VitaGlobals.TicksToDateTime(ticksString);
            returnString = "Session" + sessionDateTime.Month.ToString().PadLeft(2, '0') + sessionDateTime.Day.ToString().PadLeft(2, '0') + "_" + sessionDateTime.Hour.ToString().PadLeft(2, '0') + sessionDateTime.Minute.ToString().PadLeft(2, '0');
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("{0}", e.ToString());
        }

        return returnString;
    }

    void UpdateSessionInfo() { StartCoroutine(UpdateSessionInfoInternal()); }
    IEnumerator UpdateSessionInfoInternal()
    {
        #region Session Info
        //Session information
        //Get start & end interview event, which holds session info
        DBSession.EventData startEvent = null;
        DBSession.EventData endEvent = null;
        foreach (DBSession.EventData sessionEvent in VitaGlobals.m_localSessionInfo.eventData)
        {
            if (sessionEvent.type == DBSession.EventType.StartSession)
            {
                startEvent = sessionEvent;
            }
            else if (sessionEvent.type == DBSession.EventType.EndSession)
            {
                endEvent = sessionEvent;
            }

            if (startEvent != null && endEvent != null)
            {
                break;
            }
        }

        string m_studentName = VitaGlobals.m_interviewStudent;
        string m_interviewerName = startEvent.character;
        string m_evaluatorName = startEvent.teacherUsername;
        string m_envName = startEvent.environment;
        TimeSpan m_sessionTimeSpan = VitaGlobals.TicksToDateTime(endEvent.timeStamp).Subtract(VitaGlobals.TicksToDateTime(startEvent.timeStamp));
        string m_sessionLength = string.Format("{0}h {1}m", m_sessionTimeSpan.Hours, m_sessionTimeSpan.Minutes);
        bool studentProfileFound = false;
        bool teacherProfileFound = false;

        //Get all nice names
        //Teacher & student names
        DBUser m_dbUser = GameObject.FindObjectOfType<DBUser>();
        m_dbUser.GetUser(startEvent.teacherUsername, (user, error) =>
        {
            m_evaluatorName = user.name;
            teacherProfileFound = true;
        });
        m_dbUser.GetUser(VitaGlobals.m_interviewStudent, (user, error) =>
        {
            m_studentName = user.name;
            studentProfileFound = true;
        });
        while (!teacherProfileFound || !studentProfileFound)
            yield return new WaitForEndOfFrame();

        //Character name
        foreach (VitaGlobals.CharacterInfo chrInfo in VitaGlobals.m_vitaCharacterInfo)
        {
            if (startEvent.character == chrInfo.prefab)
            {
                m_interviewerName = chrInfo.displayName;
                break;
            }
        }

        //Env name
        foreach (VitaGlobals.BackgroundInfo bgInfo in VitaGlobals.m_vitaBackgroundInfo)
        {
            if (startEvent.environment == bgInfo.sceneName)
            {
                m_envName = bgInfo.displayName;
                break;
            }
        }

        //Fill in info
        m_field_studentName.text = m_studentName;
        m_field_interviewer.text = m_interviewerName;
        m_field_evaluator.text = m_evaluatorName;
        m_field_disposition.text = startEvent.disposition;
        m_field_environment.text = m_envName;
        m_field_sessionName.text = CreateNiceSessionName(startEvent.timeStamp.ToString());
        m_field_sessionLength.text = m_sessionLength;
        m_field_date.text = VitaGlobals.TicksToString(startEvent.timeStamp);
        #endregion

        #region Event Log
        //Fill event log with session events
        List<AGUtteranceDataFilter.AGUtteranceDictionary> m_utteranceDictionary = m_chrUttCmp.GetUtteranceDictionary();
        List<List<int>> m_miasScores = new List<List<int>>();
        foreach (DBSession.EventData eventData in VitaGlobals.m_localSessionInfo.eventData)
        {
            //Teacher responses
            if (eventData.type == DBSession.EventType.PlayUtterance)
            {
                //Go through dictionary to check for utterance text
                foreach (AGUtteranceDataFilter.AGUtteranceDictionary entry in m_utteranceDictionary)
                {
                    if (eventData.utteranceId == entry.utteranceName)
                    {
                        AddWidgetToList(m_dialogTeacherPrefab, m_eventLogContent, "DialogTeacherPrefab_" + eventData.utteranceId, "Text", entry.utteranceText);
                        break;
                    }
                }
            }
            //Student responses
            else if (eventData.type == DBSession.EventType.StudentResponse)
            {
                AddWidgetToList(m_dialogStudentPrefab, m_eventLogContent, "DialogStudentPrefab", "Text", eventData.responseText);
            }
            //Scoring notes
            else if (eventData.type == DBSession.EventType.ScoreNote)
            {
                string s0 = eventData.miasScores[0] == -1 ? "-" : eventData.miasScores[0].ToString();
                string s1 = eventData.miasScores[1] == -1 ? "-" : eventData.miasScores[1].ToString();
                string s2 = eventData.miasScores[2] == -1 ? "-" : eventData.miasScores[2].ToString();
                string s3 = eventData.miasScores[3] == -1 ? "-" : eventData.miasScores[3].ToString();
                string s4 = eventData.miasScores[4] == -1 ? "-" : eventData.miasScores[4].ToString();
                string note = string.Format("F[ {0} ]    I[ {1} ]    S[ {2} ]    A[ {3} ]    C[ {4} ]\n{5}", s0, s1, s2, s3, s4, eventData.scoreNote);
                AddWidgetToList(m_dialogDataPrefab, m_eventLogContent, "DialogDataPrefab", "Text", note);
                m_miasScores.Add(eventData.miasScores);
            }
        }
        #endregion

        #region Scores
        List<RectTransform> m_scoreBarGraphs = new List<RectTransform>() { m_suggestedScore_firstImpression, m_suggestedScore_interviewResponses, m_suggestedScore_selfPromoting, m_suggestedScore_activeListening, m_suggestedScore_closing };
        List<AGGuiToggleScoreGraph> m_scoreToggleGraphs = new List<AGGuiToggleScoreGraph>() { m_finalScore_firstImpression, m_finalScore_interviewResponses, m_finalScore_selfPromoting, m_finalScore_activeListening, m_finalScore_closing };

        //Recommended score
        List<int> m_miasScoreRecommended = CalculateMiasScore(m_miasScores);
        for (int i = 0; i < 5; i++)
        {
            m_scoreBarGraphs[i].localScale = new Vector3(((float)m_miasScoreRecommended[i] + 1f) / 5f, m_scoreBarGraphs[i].localScale.y, m_scoreBarGraphs[i].localScale.z);
            m_scoreToggleGraphs[i].SetScore(m_miasScoreRecommended[i]);
        }
        m_field_responseScore.text = ""; //Currently we do not implement response score, so this is unused
        #endregion
    }

    IEnumerator CheckBadgesTotalMias(int totalMias, int score, string badgeName)
    {
        Debug.LogFormat("CheckBadgesTotalMias() - {0} - {1} - {2}", totalMias, score, badgeName);

        // check if the total mias score is equal or higher than 'low'
        if (totalMias >= score)
        {
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            if (!dbUser.DoesUserHaveBadge(m_badgeCheckUserProfile, badgeName))
            {
                Debug.LogFormat("AddBadge() - {0} - {1} - {2}", totalMias, score, badgeName);

                bool waitForAddBadge = true;
                dbUser.AddBadge(m_badgeCheckUserProfile.username, DBUser.BadgeData.NewBadge(badgeName), (profile, error) => 
                {
                    waitForAddBadge = false;

                    if (!string.IsNullOrEmpty(error))
                    {
                        PopUpDisplay.Instance.Display("Error", error);
                        return;
                    }

                    m_badgeCheckUserProfile = profile;
                });

                while (waitForAddBadge)
                    yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator CheckBadgesAnyScore(List<int> miasScores, int score, string badgeName)
    {
        // check if any mias score is equal or higher than 'low'
        bool higher = miasScores.Exists(i => i >= score);
        if (higher)
        {
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            if (!dbUser.DoesUserHaveBadge(m_badgeCheckUserProfile, badgeName))
            {
                bool waitForAddBadge = true;
                dbUser.AddBadge(m_badgeCheckUserProfile.username, DBUser.BadgeData.NewBadge(badgeName), (profile, error) => 
                {
                    waitForAddBadge = false;

                    if (!string.IsNullOrEmpty(error))
                    {
                        PopUpDisplay.Instance.Display("Error", error);
                        return;
                    }

                    m_badgeCheckUserProfile = profile;
                });

                while (waitForAddBadge)
                    yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator CheckBadgesHighestScore(List<int> miasScores, int miasIndex, string badgeName)
    {
        // check if the mias index is highest above all the others (a tie counts as yes)
        int check = miasScores[miasIndex];
        bool isHighest = !miasScores.Exists(i => i > check);  // func returns true if any index is higher than 'check'
        if (isHighest)
        {
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            if (!dbUser.DoesUserHaveBadge(m_badgeCheckUserProfile, badgeName))
            {
                bool waitForAddBadge = true;
                dbUser.AddBadge(m_badgeCheckUserProfile.username, DBUser.BadgeData.NewBadge(badgeName), (profile, error) => 
                {
                    waitForAddBadge = false;

                    if (!string.IsNullOrEmpty(error))
                    {
                        PopUpDisplay.Instance.Display("Error", error);
                        return;
                    }

                    m_badgeCheckUserProfile = profile;
                });

                while (waitForAddBadge)
                    yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator CheckBadgesDisposition(string currentDisposition, string check, string badgeName)
    {
        // check if the current disposition matches 'check'
        if (currentDisposition == check)
        {
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            if (!dbUser.DoesUserHaveBadge(m_badgeCheckUserProfile, badgeName))
            {
                bool waitForAddBadge = true;
                dbUser.AddBadge(m_badgeCheckUserProfile.username, DBUser.BadgeData.NewBadge(badgeName), (profile, error) => 
                {
                    waitForAddBadge = false;

                    if (!string.IsNullOrEmpty(error))
                    {
                        PopUpDisplay.Instance.Display("Error", error);
                        return;
                    }

                    m_badgeCheckUserProfile = profile;
                });

                while (waitForAddBadge)
                    yield return new WaitForEndOfFrame();
            }
        }
    }
    #endregion
}
