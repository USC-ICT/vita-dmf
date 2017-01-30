using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManagerPanelTeacherClasses : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    MenuManager m_menuManager;
    GameObject m_headerName;
    GameObject m_classListItem;
    GameObject m_studentListItem;
    GameObject m_classListContent;
    GameObject m_studentListContent;
    GameObject m_classDateCreated;
    List<EntityDBVitaProfile> m_users;
    List<EntityDBVitaClass> m_classes;

    GameObject m_classListSelectedItem;


    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.TeacherClasses);
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab (2)");
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_classListItem = VHUtils.FindChildRecursive(resources, "GuiTogglePrefab_Class");
        m_studentListItem = VHUtils.FindChildRecursive(resources, "GuiTextPrefab_Student");
        m_classListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Classes"), "Content");
        m_studentListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Students"), "Content");
        m_classDateCreated = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_DateCreated");
    }


    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        RefreshClassList();
    }

    public void OnMenuExit()
    {
    }


    public void BtnBack()
    {
        Debug.Log("BtnBack()");
    }

    public void BtnHome()
    {
        Debug.Log("BtnHome()");
    }

    public void OnValueChangedClassListItem(bool value)
    {
        m_classListSelectedItem = EventSystem.current.currentSelectedGameObject;
        string selectedName = m_classListSelectedItem.GetComponentInChildren<Text>().text;

        //Debug.LogFormat("MenuManagerPanelTeacherClasses.OnValueChangedClassListItem() - {0} - {1}", selectedName, value);

        // uncheck all the checkboxes except for the one selected
        GameObject [] classes = VHUtils.FindAllChildren(m_classListContent);
        foreach (var c in classes)
        {
            if (c.GetComponentInChildren<Text>().text != selectedName)
            {
                c.GetComponent<Toggle>().isOn = false;
            }
        }

        VHUtils.DeleteChildren(m_studentListContent.transform);

        if (value)
        {
            foreach (var c in m_classes)
            {
                if (selectedName == c.classname)
                {
                    foreach (var student in c.students)
                    {
                        EntityDBVitaProfile studentProfile = m_users.Find(u => u.username == student);
                        if (studentProfile != null)
                        {
                            string studentName = studentProfile.name;
                            AddStudentItem(studentName);
                        }
                    }

                    m_classDateCreated.GetComponent<InputField>().text = VitaGlobals.TicksToString(c.datecreated);
                    break;
                }
            }
        }
    }


    void AddClassItem(string classname)
    {
        GameObject classItem = Instantiate(m_classListItem);
        classItem.GetComponent<Toggle>().isOn = false;
        classItem.SetActive(true);
        classItem.GetComponentInChildren<Text>().text = classname;
        classItem.transform.SetParent(m_classListContent.transform, false);
        classItem.GetComponent<Toggle>().onValueChanged.AddListener(OnValueChangedClassListItem);
    }


    void AddStudentItem(string student)
    {
        GameObject studentItem = Instantiate(m_studentListItem);
        studentItem.SetActive(true);
        studentItem.GetComponentInChildren<Text>().text = student;
        studentItem.transform.SetParent(m_studentListContent.transform, false);
    }

    void RefreshClassList()
    {
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;

        VHUtils.DeleteChildren(m_classListContent.transform);
        VHUtils.DeleteChildren(m_studentListContent.transform);
        m_classDateCreated.GetComponent<InputField>().text = "";

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllStudentsInOrganization(loginOrganization, (users, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllUsers() error: {0}", error));
                return;
            }

            m_users = new List<EntityDBVitaProfile>();

            foreach (var user in users)
            {
                //Debug.LogFormat("{0} - {1} - {2} - {3} - {4}", user.username, user.password, user.organization, user.type, user.archived == 0 ? "Active" : "Archive");

                if (user.archived == 0)
                {
                    m_users.Add(user);
                }
            }

            m_classes = new List<EntityDBVitaClass>();
            DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
            dbClass.GetAllClassesInOrganization(loginOrganization, (classes, error2) =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("GetAllClasses() error: {0}", error2));
                    return;
                }

                foreach (var c in classes)
                {
                    //Debug.LogFormat("{0} - {1} - studentCount: {2}", c.classname, c.organization, c.students.Count);
                    if (c.teacher == VitaGlobals.m_loggedInProfile.username)
                    {
                        m_classes.Add(c);
                        AddClassItem(c.classname);
                    }
                }

                if (m_classes.Count > 0)
                {
                    GameObject [] classesContent = VHUtils.FindAllChildren(m_classListContent);
                    EventSystem.current.SetSelectedGameObject(classesContent[0]);
                    classesContent[0].GetComponent<Toggle>().isOn = true;
                }
            });
        });
    }
}
