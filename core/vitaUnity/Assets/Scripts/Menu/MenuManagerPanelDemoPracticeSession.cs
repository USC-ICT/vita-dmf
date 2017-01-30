using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelDemoPracticeSession : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    void Start()
    {
    }


    public void OnMenuEnter()
    {
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

    public void BtnRepeatQuestion()
    {
        Debug.Log("BtnRepeatQuestion()");
    }

    public void BtnNextQuestion()
    {
        Debug.Log("BtnNextQuestion()");
    }

    public void BtnCompleteSession()
    {
        Debug.Log("BtnCompleteSession()");
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.DemoConfigure);
    }
}
