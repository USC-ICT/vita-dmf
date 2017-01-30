﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelDemoConfigure : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    GameObject m_headerName;
    GameObject m_interviewerDrop;
    GameObject m_dispositionDrop;
    GameObject m_locationDrop;


    List<GameObject> m_Image_Char_List    = new List<GameObject>(); 
    List<GameObject> m_Image_Emote_List   = new List<GameObject>();
    List<GameObject> m_Image_Env_List     = new List<GameObject>();


    List<string> CharList  = new List<string> {"Image_CharAlex", "Image_CharBarbara", "Image_CharGeorge","Image_CharKevin","Image_CharMaria","Image_CharMichelle"};
    List<string> EmoteList = new List<string> {"Image_Emote01", "Image_Emote02","Image_Emote03"};
    List<string> EnvList   = new List<string> {"Image_EnvBreakRoom", "Image_EnvConferenceRoom","Image_EnvExecutiveOffice","Image_EnvHotelLobby","Image_EnvManagerOffice","Image_EnvRestaurant", "Image_EnvWarehouseOffice"};

    void LoadUICharEmoteEnv(GameObject menu)
    {
       
        foreach(var item in CharList)
            m_Image_Char_List.Add(VHUtils.FindChildRecursive(menu, item));

        foreach(var item in EmoteList)
            m_Image_Emote_List.Add(VHUtils.FindChildRecursive(menu, item));

        foreach(var item in EnvList)
            m_Image_Env_List.Add(VHUtils.FindChildRecursive(menu, item));

        OnInterviewerChanged(m_interviewerDrop.GetComponent<Dropdown>().value);
        OnDispositionChanged(m_dispositionDrop.GetComponent<Dropdown>().value);
        OnLocationChanged(m_locationDrop.GetComponent<Dropdown>().value);  

        m_interviewerDrop.GetComponent<Dropdown>().onValueChanged.AddListener(OnInterviewerChanged);
        m_dispositionDrop.GetComponent<Dropdown>().onValueChanged.AddListener(OnDispositionChanged);
        m_locationDrop.GetComponent<Dropdown>().onValueChanged.AddListener(OnLocationChanged);
            
    }


    void OnInterviewerChanged(int inx)
    {       
        m_Image_Char_List.ForEach(o => o.SetActive(false));
        m_Image_Char_List[inx].SetActive(true);
    }

    void OnDispositionChanged(int inx)
    { 
        m_Image_Emote_List.ForEach( o => o.SetActive(false));
        m_Image_Emote_List[inx].SetActive(true);
    }

    void OnLocationChanged(int inx)
    {
        m_Image_Env_List.ForEach(o=>o.SetActive(false));
        m_Image_Env_List[inx].SetActive(true);
    }


    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.DemoConfigure);
        m_headerName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Names");

        m_interviewerDrop = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Interviewer");
        m_dispositionDrop = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Disposition");
        m_locationDrop = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Location");

        LoadUICharEmoteEnv(menu);
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

        var displayList = new List<string>();
        foreach (var i in VitaGlobals.m_vitaCharacterInfo)
            displayList.Add(i.displayName);

        m_interviewerDrop.GetComponent<Dropdown>().ClearOptions();
        m_interviewerDrop.GetComponent<Dropdown>().AddOptions(displayList);

        var dispositionList = new List<string>();
        foreach (var i in VitaGlobals.m_vitaMoods)
            dispositionList.Add(i);

        m_dispositionDrop.GetComponent<Dropdown>().ClearOptions();
        m_dispositionDrop.GetComponent<Dropdown>().AddOptions(dispositionList);
        m_dispositionDrop.GetComponent<Dropdown>().value = 1;

        var locationList = new List<string>();
        foreach (var i in VitaGlobals.m_vitaBackgroundInfo)
        {
            if (i.shownInMenu)
                locationList.Add(i.displayName);
        }

        m_locationDrop.GetComponent<Dropdown>().ClearOptions();
        m_locationDrop.GetComponent<Dropdown>().AddOptions(locationList);
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

    public void OnValueChangedInterviewer(int value)
    {
        // enable/disable image
    }

    public void OnValueChangedDisposition(int value)
    {
        // enable/disable image
    }

    public void OnValueChangedLocation(int value)
    {
        // enable/disable image
    }

    public void BtnLaunch()
    {
        int environmentIdx = m_locationDrop.GetComponent<Dropdown>().value;
        int characterIdx = m_interviewerDrop.GetComponent<Dropdown>().value;
        int dispositionIdx = m_dispositionDrop.GetComponent<Dropdown>().value;

        string environment = VitaGlobals.m_vitaBackgroundInfo[environmentIdx].sceneName;
        string character = VitaGlobals.m_vitaCharacterInfo[characterIdx].prefab;
        string disposition = VitaGlobals.m_vitaMoods[dispositionIdx];

        string student = "DEMO";

        NetworkRelay.SendNetworkMessage(string.Format("startinterview {0} {1} {2} {3} {4}", environment, character, disposition, student, (int)VitaGlobals.InterviewType.Demo));
    }
}
