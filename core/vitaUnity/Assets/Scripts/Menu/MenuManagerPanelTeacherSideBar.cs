using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelTeacherSideBar : MonoBehaviour
{
    internal Image m_configureArrow;
    internal Image m_studentsArrow;
    internal Image m_classesArrow;
    internal Image m_reviewArrow;
    internal Image m_assignArrow;
    internal Image m_teacherProfileArrow;

    void Start()
    {
        m_configureArrow = GameObject.Find("ImageArrowConfigure").GetComponent<Image>();
        m_studentsArrow = GameObject.Find("ImageArrowStudents").GetComponent<Image>();
        m_classesArrow = GameObject.Find("ImageArrowClasses").GetComponent<Image>();
        m_reviewArrow = GameObject.Find("ImageArrowReview").GetComponent<Image>();
        m_assignArrow = GameObject.Find("ImageArrowAssign").GetComponent<Image>();
        m_teacherProfileArrow = GameObject.Find("ImageArrowProfile").GetComponent<Image>();
        SetAllArrows(false);
    }

    public void SetArrow(Image m_arrow, bool m_enabled)
    {
        //m_arrow.enabled = m_enabled;
    }

    void SetAllArrows(bool m_enabled)
    {
        //SetArrow(m_configureArrow, m_enabled);
        //SetArrow(m_studentsArrow, m_enabled);
        //SetArrow(m_classesArrow, m_enabled);
        //SetArrow(m_reviewArrow, m_enabled);
        //SetArrow(m_assignArrow, m_enabled);
        //SetArrow(m_teacherProfileArrow, m_enabled);
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
        //Debug.Log("BtnConfigure()");
        //SetAllArrows(false);
        //SetArrow(m_configureArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherAbout);
    }

    public void BtnConfigure()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnConfigureInternal();
            }, null);
        }
        else
        {
            BtnConfigureInternal();
        }
    }

    void BtnConfigureInternal()
    {
        //Debug.Log("BtnConfigure()");
        SetAllArrows(false);
        SetArrow(m_configureArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherConfigure);
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
        //Debug.Log("BtnStudents()");
        SetAllArrows(false);
        SetArrow(m_studentsArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherStudents);
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
        //Debug.Log("BtnClasses()");
        SetAllArrows(false);
        SetArrow(m_classesArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherClasses);
    }

    public void BtnReview()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnReviewInternal();
            }, null);
        }
        else
        {
            BtnReviewInternal();
        }
    }

    void BtnReviewInternal()
    {
        //Debug.Log("BtnReview()");
        SetAllArrows(false);
        SetArrow(m_reviewArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherReview);
    }

    public void BtnAssign()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnAssignInternal();
            }, null);
        }
        else
        {
            BtnAssignInternal();
        }
    }

    void BtnAssignInternal()
    {
        SetAllArrows(false);
        SetArrow(m_assignArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherAssignments);
    }

    public void BtnTeacherProfile()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnTeacherProfileInternal();
            }, null);
        }
        else
        {
            BtnTeacherProfileInternal();
        }
    }

    void BtnTeacherProfileInternal()
    {
        //Debug.Log("BtnTeacherProfile()");
        SetAllArrows(false);
        SetArrow(m_teacherProfileArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherProfile);
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
}
