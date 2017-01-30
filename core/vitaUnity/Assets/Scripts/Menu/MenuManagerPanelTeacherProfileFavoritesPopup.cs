using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelTeacherProfileFavoritesPopup : MonoBehaviour, MenuManager.IMenuManagerInterface
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


    public void BtnClose()
    {
        Debug.Log("BtnClose()");
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.TeacherProfile);
    }
}
