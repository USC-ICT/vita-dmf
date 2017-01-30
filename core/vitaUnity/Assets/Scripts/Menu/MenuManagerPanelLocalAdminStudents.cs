using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManagerPanelLocalAdminStudents : MonoBehaviour, MenuManager.IMenuManagerInterface
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

    GameObject m_archiveStudentButton;
    GameObject m_deleteStudentButton;
    GameObject m_saveStudentButton;
    #endregion

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.LocalAdminHubStudents);
        //GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab_Names");
        m_studentListDropdown = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiDropdownEditablePrefab_Student"), "Dropdown").GetComponent<Dropdown>();

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
//        Toggle m_progressScoreFilter_score = VHUtils.FindChildRecursive(menu, "GuiTogglePrefab_Score").GetComponent<Toggle>();
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
//        m_graphScoring = VHUtils.FindChildRecursive(menu, "GuiGraphLinePrefab_Scoring").GetComponent<AGGuiGraphLine>();

        m_archiveStudentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_ArchiveRecord");
        m_deleteStudentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_DeleteStudent");
        m_saveStudentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_SaveChanges");
    }

    public void OnMenuEnter()
    {
        dbSession = GameObject.FindObjectOfType<DBSession>();
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        VitaGlobalsUI.m_unsavedChanges = false;

        StartCoroutine(RefreshStudentDropdown());
        VHUtils.DeleteChildren(m_studentActivitiesContentObj.transform);
        VHUtils.DeleteChildren(m_interviewSessionsContentObj.transform);
    }

    public void OnMenuExit()
    {
        VitaGlobalsUI.m_unsavedChanges = false;
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

    public void BtnArchiveRecord()
    {
        int selectedIdx = m_studentListDropdown.value;
        string selectedName = VitaGlobalsUI.PruneAsterisk(m_studentListDropdown.options[selectedIdx].text);
        string selectedUsername = m_users.Find(user => user.name == selectedName).username;

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.ArchiveUser(selectedUsername, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("ArchiveUser() error: {0}", error));
                return;
            }

            //Remove dropdown option
            List<Dropdown.OptionData> optionData = new List<Dropdown.OptionData>();
            m_studentListDropdown.options.ForEach(i => optionData.Add(new Dropdown.OptionData(i.text, i.image)));
            optionData.Remove(optionData[selectedIdx]);
            m_studentListDropdown.ClearOptions();
            m_studentListDropdown.options = optionData;
            m_studentListDropdown.value = selectedIdx - 1;
        });
    }

    public void BtnDeleteRecord()
    {
        PopUpDisplay.Instance.DisplayYesNo(VitaGlobals.Confirm, "Delete student? " + VitaGlobals.CannotBeUndone, DeleteRecord, null);
    }

    public void BtnAddNewStudent()
    {
        PopUpDisplay.Instance.DisplayOkCancelInput("Add Student", "Enter Student Username", (input) =>
        {
            string studentUsername = input;

            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.GetUser(studentUsername, (user, error) =>
            {
                // if an error happens here, we can't tell the difference between error and 'no profile exists'.  however it will be caught at 'save' time
                if (user != null)
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("Student {0} already exists.  Please enter a new name.", studentUsername), "Ok", "Cancel", true, () =>
                    {
                        BtnAddNewStudent();
                    }, null);
                    return;
                }

                string loginOrganization = VitaGlobals.m_loggedInProfile.organization;

                EntityDBVitaProfile student = new EntityDBVitaProfile(studentUsername, "password", loginOrganization, "New Student", (int)DBUser.AccountType.STUDENT);
                m_users.Add(student);
                Dropdown dropdown = m_studentListDropdown;
                dropdown.AddOptions(new List<string>() { student.name } );
                dropdown.value = dropdown.options.Count - 1;
                OnValueChangedStudentDropdown(dropdown.value);
            });
        }, null);
    }

    public void BtnSave()
    {
        StartCoroutine(BtnSaveChangesInternal());
    }

    public void OnValueEditStudentName()
    {
        if (m_currentStudentProfile == null) return;
        Dropdown dropdown = m_studentListDropdown;
        string newName = VitaGlobalsUI.PruneAsterisk(string.Copy(dropdown.options[dropdown.value].text));

        Debug.LogFormat("MenuManagerPanelLocalAdminStudents.OnValueEditStudentName() - Name value edited: {0}", newName);
        m_currentStudentProfile.name = newName;
        UpdateUIStates(m_currentStudentProfile);
    }

    public void OnValueChangedStudentDropdown(int value)
    {
        if      (GetSelectedStudent() == null) return;
        else    StartCoroutine(OnValueChangedStudentDropdownInternal(value));
    }
    IEnumerator OnValueChangedStudentDropdownInternal(int value)
    {
        //Debug.LogFormat("MenuManagerPanelLocalAdminStudents.OnValueChangedStudentDropdown() - {0} - {1}", selectedName, value);

        yield return StartCoroutine(GetUserAllSessions());

        m_currentStudentProfile = GetSelectedStudent();
        m_username.GetComponent<InputField>().text = m_currentStudentProfile.username;
        m_password.GetComponent<InputField>().text = m_currentStudentProfile.password;
        m_createDate.GetComponent<InputField>().text = VitaGlobals.TicksToString(m_currentStudentProfile.createdate);
        m_studentLastLoginText.text = VitaGlobals.TicksToString(m_currentStudentProfile.lastlogin);

        // Populate interview session list (middle-top column)
        VHUtils.DeleteChildren(m_interviewSessionsContentObj.transform);
        VHUtils.DeleteChildren(m_studentActivitiesContentObj.transform);
        m_sessionHistory.Clear();

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

                    //Debug.LogWarning(eventData.type.ToString());

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
            VitaGlobals.m_returnMenu = MenuManager.Menu.LocalAdminHubStudents;
            m_menuManager.ChangeMenu(MenuManager.Menu.InterviewRecord);
        });

        m_sessionHistory.Add(new SessionHistory { SessionName = sessionNiceName, ToggleObject = VHUtils.FindChildRecursive(widget, "TextEntryName").GetComponent<Toggle>(), MiasScore = sessionMias });
    }

    public void OnValueChangedUsername(string value)
    {
        //Debug.LogFormat("MenuManagerPanelLocalAdminStudents.OnValueChangedUsername() - {0}", value);
        if (m_currentStudentProfile == null) return;
        m_currentStudentProfile.username = m_username.GetComponent<InputField>().text;
        UpdateUIStates(m_currentStudentProfile);
    }

    public void OnValueChangedPassword(string value)
    {
        //Debug.LogFormat("MenuManagerPanelLocalAdminStudents.OnValueChangedPassword() - {0}", value);
        if (m_currentStudentProfile == null) return;
        m_currentStudentProfile.password = m_password.GetComponent<InputField>().text;
        UpdateUIStates(m_currentStudentProfile);
    }
    #endregion

    void DeleteRecord()
    {
        int selectedIdx = m_studentListDropdown.value;
        string selectedName = VitaGlobalsUI.PruneAsterisk(m_studentListDropdown.options[selectedIdx].text);
        string selectedUsername = m_users.Find(user => user.name == selectedName).username;

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.DeleteUser(selectedUsername, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("ArchiveUser() error: {0}", error));
                return;
            }

            //Remove dropdown option
            List<Dropdown.OptionData> optionData = new List<Dropdown.OptionData>();
            m_studentListDropdown.options.ForEach(i => optionData.Add(new Dropdown.OptionData(i.text, i.image)));
            optionData.Remove(optionData[selectedIdx]);
            m_studentListDropdown.ClearOptions();
            m_studentListDropdown.options = optionData;
            m_studentListDropdown.value = selectedIdx - 1;
        });
    }

    /// <summary>
    /// Gets based on user nice name only
    /// </summary>
    /// <returns></returns>
    EntityDBVitaProfile GetSelectedStudent()
    {
        EntityDBVitaProfile returnUser = null;
        if (m_studentListDropdown.options.Count == 0) return null;

        //Try getting by nice name
        string selectedName = VitaGlobalsUI.PruneAsterisk(m_studentListDropdown.options[m_studentListDropdown.value].text);
        int counter = 0;
        m_users.ForEach((item) => { if (item.name == selectedName) { counter++; } });
        if (counter == 1) //Check for multi-same-name conflicts
        {
            returnUser = m_users.Find(item => item.name == selectedName);
        }

        //Try getting by list position
        if (returnUser == null && m_users.Count > m_studentListDropdown.value)
        {
            returnUser = m_users[m_studentListDropdown.value];
        }

        return returnUser;
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
                if (user.archived == 0)
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
        m_archiveStudentButton.GetComponent<Button>().interactable = true;
        m_deleteStudentButton.GetComponent<Button>().interactable = true;
        m_saveStudentButton.GetComponent<Button>().interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;

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

    IEnumerator GetUserAllSessions()
    {
        EntityDBVitaProfile student = GetSelectedStudent();
        m_studentAllSessions = new List<EntityDBVitaStudentSession>();
        if (!IsNewStudent(student))
        {
            StopCoroutine(GetUserAllSessions());
        }

        bool waitingForSessions = true;
        dbSession.GetAllSessionsForUser(student.username, (sessions, error) =>
        {
            m_studentAllSessions = new List<EntityDBVitaStudentSession>(sessions);

            waitingForSessions = false;
        });

        while (waitingForSessions)
            yield return new WaitForEndOfFrame();
    }

    bool AnyChangesMade(EntityDBVitaProfile studentProfile)
    {
        if (m_usersOriginal.Count != m_users.Count)                     return true;
        EntityDBVitaProfile studentOriginalProfile = m_usersOriginal.Find(item => item.username == studentProfile.username);
        if (studentOriginalProfile.password != studentProfile.password) return true;
        if (studentOriginalProfile.name != studentProfile.name)         return true;
        return false;
    }

    IEnumerator BtnSaveChangesInternal()
    {
        EntityDBVitaProfile originalProfile = m_usersOriginal.Find(item => item.username == m_currentStudentProfile.username);
        if (originalProfile == null)
        {
            yield return StartCoroutine(BtnSaveChangesStudentNew(m_currentStudentProfile));
            m_usersOriginal.Add(new EntityDBVitaProfile(m_currentStudentProfile));
            VitaGlobalsUI.SortListAfterAddingNew<EntityDBVitaProfile>(m_users, m_usersOriginal, "username");
        }
        else
        {
            //yield return StartCoroutine(BtnSaveChangesStudentUsername(m_currentStudentProfile));
            yield return StartCoroutine(BtnSaveChangesStudentPassword(m_currentStudentProfile));
            originalProfile.password = m_currentStudentProfile.password;

            yield return StartCoroutine(BtnSaveChangesStudentInfo(m_currentStudentProfile));
            originalProfile.username = m_currentStudentProfile.username;
            originalProfile.organization = m_currentStudentProfile.organization;
            originalProfile.name = m_currentStudentProfile.name;
            originalProfile.type = m_currentStudentProfile.type;
            originalProfile.lastlogin = m_currentStudentProfile.lastlogin;
        }

        UpdateUIStates(m_currentStudentProfile);
    }

    //IEnumerator BtnSaveChangesStudentUsername(EntityDBVitaProfile studentProfile)
    //{
    //    Debug.LogWarningFormat("BtnSaveChanges() - changing student name - {0}", studentProfile.username);
    //    bool waitingForSave = true;
    //    DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
    //    dbUser.UpdateUserName(m_usersOriginal.Find(item => item.username == studentProfile.username).username, studentProfile.username, error =>
    //    {
    //        waitingForSave = false;
    //        if (!string.IsNullOrEmpty(error))
    //        {
    //            PopUpDisplay.Instance.Display("Error", string.Format("UpdateUserName() error: {0}", error));
    //            return;
    //        }
    //    });

    //    while (waitingForSave)
    //        yield return new WaitForEndOfFrame();
    //}

    IEnumerator BtnSaveChangesStudentPassword(EntityDBVitaProfile studentProfile)
    {
        Debug.LogWarningFormat("BtnSaveChanges() - changing student password - {0}", studentProfile.password);
        bool waitingForSave = true;
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.UpdateUserPassword(m_usersOriginal.Find(item => item.username == studentProfile.username).username, studentProfile.password, error =>
        {
            waitingForSave = false;
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateUserPassword() error: {0}", error));
                return;
            }
        });

        while (waitingForSave)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesStudentInfo(EntityDBVitaProfile studentProfile)
    {
        Debug.LogWarningFormat("BtnSaveChanges() - updating student with changes - {0}", studentProfile.username);
        bool waitingForSave = true;
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.UpdateUser(studentProfile.username, studentProfile.organization, studentProfile.name, (DBUser.AccountType)studentProfile.type, studentProfile.lastlogin, error =>
        {
            waitingForSave = false;
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateUser() error: {0}", error));
                return;
            }
        });

        while (waitingForSave)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesStudentNew(EntityDBVitaProfile studentProfile)
    {
            // look for new students added
            Debug.LogWarningFormat("BtnSaveChanges() - creating new student - {0}", studentProfile.username);
            bool waitingForSave = true;
            Debug.LogWarningFormat("BtnSaveChanges() - {0} {1} {2} {3} {4} {5}", studentProfile.username, studentProfile.password, studentProfile.organization, studentProfile.name, studentProfile.type, (DBUser.AccountType)studentProfile.type);
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.CreateUser(studentProfile.username, studentProfile.password, studentProfile.organization, studentProfile.name, (DBUser.AccountType)studentProfile.type, error =>
            {
                waitingForSave = false;
                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("CreateUser() error: {0}", error));
                    return;
                }
            });

        while (waitingForSave)
            yield return new WaitForEndOfFrame();
    }

    bool IsNewStudent(EntityDBVitaProfile student)
    {
        // has the student just been added, and the user hasn't hit 'Save' yet?
        return m_usersOriginal.Find(item => item.username == student.username) == null;
    }

    void UpdateUIStates(EntityDBVitaProfile studentProfile)
    {
        bool changesMade = AnyChangesMade(studentProfile);
        m_archiveStudentButton.GetComponent<Button>().interactable = !changesMade;
        m_deleteStudentButton.GetComponent<Button>().interactable = !changesMade;
        m_saveStudentButton.GetComponent<Button>().interactable = changesMade;
        VitaGlobalsUI.m_unsavedChanges = changesMade;

        //Update widget display to indicate changes
        Dropdown dropdown = m_studentListDropdown;
        VitaGlobalsUI.SetTextWithAsteriskIfChanges<Dropdown>(dropdown, dropdown.value, changesMade);
    }
}
