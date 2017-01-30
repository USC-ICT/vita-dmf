using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelInterviewIntervieweeQuestions : MonoBehaviour, MenuManager.IMenuManagerInterface
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

    public void BtnBuyTime()
    {
        Debug.Log("BtnBuyTime()");
    }

    public void BtnClosePopup()
    {
        Debug.Log("BtnClosePopup()");
    }
}
