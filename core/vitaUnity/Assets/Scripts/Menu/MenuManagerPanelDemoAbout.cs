using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelDemoAbout : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    GameObject m_headerName;
    GameObject m_version;


    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.DemoAbout);
        m_headerName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Names");
        m_version = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_VersionNumber");
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
