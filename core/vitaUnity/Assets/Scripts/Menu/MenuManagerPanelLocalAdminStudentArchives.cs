using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelLocalAdminStudentArchives : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
    [Serializable]
    public class SessionHistory
    {
        public string SessionName;
        public Toggle ToggleObject;
        public List<int> MiasScore;
    }

    //The list of MIAS category toggle buttons
    [Serializable]
    public class MiasFilterOptions
    {
        public VitaGlobals.VitaMiasCategories MiasCategory;
        public Toggle ToggleObject;
    }

    MenuManager m_menuManager;
    DBSession dbSession;
    Dropdown m_studentListDropdown;
    //List<EntityDBVitaProfile> m_usersAll;
    List<EntityDBVitaProfile> m_usersOriginal;
    List<EntityDBVitaProfile> m_users;
    EntityDBVitaProfile m_currentStudentProfile;
    List<EntityDBVitaStudentSession> m_studentAllSessions;
    GameObject m_headerName;
    GameObject m_username;
    GameObject m_password;
    GameObject m_createDate;
    InputField m_studentLastLoginText;

    GameObject m_studentActivitiesContentObj;
    GameObject m_studentActivityEntryObj;
    GameObject m_interviewSessionsContentObj;
    GameObject m_interviewSessionEntryObj;

    //Graphs
    List<SessionHistory> m_sessionHistory = new List<SessionHistory>();
    List<MiasFilterOptions> m_miasFilterOptions = new List<MiasFilterOptions>();
    AGGuiGraphLine m_graphFirstImpression;
    AGGuiGraphLine m_graphInterviewResponses;
    AGGuiGraphLine m_graphSelfPromoting;
    AGGuiGraphLine m_graphActiveListening;
    AGGuiGraphLine m_graphClosing;
//    AGGuiGraphLine m_graphScoring;

    Button m_reinstateStudentButton;
    Button m_deleteStudentButton;
    #endregion

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.LocalAdminHubStudentArchives);
        //GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab (2)");
        m_studentListDropdown = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Student").GetComponent<Dropdown>();

        m_username = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_UserName");
        m_password = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Password");
        m_createDate = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_DateCreated");
        m_studentLastLoginText = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_DateLastLogin").GetComponent<InputField>();

        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_studentActivitiesContentObj = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_History"), "Content");
        m_interviewSessionsContentObj = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_InterviewHistory"), "Content");
        m_studentActivityEntryObj = VHUtils.FindChildRecursive(resources, "GuiListTextButtonEntry_StudentActivityPrefab");
        m_interviewSessionEntryObj = VHUtils.FindChildRecursive(resources, "GuiListTextButtonEntry_InterviewSessionPrefab");

        //Graphs
        Toggle m_progressScoreFilter_firstImpression = VHUtils.FindChildRecursive(menu, "GuiTogglePrefab_FirstImpression").GetComponent<Toggle>();
        Toggle m_progressScoreFilter_interviewResponse = VHUtils.FindChildRecursive(menu, "GuiTogglePrefab_InterviewResponse").GetComponent<Toggle>();
        Toggle m_progressScoreFilter_selfPromoting = VHUtils.FindChildRecursive(menu, "GuiTogglePrefab_SelfPromoting").GetComponent<Toggle>();
        Toggle m_progressScoreFilter_activeListening = VHUtils.FindChildRecursive(menu, "GuiTogglePrefab_ActiveListening").GetComponent<Toggle>();
        Toggle m_progressScoreFilter_closing = VHUtils.FindChildRecursive(menu, "GuiTogglePrefab_Closing").GetComponent<Toggle>();
       // Toggle m_progressScoreFilter_score = VHUtils.FindChildRecursive(menu, "GuiTogglePrefab_Score").GetComponent<Toggle>();
        m_miasFilterOptions.Add(new MiasFilterOptions { MiasCategory = VitaGlobals.VitaMiasCategories.FirstImpression, ToggleObject = m_progressScoreFilter_firstImpression });
        m_miasFilterOptions.Add(new MiasFilterOptions { MiasCategory = VitaGlobals.VitaMiasCategories.InterviewResponse, ToggleObject = m_progressScoreFilter_interviewResponse });
        m_miasFilterOptions.Add(new MiasFilterOptions { MiasCategory = VitaGlobals.VitaMiasCategories.SelfPromoting, ToggleObject = m_progressScoreFilter_selfPromoting });
        m_miasFilterOptions.Add(new MiasFilterOptions { MiasCategory = VitaGlobals.VitaMiasCategories.ActiveListening, ToggleObject = m_progressScoreFilter_activeListening });
        m_miasFilterOptions.Add(new MiasFilterOptions { MiasCategory = VitaGlobals.VitaMiasCategories.Closing, ToggleObject = m_progressScoreFilter_closing });
