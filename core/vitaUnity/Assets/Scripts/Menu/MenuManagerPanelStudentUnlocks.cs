using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelStudentUnlocks : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;

    GameObject m_headerName;
    MenuManagerPanelStudentSideBar m_studentSideBar;

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.StudentUnlocks);
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");
        m_studentSideBar = VHUtils.FindChildRecursive(menu, "PanelStudentSideBarPrefab").GetComponent<MenuManagerPanelStudentSideBar>();
    }


    public void OnMenuEnter()
    {
        m_studentSideBar.SideBarOnMenuEnter();
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;
    }

    public void OnMenuExit()
    {
        m_studentSideBar.SideBarOnMenuExit();
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
