using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.EventSystems;

public class MenuManagerPanelDMFAdminHub : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    ToggleGroup m_orgToggleGrp;
    GameObject m_orgListItem;
    GameObject m_orgListContent;
    List<EntityDBVitaOrganization> m_orgsAll;
    List<EntityDBVitaOrganization> m_orgsOriginal;
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

    Button m_addNewOrgButton;
    Button m_archiveOrgButton;
    Button m_saveOrgButton;
    GameObject m_orgLoadIcon;
    GameObject m_orgInfoLoadIcon;
    GameObject m_addNewOrgLoadIcon;
    GameObject m_archiveOrgLoadIcon;
    GameObject m_saveOrgLoadIcon;

    string m_saveChangesDupCheckOrgName = "";
    string m_saveChangesDupCheckUsername = "";

    #region Menu Init
    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.DMFAdminHub);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_orgToggleGrp = VHUtils.FindChildRecursive(menu, "OrgToggleGrp").GetComponent<ToggleGroup>();
        m_orgListItem = VHUtils.FindChildRecursive(resources, "GuiTogglePrefab");
        m_orgListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "ScrollView_Organizations"), "Content");

        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab_Names");

        m_orgName = VHUtils.FindChildRecursive(menu, "GiuInputOnlyPrefab");
        m_orgContactFirst = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_ContactFirstName");
        m_orgContactLast = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_ContactLastName");
        m_orgContactEmail = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_ContactEmail");
        m_orgContactPhone = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_ContactPhoneNum");
        m_orgUserName = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_AccountUserName");
        m_orgNumTeachers = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_NumTeachers").GetComponent<InputField>();
        m_orgNumStudents = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_NumStudents").GetComponent<InputField>();
        m_orgPassword = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_AccountPassword");
        m_orgCreated = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_AccountCreated");
        m_orgExpires = VHUtils.FindChildRecursive(menu, "GuiDropdownDatePrefab_AccountExpires").GetComponent<AGDropdownDate>();

        m_addNewOrgButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_AddNewOrganization").GetComponent<Button>();
        m_archiveOrgButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_ArchiveOrganization").GetComponent<Button>();
        m_saveOrgButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Save").GetComponent<Button>();
        m_orgLoadIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_Orgs");
        m_orgInfoLoadIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_OrgInfo");
        m_addNewOrgLoadIcon = VHUtils.FindChildRecursive(m_addNewOrgButton.gameObject, "LoadingIcon");
        m_archiveOrgLoadIcon = VHUtils.FindChildRecursive(m_archiveOrgButton.gameObject, "LoadingIcon");
        m_saveOrgLoadIcon = VHUtils.FindChildRecursive(m_saveOrgButton.gameObject, "LoadingIcon");
    }

    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        VitaGlobalsUI.m_unsavedChanges = false;

        m_addNewOrgButton.interactable = true;
        m_archiveOrgButton.interactable = true;
        m_saveOrgButton.interactable = true;
        m_orgLoadIcon.SetActive(false);
        m_orgInfoLoadIcon.SetActive(false);
        m_addNewOrgLoadIcon.SetActive(false);
        m_archiveOrgLoadIcon.SetActive(false);
        m_saveOrgLoadIcon.SetActive(false);
        RefreshOrgList();
    }

    public void OnMenuExit()
    {
        VitaGlobalsUI.m_unsavedChanges = false;
    }
    #endregion

    #region UI
    public void BtnDeleteOrganization()
    {
        if (m_orgListSelectedItem == null)
            return;

        string selectedName = m_orgListSelectedItem.GetComponentInChildren<Text>().text;

        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        dbOrg.DeleteOrganizationAndData(selectedName, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("DeleteOrganizationAndData() error: {0}", error));
                return;
            }

            RefreshOrgList();
        });
    }

    public void BtnArchiveOrganization()
    {
        if (m_orgListSelectedItem == null)
            return;

        string selectedName = m_orgListSelectedItem.GetComponentInChildren<Text>().text;

        m_saveOrgButton.interactable = false;
        m_archiveOrgButton.interactable = false;
        m_addNewOrgButton.interactable = false;
        m_archiveOrgLoadIcon.SetActive(true);

        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        dbOrg.ArchiveOrganization(selectedName, error =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("ArchiveOrganization() error: {0}", error));
                return;
            }

            RefreshOrgList();
        });
    }

    public void BtnCreateNewOrganization()
    {
        PopUpDisplay.Instance.DisplayOkCancelInput("Add Organization", "Enter Organization Name", (input) =>
        {
            string orgName = input;

            m_saveOrgButton.interactable = false;
            m_archiveOrgButton.interactable = false;
            m_addNewOrgButton.interactable = false;
            m_addNewOrgLoadIcon.SetActive(true);

            DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
            dbOrg.GetOrganization(orgName, (org, error) =>
            {
                // if an error happens here, we can't tell the difference between error and 'no profile exists'.  however it will be caught at 'save' time
                if (org != null)
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("Organization {0} already exists.  Please enter a new name.", orgName), "Ok", "Cancel", true, () =>
                    {
                        BtnCreateNewOrganization();
                    }, null);
                    return;
                }

                EntityDBVitaOrganization organization = new EntityDBVitaOrganization(orgName, "First Name", "Last Name", "email@example.com", "800-555-5555", orgName + "-admin", "password");
                m_orgs.Add(organization);
                GameObject orgWidget = VitaGlobalsUI.AddWidgetToList(m_orgListItem, m_orgListContent, m_orgListItem.name + "_" + organization.name, "Label", organization.name);
                Toggle orgToggle = orgWidget.GetComponent<Toggle>();
                orgToggle.group = m_orgToggleGrp;
                orgToggle.onValueChanged.AddListener(OrgListItemOnValueChanged);
                EventSystem.current.SetSelectedGameObject(orgWidget);
                orgToggle.isOn = true;

                UpdateUIState(organization);
            });
        }, null);
    }

    public void BtnSaveChanges() { StartCoroutine(BtnSaveChangesInternal()); }
    IEnumerator BtnSaveChangesInternal()
    {
        m_saveOrgButton.interactable = false;
        m_archiveOrgButton.interactable = false;
        m_addNewOrgButton.interactable = false;
        m_saveOrgLoadIcon.SetActive(true);

        yield return StartCoroutine(BtnSaveChangesCheckForDuplicateOrgName());
        if (!string.IsNullOrEmpty(m_saveChangesDupCheckOrgName))
        {
            PopUpDisplay.Instance.Display("Error", string.Format("BtnSaveChangesInternal() - '{0}' - duplicate organization name is in the list.  Organization names need to be unique.", m_saveChangesDupCheckOrgName));
            yield break;
        }

        yield return StartCoroutine(BtnSaveChangesCheckForDuplicateUsername());
        if (!string.IsNullOrEmpty(m_saveChangesDupCheckOrgName) &&
            !string.IsNullOrEmpty(m_saveChangesDupCheckUsername))
        {
            PopUpDisplay.Instance.Display("Error", string.Format("In Organization '{0}' - Admin user '{1}' already exists.  Please enter a new username.", m_saveChangesDupCheckOrgName, m_saveChangesDupCheckUsername));
            yield break;
        }

        //yield return StartCoroutine(BtnSaveChangesOrgName());
        yield return StartCoroutine(BtnSaveChangesOrgUsername());
        yield return StartCoroutine(BtnSaveChangesOrgPassword());
        yield return StartCoroutine(BtnSaveChangesOrgInfo());
        yield return StartCoroutine(BtnSaveChangesOrgNew());

        RefreshOrgList();
    }

    public void BtnHome()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnBack()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
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

        //Debug.LogFormat("MenuManagerPanelDMFAdminHub.OrgListItemOnValueChanged() - {0} - {1}", selectedName, value);

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

    public void OnEndEditOrgName(string value)
    {
        var org = GetSelectedOrganization();

        string selectedName = string.Copy(m_orgListSelectedItem.GetComponentInChildren<Text>().text);
        string newName = string.Copy(m_orgName.GetComponent<InputField>().text);

        //Debug.LogFormat("MenuManagerPanelDMFAdminHub.OrgNameInputFieldOnValueChanged() - {0} - {1} - {2}", value, newName, selectedName);

        if (selectedName != newName)
        {
            org.name = newName;
            m_orgListSelectedItem.GetComponentInChildren<Text>().text = newName;
            m_addNewOrgButton.interactable = true;
            m_archiveOrgButton.interactable = !AnyChangesMade();
            m_saveOrgButton.interactable = AnyChangesMade();
            VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
        }
    }

    public void OnValueChangedFirstName(string value)
    {
        var selected = GetSelectedOrganization();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelDMFAdminHub.OnValueChangedFirstName() - {0}", value);
        selected.firstname = m_orgContactFirst.GetComponent<InputField>().text;
        m_archiveOrgButton.interactable = !AnyChangesMade();
        m_saveOrgButton.interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }

    public void OnValueChangedLastName(string value)
    {
        var selected = GetSelectedOrganization();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelDMFAdminHub.OnValueChangedLastName() - {0}", value);
        selected.lastname = m_orgContactLast.GetComponent<InputField>().text;
        m_archiveOrgButton.interactable = !AnyChangesMade();
        m_saveOrgButton.interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }

    public void OnValueChangedEmail(string value)
    {
        var selected = GetSelectedOrganization();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelDMFAdminHub.OnValueChangedEmail() - {0}", value);
        selected.email = m_orgContactEmail.GetComponent<InputField>().text;
        m_archiveOrgButton.interactable = !AnyChangesMade();
        m_saveOrgButton.interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }

    public void OnValueChangedPhone(string value)
    {
        var selected = GetSelectedOrganization();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelDMFAdminHub.OnValueChangedPhone() - {0}", value);
        selected.phone = m_orgContactPhone.GetComponent<InputField>().text;
        m_archiveOrgButton.interactable = !AnyChangesMade();
        m_saveOrgButton.interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }

    public void OnValueChangedUsername(string value)
    {
        var selected = GetSelectedOrganization();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelDMFAdminHub.OnValueChangedUsername() - {0}", value);
        selected.username = m_orgUserName.GetComponent<InputField>().text;
        m_archiveOrgButton.interactable = !AnyChangesMade();
        m_saveOrgButton.interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }

    public void OnValueChangedPassword(string value)
    {
        var selected = GetSelectedOrganization();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelDMFAdminHub.OnValueChangedPassword() - {0}", value);
        selected.password = m_orgPassword.GetComponent<InputField>().text;
        m_archiveOrgButton.interactable = !AnyChangesMade();
        m_saveOrgButton.interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }

    public void OnValueChangeExpirationDate()
    {
        var selected = GetSelectedOrganization();
        if (selected == null)
            return;

        selected.accexpire = VitaGlobals.StringToTicksString(m_orgExpires.GetDate());
        m_archiveOrgButton.interactable = !AnyChangesMade();
        m_saveOrgButton.interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
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
        m_orgNumTeachers.text = "";
        m_orgNumStudents.text = "";
        m_orgPassword.GetComponent<InputField>().text = "";
        m_orgCreated.GetComponent<InputField>().text = "";
        //m_orgExpires.SetDate("");

        m_archiveOrgButton.interactable = false;
        m_saveOrgButton.interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;
        m_orgLoadIcon.SetActive(true);
        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        dbOrg.GetAllOrganizations((orgs, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllOrganizations() error: {0}", error));
                return;
            }

            m_orgsAll = orgs;
            m_orgs = new List<EntityDBVitaOrganization>();
            m_orgsOriginal = new List<EntityDBVitaOrganization>();

            // filter only what we are interested in
            foreach (var org in orgs)
            {
                //Debug.LogFormat("{0} - {1} - {2} - {3} - {4} - {5} - {6}", org.name, org.firstname, org.lastname, org.email, org.phone, org.acccreated, org.archived == 0 ? "Active" : "Archived");

                if (org.name != "root")  // filter out special case org for root admin
                {
                    if (org.archived == 0)
                    {
                        m_orgsOriginal.Add(org);
                        GameObject orgWidget = VitaGlobalsUI.AddWidgetToList(m_orgListItem, m_orgListContent, m_orgListItem.name + "_" + org.name, "Label", org.name);
                        Toggle orgToggle = orgWidget.GetComponent<Toggle>();
                        orgToggle.group = m_orgToggleGrp;
                        orgToggle.onValueChanged.AddListener(OrgListItemOnValueChanged);
                    }
                }
            }

            // make a copy
            m_orgsOriginal.ForEach(i => m_orgs.Add(new EntityDBVitaOrganization(i)));

            if (m_orgs.Count > 0)
            {
                GameObject[] orgsContent = VHUtils.FindAllChildren(m_orgListContent);
                int selectedIdx = 0;
                if (!string.IsNullOrEmpty(selectedName))
                    selectedIdx = Array.FindIndex<GameObject>(orgsContent, o => o.GetComponentInChildren<Text>().text == selectedName);

                if (selectedIdx == -1)
                    selectedIdx = 0;

                EventSystem.current.SetSelectedGameObject(orgsContent[selectedIdx]);
                orgsContent[selectedIdx].GetComponent<Toggle>().isOn = true;
            }

            UpdateUIState(GetSelectedOrganization());
        });
    }

    bool OrgEquals(EntityDBVitaOrganization originalOrg, EntityDBVitaOrganization newOrg)
    {
        /*
        Debug.LogFormat("{0} - {1}", originalOrg.username, newOrg.username);
        Debug.LogFormat("{0} - {1}", originalOrg.password, newOrg.password);
        Debug.LogFormat("{0} - {1}", originalOrg.firstname, newOrg.firstname);
        Debug.LogFormat("{0} - {1}", originalOrg.lastname, newOrg.lastname);
        Debug.LogFormat("{0} - {1}", originalOrg.email, newOrg.email);
        Debug.LogFormat("{0} - {1}", originalOrg.phone, newOrg.phone);
        Debug.LogFormat("{0} - {1}", originalOrg.archived, newOrg.archived);
        */

        return /*originalOrg.name == newOrg.name &&*/    // ignore name since we special case it
               originalOrg.username == newOrg.username &&
               originalOrg.password == newOrg.password &&
               originalOrg.firstname == newOrg.firstname &&
               originalOrg.lastname == newOrg.lastname &&
               originalOrg.email == newOrg.email &&
               originalOrg.phone == newOrg.phone &&
               originalOrg.accexpire == newOrg.accexpire &&
               originalOrg.archived == newOrg.archived;
    }

    bool AnyChangesMade(EntityDBVitaOrganization organization)
    {
        return AnyChangesMade();
    }
    bool AnyChangesMade()
    {
        if (m_orgsOriginal.Count != m_orgs.Count)
        {
            //Debug.LogWarningFormat("AnyChangesMade() - count");
            return true;
        }

        for (int i = 0; i < m_orgs.Count; i++)
        {
            if (m_orgsOriginal[i].name != m_orgs[i].name)
            {
                //Debug.LogWarningFormat("AnyChangesMade() - name - {0} - {1}", m_orgsOriginal[i].name, m_orgs[i].name);
                return true;
            }

            if (!OrgEquals(m_orgsOriginal[i], m_orgs[i]))
            {
                //Debug.LogWarningFormat("AnyChangesMade() - orgequal - {0} - {1}", m_orgsOriginal[i].name, m_orgs[i].name);
                return true;
            }
        }

        return false;
    }

    List<int> GetOrgsWithChangedName()
    {
        List<int> orgIdxs = new List<int>();

        for (int i = 0; i < m_orgs.Count; i++)
        {
            if (i < m_orgsOriginal.Count)
            {
                // look if the name of the org has changed
                if (m_orgsOriginal[i].name != m_orgs[i].name)
                {
                    orgIdxs.Add(i);
                }
            }
        }

        return orgIdxs;
    }

    List<int> GetOrgsWithChangedUserName()
    {
        List<int> orgIdxs = new List<int>();

        for (int i = 0; i < m_orgs.Count; i++)
        {
            if (i < m_orgsOriginal.Count)
            {
                // look for changes made to the linked username account
                if (m_orgsOriginal[i].username != m_orgs[i].username)
                {
                    orgIdxs.Add(i);
                }
            }
        }

        return orgIdxs;
    }

    List<int> GetOrgsWithChangedPassword()
    {
        List<int> orgIdxs = new List<int>();

        for (int i = 0; i < m_orgs.Count; i++)
        {
            if (i < m_orgsOriginal.Count)
            {
                // look for changes made to the password on the linked username account
                if (m_orgsOriginal[i].password != m_orgs[i].password)
                {
                    orgIdxs.Add(i);
                }
            }
        }

        return orgIdxs;
    }

    List<int> GetOrgsWithChangedInfo()
    {
        List<int> orgIdxs = new List<int>();

        for (int i = 0; i < m_orgs.Count; i++)
        {
            if (i < m_orgsOriginal.Count)
            {
                // look for changes made in existing orgs
                if (!OrgEquals(m_orgsOriginal[i], m_orgs[i]))
                {
                    orgIdxs.Add(i);
                }
            }
        }

        return orgIdxs;
    }

    List<int> GetOrgsNewlyAdded()
    {
        List<int> orgIdxs = new List<int>();

        for (int i = 0; i < m_orgs.Count; i++)
        {
            // look for new orgs added
            if (i >= m_orgsOriginal.Count)
            {
                orgIdxs.Add(i);
            }
        }

        return orgIdxs;
    }

    IEnumerator BtnSaveChangesCheckForDuplicateOrgName()
    {
        // these dup checks are, I believe now unneccessary because we disallow duplicates on add.
        // however, the check don't hurt, so let's keep them in for now.

        // check for duplicates.  error out because these are not allowed.
        m_saveChangesDupCheckOrgName = "";
        HashSet<string> m_dupCheck = new HashSet<string>();
        foreach (var org in m_orgsAll)
        {
            if (!m_dupCheck.Add(org.name))
            {
                m_saveChangesDupCheckOrgName = org.name;
                yield break;
            }
        }

        m_dupCheck = new HashSet<string>();
        foreach (var org in m_orgs)
        {
            if (!m_dupCheck.Add(org.name))
            {
                m_saveChangesDupCheckOrgName = org.name;
                yield break;
            }
        }
    }

    IEnumerator BtnSaveChangesCheckForDuplicateUsername()
    {
        // check for duplicate local admin username.

        m_saveChangesDupCheckOrgName = "";
        m_saveChangesDupCheckUsername = "";
        List<int> orgIdxs = GetOrgsWithChangedUserName();
        orgIdxs.AddRange(GetOrgsNewlyAdded());
        int orgUsernameChangeCount = orgIdxs.Count;

        foreach (int i in orgIdxs)
        {
            string org = m_orgs[i].name;
            string username = m_orgs[i].username;

            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.GetUser(username, (user, error) =>
            {
                orgUsernameChangeCount--;

                // if an error happens here, we can't tell the difference between error and 'no profile exists'.  however it will be caught at 'save' time
                if (user != null)
                {
                    m_saveChangesDupCheckOrgName = org;
                    m_saveChangesDupCheckUsername = username;
                }
            });
        }

        while (orgUsernameChangeCount > 0)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesOrgName()
    {
        List<int> orgIdxs = GetOrgsWithChangedName();
        int orgNameChangeCount = orgIdxs.Count;

        foreach (int i in orgIdxs)
        {
            Debug.LogWarningFormat("BtnSaveChanges() - changing org name - {0} - {1} - {2}", i, m_orgsOriginal[i].name, m_orgs[i].name);

            DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
            dbOrg.UpdateOrganizationName(m_orgsOriginal[i].name, m_orgs[i].name, error =>
            {
                orgNameChangeCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateOrganizationName() error: {0}", error));
                    return;
                }
            });
        }

        while (orgNameChangeCount > 0)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesOrgUsername()
    {
        List<int> orgIdxs = GetOrgsWithChangedUserName();
        int orgUsernameChangeCount = orgIdxs.Count;

        foreach (int i in orgIdxs)
        {
            Debug.LogWarningFormat("BtnSaveChanges() - changing local admin account name - {0} - {1} - {2}", m_orgs[i].name, m_orgsOriginal[i].username, m_orgs[i].username);

            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.UpdateUserName(m_orgsOriginal[i].username, m_orgs[i].username, error =>
            {
                orgUsernameChangeCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateUserName() error: {0}", error));
                    return;
                }
            });
        }

        while (orgUsernameChangeCount > 0)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesOrgPassword()
    {
        List<int> orgIdxs = GetOrgsWithChangedPassword();
        int orgPasswordChangeCount = orgIdxs.Count;

        foreach (int i in orgIdxs)
        {
            Debug.LogWarningFormat("BtnSaveChanges() - changing local admin password - {0} - {1} - {2} - {3}", m_orgs[i].name, m_orgs[i].username, m_orgsOriginal[i].password, m_orgs[i].password);

            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.UpdateUserPassword(m_orgsOriginal[i].username, m_orgs[i].password, error =>
            {
                orgPasswordChangeCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateUserPassword() error: {0}", error));
                    return;
                }
            });
        }

        while (orgPasswordChangeCount > 0)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesOrgInfo()
    {
        List<int> orgIdxs = GetOrgsWithChangedInfo();
        int orgChangeCount = orgIdxs.Count;

        foreach (int i in orgIdxs)
        {
            Debug.LogWarningFormat("BtnSaveChanges() - updating org with changes - {0}", m_orgs[i].name);

            DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
            dbOrg.UpdateOrganization(m_orgs[i].name, m_orgs[i].firstname, m_orgs[i].lastname, m_orgs[i].email, m_orgs[i].phone, m_orgs[i].username, m_orgs[i].password, m_orgs[i].accexpire, error =>
            {
                orgChangeCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateOrganization() error: {0}", error));
                    return;
                }
            });
        }

        while (orgChangeCount > 0)
            yield return new WaitForEndOfFrame();
    }

    IEnumerator BtnSaveChangesOrgNew()
    {
        List<int> orgIdxs = GetOrgsNewlyAdded();
        int orgNewCount = orgIdxs.Count;

        foreach (int i in orgIdxs)
        {
            // look for new orgs added
            Debug.LogWarningFormat("BtnSaveChanges() - creating new org - {0} - {1}", i, m_orgs[i].name);

            DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
            dbOrg.CreateOrganization(m_orgs[i].name, m_orgs[i].firstname, m_orgs[i].lastname, m_orgs[i].email, m_orgs[i].phone, m_orgs[i].username, m_orgs[i].password, error =>
            {
                orgNewCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("CreateOrganization() error: {0}", error));
                }
            });
        }

        while (orgNewCount > 0)
            yield return new WaitForEndOfFrame();
    }

    void UpdateUIState(EntityDBVitaOrganization organization)
    {
        bool changesMade = AnyChangesMade(organization);
        m_addNewOrgButton.interactable = true;
        m_archiveOrgButton.interactable = !changesMade;
        m_saveOrgButton.interactable = changesMade;
        m_orgLoadIcon.SetActive(false);
        //m_orgInfoLoadIcon.SetActive(false);
        m_addNewOrgLoadIcon.SetActive(false);
        m_archiveOrgLoadIcon.SetActive(false);
        m_saveOrgLoadIcon.SetActive(false);
        VitaGlobalsUI.m_unsavedChanges = changesMade;
    }
    #endregion
}
