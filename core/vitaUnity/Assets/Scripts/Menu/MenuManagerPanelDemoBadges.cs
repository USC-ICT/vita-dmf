using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelDemoBadges : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    GameObject m_headerName;
    GameObject m_badgesContent;
    GameObject m_badgeWidget;

    public GameObject m_badgesPrefab;
    GameObject m_badgesPrefabInstance;

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.DemoBadges);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_headerName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Names");

        m_badgesContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_Badges"), "Content");
        m_badgeWidget = VHUtils.FindChildRecursive(resources, "GuiButtonBadgesPrefab");
    }


    public void OnMenuEnter()
    {
        string loginName = "Demo User";
        string loginOrganization = "Demo Organization";
        if (VitaGlobals.m_loggedInProfile != null)
        {
            loginName = VitaGlobals.m_loggedInProfile.name;
            loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        }
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;
        m_badgesPrefabInstance = Instantiate(m_badgesPrefab);

        PopulateBadgesList();
    }

    public void OnMenuExit()
    {
        Destroy(m_badgesPrefabInstance);
        m_badgesPrefabInstance = null;
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

        foreach (AGBadge badge in m_badgesPrefabInstance.GetComponentsInChildren<AGBadge>())
        {
            AddBadgeWidget(badge);
        }
    }

    /// <summary>
    /// Adds widget and sets all options on it based on AGBadge class
    /// </summary>
    /// <param name="m_badge"></param>
    void AddBadgeWidget(AGBadge m_badge)
    {
        GameObject badge = AddWidgetToList(m_badgeWidget, m_badgesContent, "BadgeWidget_" + m_badge.m_badgeName, "TextTitle", m_badge.m_badgeName);
        VHUtils.FindChildRecursive(badge, "TextDescription").GetComponent<Text>().text = m_badge.m_badgeDescription;
        Image unlockedImage = VHUtils.FindChildRecursive(badge, "ImageIconLeft").GetComponent<Image>();
        if (m_badge.m_badgeIsUnlocked)
        {
            unlockedImage.sprite = m_badge.m_badgeIcon;
        }
        else
        {
            unlockedImage.sprite = m_badge.m_badgeLockedIcon;
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
