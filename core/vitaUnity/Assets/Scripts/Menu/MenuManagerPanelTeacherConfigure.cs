using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelTeacherConfigure : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    List<EntityDBVitaProfile> m_users;

    MenuManager m_menuManager;
    GameObject m_headerName;
    GameObject m_studentDrop;
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
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.TeacherConfigure);
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");
        m_studentDrop = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Student");
        m_interviewerDrop = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Interviewer");
        m_dispositionDrop = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Disposition");
        m_locationDrop = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Location");


        LoadUICharEmoteEnv(menu);
    }



    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        m_studentDrop.GetComponent<Dropdown>().ClearOptions();
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllStudentsInOrganization(loginOrganization, (users, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllClasses() error: {0}", error));
                return;
            }

            m_users = new List<EntityDBVitaProfile>();

            foreach (var user in users)
            {
                //Debug.LogFormat("{0} - {1} - {2} - {3} - {4}", user.username, user.password, user.organization, user.type, user.archived == 0 ? "Active" : "Archive");

                if (user.archived == 0)
                {
                    m_users.Add(user);
                    m_studentDrop.GetComponent<Dropdown>().AddOptions(new List<string>() { user.name } );
                }
            }
        });

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

    //This button disabled
    public void BtnHome()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    //This button disabled
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
        if (VitaGlobals.m_runMode == VitaGlobals.RunMode.SingleScreen)
        {
            string popupText = "Single screen practice sessions are only accessible through a Student's account. If you would like to continue, please log out and have your Student log in.";
            PopUpDisplay.Instance.Display("Warning", popupText, "Logout", "Cancel", false, delegate { Logout(); }, null);
            return;
        }

        string environmentName = m_locationDrop.GetComponent<Dropdown>().options[m_locationDrop.GetComponent<Dropdown>().value].text;
        int characterIdx = m_interviewerDrop.GetComponent<Dropdown>().value;
        int dispositionIdx = m_dispositionDrop.GetComponent<Dropdown>().value;

        string environment = VitaGlobals.m_vitaBackgroundInfo.Find(i => i.displayName == environmentName).sceneName;
        string character = VitaGlobals.m_vitaCharacterInfo[characterIdx].prefab;
        string disposition = VitaGlobals.m_vitaMoods[dispositionIdx];

        string studentDisplayName = m_studentDrop.GetComponent<Dropdown>().options[m_studentDrop.GetComponent<Dropdown>().value].text;
        string student = m_users.Find(i => i.name == studentDisplayName).username;

        NetworkRelay.SendNetworkMessage(string.Format("startinterview {0} {1} {2} {3} {4}", environment, character, disposition, student.Replace(" ", "~"), (int)VitaGlobals.InterviewType.Interview));
    }

    void Logout()
    {
        VitaGlobals.Logout((error) =>
        {
            GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
        });
    }
}
