using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelLocalAdminExport : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;

    GameObject m_headerName;
    GameObject m_classListContent;
    GameObject m_studentListContent;
    GameObject m_dataListContent;
    GameObject m_sessionListContent;


    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.LocalAdminHubExport);

        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab (2)");
    }


    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.LocalAdminHubExport);
        m_classListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Scroll View Classes"), "Content");
        m_studentListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Scroll View Teachers"), "Content");
        m_dataListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Scroll View Teachers (1)"), "Content");
        m_sessionListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Scroll View Teachers (2)"), "Content");

        VHUtils.DeleteChildren(m_classListContent.transform);
        VHUtils.DeleteChildren(m_studentListContent.transform);
        VHUtils.DeleteChildren(m_dataListContent.transform);
        VHUtils.DeleteChildren(m_sessionListContent.transform);
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

    public void BtnDeselectAll()
    {
        Debug.Log("BtnDeselectAll()");
    }

    public void BtnExportRecord()
    {
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;

        DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
        dbProfile.ExportStudentsDataByOrganization(loginOrganization, (studentsCSV, sessionsCSV, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("ExportStudentsDataByOrganization() error: {0}", error));
                return;
            }

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string studentsFilename = string.Format("{0}\\{1}_{2}_students.csv", folder, loginOrganization, date);
            string sessionsFilename = string.Format("{0}\\{1}_{2}_sessions.csv", folder, loginOrganization, date);

            System.IO.File.WriteAllText(studentsFilename, studentsCSV);
            System.IO.File.WriteAllText(sessionsFilename, sessionsCSV);

            string text = string.Format("Data was successfully exported.  You will find the files here:\n{0}\n{1}\n\n Hit OK to bring up the folder.", studentsFilename, sessionsFilename);

            PopUpDisplay.Instance.DisplayOkCancel("Export Success", text, delegate { System.Diagnostics.Process.Start(folder); }, null);
        });
    }

    public void BtnSelectAll()
    {
        Debug.Log("BtnSelectAll()");
    }
}
