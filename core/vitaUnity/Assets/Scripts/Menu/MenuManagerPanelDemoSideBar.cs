using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelDemoSideBar : MonoBehaviour
{
    //GameObject m_configureArrow;
    //GameObject m_unlocksArrow;
    //GameObject m_badgesArrow;
    //GameObject m_aboutArrow;

    //void Start()
    //{
    //    m_configureArrow = VHUtils.FindChild(this.gameObject, "GuiButtonPrefab_ArrowConfigure");
    //    m_unlocksArrow = VHUtils.FindChild(this.gameObject, "GuiButtonPrefab_ArrowUnlocks");
    //    m_badgesArrow = VHUtils.FindChild(this.gameObject, "GuiButtonPrefab_ArrowBadges");
    //    m_aboutArrow = VHUtils.FindChild(this.gameObject, "GuiButtonPrefab_ArrowAbout");
    //    SetAllArrows(false);
    //}

    void SetArrow(GameObject m_arrow, bool m_enabled)
    {
    //    m_arrow.SetActive(m_enabled);
    }

    void SetAllArrows(bool m_enabled)
    {
    //    SetArrow(m_configureArrow, m_enabled);
    //    SetArrow(m_unlocksArrow, m_enabled);
    //    SetArrow(m_badgesArrow, m_enabled);
    //    SetArrow(m_aboutArrow, m_enabled);
    }

    public void BtnPracticeSession()
    {
        //Joe not using this 9/28; I think pratice/configure butotn launches config screen, then pratice screen is reached from the launch buttun there
        //Debug.Log("Joe not using this 9/28; I think pratice/configure butotn launches config screen, then pratice screen is reached from the launch buttun there");
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DemoConfigure);
    }

    public void BtnUnlocks()
    {
        SetAllArrows(false);
        //SetArrow(m_unlocksArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DemoUnlocks);
    }

    public void BtnBadges()
    {
        SetAllArrows(false);
        //SetArrow(m_badgesArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DemoBadges);
    }

    public void BtnAbout()
    {
        SetAllArrows(false);
        //SetArrow(m_aboutArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DemoAbout);
    }

    public void BtnConfigure()
    {
        SetAllArrows(false);
        //SetArrow(m_configureArrow, true);
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DemoConfigure);
    }

    public void BtnLogout()
    {
        VitaGlobals.m_loggedInProfile = null;

        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }
}
