using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelLocalAdminSideBar : MonoBehaviour
{
    internal Image m_teachersArrow;
    internal Image m_studentsArrow;
    internal Image m_classesArrow;
    internal Image m_studentArchivesArrow;
    internal Image m_teacherArchivesArrow;
    internal Image m_exportArrow;
    internal Image m_aboutArrow;

    void Start()
    {
        m_teachersArrow = GameObject.Find("ImageArrowTeachers").GetComponent<Image>();
        m_studentsArrow = GameObject.Find("ImageArrowStudents").GetComponent<Image>();
        m_classesArrow = GameObject.Find("ImageArrowClasses").GetComponent<Image>();
        m_studentArchivesArrow = GameObject.Find("ImageArrowStudentArchives").GetComponent<Image>();
        m_teacherArchivesArrow = GameObject.Find("ImageArrowTeacherArchives").GetComponent<Image>();
        m_exportArrow = GameObject.Find("ImageArrowExportRecords").GetComponent<Image>();
        m_aboutArrow = GameObject.Find("ImageArrowAboutVita").GetComponent<Image>();
        SetAllArrows(false);
    }

    public void SetArrow(Image m_arrow, bool m_enabled)
    {
        //m_arrow.enabled = m_enabled;
    }

    void SetAllArrows(bool m_enabled)
    {
        //SetArrow(m_teachersArrow, m_enabled);
        //SetArrow(m_studentsArrow, m_enabled);
        //SetArrow(m_classesArrow, m_enabled);
        //SetArrow(m_studentArchivesArrow, m_enabled);
        //SetArrow(m_teacherArchivesArrow, m_enabled);
        //SetArrow(m_exportArrow, m_enabled);
        //SetArrow(m_aboutArrow, m_enabled);
    }

    public void BtnAbout()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnAboutInternal();
            }, null);
        }
        else
        {
            BtnAboutInternal();
        }
    }

    void BtnAboutInternal()
    {
        Debug.LogWarning("BtnAbout()");
        SetAllArrows(false);
        SetArrow(m_aboutArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.LocalAdminHubAboutVita);
    }

    public void BtnClasses()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnClassesInternal();
            }, null);
        }
        else
        {
            BtnClassesInternal();
        }
    }

    void BtnClassesInternal()
    {
        SetAllArrows(false);
        SetArrow(m_classesArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.LocalAdminHubClasses);
    }

    public void BtnExportRecords()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnExportRecordsInternal();
            }, null);
        }
        else
        {
            BtnExportRecordsInternal();
        }
    }

    void BtnExportRecordsInternal()
    {
        SetAllArrows(false);
        SetArrow(m_exportArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.LocalAdminHubExport);
    }

    public void BtnLogout()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnLogoutInternal();
            }, null);
        }
        else
        {
            BtnLogoutInternal();
        }
    }

    void BtnLogoutInternal()
    {
        VitaGlobals.Logout((error) =>
        {
            GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
        });
    }

    public void BtnStudents()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnStudentsInternal();
            }, null);
        }
        else
        {
            BtnStudentsInternal();
        }
    }

    void BtnStudentsInternal()
    {
        SetAllArrows(false);
        SetArrow(m_studentsArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.LocalAdminHubStudents);
    }

    public void VtnStudentArchives()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                VtnStudentArchivesInternal();
            }, null);
        }
        else
        {
            VtnStudentArchivesInternal();
        }
    }

    void VtnStudentArchivesInternal()
    {
        SetAllArrows(false);
        SetArrow(m_studentArchivesArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.LocalAdminHubStudentArchives);
    }

    public void BtnTeachers()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnTeachersInternal();
            }, null);
        }
        else
        {
            BtnTeachersInternal();
        }
    }

    void BtnTeachersInternal()
    {
        SetAllArrows(false);
        SetArrow(m_teachersArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.LocalAdminHubTeachers);
    }

    public void BtnTeacherArchives()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnTeacherArchivesInternal();
            }, null);
        }
        else
        {
            BtnTeacherArchivesInternal();
        }
    }

    void BtnTeacherArchivesInternal()
    {
        SetAllArrows(false);
        SetArrow(m_teacherArchivesArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.LocalAdminHubTeacherArchives);
    }
}
