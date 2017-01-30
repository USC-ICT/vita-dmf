using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManagerPanelLocalAdminTeacherArchives : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    List<EntityDBVitaProfile> m_users;
    List<EntityDBVitaClass> m_classes;

    GameObject m_headerName;
    GameObject m_name;
    GameObject m_userName;
    GameObject m_password;
    GameObject m_dateCreated;
    GameObject m_lastLogin;

    GameObject m_teacherListSelectedItem;
    GameObject m_teacherListItem;
    GameObject m_teacherListContent; 
    GameObject m_classListContent;
    GameObject m_classListItem;
    ToggleGroup m_teacherListToggleGrp;

    //UI
    GameObject m_teachersLoadIcon;
    GameObject m_classesLoadIcon;
    Button m_reinstateTeacherBtn;
    GameObject m_reinstateTeacherLoadIcon;
    Button m_deleteTeacherBtn;
    GameObject m_deleteTeacherLoadIcon;

    #region Menu Init
    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.LocalAdminHubTeacherArchives);
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab (2)");

        m_name = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Name");
        m_userName = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_UserName");
        m_password = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Password");
        m_dateCreated = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_DateCreated");
        m_lastLogin = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_LastLogin");

        //Resources
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_teacherListItem = VHUtils.FindChildRecursive(resources, "GuiTogglePrefab_Teacher");
        m_teacherListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_ArchivedTeachers"), "Content");
        m_teacherListToggleGrp = VHUtils.FindChildRecursive(menu, "TeacherListToggleGrp").GetComponent<ToggleGroup>();
        m_classListItem = VHUtils.FindChildRecursive(resources, "GuiTextPrefab_Class");
        m_classListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_Classes"), "Content");

        //UI
        m_teachersLoadIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_Teachers");
        m_classesLoadIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_Classes");
        m_reinstateTeacherBtn = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_ReinstateTeacher").GetComponent<Button>();
        m_reinstateTeacherLoadIcon = VHUtils.FindChildRecursive(m_reinstateTeacherBtn.gameObject, "LoadingIcon");
        m_deleteTeacherBtn = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_DeleteTeacher").GetComponent<Button>();
        m_deleteTeacherLoadIcon = VHUtils.FindChildRecursive(m_deleteTeacherBtn.gameObject, "LoadingIcon");
    }

    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        m_teachersLoadIcon.SetActive(false);
        m_classesLoadIcon.SetActive(false);
        m_reinstateTeacherBtn.interactable = true;
        m_reinstateTeacherLoadIcon.SetActive(false);
        m_deleteTeacherBtn.interactable = true;
        m_deleteTeacherLoadIcon.SetActive(false);
        RefreshTeacherList();
    }

    public void OnMenuExit()
    {
    }
    #endregion

    #region UI Hooks
    public void BtnHome()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnBack()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnDeleteTeacher()
    {
        PopUpDisplay.Instance.DisplayYesNo(VitaGlobals.Confirm, "Delete teacher? " + VitaGlobals.CannotBeUndone, DeleteTeacher, null);
    }

    public void DeleteTeacher()
    {
        if (m_teacherListSelectedItem == null)
            return;

        string selectedName = m_teacherListSelectedItem.GetComponentInChildren<Text>().text;

        string selectedUsername = m_users.Find(user => user.name == selectedName).username;

        m_reinstateTeacherBtn.interactable = false;
        m_deleteTeacherBtn.interactable = false;
        m_deleteTeacherLoadIcon.SetActive(true);
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.DeleteUser(selectedUsername, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("DeleteUser() error: {0}", error));
                return;
            }

            m_deleteTeacherLoadIcon.SetActive(false);
            RefreshTeacherList();
        });
    }

    public void BtnReinstateTeacher()
    {
        if (m_teacherListSelectedItem == null)
            return;

        string selectedName = m_teacherListSelectedItem.GetComponentInChildren<Text>().text;

        string selectedUsername = m_users.Find(user => user.name == selectedName).username;

        m_reinstateTeacherBtn.interactable = false;
        m_deleteTeacherBtn.interactable = false;
        m_reinstateTeacherLoadIcon.SetActive(true);
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.ReinstateUser(selectedUsername, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("ReinstateUser() error: {0}", error));
                return;
            }

            m_reinstateTeacherLoadIcon.SetActive(false);
            RefreshTeacherList();
        });
    }

    public void OnClickTeacherListItem()
    {
        m_teacherListSelectedItem = EventSystem.current.currentSelectedGameObject;
        string selectedName = m_teacherListSelectedItem.GetComponentInChildren<Text>().text;

        //Debug.LogFormat("MenuManagerPanelLocalAdminTeacherArchives.TeacherListItemOnClick() - {0}", selectedName);

        VHUtils.DeleteChildren(m_classListContent.transform);

        foreach (var user in m_users)
        {
            if (selectedName == user.name)
            {
                m_name.GetComponent<InputField>().text = user.name;
                m_userName.GetComponent<InputField>().text = user.username;
                m_password.GetComponent<InputField>().text = user.password;
                m_dateCreated.GetComponent<InputField>().text = VitaGlobals.TicksToString(user.createdate);
                m_lastLogin.GetComponent<InputField>().text = VitaGlobals.TicksToString(user.lastlogin);

                foreach (var c in m_classes)
                {
                    if (c.teacher == user.username)
                    {
                        GameObject classWidget = VitaGlobalsUI.AddWidgetToList(m_classListItem, m_classListContent, m_classListItem.name + "_" + c.classname, null, c.classname);
                        classWidget.GetComponent<Text>().text = c.classname;
                    }
                }

                break;
            }
        }
    }
    #endregion

    void RefreshTeacherList() { StartCoroutine(RefreshTeacherListInternal()); }
    IEnumerator RefreshTeacherListInternal()
    {
        string selectedName = "";
        if (m_teacherListSelectedItem != null)
            selectedName = m_teacherListSelectedItem.GetComponentInChildren<Text>().text;

        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;

        m_teacherListSelectedItem = null;
        VHUtils.DeleteChildren(m_teacherListContent.transform);
        VHUtils.DeleteChildren(m_classListContent.transform);
        m_name.GetComponent<InputField>().text = "";
        m_userName.GetComponent<InputField>().text = "";
        m_password.GetComponent<InputField>().text = "";
        m_dateCreated.GetComponent<InputField>().text = "";
        m_lastLogin.GetComponent<InputField>().text = "";

        //Get classes
        yield return StartCoroutine(GetClasses());

        m_reinstateTeacherBtn.interactable = false;
        m_deleteTeacherBtn.interactable = false;
        m_teachersLoadIcon.SetActive(true);

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllTeachersInOrganization(loginOrganization, (users, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllUsers() error: {0}", error));
                return;
            }

            m_users = new List<EntityDBVitaProfile>();

            foreach (var teacher in users)
            {
                //Debug.LogFormat("{0} - {1} - {2} - {3} - {4} - {5}", user.username, user.password, user.organization, user.type, user.name, user.archived == 0 ? "Active" : "Archived" );

                if (teacher.archived == 1)
                {
                    m_users.Add(teacher);
                    GameObject newTeacher = VitaGlobalsUI.AddWidgetToList(m_teacherListItem, m_teacherListContent, m_teacherListItem.name + "_" + teacher.name, "Label", teacher.name);
                    Toggle newTeacherToggle = newTeacher.GetComponent<Toggle>();
                    newTeacherToggle.group = m_teacherListToggleGrp;
                    newTeacherToggle.onValueChanged.AddListener(delegate { OnClickTeacherListItem(); });
                }
            }

            //Select the first user, if any
            if (m_users.Count > 0)
            {
                GameObject[] teachersContent = VHUtils.FindAllChildren(m_teacherListContent);
                int selectedIdx = 0;
                if (!string.IsNullOrEmpty(selectedName))
                    selectedIdx = Array.FindIndex<GameObject>(teachersContent, t => t.GetComponentInChildren<Text>().text == selectedName);

                if (selectedIdx == -1)
                    selectedIdx = 0;

                EventSystem.current.SetSelectedGameObject(teachersContent[selectedIdx]);
                teachersContent[selectedIdx].GetComponent<Toggle>().isOn = true;
                OnClickTeacherListItem();
            }

            m_teachersLoadIcon.SetActive(false);
            m_reinstateTeacherBtn.interactable = true;
            m_deleteTeacherBtn.interactable = true;
        });
    }

    /// <summary>
    /// Grabs m_classes
    /// </summary>
    IEnumerator GetClasses()
    {
        m_classesLoadIcon.SetActive(true);
        bool waitingForClasses = true;
        m_classes = new List<EntityDBVitaClass>();
        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
        dbClass.GetAllClassesInOrganization(VitaGlobals.m_loggedInProfile.organization, (classes, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllClasses() error: {0}", error));
                return;
            }
            m_classes = classes;
            waitingForClasses = false;
            m_classesLoadIcon.SetActive(false);
        });

        while (waitingForClasses)
            yield return new WaitForEndOfFrame();
    }
}
