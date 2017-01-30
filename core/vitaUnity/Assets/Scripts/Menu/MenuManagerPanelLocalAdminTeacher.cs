using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManagerPanelLocalAdminTeacher : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    [Serializable]
    public class WidgetObject
    {
        public string Name;
        public GameObject Widget;
    }

    MenuManager m_menuManager;
    List<EntityDBVitaProfile> m_usersOriginal;
    List<EntityDBVitaProfile> m_users;
    List<WidgetObject> m_userWidgets = new List<WidgetObject>();
    GameObject m_teacherListContent;
    ToggleGroup m_teacherListToggleGrp;

    GameObject m_headerName;
    GameObject m_name;
    GameObject m_userName;
    GameObject m_password;
    GameObject m_dateCreated;
    GameObject m_lastLogin;

    GameObject m_classListContent;
    List<EntityDBVitaClass> m_classes;

    EntityDBVitaProfile m_teacherSelected;
    GameObject m_teacherListSelectedItem;

    GameObject m_deleteTeacherButton;
    GameObject m_archiveTeacherButton;
    GameObject m_saveTeacherButton;

    //Resources
    GameObject m_teacherListItem;
    GameObject m_classListItem;

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.LocalAdminHubTeachers);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab (2)");
        m_teacherListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_Teachers"), "Content");
        m_teacherListToggleGrp = VHUtils.FindChildRecursive(menu, "TeacherListToggleGrp").GetComponent<ToggleGroup>();

        //Resources
        m_teacherListItem = VHUtils.FindChildRecursive(resources, "GuiTogglePrefab_Teacher");
        m_classListItem = VHUtils.FindChildRecursive(resources, "GuiTextPrefab_Class");

        m_name = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Name");
        m_userName = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_UserName");
        m_password = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Password");
        m_dateCreated = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_DateCreated");
        m_lastLogin = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_LastLogin");

        m_classListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_Classes"), "Content");

        m_deleteTeacherButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_DeleteTeacher");
        m_archiveTeacherButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_ArchiveTeacher");
        m_saveTeacherButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_SaveTeacher");
    }

    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        VitaGlobalsUI.m_unsavedChanges = false;

        StartCoroutine(RefreshTeacherList());
    }

    public void OnMenuExit()
    {
        VitaGlobalsUI.m_unsavedChanges = false;
    }

    #region UI Hooks
    public void BtnArchiveTeacher()
    {
        if (m_teacherSelected == null)
            return;
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.ArchiveUser(m_teacherSelected.username, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("ArchiveUser() error: {0}", error));
                return;
            }

            Destroy(m_teacherListSelectedItem);
            m_teacherListSelectedItem = null;
            m_teacherSelected = null;
        });
    }

    public void BtnDeleteTeacher()
    {
        PopUpDisplay.Instance.DisplayYesNo(VitaGlobals.Confirm, "Delete teacher? " + VitaGlobals.CannotBeUndone, DeleteTeacherInternal, null);
    }

    public void DeleteTeacherInternal()
    {
        if (m_teacherSelected == null)
            return;
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.DeleteUser(m_teacherSelected.username, (error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("DeleteUser() error: {0}", error));
                return;
            }

            Destroy(m_teacherListSelectedItem);
            m_teacherListSelectedItem = null;
            m_teacherSelected = null;
        });
    }

    public void BtnAddNewTeacher()
    {
        PopUpDisplay.Instance.DisplayOkCancelInput("Add Teacher", "Enter Teacher Username", (input) =>
        {
            string teacherUsername = input;

            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.GetUser(teacherUsername, (user, error) =>
            {
                // if an error happens here, we can't tell the difference between error and 'no profile exists'.  however it will be caught at 'save' time
                if (user != null)
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("Teacher {0} already exists.  Please enter a new name.", teacherUsername), "Ok", "Cancel", true, () =>
                    {
                        BtnAddNewTeacher();
                    }, null);
                    return;
                }

                string loginOrganization = VitaGlobals.m_loggedInProfile.organization;

                EntityDBVitaProfile teacher = new EntityDBVitaProfile(teacherUsername, "password", loginOrganization, "New Teacher", (int)DBUser.AccountType.TEACHER);
                m_users.Add(teacher);
                GameObject newTeacher = VitaGlobalsUI.AddWidgetToList(m_teacherListItem, m_teacherListContent, m_teacherListItem.name + "_" + teacher.name, "Label", teacher.name);
                Toggle newTeacherToggle = newTeacher.GetComponent<Toggle>();
                newTeacherToggle.group = m_teacherListToggleGrp;
                newTeacherToggle.onValueChanged.AddListener(delegate { OnClickTeacherListItem(); });
                m_userWidgets.Add(new WidgetObject { Name = teacher.username, Widget = newTeacher });
                EventSystem.current.SetSelectedGameObject(newTeacher);
                newTeacherToggle.isOn = true;
                OnClickTeacherListItem();
            });
        }, null);
    }

    public void BtnSave() { StartCoroutine(BtnSaveChangesInternal()); }
    IEnumerator BtnSaveChangesInternal()
    {
        if (m_usersOriginal.Find(item => item.username == m_teacherSelected.username) == null)
        {
            yield return StartCoroutine(BtnSaveChangesTeacherNew(m_teacherSelected));
        }
        else
        {
            //yield return StartCoroutine(BtnSaveChangesTeacherUsername(m_teacherSelected));
            yield return StartCoroutine(BtnSaveChangesTeacherPassword(m_teacherSelected));
            yield return StartCoroutine(BtnSaveChangesTeacherInfo(m_teacherSelected));
        }

        UpdateUIStates(m_teacherSelected);
    }

    public void OnClickTeacherListItem() {
        m_teacherListSelectedItem = EventSystem.current.currentSelectedGameObject;
        WidgetObject selectedWidget = m_userWidgets.Find(item => item.Widget == m_teacherListSelectedItem);
        m_teacherSelected = m_users.Find(item => item.username == selectedWidget.Name);

        //Debug.LogFormat("MenuManagerPanelLocalAdminTeacher.OnClickTeacherListItem() - {0}", m_teacherSelected.name);
        VHUtils.DeleteChildren(m_classListContent.transform);
        m_name.GetComponent<InputField>().text = m_teacherSelected.name;
        m_userName.GetComponent<InputField>().text = m_teacherSelected.username;
        m_password.GetComponent<InputField>().text = m_teacherSelected.password;
        m_dateCreated.GetComponent<InputField>().text = VitaGlobals.TicksToString(m_teacherSelected.createdate);
        m_lastLogin.GetComponent<InputField>().text = VitaGlobals.TicksToString(m_teacherSelected.lastlogin);

        UpdateUIStates(m_teacherSelected);

        foreach (var c in m_classes)
        {
            if (c.teacher == m_teacherSelected.username)
            {
                GameObject classWidget = VitaGlobalsUI.AddWidgetToList(m_classListItem, m_classListContent, m_classListItem.name + "_" + c.classname, null, c.classname);
                classWidget.GetComponent<Text>().text = c.classname;
            }
        }
    }

    public void OnEndEditChangedName(string value)
    {
        if (m_teacherSelected == null)
            return;
        //Debug.LogFormat("MenuManagerPanelLocalAdminTeacher.OnValueChangedName() - {0}", value);
        m_teacherSelected.name = m_name.GetComponent<InputField>().text;
        m_teacherListSelectedItem.GetComponentInChildren<Text>().text = m_name.GetComponent<InputField>().text;

        UpdateUIStates(m_teacherSelected);
    }

    public void OnValueChangedUsername(string value)
    {
        if (m_teacherSelected == null)
            return;
        //Debug.LogFormat("MenuManagerPanelLocalAdminTeacher.OnValueChangedUsername() - {0}", value);
        m_teacherSelected.username = m_userName.GetComponent<InputField>().text;

        UpdateUIStates(m_teacherSelected);
    }

    public void OnValueChangedPassword(string value)
    {
        if (m_teacherSelected == null)
            return;
        //Debug.LogFormat("MenuManagerPanelLocalAdminTeacher.OnValueChangedPassword() - {0}", value);
        m_teacherSelected.password = m_password.GetComponent<InputField>().text;

        UpdateUIStates(m_teacherSelected);
    }
    #endregion

    IEnumerator RefreshTeacherList()
    {
        string selectedName = "";
        if (m_teacherListSelectedItem != null)
            selectedName = m_teacherListSelectedItem.GetComponentInChildren<Text>().text;

        m_teacherListSelectedItem = null;
        m_teacherSelected = null;
        m_userWidgets = new List<WidgetObject>();
        VHUtils.DeleteChildren(m_teacherListContent.transform);
        VHUtils.DeleteChildren(m_classListContent.transform);
        m_name.GetComponent<InputField>().text = "";
        m_userName.GetComponent<InputField>().text = "";
        m_password.GetComponent<InputField>().text = "";
        m_dateCreated.GetComponent<InputField>().text = "";
        m_lastLogin.GetComponent<InputField>().text = "";

        //Get teachers and classes
        yield return StartCoroutine(GetClasses());
        yield return StartCoroutine(GetTeachers());

        foreach (var user in m_users)
        {
            GameObject teacherWidget = VitaGlobalsUI.AddWidgetToList(m_teacherListItem, m_teacherListContent, m_teacherListItem.name + "_" + user.name, "Label", user.name);
            m_userWidgets.Add(new WidgetObject { Name = user.username, Widget = teacherWidget });
            teacherWidget.GetComponent<Toggle>().group = m_teacherListToggleGrp;
            teacherWidget.GetComponent<Toggle>().onValueChanged.AddListener(delegate { OnClickTeacherListItem(); });
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

        m_deleteTeacherButton.GetComponent<Button>().interactable = true;
        m_archiveTeacherButton.GetComponent<Button>().interactable = true;
        m_saveTeacherButton.GetComponent<Button>().interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;
    }

    bool AnyChangesMade(EntityDBVitaProfile teacherProfile)
    {
        if (teacherProfile == null) return false;
        EntityDBVitaProfile userOriginal = m_usersOriginal.Find(item => item.username == teacherProfile.username);
        EntityDBVitaProfile userCurrent = m_users.Find(item => item.username == teacherProfile.username);

        if (userOriginal == null || userCurrent == null)    return true;
        if (userOriginal.username != userCurrent.username)  return true;
        if (userOriginal.name != userCurrent.name)          return true;
        if (userOriginal.password != userCurrent.password)  return true;

        return false;
    }

    IEnumerator GetTeachers()
    {
        bool waitingForTeachers = true;
        m_usersOriginal = new List<EntityDBVitaProfile>();
        m_users = new List<EntityDBVitaProfile>();
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllTeachersInOrganization(VitaGlobals.m_loggedInProfile.organization, (users, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllUsers() error: {0}", error));
                return;
            }

            // filter only what we are interested in
            foreach (var user in users)
            {
                //Debug.LogFormat("{0} - {1} - {2} - {3} - {4} - {5}", user.username, user.password, user.organization, user.type, user.name, user.archived == 0 ? "Active" : "Archived" );
                if (user.archived == 0)
                {
                    m_usersOriginal.Add(user);
                }
            }

            // make a copy
            m_usersOriginal.ForEach(i => m_users.Add(new EntityDBVitaProfile(i)));
            waitingForTeachers = false;
        });

        while (waitingForTeachers)
            yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// Grabs m_classesOriginal, m_classes
    /// </summary>
    IEnumerator GetClasses()
    {
        //m_loadingClassList.SetActive(true);
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
            //m_loadingClassList.SetActive(false);
        });

        while (waitingForClasses)
            yield return new WaitForEndOfFrame();
    }

    //IEnumerator BtnSaveChangesTeacherUsername(EntityDBVitaProfile teacherProfile)
    //{
    //    bool waitingForSave = true;
    //    Debug.LogFormat("BtnSaveChanges() - changing teacher name - {0}", teacherProfile.username);
    //    EntityDBVitaProfile originalProfile = m_usersOriginal.Find(item => item.username == teacherProfile.username);
    //    DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
    //    dbUser.UpdateUserName(originalProfile.username, teacherProfile.username, error =>
    //    {
    //        waitingForSave = false;
    //        if (!string.IsNullOrEmpty(error))
    //        {
    //            PopUpDisplay.Instance.Display("Error", string.Format("UpdateUserName() error: {0}", error));
    //            return;
    //        }
    //    });

    //    while (waitingForSave)
    //        yield return new WaitForEndOfFrame();
    //}

    IEnumerator BtnSaveChangesTeacherPassword(EntityDBVitaProfile teacherProfile)
    {
        bool waitingForSave = true;
        Debug.LogFormat("BtnSaveChanges() - changing teacher password - {0}", teacherProfile.password);
        EntityDBVitaProfile originalProfile = m_usersOriginal.Find(item => item.username == teacherProfile.username);
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.UpdateUserPassword(originalProfile.username, teacherProfile.password, error =>
        {
            waitingForSave = false;
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateUserPassword() error: {0}", error));
                return;
            }

            originalProfile.password = teacherProfile.password;
        });

        while (waitingForSave)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesTeacherInfo(EntityDBVitaProfile teacherProfile)
    {
        bool waitingForSave = true;
        Debug.LogFormat("BtnSaveChanges() -updating teacher with changes - {0}", teacherProfile.username);
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.UpdateUser(teacherProfile.username, teacherProfile.organization, teacherProfile.name, (DBUser.AccountType)teacherProfile.type, teacherProfile.lastlogin, error =>
        {
            waitingForSave = false;
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateUser() error: {0}", error));
                return;
            }

            var originalProfile = m_usersOriginal.Find(item => item.username == teacherProfile.username);
            originalProfile.name = teacherProfile.name;
            originalProfile.lastlogin = teacherProfile.lastlogin;
        });

        while (waitingForSave)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesTeacherNew(EntityDBVitaProfile teacherProfile)
    {
        bool waitingForSave = true;
        Debug.LogFormat("BtnSaveChanges() - creating new teacher - {0}", teacherProfile.username);
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.CreateUser(teacherProfile.username, teacherProfile.password, teacherProfile.organization, teacherProfile.name, (DBUser.AccountType)teacherProfile.type, error =>
        {
            waitingForSave = false;
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("CreateUser() error: {0}", error));
                return;
            }
        });

        while (waitingForSave)
            yield return new WaitForEndOfFrame();

        m_usersOriginal.Add(new EntityDBVitaProfile(teacherProfile));
        VitaGlobalsUI.SortListAfterAddingNew<EntityDBVitaProfile>(m_users, m_usersOriginal, "username");
    }

    void UpdateUIStates(EntityDBVitaProfile teacherProfile)
    {
        if (teacherProfile == null) return;
        bool changesMade = AnyChangesMade(teacherProfile);
        //Debug.LogWarningFormat("UpdateUIStates({0}), changes={1}", classObj.classname, changesMade);
        m_deleteTeacherButton.GetComponent<Button>().interactable = !changesMade;
        m_archiveTeacherButton.GetComponent<Button>().interactable = !changesMade;
        m_saveTeacherButton.GetComponent<Button>().interactable = changesMade;
        VitaGlobalsUI.m_unsavedChanges = changesMade;

        //Update widget display to indicate changes
        if (m_teacherListSelectedItem != null)
        {
            Text widgetText = VHUtils.FindChildRecursive(m_teacherListSelectedItem, "Label").GetComponent<Text>();
            VitaGlobalsUI.SetTextWithAsteriskIfChanges<Text>(widgetText, AnyChangesMade(teacherProfile));
        }
    }
}
