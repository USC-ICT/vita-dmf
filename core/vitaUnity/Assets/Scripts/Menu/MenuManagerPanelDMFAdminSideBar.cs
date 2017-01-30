using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelDMFAdminSideBar : MonoBehaviour
{
    internal Image m_organizationsArrow;
    internal Image m_archivesArrow;

    void Start()
    {
        m_organizationsArrow = GameObject.Find("ImageArrowOrganizations").GetComponent<Image>();
        m_archivesArrow = GameObject.Find("ImageArrowArchives").GetComponent<Image>();
        SetAllArrows(false);
    }

    public void SetArrow(Image m_arrow, bool m_enabled)
    {
        //m_arrow.enabled = m_enabled;
    }

    void SetAllArrows(bool m_enabled)
    {
        //SetArrow(m_organizationsArrow, m_enabled);
        //SetArrow(m_archivesArrow, m_enabled);
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
        //SetAllArrows(false);
        //SetArrow(m_organizationsArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DMFAdminHubAboutVita);
    }

    public void BtnArchives()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnArchivesInternal();
            }, null);
        }
        else
        {
            BtnArchivesInternal();
        }
    }

    void BtnArchivesInternal()
    {
        SetAllArrows(false);
        SetArrow(m_archivesArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DMFAdminHubArchive);
    }

    public void BtnOrganizations()
    {
        if (VitaGlobalsUI.m_unsavedChanges)
        {
            PopUpDisplay.Instance.DisplayYesNo(VitaGlobalsUI.m_unsavedChangesTitle, VitaGlobalsUI.m_unsavedChangesMessage, () =>
            {
                BtnOrganizationsInternal();
            }, null);
        }
        else
        {
            BtnOrganizationsInternal();
        }
    }

    void BtnOrganizationsInternal()
    {
        SetAllArrows(false);
        SetArrow(m_organizationsArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DMFAdminHub);
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
