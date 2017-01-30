using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManagerPanelDMFAdminOrganizationArchives : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    ToggleGroup m_orgToggleGrp;
    GameObject m_orgListItem;
    GameObject m_orgListContent;
    List<EntityDBVitaOrganization> m_orgs;
    List<EntityDBVitaProfile> m_teachers;
    List<EntityDBVitaProfile> m_students;

    GameObject m_headerName;
    GameObject m_orgName;
    GameObject m_orgContactFirst;
    GameObject m_orgContactLast;
    GameObject m_orgContactEmail;
    GameObject m_orgContactPhone;
    GameObject m_orgUserName;
    GameObject m_orgPassword;
    InputField m_orgNumTeachers;
    InputField m_orgNumStudents;
    GameObject m_orgCreated;
    AGDropdownDate m_orgExpires;

    GameObject m_orgListSelectedItem;

    Button m_reinstateOrgButton;
    Button m_deleteOrgButton;
    GameObject m_orgLoadIcon;
    GameObject m_orgInfoLoadIcon;
    GameObject m_reinstateOrgLoadIcon;
    GameObject m_deleteOrgLoadIcon;

    #region Menu Init
    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.DMFAdminHubArchive);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_orgToggleGrp = VHUtils.FindChildRecursive(menu, "OrgToggleGrp").GetComponent<ToggleGroup>();
        m_orgListItem = VHUtils.FindChildRecursive(resources, "GuiTogglePrefab");
        m_orgListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_Organizations"), "Content");

        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab_Names");

        m_orgName = VHUtils.FindChildRecursive(menu, "GiuInputOnlyPrefab_NameOfOrganization");
        m_orgContactFirst = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_FirstName");
        m_orgContactLast = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_LastName");
        m_orgContactEmail = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_EmailAddress");
        m_orgContactPhone = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_PhoneNumber");
        m_orgUserName = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_UserName");
        m_orgPassword = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Password");
        m_orgNumTeachers = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_NumTeachers").GetComponent<InputField>();
        m_orgNumStudents = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_NumStudents").GetComponent<InputField>(); 
        m_orgCreated = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_AccountCreated");
        m_orgExpires = VHUtils.FindChildRecursive(menu, "GuiDropdownDatePrefab_AccountExpires").GetComponent<AGDropdownDate>();

        m_reinstateOrgButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Reinstate").GetComponent<Button>();
        m_deleteOrgButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_DeleteOrganization").GetComponent<Button>();
        m_orgLoadIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_Orgs");
        m_orgInfoLoadIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_OrgInfo");
        m_reinstateOrgLoadIcon = VHUtils.FindChildRecursive(m_reinstateOrgButton.gameObject, "LoadingIcon");
        m_deleteOrgLoadIcon = VHUtils.FindChildRecursive(m_deleteOrgButton.gameObject, "LoadingIcon");
    }

    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        m_reinstateOrgButton.interactable = true;
        m_deleteOrgButton.interactable = true;
        m_orgLoadIcon.SetActive(false);
        m_orgInfoLoadIcon.SetActive(false);
        m_reinstateOrgLoadIcon.SetActive(false);
        m_deleteOrgLoadIcon.SetActive(false);
        RefreshOrgList();
    }

    public void OnMenuExit()
    {
    }
    #endregion

    #region UI
    public void BtnDeleteOrganization()
    {
        PopUpDisplay.Instance.DisplayYesNo(VitaGlobals.Confirm, "Delete organization? " + VitaGlobals.CannotBeUndone, DeleteOrganization, null);
    }

    public void DeleteOrganization()
    {
        if (m_orgListSelectedItem == null)
            return;

        string selectedName = m_orgListSelectedItem.GetComponentInChildren<Text>().text;

        m_reinstateOrgButton.interactable = false;
        m_deleteOrgButton.interactable = false;
        m_deleteOrgLoadIcon.SetActive(true);

        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        dbOrg.DeleteOrganizationAndData(selectedName, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("DeleteOrganizationAndData() error: {0}", error));
                return;
            }

            m_reinstateOrgButton.interactable = true;
            m_deleteOrgButton.interactable = true;
            m_deleteOrgLoadIcon.SetActive(false);
            RefreshOrgList();
        });
    }

    public void BtnReinstateOrganization()
    {
        if (m_orgListSelectedItem == null)
            return;

        string selectedName = m_orgListSelectedItem.GetComponentInChildren<Text>().text;

        m_reinstateOrgButton.interactable = false;
        m_deleteOrgButton.interactable = false;
        m_reinstateOrgLoadIcon.SetActive(true);

        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        dbOrg.ReinstateOrganization(selectedName, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("ReinstateOrganization() error: {0}", error));
                return;
            }

            m_reinstateOrgButton.interactable = true;
            m_deleteOrgButton.interactable = true;
            m_reinstateOrgLoadIcon.SetActive(false);
            RefreshOrgList();
        });
    }

    public void OrgListItemOnValueChanged(bool value) { StartCoroutine(OrgListItemOnValueChangedInternal(value)); }
    IEnumerator OrgListItemOnValueChangedInternal(bool value)
    {
        m_orgListSelectedItem = EventSystem.current.currentSelectedGameObject;
        m_orgInfoLoadIcon.SetActive(true);

        // TODO - make sure selected item is actually in the Content list, sometimes EventSystem.current can be something else

        string selectedName = m_orgListSelectedItem.GetComponentInChildren<Text>().text;
        yield return GetTeachers(selectedName);
        yield return GetStudents(selectedName);

        //Debug.LogFormat("MenuManagerPanelDMFAdminOrganizationArchives.OrgListItemOnValueChanged() - {0} - {1}", selectedName, value);

        if (value)
        {
            var org = GetSelectedOrganization();
            m_orgName.GetComponent<InputField>().text = org.name;
            m_orgContactFirst.GetComponent<InputField>().text = org.firstname;
            m_orgContactLast.GetComponent<InputField>().text = org.lastname;
            m_orgContactEmail.GetComponent<InputField>().text = org.email;
            m_orgContactPhone.GetComponent<InputField>().text = org.phone;
            m_orgUserName.GetComponent<InputField>().text = org.username;
            m_orgNumTeachers.text = m_teachers.Count.ToString();
            m_orgNumStudents.text = m_students.Count.ToString();
            m_orgPassword.GetComponent<InputField>().text = org.password;
            m_orgCreated.GetComponent<InputField>().text = VitaGlobals.TicksToString(org.acccreated);
            m_orgExpires.SetDate(VitaGlobals.TicksToString(org.accexpire));
        }
        m_orgInfoLoadIcon.SetActive(false);
    }
    #endregion

    #region Private Functions
    IEnumerator GetTeachers(string organization)
    {
        bool waitingForTeachers = true;
        m_teachers = new List<EntityDBVitaProfile>();
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllTeachersInOrganization(organization, (users, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllUsers() error: {0}", error));
                return;
            }

            m_teachers.Clear();
            foreach (var user in users)
            {
                if (user.archived == 0)
                {
                    m_teachers.Add(user);
                }
            }

            waitingForTeachers = false;
        });

        while (waitingForTeachers)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator GetStudents(string organization)
    {
        bool waitingForStudents = true;
        m_students = new List<EntityDBVitaProfile>();
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllStudentsInOrganization(organization, (users, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllUsers() error: {0}", error));
                return;
            }

            m_students.Clear();
            foreach (var user in users)
            {
                if (user.archived == 0)
                {
                    m_students.Add(user);
                }
            }

            waitingForStudents = false;
        });

        while (waitingForStudents)
            yield return new WaitForEndOfFrame();
    }

    EntityDBVitaOrganization GetSelectedOrganization()
    {
        if (m_orgListSelectedItem == null)
            return null;

        string selectedName = m_orgListSelectedItem.GetComponentInChildren<Text>().text;
        return m_orgs.Find(item => item.name == selectedName);
    }

    void RefreshOrgList()
    {
        string selectedName = "";
        if (m_orgListSelectedItem != null)
            selectedName = m_orgListSelectedItem.GetComponentInChildren<Text>().text;

        VHUtils.DeleteChildren(m_orgListContent.transform);
        m_orgName.GetComponent<InputField>().text = "";
        m_orgContactFirst.GetComponent<InputField>().text = "";
        m_orgContactLast.GetComponent<InputField>().text = "";
        m_orgContactEmail.GetComponent<InputField>().text = "";
        m_orgContactPhone.GetComponent<InputField>().text = "";
        m_orgUserName.GetComponent<InputField>().text = "";
        m_orgPassword.GetComponent<InputField>().text = "";
        m_orgCreated.GetComponent<InputField>().text = "";

        m_reinstateOrgButton.interactable = false;
        m_deleteOrgButton.interactable = false;
        m_orgLoadIcon.SetActive(true);
        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        dbOrg.GetAllOrganizations((orgs, error) =>
        {
            try
            {
                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("GetAllOrganizations() error: {0}", error));
                    return;
                }

                m_orgs = new List<EntityDBVitaOrganization>();

                foreach (var org in orgs)
                {
                    //Debug.LogFormat("{0} - {1} - {2} - {3} - {4} - {5} - {6}", org.name, org.firstname, org.lastname, org.email, org.phone, org.acccreated, org.archived == 0 ? "Active" : "Archived");

                    if (org.name != "root")  // filter out special case org for root admin
                    {
                        if (org.archived == 1)
                        {
                            m_orgs.Add(org);
                            GameObject orgWidget = VitaGlobalsUI.AddWidgetToList(m_orgListItem, m_orgListContent, m_orgListItem.name + "_" + org.name, "Label", org.name);
                            Toggle orgToggle = orgWidget.GetComponent<Toggle>();
                            orgToggle.group = m_orgToggleGrp;
                            orgToggle.onValueChanged.AddListener(OrgListItemOnValueChanged);
                        }
                    }
                }

                if (m_orgs.Count > 0)
                {
                    GameObject [] orgsContent = VHUtils.FindAllChildren(m_orgListContent);
                    int selectedIdx = 0;
                    if (!string.IsNullOrEmpty(selectedName))
                        selectedIdx = Array.FindIndex<GameObject>(orgsContent, o => o.GetComponentInChildren<Text>().text == selectedName);

                    if (selectedIdx == -1)
                        selectedIdx = 0;

                    EventSystem.current.SetSelectedGameObject(orgsContent[selectedIdx]);
                    orgsContent[selectedIdx].GetComponent<Toggle>().isOn = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0}", e.ToString());
            }

            m_reinstateOrgButton.interactable = true;
            m_deleteOrgButton.interactable = true;
            m_orgLoadIcon.SetActive(false);
        });
    }
    #endregion
}
