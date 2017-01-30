using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManagerPanelLocalAdminClasses : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    List<EntityDBVitaClass> m_classesOriginal;
    List<EntityDBVitaClass> m_classes;
    List<EntityDBVitaProfile> m_teachers;
    List<EntityDBVitaProfile> m_students;

    MenuManager m_menuManager;
    GameObject m_headerName;
    GameObject m_classListItem;
    GameObject m_studentListItem;
    GameObject m_classListContent;
    Dropdown m_teacherListDropdown;
    GameObject m_studentListContent;
    GameObject m_name;
    GameObject m_dateCreated;

    GameObject m_classListSelectedItem;
    ToggleGroup m_classListToggleGrp;
    Toggle m_studentListHideToggle;

    GameObject m_deleteClassButton;
    GameObject m_saveClassButton;
    Button m_addClassButton;

    //Loading
    GameObject m_loadingClassList;
    GameObject m_loadingStudentsList;
    GameObject m_loadingSaveClass;
    GameObject m_loadingAddNewClass;
    GameObject m_loadingDeleteClass;

    bool m_refreshingClassList = false;
    bool waitingForClasses = false;
    bool waitingForTeachersStudents = false;
    bool m_ignoreStudentListCallback = false;

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.LocalAdminHubClasses);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab (2)");
        m_classListItem = VHUtils.FindChildRecursive(resources, "GuiTogglePrefab_Class");
        m_studentListItem = VHUtils.FindChildRecursive(resources, "GuiTogglePrefab_Student");
        m_classListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Scroll View Teachers"), "Content");
        m_teacherListDropdown = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Teachers").GetComponent<Dropdown>();
        m_studentListContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Scroll View Classes"), "Content");
        m_name = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_ClassName");
        m_dateCreated = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_DateCreated");

        m_deleteClassButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_DeleteClass");
        m_saveClassButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Save");
        m_addClassButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_AddNew").GetComponent<Button>();
        m_studentListHideToggle = VHUtils.FindChildRecursive(menu, "GuiTogglePrefab_HideAssigned").GetComponent<Toggle>();

        //Loading
        m_loadingClassList = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_ClassList");
        m_loadingStudentsList = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_StudentsList");
        m_loadingSaveClass = VHUtils.FindChildRecursive(m_saveClassButton, "LoadingIcon");
        m_loadingAddNewClass = VHUtils.FindChildRecursive(m_addClassButton.gameObject, "LoadingIcon");
        m_loadingDeleteClass = VHUtils.FindChildRecursive(m_deleteClassButton, "LoadingIcon");

        m_classListToggleGrp = VHUtils.FindChildRecursive(menu, "ClassListToggleGrp").GetComponent<ToggleGroup>();
    }

    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        VitaGlobalsUI.m_unsavedChanges = false;

        m_deleteClassButton.GetComponent<Button>().interactable = false;
        m_saveClassButton.GetComponent<Button>().interactable = true;
        m_addClassButton.interactable = true;
        m_loadingClassList.SetActive(false);
        m_loadingStudentsList.SetActive(false);
        m_loadingSaveClass.SetActive(false);
        m_loadingAddNewClass.SetActive(false);
        m_loadingDeleteClass.SetActive(false);

        StartCoroutine(RefreshClassListInternal());
    }

    public void OnMenuExit()
    {
        VitaGlobalsUI.m_unsavedChanges = false;
    }

    #region UI Hooks
    public void BtnHome()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnBack()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnDeleteClass()
    {
        PopUpDisplay.Instance.DisplayYesNo(VitaGlobals.Confirm, "Delete class? " + VitaGlobals.CannotBeUndone, DeleteClass, null);
    }

    public void DeleteClass()
    {
        if (m_classListSelectedItem == null)
            return;

        m_deleteClassButton.GetComponent<Button>().interactable = false;
        m_saveClassButton.GetComponent<Button>().interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;
        m_addClassButton.interactable = false;
        m_loadingDeleteClass.SetActive(true);

        var selected = GetSelectedClass();
        if (selected == null)
        {
            m_loadingDeleteClass.SetActive(false);
            return;
        }

        bool classFound = false;
        foreach (var c in m_classesOriginal)
        {
            if (selected.classname == c.classname)
            {
                classFound = true;
                break;
            }
        }

        if (!classFound)
        {
            m_loadingDeleteClass.SetActive(false);
        }
        else
        {
            DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
            dbClass.DeleteClass(selected.classname, error =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("DeleteClass() error: {0}", error));
                    return;
                }
                m_loadingDeleteClass.SetActive(false);

                m_classes.Remove(selected);
                m_classesOriginal.Remove(selected);
                GameObject.Destroy(m_classListSelectedItem);
                GameObject[] classWidgets = VHUtils.FindAllChildren(m_classListContent);
                if (classWidgets.Length > 0)
                {
                    classWidgets[0].GetComponent<Toggle>().isOn = true;
                }
            });
        }
    }

    public void StudentList_HideAssigned(bool value)
    {
        GameObject[] studentWidgets = VHUtils.FindAllChildren(m_studentListContent);
        foreach (var studentWidget in studentWidgets)
        {
            if (studentWidget.GetComponent<Toggle>().isOn)
            {
                studentWidget.SetActive(!value);
            }
        }
    }

    public void BtnAddNewClass()
    {
        m_deleteClassButton.GetComponent<Button>().interactable = false;
        m_saveClassButton.GetComponent<Button>().interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;
        m_addClassButton.interactable = false; 
        m_loadingAddNewClass.SetActive(true);

        PopUpDisplay.Instance.DisplayOkCancelInput("Add Class", "Enter Class Name", (input) =>
        {
            string className = input;

            DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
            dbClass.GetClass(className, (c, error) =>
            {
                // if an error happens here, we can't tell the difference between error and 'no profile exists'.  however it will be caught at 'save' time
                if (c != null)
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("Class {0} already exists.  Please enter a new name.", className), "Ok", "Cancel", true, () =>
                    {
                        BtnAddNewClass();
                    }, null);
                    return;
                }

                string loginOrganization = VitaGlobals.m_loggedInProfile.organization;

                EntityDBVitaClass newClass = new EntityDBVitaClass(className, loginOrganization);
                m_classes.Add(newClass);
                GameObject newClassObj = VitaGlobalsUI.AddWidgetToList(m_classListItem, m_classListContent, m_classListItem.name + "_" + newClass.classname, "Label", newClass.classname);
                Toggle classToggle = newClassObj.GetComponent<Toggle>();
                classToggle.group = m_classListToggleGrp;
                classToggle.onValueChanged.AddListener(OnValueChangedClassListItem);
                EventSystem.current.SetSelectedGameObject(newClassObj);
                classToggle.isOn = true;

                //UI
                UpdateUIStates(newClass);
                m_loadingAddNewClass.SetActive(false);
            });
        }, null);
    }

    public void BtnSaveChanges()
    {
        StartCoroutine(BtnSaveChangesInternal());
    }

    IEnumerator BtnSaveChangesInternal()
    {
        var selected = GetSelectedClass();
        m_deleteClassButton.GetComponent<Button>().interactable = false;
        m_saveClassButton.GetComponent<Button>().interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;
        m_addClassButton.interactable = false;
        m_loadingSaveClass.SetActive(true);

        if (m_classesOriginal.Find(item => item.classname == selected.classname) != null)
        {
            //Debug.LogWarningFormat("Saving existing class {0}", selected.classname);
            yield return StartCoroutine(BtnSaveChangesClassTeacher(selected));

            //Update UI again since BtnSaveChangesClassTeacher() will update UI mid-saving
            //m_deleteClassButton.GetComponent<Button>().interactable = false;
            //m_saveClassButton.GetComponent<Button>().interactable = false;
            //m_addClassButton.interactable = false;
            yield return StartCoroutine(BtnSaveChangesClassStudents(selected));
        }
        else
        {
            //Debug.LogWarningFormat("Saving new class {0}", selected.classname);
            yield return StartCoroutine(BtnSaveChangesClassNew(selected));
        }

        UpdateUIStates(selected);
        //m_addClassButton.interactable = true;
        m_loadingSaveClass.SetActive(false);
    }

    public void OnEndEditChangedName(string value)
    {
        var selected = GetSelectedClass();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelLocalAdminTeacher.OnValueChangedName() - {0}", value);
        selected.classname = m_name.GetComponent<InputField>().text;
        m_classListSelectedItem.GetComponentInChildren<Text>().text = m_name.GetComponent<InputField>().text;

        UpdateUIStates(selected);
    }

    public void OnValueChangedClassListItem(bool value)
    {
        StartCoroutine(OnValueChangedClassListItemInternal(value));
    }

    IEnumerator OnValueChangedClassListItemInternal(bool value)
    {
        //Debug.LogErrorFormat("OnValueChangedClassListItem()");
        while (m_refreshingClassList)
        {
            yield return new WaitForEndOfFrame();
        }

        //Show all students again
        m_studentListHideToggle.isOn = false;

        m_classListSelectedItem = EventSystem.current.currentSelectedGameObject;
        var selected = GetSelectedClass();
        if (selected == null)
            yield break;

        string selectedName = selected.classname;// m_classListSelectedItem.GetComponentInChildren<Text>().text;

        GameObject[] students = VHUtils.FindAllChildren(m_studentListContent);
        foreach (var s in students)
        {
            if (s.GetComponent<Toggle>().isOn)
            {
                m_ignoreStudentListCallback = true;
                s.GetComponent<Toggle>().isOn = false;
            }
        }

        if (value)
        {
            foreach (var c in m_classes)
            {
                if (selectedName == c.classname)
                {
                    int idx = 0; //First position is blank
                    for (int iTeacher = 0; iTeacher < m_teachers.Count; iTeacher++)
                    {
                        if (m_teachers[iTeacher].username == c.teacher)
                        {
                            idx = iTeacher;
                            break;
                        }
                    }
                    if (idx == -1)
                    {
                        Debug.LogErrorFormat("OnValueChangedClassListItem() - teacher assigned to class doesn't exist (archived? deleted?) - {0}", c.teacher);
                        idx = 0;
                    }

                    m_teacherListDropdown.value = idx;
                    m_name.GetComponent<InputField>().text = c.classname;
                    m_dateCreated.GetComponent<InputField>().text = VitaGlobals.TicksToString(c.datecreated);

                    foreach (var student in students)
                    {
                        string studentName = student.GetComponentInChildren<Text>().text;
                        string studentUsername = m_students.Find(i => i.name == studentName).username;
                        if (c.students.Contains(studentUsername))
                        {
                            if (!student.GetComponent<Toggle>().isOn)
                            {
                                m_ignoreStudentListCallback = true;
                                student.GetComponent<Toggle>().isOn = true;
                            }
                        }
                    }

                    UpdateUIStates(c);
                    break;
                }
            }
        }
    }

    public void OnValueChangedTeacherDropdown(bool value)
    {
        var selected = GetSelectedClass();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelLocalAdminClasses.OnValueChangedTeacherDropdown() - {0} - {1}", selectedName, value);

        selected.teacher = m_teachers[m_teacherListDropdown.value].username;

        UpdateUIStates(selected);
    }

    public void OnValueChangedStudentListItem(bool value)
    {
        if (m_ignoreStudentListCallback)
        {
            Debug.LogFormat("MenuManagerPanelLocalAdminClasses.OnValueChangedStudentListItem() - pass");
            m_ignoreStudentListCallback = false;
            return;
        }

        var selected = GetSelectedClass();
        if (selected == null)
            return;

        //Debug.LogFormat("MenuManagerPanelLocalAdminClasses.OnValueChangedStudentListItem() - {0} - {1}", selected.classname, value);

        selected.students.Clear();

        GameObject [] students = VHUtils.FindAllChildren(m_studentListContent);
        foreach (var student in students)
        {
            if (student.GetComponent<Toggle>().isOn)
            {
                string studentName = student.GetComponentInChildren<Text>().text;
                string studentUsername = m_students.Find(i => i.name == studentName).username;

                selected.students.Add(studentUsername);
            }
        }

        UpdateUIStates(selected);
    }
    #endregion

    EntityDBVitaClass GetSelectedClass()
    {
        if (m_classListSelectedItem == null)
            return null;
        string selectedName = VitaGlobalsUI.PruneAsterisk(m_classListSelectedItem.GetComponentInChildren<Text>().text);
        return m_classes.Find(item => item.classname == selectedName);
    }

    IEnumerator RefreshClassListInternal()
    {
        m_refreshingClassList = true;
        waitingForClasses = true;
        waitingForTeachersStudents = true;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        string selectedName = "";
        if (m_classListSelectedItem != null)
            selectedName = m_classListSelectedItem.GetComponentInChildren<Text>().text;

        VHUtils.DeleteChildren(m_classListContent.transform);
        m_teacherListDropdown.ClearOptions();
        m_name.GetComponent<InputField>().text = "";
        m_dateCreated.GetComponent<InputField>().text = "";
        m_ignoreStudentListCallback = false;
        VHUtils.DeleteChildren(m_studentListContent.transform);

        //Grab teachers and classes; proceed when DB communication is done
        m_deleteClassButton.GetComponent<Button>().interactable = false;
        m_saveClassButton.GetComponent<Button>().interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;
        m_addClassButton.interactable = false;
        GetClasses(loginOrganization); //m_classesOriginal, m_classes
        GetTeachersStudents(loginOrganization); //m_teachers, m_students
        while (waitingForClasses || waitingForTeachersStudents)
        {
            yield return new WaitForEndOfFrame();
        }

        //Fill UI lists
        foreach (var c in m_classesOriginal)
        {
            GameObject classItem = VitaGlobalsUI.AddWidgetToList(m_classListItem, m_classListContent, m_classListItem.name + "_" + c.classname, "Label", c.classname);
            Toggle classToggle = classItem.GetComponent<Toggle>();
            classToggle.isOn = false;
            classToggle.group = m_classListToggleGrp;
            classToggle.onValueChanged.AddListener(OnValueChangedClassListItem);
        }
        foreach (var t in m_teachers)
        {
            m_teacherListDropdown.AddOptions(new List<string>() { t.name });
        }
        foreach (var s in m_students)
        {
            GameObject studentItem = VitaGlobalsUI.AddWidgetToList(m_studentListItem, m_studentListContent, m_studentListItem.name + "_" + s.name, "Label", s.name);
            Toggle studentToggle = studentItem.GetComponent<Toggle>();
            studentToggle.isOn = false;
            studentToggle.onValueChanged.AddListener(OnValueChangedStudentListItem);
        }
        //Update rest of UI
        if (m_classes.Count > 0)
        {
            GameObject[] classesContent = VHUtils.FindAllChildren(m_classListContent);

            int selectedIdx = 0;
            if (!string.IsNullOrEmpty(selectedName))
                selectedIdx = Array.FindIndex<GameObject>(classesContent, t => t.GetComponentInChildren<Text>().text == selectedName);

            if (selectedIdx == -1)
                selectedIdx = 0;

            EventSystem.current.SetSelectedGameObject(classesContent[selectedIdx]);
            classesContent[selectedIdx].GetComponent<Toggle>().isOn = true;
        }

        m_deleteClassButton.GetComponent<Button>().interactable = m_classes.Count > 0;
        m_saveClassButton.GetComponent<Button>().interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;
        m_addClassButton.interactable = true;
        m_refreshingClassList = false; //Blocks OnValueChangedClassListItem()
    }

    /// <summary>
    /// Grabs m_classesOriginal, m_classes
    /// </summary>
    /// <param name="loginOrganization"></param>
    void GetClasses(string loginOrganization)
    {
        m_loadingClassList.SetActive(true);
        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
        dbClass.GetAllClassesInOrganization(loginOrganization, (classes, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllClasses() error: {0}", error));
                return;
            }

            //m_classesAll = classes;
            m_classesOriginal = classes;

            // make a copy
            m_classes = new List<EntityDBVitaClass>();
            m_classesOriginal.ForEach(i => m_classes.Add(new EntityDBVitaClass(i)));

            waitingForClasses = false;
            m_loadingClassList.SetActive(false);
        });
    }

    /// <summary>
    /// Grabs m_teaches, m_students
    /// </summary>
    /// <param name="loginOrganization"></param>
    void GetTeachersStudents(string loginOrganization)
    {
        m_loadingStudentsList.SetActive(true);
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllUsersInOrganization(loginOrganization, (users, userError) =>
        {
            if (!string.IsNullOrEmpty(userError))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllUsers() error: {0}", userError));
                return;
            }

            m_teachers = new List<EntityDBVitaProfile>();
            m_teachers.Add(new EntityDBVitaProfile { username = " ", name = "--" }); //This add to match UI's empty option
            m_students = new List<EntityDBVitaProfile>();

            // filter only what we are interested in
            foreach (var user in users)
            {
                //Debug.LogFormat("{0} - {1} - {2} - {3} - {4} - {5}", user.username, user.password, user.organization, user.type, user.name, user.archived == 0 ? "Active" : "Archived" );
                if (user.archived == 0)
                {
                    if      (user.type == (int)DBUser.AccountType.TEACHER)  m_teachers.Add(user);
                    else if (user.type == (int)DBUser.AccountType.STUDENT)  m_students.Add(user);
                }
            }

            waitingForTeachersStudents = false;
            m_loadingStudentsList.SetActive(false);
        });
    }

    bool ClassStudentsEquals(EntityDBVitaClass originalClass, EntityDBVitaClass newClass)
    {
        bool studentsEqual = true;

        if (originalClass.students.Count != newClass.students.Count)
        {
            studentsEqual = false;
        }
        else
        {
            //Separate checker for students as its students may be out of order, yielding a false positive
            int stuCounter = 0;
            foreach (string studentName in newClass.students)
            {
                if (originalClass.students.Contains(studentName))
                {
                    stuCounter++;
                }
            }

            if (stuCounter != originalClass.students.Count)
            {
                studentsEqual = false;
            }
        }

        return studentsEqual;
    }

    /// <summary>
    /// Check a single assignment
    /// </summary>
    /// <param name="assignment"></param>
    /// <returns></returns>
    bool AnyChangesMade(EntityDBVitaClass classObj)
    {
        if (classObj == null) return false;
        EntityDBVitaClass classOriginal = VitaGlobalsUI.GetEntryInList<EntityDBVitaClass>(classObj.classname, "classname", m_classesOriginal);
        EntityDBVitaClass classCurrent = VitaGlobalsUI.GetEntryInList<EntityDBVitaClass>(classObj.classname, "classname", m_classes);

        //if (classOriginal == null || classCurrent == null)              Debug.LogWarningFormat("AnyChangesMade() - classNull");
        if (classOriginal == null || classCurrent == null)              return true;
        //if (classOriginal.classname != classCurrent.classname)          Debug.LogWarningFormat("AnyChangesMade() - classname - {0} - {1}", classOriginal.classname, classCurrent.classname);
        if (classOriginal.classname != classCurrent.classname)          return true;
        //if (classOriginal.organization != classCurrent.organization)    Debug.LogWarningFormat("AnyChangesMade() - organization - {0} - {1}", classOriginal.organization, classCurrent.organization);
        if (classOriginal.organization != classCurrent.organization)    return true;
        //if (classOriginal.teacher != classCurrent.teacher)              Debug.LogWarningFormat("AnyChangesMade() - teacher - {0} - {1}", classOriginal.teacher, classCurrent.teacher);
        if (classOriginal.teacher != classCurrent.teacher)              return true;
        //if (ClassStudentsEquals(classOriginal, classCurrent) == false)  Debug.LogWarningFormat("AnyChangesMade() - studentNum - {0} - {1}", classOriginal.students.Count, classCurrent.students.Count);
        if (ClassStudentsEquals(classOriginal, classCurrent) == false)  return true;

        return false;
    }

    IEnumerator BtnSaveChangesClassTeacher(EntityDBVitaClass classObj)
    {
        Debug.LogFormat("BtnSaveChanges() - changing class teacher - {0} {1}", classObj.classname, classObj.teacher);
        bool waitForTeacherSave = true;
        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();

        dbClass.UpdateClassTeacher(classObj.classname, classObj.teacher, error =>
        {
            waitForTeacherSave = false;

            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateClassTeacher() error: {0}", error));
                return;
            }

            DBAssignment  dbAssignment = GameObject.FindObjectOfType<DBAssignment>();

            dbAssignment.ChangeAssigmentsTeacher(classObj.classname, classObj.teacher, (error2 ) =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("ChangeAssigmentsTeacher() error: {0}", error2));
                    return;
                }

            });

        });





        while (waitForTeacherSave)
            yield return new WaitForEndOfFrame();

        //Update "original" list; rather than a full update that flushes everything, we handle it directly
        EntityDBVitaClass originalClass = m_classesOriginal.Find(item => item.classname == classObj.classname);
        originalClass.teacher = classObj.teacher;
    }

    IEnumerator BtnSaveChangesClassStudents(EntityDBVitaClass classObj)
    {
        Debug.LogFormat("BtnSaveChanges() - removing all students from class - {0} - {1}", classObj.classname, classObj.students.Count);
        bool waitForDelete = true;
        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();

        //Remove all students
        dbClass.RemoveAllStudentsFromClass(classObj.classname, error =>
        {
            waitForDelete = false;
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("RemoveAllStudentsFromClass() error: {0}", error));
                return;
            }
        });
        while (waitForDelete)
            yield return new WaitForEndOfFrame();

        //Add all selected students
        int studentAddCount = classObj.students.Count;
        foreach (var student in classObj.students)
        {
            Debug.LogFormat("BtnSaveChanges() - adding students to class - {0} - {1}", classObj.classname, student);
            dbClass.AddStudentToClass(classObj.classname, student, error =>
            {
                studentAddCount--;
                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("AddStudentToClass() error: {0}", error));
                    return;
                }
            });
        }
        while (studentAddCount > 0)
            yield return new WaitForEndOfFrame();

        //Update "original" list; rather than a full update that flushes everything, we handle it directly
        EntityDBVitaClass originalClass = m_classesOriginal.Find(item => item.classname == classObj.classname);
        originalClass.students.Clear();
        foreach (var s in classObj.students)
        {
            originalClass.students.Add(s);
        }
    }

    IEnumerator BtnSaveChangesClassNew(EntityDBVitaClass classObj)
    {
        Debug.LogFormat("BtnSaveChanges() - creating new class - {0}", classObj.classname);
        bool waitForNewClassFinishSaving = true;

        //Create new
        bool waitForCreate = true;
        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
        dbClass.CreateClass(classObj.classname, classObj.organization, error =>
        {
            waitForCreate = false;
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("CreateClass() error: {0}", error));
                return;
            }
        });
        while (waitForCreate)
            yield return new WaitForEndOfFrame();

        //Assign teacher
        Debug.LogFormat("BtnSaveChanges() - changing class teacher - {0} {1}", classObj.classname, classObj.teacher);
        bool waitForUpdateTeacher = true;
        dbClass.UpdateClassTeacher(classObj.classname, classObj.teacher, error =>
        {
            waitForUpdateTeacher = false;
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateClassTeacher() error: {0}", error));
                return;
            }
        });
        while (waitForUpdateTeacher)
            yield return new WaitForEndOfFrame();

        //Assign students
        int studentAddCount = classObj.students.Count;
        foreach (var student in classObj.students)
        {
            Debug.LogFormat("BtnSaveChanges() - adding students to class - {0} - {1}", classObj.classname, student);
            dbClass.AddStudentToClass(classObj.classname, student, error =>
            {
                studentAddCount--;
                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("AddStudentToClass() error: {0}", error));
                    return;
                }
            });
        }
        while (studentAddCount > 0)
            yield return new WaitForEndOfFrame();

        waitForNewClassFinishSaving = false;
        while (waitForNewClassFinishSaving)
            yield return new WaitForEndOfFrame();

        //Finally add this to the "original" list; Reorder to make sure
        m_classesOriginal.Add(classObj);
        VitaGlobalsUI.SortListAfterAddingNew<EntityDBVitaClass>(m_classes, m_classesOriginal, "classname");
    }

    void UpdateUIStates(EntityDBVitaClass classObj)
    {
        bool changesMade = AnyChangesMade(classObj);
        //Debug.LogWarningFormat("UpdateUIStates({0}), changes={1}", classObj.classname, changesMade);
        m_deleteClassButton.GetComponent<Button>().interactable = !changesMade;
        m_saveClassButton.GetComponent<Button>().interactable = changesMade;
        VitaGlobalsUI.m_unsavedChanges = changesMade;
        m_addClassButton.GetComponent<Button>().interactable = true;

        //Update widget display to indicate changes
        GameObject widget = m_classListSelectedItem;
        Text widgetText = VHUtils.FindChildRecursive(widget, "Label").GetComponent<Text>();
        VitaGlobalsUI.SetTextWithAsteriskIfChanges<Text>(widgetText, AnyChangesMade(GetSelectedClass()));

        VitaGlobalsUI.m_unsavedChanges = false;
        foreach (var c in m_classes)
        {
            bool allClassesChangesMade = AnyChangesMade(c);
            if (allClassesChangesMade)
            {
                VitaGlobalsUI.m_unsavedChanges = true;
                break;
            }
        }
    }
}
