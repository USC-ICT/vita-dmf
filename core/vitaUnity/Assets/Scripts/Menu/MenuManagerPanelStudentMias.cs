using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelStudentMias : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
    [Serializable]
    public class SessionHistory
    {
        public string SessionName;
        public string SessionDate;
        public List<Toggle> ToggleObjects;
        public List<int> MiasScore;
        public string SessionComment;
    }

    MenuManager m_menuManager;
    GameObject m_headerName;
    List<EntityDBVitaProfile> m_users;
    DBSession dbSession;
    List<SessionHistory> m_sessionHistory = new List<SessionHistory>();
    MenuManagerPanelStudentSideBar m_studentSideBar;

    //UI
    Text m_sessionName;
    Text m_sessionDate;
    Text m_sessionComments;
    AGGuiCircularAreaGraph m_graphCircularScore;
    AGGuiGraphLine m_graphFirstImpression;
    AGGuiGraphLine m_graphInterviewResponses;
    AGGuiGraphLine m_graphSelfPromoting;
    AGGuiGraphLine m_graphActiveListening;
    AGGuiGraphLine m_graphClosing;
    GameObject m_tooltipOverlay;
    Text m_tooltipText;
    #endregion

    #region Menu Init
    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.StudentMias);
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");
        m_studentSideBar = VHUtils.FindChildRecursive(menu, "PanelStudentSideBarPrefab").GetComponent<MenuManagerPanelStudentSideBar>();

        m_sessionName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_SessionNum").GetComponent<Text>();
        m_sessionDate = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssignmentDate").GetComponent<Text>();
        m_sessionComments = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Comments").GetComponent<Text>();
        m_graphCircularScore = VHUtils.FindChildRecursive(menu, "GuiCircularAreaGraphPrefab").GetComponent<AGGuiCircularAreaGraph>();
        m_graphFirstImpression = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_FirstImpressions").GetComponent<AGGuiGraphLine>();
        m_graphInterviewResponses = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_InterviewResponses").GetComponent<AGGuiGraphLine>();
        m_graphSelfPromoting = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_SelfPromoting").GetComponent<AGGuiGraphLine>();
        m_graphActiveListening = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_ActiveListening").GetComponent<AGGuiGraphLine>();
        m_graphClosing = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_Closing").GetComponent<AGGuiGraphLine>();
        m_tooltipOverlay = VHUtils.FindChildRecursive(menu, "TooltipImage");
        m_tooltipText = VHUtils.FindChildRecursive(menu, "TooltipText").GetComponent<Text>();
    }

    public void OnMenuEnter()
    {
        m_studentSideBar.SideBarOnMenuEnter();
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;
        dbSession = GameObject.FindObjectOfType<DBSession>();

        m_sessionName.text = "Session ##";
        m_sessionDate.text = "Year/Month/Day";
        m_sessionComments.text = "";
        m_graphFirstImpression.Clear();
        m_graphInterviewResponses.Clear();
        m_graphSelfPromoting.Clear();
        m_graphActiveListening.Clear();
        m_graphClosing.Clear();

        GetSessionData();
    }

    public void OnMenuExit()
    {
        m_studentSideBar.SideBarOnMenuExit();
    }
    #endregion

    #region Private Functions
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

    /// <summary>
    /// Grabs the session data and store it in internal struct. At the end, update scores/populate graphs
    /// </summary>
    void GetSessionData()
    {
        EntityDBVitaProfile userProfile = VitaGlobals.m_loggedInProfile;

        m_sessionHistory.Clear();
        dbSession.GetAllSessions((sessions, error) =>
        {
            foreach (var session in sessions)
            {
                if (session.username == userProfile.username)
                {
                    if (session.sessionname != "GLOBAL")
                    {
                        Debug.LogFormat("Session: {0} - {1}", session.sessionname, session.username);

                        string sessionFullName = session.sessionname; //This is whats stored in DB
                        string[] sessionNameSplit = sessionFullName.Split('_');
                        string sessionDate = sessionNameSplit[0];
                        //string sessionGuid = sessionNameSplit[1];
                        //string sessionStudent = session.username; //Making a copy for delegate
                        string sessionNiceName = CreateNiceSessionName(sessionDate);
                        string sessionYYMMDD = VitaGlobals.TicksToString(sessionDate);
                        List<int> sessionMias = new List<int>() { -1, -1, -1, -1, -1 };
                        string sessionComment = "";

                        foreach (string eventString in session.events)
                        {
                            DBSession.EventData eventData = DBSession.EventData.ToEvent(eventString);
                            if (eventData.type == DBSession.EventType.ScoreNoteFinal)
                            {
                                sessionMias = eventData.miasScores;
                                sessionComment = eventData.scoreNote;
                                break;
                            }
                        }
                        
                        m_sessionHistory.Add(new SessionHistory { SessionName = sessionNiceName, SessionDate = sessionYYMMDD, ToggleObjects = new List<Toggle>(), MiasScore = sessionMias, SessionComment = sessionComment });
                    }
                }
            }
            RefreshScores();
        });
    }

    public void RefreshScores()
    {
        //Plot graphs
        List<AGGuiGraphLine> m_graphs = new List<AGGuiGraphLine>() { m_graphFirstImpression, m_graphInterviewResponses, m_graphSelfPromoting, m_graphActiveListening, m_graphClosing };
        for (int i = 0; i < m_graphs.Count; i++)
        {
            m_graphs[i].Clear();

            foreach (SessionHistory session in m_sessionHistory)
            {
                //Category MIAS score
                float normalizedScore = (float)(session.MiasScore[i] + 1f) / 5f;
                GameObject newDataPoint = m_graphs[i].AddDataPoint(normalizedScore);

                SessionHistory sessionHistoryClass = session;
                Toggle newDataToggle = newDataPoint.GetComponentInChildren<Toggle>();
                session.ToggleObjects.Add(newDataToggle); //This gathers all the affilated data points for this one session across graphs
                newDataToggle.onValueChanged.AddListener(delegate { OnGraphDataPointClicked(sessionHistoryClass); });
            }
        }

        //Now toggle on the last session for display
        if (m_sessionHistory.Count > 0)
        {
            m_sessionHistory[m_sessionHistory.Count - 1].ToggleObjects[0].isOn = true;
        }
    }
    
    /// <summary>
    /// Updates the selected session details
    /// </summary>
    /// <param name="sessionHistoryClass"></param>
    void UpdateSessionDetailedInfo(SessionHistory sessionHistoryClass)
    {
        //Right column info
        m_sessionName.text = sessionHistoryClass.SessionName;
        m_sessionDate.text = sessionHistoryClass.SessionDate;
        m_sessionComments.text = sessionHistoryClass.SessionComment;
        m_graphCircularScore.SetScores( firstImpressionScore:   sessionHistoryClass.MiasScore[0],
                                        interviewResponseScore: sessionHistoryClass.MiasScore[1],
                                        selfPromotingScore:     sessionHistoryClass.MiasScore[2],
                                        activeListeningScore:   sessionHistoryClass.MiasScore[3],
                                        closingScore:           sessionHistoryClass.MiasScore[4]);
    }
    #endregion

    #region UI Hooks
    /// <summary>
    /// Callback triggered by plot graph data points. This will enable all of this session's toggles across the various graphs.
    /// </summary>
    /// <param name="sessionHistoryClass"></param>
    public void OnGraphDataPointClicked(SessionHistory sessionHistoryClass)
    {
        //Reset all
        foreach (SessionHistory session in m_sessionHistory)
        {
            foreach (Toggle sessionToggle in session.ToggleObjects)
            {
                sessionToggle.onValueChanged.RemoveAllListeners();
                sessionToggle.isOn = false;
                SessionHistory sessionHistoryClassInternal = session;
                sessionToggle.onValueChanged.AddListener(delegate { OnGraphDataPointClicked(sessionHistoryClassInternal); });
            }
        }

        //Set selected
        foreach (Toggle sessionToggle in sessionHistoryClass.ToggleObjects)
        {
            sessionToggle.onValueChanged.RemoveAllListeners();
            sessionToggle.isOn = true;
            SessionHistory sessionHistoryClassInternal = sessionHistoryClass;
            sessionToggle.onValueChanged.AddListener(delegate { OnGraphDataPointClicked(sessionHistoryClassInternal); });
        }

        //Updated right column info
        UpdateSessionDetailedInfo(sessionHistoryClass);
    }

    public void BtnHome()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnBack()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    // MIAS score rollovers, first impression
    public void PointerEnterFirstImpressions()
    {
        if (m_graphCircularScore.m_firstImpressionScore == 0)
        {
            TooltipSetText("0 : Focus on making some eye contact and smiling when you’re greeted.  Review appropriate interview clothing and prepare a personal story ahead of time.", true);
        }
        else if (m_graphCircularScore.m_firstImpressionScore == 1)
        {
            TooltipSetText("1 : Look to make more eye contact and give a smile when you walk in.  When asked about yourself, don’t skip the details, but make them work related!", true);
        }
        else if (m_graphCircularScore.m_firstImpressionScore == 2)
        {
            TooltipSetText("2 : Make sure you have strong eye contact, a sincere smile, and you’re not going off topic in your personal details.", true);
        }
        else if (m_graphCircularScore.m_firstImpressionScore == 3)
        {
            TooltipSetText("3 : Be sure to relate your personal details to the job at hand, and tell a unique story!", true);
        }
        else if (m_graphCircularScore.m_firstImpressionScore == 4)
        {
            TooltipSetText("4 : You’re making a great first impression, keep up the smiles!", true);
        }
        else
        {
            Debug.LogWarning("Value not matched for First Impression");
        }
    }

    public void PointerExitFirstImpressions()
    {
        TooltipSetText("", false);
    }

    // MIAS score rollovers, interview response
    public void PointerEnterInterviewResponse()
    {
        if (m_graphCircularScore.m_interviewResponseScore == 0)
        {
            TooltipSetText("0 : Start with one or two word answers to the questions.  Yes, no, and basic skills are a good start.", true);
        }
        else if (m_graphCircularScore.m_interviewResponseScore == 1)
        {
            TooltipSetText("1 : Make sure you are answering the question being asked, and try to expand your answers into full sentences.", true);
        }
        else if (m_graphCircularScore.m_interviewResponseScore == 2)
        {
            TooltipSetText("2 : Keep working on your answers, make sure you are on topic when responding and make sure you answer everything in full sentences.", true);
        }
        else if (m_graphCircularScore.m_interviewResponseScore == 3)
        {
            TooltipSetText("3 : You’re answering all the interview questions well, but you can add more tie-in to the job at hand and try repeating the question asked in your responses.", true);
        }
        else if (m_graphCircularScore.m_interviewResponseScore == 4)
        {
            TooltipSetText("4 : You’re communication is great!", true);
        }
        else
        {
            Debug.LogWarning("Value not matched for Interview Response");
        }
    }

    public void PointerExitInterviewResponse()
    {
        TooltipSetText("", false);
    }

    // MIAS score rollovers, self promoting
    public void PointerEnterSelfPromoting()
    {
        if (m_graphCircularScore.m_selfPromotingScore == 0)
        {
            TooltipSetText("0 : Start with one or two word answers to the questions.  Yes, no, and basic skills are a good start.", true);
        }
        else if (m_graphCircularScore.m_selfPromotingScore == 1)
        {
            TooltipSetText("1 : Add some details to your responses, move away from one or two word answers.  Make sure you are promoting actual job skills.", true);
        }
        else if (m_graphCircularScore.m_selfPromotingScore == 2)
        {
            TooltipSetText("2 : Tell a story when explaining your strengths and skills and make sure you are using adequate enthusiasm.", true);
        }
        else if (m_graphCircularScore.m_selfPromotingScore == 3)
        {
            TooltipSetText("3 : Look to relate your skills to the direct job requirements, and provide some direct examples from your past to highlight your strengths.", true);
        }
        else if (m_graphCircularScore.m_selfPromotingScore == 4)
        {
            TooltipSetText("4 : You sure know how to self-promote!", true);
        }
        else
        {
            Debug.LogWarning("Value not matched for Self Promoting");
        }
    }

    public void PointerExitSelfPromoting()
    {
        TooltipSetText("", false);
    }

    // MIAS score rollovers, active listening
    public void PointerEnterActiveListening()
    {
        if (m_graphCircularScore.m_activeListeningScore == 0)
        {
            TooltipSetText("0 : Start by making eye contact when the interviewer first starts speaking and make sure your body is facing the interviewer.", true);
        }
        else if (m_graphCircularScore.m_activeListeningScore == 1)
        {
            TooltipSetText("1 : Try making eye contact at regular intervals.  Don’t look too far away from the interviewer or sit with your arms crossed.  ", true);
        }
        else if (m_graphCircularScore.m_activeListeningScore == 2)
        {
            TooltipSetText("2 : It’s time to stop interrupting and looking away, keep regular eye contact with the interviewer and use open body language and good posture.", true);
        }
        else if (m_graphCircularScore.m_activeListeningScore == 3)
        {
            TooltipSetText("3 : Always make eye contact when the interview is speaking, and add some signs that you are interested through non-verbal gestures.", true);
        }
        else if (m_graphCircularScore.m_activeListeningScore == 4)
        {
            TooltipSetText("4 : Your body language and gestures say it all!", true);
        }
        else
        {
            Debug.LogWarning("Value not matched for Active Listening");
        }
    }

    public void PointerExitActiveListening()
    {
        TooltipSetText("", false);
    }

    // MIAS score rollovers, closing
    public void PointerEnterClosing()
    {
        if (m_graphCircularScore.m_closingScore == 0)
        {
            TooltipSetText("0 : Make sure to wait until the interview is done speaking to leave and try to thank the interviewer verbally.  You should be practicing at least one question to ask.", true);
        }
        else if (m_graphCircularScore.m_closingScore == 1)
        {
            TooltipSetText("1 : Practice one or two questions to always ask at the end of an interview and be sure to thank the interviewer before you leave.", true);
        }
        else if (m_graphCircularScore.m_closingScore == 2)
        {
            TooltipSetText("2 : You should always be asking a question and be sure to thank the interview with a smile for their time.  Try to add in a little bit about how excited you are for the opportunity and shake the interviewers hand if you’re able.", true);
        }
        else if (m_graphCircularScore.m_closingScore == 3)
        {
            TooltipSetText("3 : Your end of interview question should be of high quality and more than just about dress code, don’t just “thank” the interviewer, show your enthusiasm by stating why you really want the job and why you are the best fit.", true);
        }
        else if (m_graphCircularScore.m_closingScore == 4)
        {
            TooltipSetText("4 : You can close an interview like a champ!", true);
        }
        else
        {
            Debug.LogWarning("Value not matched for Closing");
        }
    }

    public void PointerExitClosing()
    {
        TooltipSetText("", false);
    }

    /// <summary>
    /// Control the display of MIAS circular graph tooltips.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="backgroundOn"></param>
    void TooltipSetText(string text, bool backgroundOn)
    {
        m_tooltipText.text = text;
        m_tooltipOverlay.SetActive(backgroundOn);
    }
    #endregion
}
