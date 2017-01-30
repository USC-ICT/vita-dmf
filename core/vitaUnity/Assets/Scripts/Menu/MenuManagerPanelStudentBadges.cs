using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelStudentBadges : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    MenuManagerPanelStudentSideBar m_studentSideBar;

    GameObject m_headerName;
    GameObject m_badgesContent;
    GameObject m_badgeWidget;

    public GameObject m_badgesPrefab;
    GameObject m_badgesPrefabInstance;

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.StudentBadges);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");
        m_studentSideBar = VHUtils.FindChildRecursive(menu, "PanelStudentSideBarPrefab").GetComponent<MenuManagerPanelStudentSideBar>();

        m_badgesContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_Badges"), "Content");
        m_badgeWidget = VHUtils.FindChildRecursive(resources, "GuiButtonBadgesPrefab");
    }


    public void OnMenuEnter()
    {
        m_studentSideBar.SideBarOnMenuEnter();
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

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

        PopulateBadgesList();
    }

    public void OnMenuExit()
    {
        m_studentSideBar.SideBarOnMenuExit();
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
    #endregion

    void PopulateBadgesList()
    {
        VHUtils.DeleteChildren(m_badgesContent.transform);

        // skip badges that haven't been implemented yet
        List<string> unimplementedBadges = new List<string>()
        {
            "Tic-Tac-Toe",  // three scores of 5 in a row
            "Dress For Success",  // all assignments turned in on time this week
            "Bingo",  // earned a 5 on 5 assignments in a row
            "Moving Up",  // latest assignment score improved over previous
            "Practice Makes Perfect",  // Beat your previous top score
            "Golden Hello",  // Personal best interview responses score
            "Blue Chip Award",  // Personal best interview responses score
            "Branding Champ",  // Personal best self-promoting score
            "Communication Certification",  // Personal best active listening score
            "Closing Bell",  // Personal best Closing score
            "Class Act",  // Entire class's assignment scores improved
            "Worker Bee",  // Recognition of consistent hark work
            "Above and Beyond",  // Recognition of practice above and beyond class assignments
            "Promotion",  // Recognition of huge score improvements
            "Anger Management",  // First disposition unlocked
            "Mood Ring: Alex",  // Unlocked all dispositions for Alex
            "Mood Ring: Barbara",  // Unlocked all dispositions for Barbara
            "Mood Ring: George",  // Unlocked all dispositions for George
            "Mood Ring: Kevin",  // Unlocked all dispositions for Kevin
            "Mood Ring: Maria",  // Unlocked all dispositions for Maria
            "Mood Ring: Michelle",  // Unlocked all dispositions for Michelle
            "Office Party",  // First environment unlocked
            "Real-estate Developer",  // All environments unlocked
            "Company Picnic",  // First character unlocked
            "Gangs All Here",  // All characters unlocked
            "The Big 5",  // Five assignments completed
            "The Big 10",  // Ten assignments completed
            "Bronze 15",  // fifteen assignments completed
            "Silver 20",  // Twenty assignments completed
            "Golden 24",  // Twenty-four assignments completed
            "Consistency",  // Logged into system 10 business days in a row
        };

        AGBadge [] badges = m_badgesPrefabInstance.GetComponentsInChildren<AGBadge>();

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        foreach (AGBadge badge in badges)
        {
            if (unimplementedBadges.Contains(badge.m_badgeName))
                continue;

            if (dbUser.DoesUserHaveBadge(VitaGlobals.m_loggedInProfile, badge.m_badgeName))
                AddBadgeWidget(badge, true);
        }

        foreach (AGBadge badge in badges)
        {
            if (unimplementedBadges.Contains(badge.m_badgeName))
                continue;

            if (!dbUser.DoesUserHaveBadge(VitaGlobals.m_loggedInProfile, badge.m_badgeName))
                AddBadgeWidget(badge, false);
        }
    }

    /// <summary>
    /// Adds widget and sets all options on it based on AGBadge class
    /// </summary>
    /// <param name="m_badge"></param>
    void AddBadgeWidget(AGBadge badge, bool unlocked)
    {
        GameObject badgeObj = AddWidgetToList(m_badgeWidget, m_badgesContent, "BadgeWidget_" + badge.m_badgeName, "TextTitle", badge.m_badgeName);
        VHUtils.FindChildRecursive(badgeObj, "TextDescription").GetComponent<Text>().text = badge.m_badgeDescription;
        Image unlockedImage = VHUtils.FindChildRecursive(badgeObj, "ImageIconLeft").GetComponent<Image>();
        if (unlocked)
        {
            unlockedImage.sprite = badge.m_badgeIcon;
        }
        else
        {
            unlockedImage.sprite = badge.m_badgeLockedIcon;
            VHUtils.FindChildRecursive(badgeObj, "TextTitle").GetComponent<Text>().color = new Color(0.5f, 0.5f, 0.5f);
            VHUtils.FindChildRecursive(badgeObj, "TextDescription").GetComponent<Text>().color = new Color(0.5f, 0.5f, 0.5f);
            VHUtils.FindChildRecursive(badgeObj, "Progress").GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
            badgeObj.GetComponent<Image>().color = new Color(0.785f, 0.785f, 0.785f);
        }
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
}
