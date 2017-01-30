using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelDMFAdminAbout : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    GameObject m_headerName;
    GameObject m_version;


    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.DMFAdminHubAboutVita);
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab (2)");
        m_version = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_VersionNumber");
    }


    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;
        m_version.GetComponent<Text>().text = VitaGlobals.m_version;
    }

    public void OnMenuExit()
    {
    }


    public void BtnHome()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnBack()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }
}
