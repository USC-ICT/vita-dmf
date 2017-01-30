using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelStudentSideBar : MonoBehaviour
{
    DBSession m_dbSession;

    //Assignments
    Text m_assNum;
    Text m_assLate;
    Text m_assSumbitted;
    Text m_assGraded;
    Text m_assNewStatus;
    Text m_assLateStatus;
    Text m_assGradedStatus;
    GameObject m_assignmentsLoading;
    string m_assNewStatusString = " New assignment received";
    string m_assLateStatusString = " Assignment is now late";
    string m_assGradedStatusString = " Assignment graded";
    string m_assNewStatusStringPlural = " New assignments received";
    string m_assLateStatusStringPlural = " Assignments are now late";
    string m_assGradedStatusStringPlural = " Assignments graded";

    //MIAS
    Text m_miasSessionName;
    Text m_firstImpressionScore;
    Text m_interviewResponsesScore;
    Text m_selfPromotingScore;
    Text m_activeListeningScore;
    Text m_closingScore;
    GameObject m_miasLoading;

    //Unlocks
    GameObject m_unlocksLoading;
    GameObject m_unlocksProgress;

    //Badges
    public GameObject m_badgesPrefab;
    GameObject m_badgesPrefabInstance;
    GameObject m_badgesLoading;
    GameObject m_badgesProgress;
    GameObject m_badgesMessagesContent;
    
    //Resources
    GameObject m_badgeMessagePrefab;

    #region Menu Init
    void Start()
    {
        GameObject menu = this.gameObject;

        //Assignments
        m_assignmentsLoading = VHUtils.FindChildRecursive(menu, "IconLoading_Assignments");
        m_assNum = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssNum").GetComponent<Text>();
        m_assLate = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssLate").GetComponent<Text>();
        m_assSumbitted = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssSubmitted").GetComponent<Text>();
        m_assGraded = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssGraded").GetComponent<Text>();
        m_assNewStatus = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssNewStatus").GetComponent<Text>();
        m_assLateStatus = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssLateStatus").GetComponent<Text>();
        m_assGradedStatus = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssGradedStatus").GetComponent<Text>();

        //MIAS
        m_miasSessionName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_MIASSessionName").GetComponent<Text>();
        m_firstImpressionScore = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_FirstImpressionScore").GetComponent<Text>();
        m_interviewResponsesScore = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_InterviewResponseScore").GetComponent<Text>();
        m_selfPromotingScore = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_SelfPromotingScore").GetComponent<Text>();
        m_activeListeningScore = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_ActiveListeningScore").GetComponent<Text>();
        m_closingScore = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_ClosingScore").GetComponent<Text>();
        m_miasLoading = VHUtils.FindChildRecursive(menu, "IconLoading_MIAS");

        //Unlocks
        m_unlocksLoading = VHUtils.FindChildRecursive(menu, "IconLoading_Unlocks");
        m_unlocksProgress = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiProgressBarPrefab_Unlocks"), "Progress");

        //Badges
        m_badgesLoading = VHUtils.FindChildRecursive(menu, "IconLoading_Badges");
        m_badgesProgress = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiProgressBarPrefab_Badges"), "Progress");
        m_badgesMessagesContent = VHUtils.FindChildRecursive(menu, "ContentBadges");

        //Resources
        GameObject resources = VHUtils.FindChildRecursive(menu, "SideBarResources");
        m_badgeMessagePrefab = VHUtils.FindChildRecursive(resources, "GuiBadgeSideBarMessagePrefab");
    }

    public void SideBarOnMenuEnter()
    {
        //Get badges prefab
        if (m_badgesPrefabInstance == null)
        {
            //Get by component
            if (GameObject.FindObjectOfType<AGBadges>() != null)
            {
                m_badgesPrefabInstance = GameObject.FindObjectOfType<AGBadges>().gameObject;
            }

            //Create new if none found
            if (m_badgesPrefabInstance == null)
            {
                m_badgesPrefabInstance = Instantiate(m_badgesPrefab);
            }
        }

        //Clear menu
        m_assNum.text = "--";
        m_assLate.text = "--";
        m_assLate.text = "--";
        m_assGraded.text = "--";
        m_assNewStatus.text = "--" + m_assNewStatusString;
        m_assLateStatus.text = "--" + m_assLateStatusString;
        m_assGradedStatus.text = "--" + m_assGradedStatusString;
        m_miasSessionName.text = "Session ##";
        m_firstImpressionScore.text = "--";
        m_interviewResponsesScore.text = "--";
        m_selfPromotingScore.text = "--";
        m_activeListeningScore.text = "--";
        m_closingScore.text = "--";
        m_assNewStatus.gameObject.SetActive(false);
        m_assLateStatus.gameObject.SetActive(false);
        m_assGradedStatus.gameObject.SetActive(false);
        VHUtils.DeleteChildren(m_badgesMessagesContent.transform);

        //Assignments
        GetAssignments();

        //MIAS
        GetLastSessionScore();

        //Unlocks
        GetUnlocks();

        //Badges
        GetBadges();
    }

    public void SideBarOnMenuExit()
    {
    }

    void GetAssignments()
    {
        m_assignmentsLoading.SetActive(true);
        int assNum = 0;
        int assLate = 0;
        int assSubmitted = 0;
        int assGraded = 0;
        int assNewStatus = 0;
        int assNewGradedStatus = 0;

        DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
        dbAssignment.GetAllAssignmentsForStudent(VitaGlobals.m_loggedInProfile.username, VitaGlobals.m_loggedInProfile.organization, (assignments, error) =>
        {
            //Count assignments by status
            foreach (var assignment in assignments)
            {
                if (assignment.status == (int)DBAssignment.Status.ASSIGNED)
                {
                    if (VitaGlobals.TicksToDateTime(assignment.datedue) < DateTime.Now)
                    {
                        assLate++;
                    }
                    else
                    {
                        assNum++;
                        //Newly assigned assignments are considered 1 day old
                        if (VitaGlobals.TicksToDateTime(assignment.createdate) > DateTime.Today.AddDays(-1))
                        {
                            assNewStatus++;
                        }
                    }
                }
                else if (assignment.status == (int)DBAssignment.Status.SUBMITTED)
                {
                    assSubmitted++;
                }
                else if (assignment.status == (int)DBAssignment.Status.GRADED)
                {
                    assGraded++;
                    assNewGradedStatus++;
                }
            }

            m_assNum.text = assNum.ToString();
            m_assLate.text = assLate.ToString();
            m_assSumbitted.text = assSubmitted.ToString();
            m_assGraded.text = assGraded.ToString();

            //Single/plural messages
            m_assNewStatus.text = assNewStatus.ToString() + ((assNewStatus > 1) ? m_assNewStatusStringPlural : m_assNewStatusString);
            m_assLateStatus.text = assLate.ToString() + ((assLate > 1) ? m_assLateStatusStringPlural : m_assLateStatusString);
            m_assGradedStatus.text = assNewGradedStatus.ToString() + ((assNewGradedStatus > 1) ? m_assGradedStatusStringPlural : m_assGradedStatusString);

            //Disable if zero
            if (assNewStatus > 0) m_assNewStatus.gameObject.SetActive(true);
            if (assLate > 0) m_assLateStatus.gameObject.SetActive(true);
            if (assNewGradedStatus > 0) m_assGradedStatus.gameObject.SetActive(true);

            m_assignmentsLoading.SetActive(false);
        });
    }

    void GetBadges()
    {
        int totalBadges = 0;
        int userHasNumBadges = 0;

        m_badgesLoading.SetActive(true);
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        foreach (AGBadge badge in m_badgesPrefabInstance.GetComponentsInChildren<AGBadge>())
        {
            totalBadges++;
            if (dbUser.DoesUserHaveBadge(VitaGlobals.m_loggedInProfile, badge.m_badgeName))
                userHasNumBadges++;
        }

        //Set progress bar
        m_badgesProgress.transform.localScale = new Vector3(((float)userHasNumBadges / (float)totalBadges), m_badgesProgress.transform.localScale.y, m_badgesProgress.transform.localScale.z);
        
        //Get last few badges and display
        for (int i = 1; i < 6 && i < VitaGlobals.m_loggedInProfile.badges.Count; i++)
        {
            string badgeString = VitaGlobals.m_loggedInProfile.badges[VitaGlobals.m_loggedInProfile.badges.Count - i];
            DBUser.BadgeData badgeData = DBUser.BadgeData.ToBadge(badgeString);

            foreach (AGBadge badge in m_badgesPrefabInstance.GetComponentsInChildren<AGBadge>())
            {
                if (badgeData.BadgeId == badge.m_badgeName)
                {
                    GameObject badgeMessageWidget = AddWidgetToList(m_badgeMessagePrefab, m_badgesMessagesContent, m_badgeMessagePrefab.name + "_" + badge.m_badgeName, "Text", string.Format("'{0}' Unlocked", badge.m_badgeName));
                    VHUtils.FindChildRecursive(badgeMessageWidget, "Icon").GetComponent<Image>().sprite = badge.m_badgeIcon;
                    break;
                }
            }
        }

        m_badgesLoading.SetActive(false);
    }

    /// <summary>
    /// Fills MIAS score info
    /// </summary>
    void GetLastSessionScore()
    {
        m_miasLoading.SetActive(true);
        m_dbSession = GameObject.FindObjectOfType<DBSession>();
        EntityDBVitaProfile userProfile = VitaGlobals.m_loggedInProfile;

        m_dbSession.GetAllSessions((sessions, error) =>
        {
            foreach (var session in sessions)
            {
                if (session.username != userProfile.username) continue;
                if (session.sessionname == "GLOBAL") continue;

                foreach (string eventString in session.events)
                {
                    DBSession.EventData eventData = DBSession.EventData.ToEvent(eventString);
                    if (eventData.type == DBSession.EventType.ScoreNoteFinal)
                    {
                        m_miasSessionName.text = CreateNiceSessionName(eventData.timeStamp.ToString());
                        m_firstImpressionScore.text = eventData.miasScores[0] == -1 ? "--" : eventData.miasScores[0].ToString();
                        m_interviewResponsesScore.text = eventData.miasScores[1] == -1 ? "--" : eventData.miasScores[1].ToString();
                        m_selfPromotingScore.text = eventData.miasScores[2] == -1 ? "--" : eventData.miasScores[2].ToString();
                        m_activeListeningScore.text = eventData.miasScores[3] == -1 ? "--" : eventData.miasScores[3].ToString();
                        m_closingScore.text = eventData.miasScores[4] == -1 ? "--" : eventData.miasScores[4].ToString();
                        m_miasLoading.SetActive(false);
                        break;
                    }
                }
            }
            m_miasLoading.SetActive(false); //If this user doesn't have any sessions, the loading can still exit
        });
    }

    void GetUnlocks()
    {
        m_unlocksLoading.SetActive(true);
        m_unlocksProgress.transform.localScale = new Vector3(((float)0 / (float)1), m_unlocksProgress.transform.localScale.y, m_unlocksProgress.transform.localScale.z);
        m_unlocksLoading.SetActive(false);
    }
    #endregion

    #region UI Hooks
    public void BtnAbout()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.StudentAboutVita);
    }

    public void BtnAssignments()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.StudentAssignments);
    }

    public void BtnPracticeSession()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.StudentSessionConfiguration);
    }

    public void BtnMias()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.StudentMias);
    }

    public void BtnUnlocks()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.StudentUnlocks);
    }

    public void BtnBadges()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.StudentBadges);
    }

    public void BtnLogout()
    {
        Destroy(m_badgesPrefabInstance);
        m_badgesPrefabInstance = null;

        VitaGlobals.Logout((error) =>
        {
            GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
        });
    }
    #endregion

    #region Private Functions
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
    #endregion
}