//        m_miasFilterOptions.Add(new MiasFilterOptions { MiasCategory = VitaGlobals.VitaMiasCategories.Score, ToggleObject = m_progressScoreFilter_score });
        m_graphFirstImpression = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_FirstImpressions").GetComponent<AGGuiGraphLine>();
        m_graphInterviewResponses = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_InterviewResponses").GetComponent<AGGuiGraphLine>();
        m_graphSelfPromoting = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_SelfPromoting").GetComponent<AGGuiGraphLine>();
        m_graphActiveListening = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_ActiveListening").GetComponent<AGGuiGraphLine>();
        m_graphClosing = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_Closing").GetComponent<AGGuiGraphLine>();
     //   m_graphScoring = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_Scoring").GetComponent<AGGuiGraphLine>();

        m_reinstateStudentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_ReinstateRecord").GetComponent<Button>();
        m_deleteStudentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_DeleteRecord").GetComponent<Button>();
    }

    public void OnMenuEnter()
    {
        dbSession = GameObject.FindObjectOfType<DBSession>();
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        StartCoroutine(RefreshStudentDropdown());
        VHUtils.DeleteChildren(m_studentActivitiesContentObj.transform);
        VHUtils.DeleteChildren(m_interviewSessionsContentObj.transform);
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

    public void BtnDeleteRecord()
    {
        PopUpDisplay.Instance.DisplayYesNo(VitaGlobals.Confirm, "Delete student? " + VitaGlobals.CannotBeUndone, DeleteRecord, null);
    }

    public void BtnReinstateRecord()
    {
        int selectedIdx = m_studentListDropdown.value;
        string selectedName = m_studentListDropdown.options[selectedIdx].text;

        string selectedUsername = m_users.Find(user => user.name == selectedName).username;

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.ReinstateUser(selectedUsername, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("DeleteUser() error: {0}", error));
                return;
            }

            RefreshStudentDropdown();
        });
    }

    public void OnValueChangedStudentDropdown(int value)
    {
        if (GetSelectedStudent() == null) return;
        else StartCoroutine(OnValueChangedStudentDropdownInternal(value));
    }
    IEnumerator OnValueChangedStudentDropdownInternal(int value)
    {
        m_currentStudentProfile = GetSelectedStudent();
        m_username.GetComponent<InputField>().text = m_currentStudentProfile.username;
        m_password.GetComponent<InputField>().text = m_currentStudentProfile.password;
        m_createDate.GetComponent<InputField>().text = VitaGlobals.TicksToString(m_currentStudentProfile.createdate);
        m_studentLastLoginText.text = VitaGlobals.TicksToString(m_currentStudentProfile.lastlogin);

        // Populate interview session list (middle-top column)
        VHUtils.DeleteChildren(m_interviewSessionsContentObj.transform);
        VHUtils.DeleteChildren(m_studentActivitiesContentObj.transform);
        m_sessionHistory.Clear();
        yield return GetUserAllessions();

        // Populate student activity history (left column)
        foreach (var session in m_studentAllSessions)
        {
            //Debug.LogFormat("Session: {0} - {1}", session.sessionname, session.username);
            string sessionFullName = session.sessionname; //This is whats stored in DB
            string[] sessionNameSplit = sessionFullName.Split('_');
            string sessionDate = sessionNameSplit[0];

            //Global sessions (TO-DO: Tease out different kinds of globals)
            if (session.sessionname == "GLOBAL")
            {
                foreach (string sessionEventString in session.events)
                {
                    DBSession.EventData eventData = DBSession.EventData.ToEvent(sessionEventString);
                    GameObject widgetGlobal = null;
                    if (eventData.type == DBSession.EventType.AccountCreation)
                    {
                        widgetGlobal = VitaGlobalsUI.AddWidgetToList(m_studentActivityEntryObj, m_studentActivitiesContentObj, "ActivityEntry_AcctCreated" + session.sessionname, "TextEntryName", "Account created");
                    }
                    else if (eventData.type == DBSession.EventType.Login)
                    {
                        widgetGlobal = VitaGlobalsUI.AddWidgetToList(m_studentActivityEntryObj, m_studentActivitiesContentObj, "ActivityEntry_Login" + session.sessionname, "TextEntryName", "Logged in");
                    }
                    else if (eventData.type == DBSession.EventType.Logout)
                    {
                        widgetGlobal = VitaGlobalsUI.AddWidgetToList(m_studentActivityEntryObj, m_studentActivitiesContentObj, "ActivityEntry_Logout" + session.sessionname, "TextEntryName", "Logged out");
                    }
                    else if (eventData.type == DBSession.EventType.PracticeStarted)
                    {
                        widgetGlobal = VitaGlobalsUI.AddWidgetToList(m_studentActivityEntryObj, m_studentActivitiesContentObj, "ActivityEntry_PracticeStarted" + session.sessionname, "TextEntryName", "Practice started");
                    }
                    else if (eventData.type == DBSession.EventType.BadgeEarned)
                    {
                        widgetGlobal = VitaGlobalsUI.AddWidgetToList(m_studentActivityEntryObj, m_studentActivitiesContentObj, "ActivityEntry_BadgeEarned" + session.sessionname, "TextEntryName", "Badge earned");
                    }
                    else if (eventData.type == DBSession.EventType.HomeworkSubmitted)
                    {
                        widgetGlobal = VitaGlobalsUI.AddWidgetToList(m_studentActivityEntryObj, m_studentActivitiesContentObj, "ActivityEntry_Hw" + session.sessionname, "TextEntryName", "Homework submitted");
                    }

                    if (widgetGlobal != null) VHUtils.FindChildRecursive(widgetGlobal, "TextEntryDate").GetComponent<Text>().text = VitaGlobals.TicksToString(eventData.timeStamp);
                }
            }

            //Interview sessions
            else
            {
                string sessionNiceName = VitaGlobals.CreateNiceSessionName(sessionDate);
                GameObject widgetInterview = VitaGlobalsUI.AddWidgetToList(m_studentActivityEntryObj, m_studentActivitiesContentObj, "ActivityEntry_" + sessionNiceName, "TextEntryName", sessionNiceName);
                VHUtils.FindChildRecursive(widgetInterview, "TextEntryDate").GetComponent<Text>().text = VitaGlobals.TicksToString(sessionDate);
            }
        }

        //Interview sessions
        foreach (var session in m_studentAllSessions)
        {
            if (session.sessionname == "GLOBAL")
                continue;

            Debug.LogFormat("Session: {0} - {1}", session.sessionname, session.username);

            string sessionFullName = session.sessionname; //This is whats stored in DB
            string[] sessionNameSplit = sessionFullName.Split('_');
            string sessionDate = sessionNameSplit[0];
            string sessionStudent = session.username; //Making a copy for delegate
            string sessionNiceName = VitaGlobals.CreateNiceSessionName(sessionDate);
            List<int> sessionMias = new List<int>() { -1, -1, -1, -1, -1 };

            foreach (string eventString in session.events)
            {
                DBSession.EventData eventData = DBSession.EventData.ToEvent(eventString);
                if (eventData.type == DBSession.EventType.ScoreNoteFinal)
                {
                    sessionMias = eventData.miasScores;
                    break;
                }
            }

            AddInterviewSessionEntry(session, sessionNiceName, sessionDate, sessionStudent, sessionFullName, sessionMias);
        }

        RefreshScores();
    }

    void AddInterviewSessionEntry(EntityDBVitaStudentSession session, string sessionNiceNameArg, string sessionDateArg, string sessionStudentArg, string sessionFullNameArg, List<int> sessionMias)
    {
        // make copy of arguments for delegate
        string sessionNiceName = string.Copy(sessionNiceNameArg);
        string sessionDate = string.Copy(sessionDateArg);
        string sessionStudent = string.Copy(sessionStudentArg);
        string sessionFullName = string.Copy(sessionFullNameArg);

        GameObject widget = VitaGlobalsUI.AddWidgetToList(m_interviewSessionEntryObj, m_interviewSessionsContentObj, "InterviewEntry_" + session, "TextEntryName", sessionNiceName);
        VHUtils.FindChildRecursive(widget, "TextEntryName").GetComponent<Toggle>().onValueChanged.AddListener(delegate { RefreshScores(); });
        VHUtils.FindChildRecursive(widget, "TextEntryDate").GetComponent<Text>().text = VitaGlobals.TicksToString(sessionDate);
        VHUtils.FindChildRecursive(widget, "BtnView").GetComponent<Button>().onClick.AddListener(delegate
        {
            VitaGlobals.m_interviewStudent = sessionStudent;
            VitaGlobals.m_selectedSession = sessionFullName;
            VitaGlobals.m_returnMenu = MenuManager.Menu.LocalAdminHubStudentArchives;
            m_menuManager.ChangeMenu(MenuManager.Menu.InterviewRecord);
        });

        m_sessionHistory.Add(new SessionHistory { SessionName = sessionNiceName, ToggleObject = VHUtils.FindChildRecursive(widget, "TextEntryName").GetComponent<Toggle>(), MiasScore = sessionMias });
    }
    #endregion

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
        m_widgetDisplayString = m_widgetDisplayString.Replace("\n", "");
        //Debug.LogFormat("AddWidgetToList({0})", m_widgetDisplayString);
        GameObject widgetGo = Instantiate(m_widget);
        GameObject widgetText = VHUtils.FindChildRecursive(widgetGo, m_textObjName);
        widgetGo.name = m_widgetName;
        widgetGo.SetActive(true);
        widgetGo.transform.SetParent(m_listContent.transform, false);
        widgetText.GetComponent<Text>().text = m_widgetDisplayString;
        return widgetGo;
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

    public void DeleteRecord()
    {
        int selectedIdx = m_studentListDropdown.value;
        string selectedName = m_studentListDropdown.options[selectedIdx].text;

        string selectedUsername = m_users.Find(user => user.name == selectedName).username;

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.DeleteUser(selectedUsername, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("DeleteUser() error: {0}", error));
                return;
            }

            RefreshStudentDropdown();
        });
    }

    EntityDBVitaProfile GetSelectedStudent()
    {
        Dropdown dropdown = m_studentListDropdown;

        if (dropdown.options.Count == 0)
            return null;

        string selectedName = dropdown.options[dropdown.value].text;
        return m_users.Find(item => item.name == selectedName);
    }

    /// <summary>
    /// Updates the score display panel with user selected options
    /// </summary>
    public void RefreshScores()
    {
        // (right column)
        List<AGGuiGraphLine> m_graphs = new List<AGGuiGraphLine>() { m_graphFirstImpression, m_graphInterviewResponses, m_graphSelfPromoting, m_graphActiveListening, m_graphClosing /*, m_graphScoring */};
        for (int i = 0; i < m_miasFilterOptions.Count; i++)// (MiasFilterOptions category in m_miasFilterOptions)
        {
            MiasFilterOptions category = m_miasFilterOptions[i];
            m_graphs[i].gameObject.SetActive(category.ToggleObject.isOn);
            m_graphs[i].Clear();
            if (category.ToggleObject.isOn == false)
            {
                continue;
            }

            foreach (SessionHistory session in m_sessionHistory)
            {
                if (session.ToggleObject.isOn)
                {
                    if (i == 5)
                    {
                        //Calculate average score
                        int totalScore = 0;
                        foreach (int score in session.MiasScore)
                        {
                            totalScore += score;
                        }
                        //float normalizedScore = (float)totalScore / (float)session.MiasScore.Count;

                        GameObject dataPointObj = m_graphs[i].AddDataPoint(UnityEngine.Random.Range(0f, 1f));
                        dataPointObj.GetComponentInChildren<Toggle>().interactable = false;
                    }
                    else
                    {
                        //Category MIAS score
                        float normalizedScore = (float)(session.MiasScore[i] + 1f) / 5f;
                        GameObject dataPointObj = m_graphs[i].AddDataPoint(normalizedScore);
                        dataPointObj.GetComponentInChildren<Toggle>().interactable = false;
                    }
                }
            }
        }
    }

    IEnumerator RefreshStudentDropdown()
    {
        string selectedName = "";
        m_currentStudentProfile = null;
        if (m_studentListDropdown.options.Count > 0 && m_studentListDropdown.value >= 0)
            selectedName = VitaGlobalsUI.PruneAsterisk(m_studentListDropdown.options[m_studentListDropdown.value].text);

        bool gettingStudents = true;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;

        m_studentListDropdown.ClearOptions();
        m_username.GetComponent<InputField>().text = "";
        m_password.GetComponent<InputField>().text = "";
        m_createDate.GetComponent<InputField>().text = "";
        m_studentLastLoginText.text = "";

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllStudentsInOrganization(loginOrganization, (users, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllUsers() error: {0}", error));
                return;
            }

            // filter only what we are interested in
            m_usersOriginal = new List<EntityDBVitaProfile>();
            m_users = new List<EntityDBVitaProfile>();
            foreach (var user in users)
            {
                if (user.archived == 1)
                {
                    m_usersOriginal.Add(user);
                    m_users.Add(new EntityDBVitaProfile(user)); // make a copy
                }
            }

            gettingStudents = false;
        });

        while (gettingStudents)
            yield return new WaitForEndOfFrame();

        //UI
        m_reinstateStudentButton.interactable = true;
        m_deleteStudentButton.interactable = true;

        foreach (var user in m_usersOriginal)
        {
            m_studentListDropdown.AddOptions(new List<string>() { user.name });
        }

        if (m_studentListDropdown.options.Count > 0)
        {
            int selectedIdx = m_studentListDropdown.options.FindIndex(s => s.text == selectedName);
            if (selectedIdx >= 0)
                m_currentStudentProfile = m_users.Find(item => item.name == selectedName);
            else
                selectedIdx = 0;

            m_studentListDropdown.value = selectedIdx;
            OnValueChangedStudentDropdown(selectedIdx);
        }
    }

    IEnumerator GetUserAllessions()
    {
        EntityDBVitaProfile student = GetSelectedStudent();
        m_studentAllSessions = new List<EntityDBVitaStudentSession>();

        bool waitingForSessions = true;
        dbSession.GetAllSessions((sessions, error) =>
        {
            foreach (EntityDBVitaStudentSession session in sessions)
            {
                if (session.username == student.username)
                {
                    m_studentAllSessions.Add(session);
                }
            }

            waitingForSessions = false;
        });

        while (waitingForSessions)
            yield return new WaitForEndOfFrame();
    }
}
