using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelTeacherReview : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
    [Serializable]
    public class ClassAssignmentClass
    {
        [Serializable]
        public class ClassStudent
        {
            public string StudentName;
            public GameObject StudentWidget;
            public EntityDBVitaClassHomeworkAssigment StudentAssignment;
        }

        [Serializable]
        public class ClassAssignment
        {
            public string AssignmentName;
            public GameObject AssignmentWidget;
            public List<ClassStudent> AssignedStudents = new List<ClassStudent>(); //Stores assignment instances, so each list item stores a student
        }

        [Serializable]
        public class ClassAssignmentsOverview
        {
            public string ClassName;
            public int AssignedNum;
            public int LateNum;
            public int SubmittedNum;
            public int GradedNum;
            public GameObject ClassWidget;
            public List<ClassAssignment> Assignments = new List<ClassAssignment>();
        }

        public List<ClassAssignmentsOverview> m_classAssignmentsOverview = new List<ClassAssignmentsOverview>();

        #region Class Functions
        public bool AssignmentContainsStudent(string assignmentName, EntityDBVitaClassHomeworkAssigment studentAssignment)
        {
            bool returnBool = false;
            ClassAssignment ca = GetClassAssignmentFromAssignedStudent(studentAssignment);
            if (ca == null)
            {
            }
            else if (ca.AssignmentName == assignmentName)
            {
                returnBool = true;
            }
            return returnBool;
        }

        public bool ClassContainsAssignment(string className, string assignmentName)
        {
            bool returnBool = false;
            foreach (ClassAssignment classAssignment in GetClassByName(className).Assignments)
            {
                if (classAssignment.AssignmentName == assignmentName)
                {
                    returnBool = true;
                    break;
                }
            }
            return returnBool;
        }

        public bool ContainsClass(string className)
        {
            bool returnBool = false;
            foreach (ClassAssignmentsOverview classObj in m_classAssignmentsOverview)
            {
                if (classObj.ClassName == className)
                {
                    returnBool = true;
                    break;
                }
            }
            return returnBool;
        }

        public List<ClassStudent> GetAllClassStudents()
        {
            List<ClassStudent> returnList = new List<ClassStudent>();

            foreach (ClassAssignmentsOverview o in m_classAssignmentsOverview)
            {
                foreach (ClassAssignment a in o.Assignments)
                {
                    foreach (ClassStudent s in a.AssignedStudents)
                    {
                        returnList.Add(s);
                    }
                }
            }

            return returnList;
        }

        public ClassStudent GetClassStudent(EntityDBVitaClassHomeworkAssigment assignment)
        {
            ClassStudent returnClassStudent = null;

            foreach (ClassAssignmentsOverview o in m_classAssignmentsOverview)
            {
                foreach (ClassAssignment a in o.Assignments)
                {
                    foreach (ClassStudent s in a.AssignedStudents)
                    {
                        if (s.StudentAssignment == assignment)
                        {
                            returnClassStudent = s;
                            break;
                        }
                    }
                    if (returnClassStudent != null) break;
                }
                if (returnClassStudent != null) break;
            }

            return returnClassStudent;
        }

        public ClassAssignment GetClassAssignmentByName(string className, string assignmentName)
        {
            ClassAssignment returnClassAssignment = null;
            foreach (ClassAssignment classAssignment in GetClassByName(className).Assignments)
            {
                if (classAssignment.AssignmentName == assignmentName)
                {
                    returnClassAssignment = classAssignment;
                }
            }
            return returnClassAssignment;
        }

        public ClassAssignmentsOverview GetClassByName(string className)
        {
            ClassAssignmentsOverview returnClass = null;
            foreach (ClassAssignmentsOverview classObj in m_classAssignmentsOverview)
            {
                if (classObj.ClassName == className)
                {
                    returnClass = classObj;
                }
            }
            return returnClass;
        }

        /// <summary>
        /// Gets the class overview object using the unique student-assignment. This is useful for passing to RecordAssignmentStatus().
        /// </summary>
        /// <param name="studentAssignment"></param>
        /// <returns></returns>
        public ClassAssignmentsOverview GetClassAssignmentsOverviewFromAssignedStudent(EntityDBVitaClassHomeworkAssigment studentAssignment)
        {
            ClassAssignmentsOverview returnO = null;
            foreach (ClassAssignmentsOverview o in m_classAssignmentsOverview)
            {
                foreach (ClassAssignment a in o.Assignments)
                {
                    foreach (ClassStudent sa in a.AssignedStudents)
                    {
                        if (sa.StudentAssignment == studentAssignment)
                        {
                            returnO = o;
                            break;
                        }
                    }
                    if (returnO != null) break;
                }
                if (returnO != null) break;
            }
            return returnO;
        }

        public ClassAssignment GetClassAssignmentFromAssignedStudent(EntityDBVitaClassHomeworkAssigment studentAssignment)
        {
            ClassAssignment returnA = null;
            foreach (ClassAssignmentClass.ClassAssignmentsOverview o in m_classAssignmentsOverview)
            {
                foreach (ClassAssignmentClass.ClassAssignment a in o.Assignments)
                {
                    foreach (ClassAssignmentClass.ClassStudent sa in a.AssignedStudents)
                    {
                        if (sa.StudentAssignment == studentAssignment)
                        {
                            returnA = a;
                            break;
                        }
                    }
                    if (returnA != null) break;
                }
                if (returnA != null) break;
            }
            return returnA;
        }
        #endregion
    }

    MenuManager m_menuManager;
    DBAssignment m_dbAssignment;
    GameObject m_headerName;
    GameObject m_classListItem;
    GameObject m_assignmentListItem;
    GameObject m_studentListItem;
    GameObject m_classAssignmentStudentListContent;
    ClassAssignmentClass m_classAssignmentsObj = new ClassAssignmentClass();
    List<EntityDBVitaClassHomeworkAssigment> m_assignmentsList = new List<EntityDBVitaClassHomeworkAssigment>();
    ClassAssignmentClass.ClassAssignment m_selectedAssignmentTemplate;
    EntityDBVitaClassHomeworkAssigment m_selectedAssignment;
    GameObject m_classesAssignmentLoading;
    ToggleGroup m_classesToggleGrp;
    ToggleGroup m_assignmentsToggleGrp;
    ToggleGroup m_studentsToggleGrp;
    List<EntityDBVitaProfile> m_students;

    //Per class info
    GameObject m_assignedNumText;
    GameObject m_lateNumText;
    GameObject m_submittedNumText;
    GameObject m_gradedNumText;

    //Per assignment info
    GameObject m_assignmentNameText;
    GameObject m_assignmentDescriptionText;
    GameObject m_assignmentInstructionsText;
    GameObject m_dueDateText;

    //Per student info
    GameObject m_studentNameText;
    GameObject m_studentResponseText;
    GameObject m_studentGradeText;

    //Grade
    Dropdown m_submitScoreDropdown;
    Button m_submitButton;
    GameObject m_submitLoading;
    bool waitForAllClasses = true;
    bool waitForAllAssignments = true;

    //Filters
    Toggle m_filterAssignedToggle;
    Toggle m_filterLateToggle;
    Toggle m_filterSubmittedToggle;
    Toggle m_filterGradedToggle;
    #endregion

    #region Menu init
    void Start()
    {
        m_dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.TeacherReview);

        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");
        m_classesToggleGrp = VHUtils.FindChildRecursive(menu, "ClassesToggleGrp").GetComponent<ToggleGroup>();
        m_assignmentsToggleGrp = VHUtils.FindChildRecursive(menu, "AssignmentsToggleGrp").GetComponent<ToggleGroup>();
        m_studentsToggleGrp = VHUtils.FindChildRecursive(menu, "StudentsToggleGrp").GetComponent<ToggleGroup>();

        //Resources
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_classListItem = VHUtils.FindChildRecursive(resources, "GuiAssignmentClassEntryPrefab");
        m_assignmentListItem = VHUtils.FindChildRecursive(resources, "GuiAssignmentAssignmentEntryPrefab");
        m_studentListItem = VHUtils.FindChildRecursive(resources, "GuiReviewAssignmentStudentEntryPrefab");

        //Scroll list
        GameObject classAssignmentStudentList = VHUtils.FindChildRecursive(menu, "ScrollView_ClassAssignmentStudent");
        m_classAssignmentStudentListContent = VHUtils.FindChildRecursive(classAssignmentStudentList, "Content");
        m_classesAssignmentLoading = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_ClassesAssignments");

        //Class info
        m_assignedNumText = VHUtils.FindChildRecursive(menu, "TextAssignedNum");
        m_lateNumText = VHUtils.FindChildRecursive(menu, "TextLateNum");
        m_submittedNumText = VHUtils.FindChildRecursive(menu, "TextSubmittedNum");
        m_gradedNumText = VHUtils.FindChildRecursive(menu, "TextGradedNum");

        //Assignment info
        m_assignmentNameText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssignmentNum");
        m_assignmentDescriptionText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Description");
        m_assignmentInstructionsText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_StudentInstructions");
        m_dueDateText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssignmentDueDate");

        //Student info
        m_studentNameText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_StudentName");
        m_studentResponseText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_StudentResponse");
        GameObject studentGradeButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_GradeNum");
        m_studentGradeText = VHUtils.FindChildRecursive(studentGradeButton, "TextMain_NoStatus");

        //Grade
        m_submitScoreDropdown = VHUtils.FindChildRecursive(menu, "GuiDropdownPrefab_Grade").GetComponent<Dropdown>();
        m_submitButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Submit").GetComponent<Button>();
        m_submitLoading = VHUtils.FindChildRecursive(m_submitButton.gameObject, "LoadingIcon");

        //Filters
        m_filterAssignedToggle = VHUtils.FindChildRecursive(menu, "AssignedCounter").GetComponent<Toggle>();
        m_filterLateToggle = VHUtils.FindChildRecursive(menu, "LateCounter").GetComponent<Toggle>();
        m_filterSubmittedToggle = VHUtils.FindChildRecursive(menu, "SubmittedCounter").GetComponent<Toggle>();
        m_filterGradedToggle = VHUtils.FindChildRecursive(menu, "GradedCounter").GetComponent<Toggle>();
    }

    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        VitaGlobalsUI.m_unsavedChanges = false;

        //Clear menu
        VHUtils.DeleteChildren(m_classAssignmentStudentListContent.transform);
        m_assignedNumText.GetComponent<Text>().text = "--";
        m_lateNumText.GetComponent<Text>().text = "--";
        m_submittedNumText.GetComponent<Text>().text = "--";
        m_gradedNumText.GetComponent<Text>().text = "--";
        m_assignmentNameText.GetComponent<Text>().text = "Assignment ##";
        m_assignmentDescriptionText.GetComponent<Text>().text = "--";
        m_assignmentInstructionsText.GetComponent<Text>().text = "--";
        m_dueDateText.GetComponent<Text>().text = "year/month/day";
        m_studentNameText.GetComponent<Text>().text = "Student ##";
        m_studentResponseText.GetComponent<Text>().text = "--";
        m_studentGradeText.GetComponent<Text>().text = "--";
        m_submitScoreDropdown.GetComponent<Dropdown>().value = 0;
        m_classesAssignmentLoading.SetActive(false);
        m_submitLoading.SetActive(false);

        m_filterAssignedToggle.isOn = false;
        m_filterLateToggle.isOn = false;
        m_filterSubmittedToggle.isOn = false;
        m_filterGradedToggle.isOn = false;

        StartCoroutine(UpdateClassAssignmentList());
        UpdateUIStates();
    }

    public void OnMenuExit()
    {
        VitaGlobalsUI.m_unsavedChanges = false;
    }
    #endregion

    #region UI Hooks
    public void BtnBack()
    {
    }

    public void BtnHome()
    {
    }

    public void OnValueChangedGrade(int value)
    {
        Debug.LogFormat("OnValueChangedGrade()");
        VitaGlobalsUI.m_unsavedChanges = true;
    }

    public void BtnGradeAssignment()
    {
        Debug.Log("BtnGradeAssignment()");

        StartCoroutine(BtnGradeAssignmentInternal());
    }

    IEnumerator BtnGradeAssignmentInternal()
    {
        if (m_selectedAssignment == null)
            yield break;

        if (m_selectedAssignment.status == (int)DBAssignment.Status.GRADED)
        {
            Debug.LogWarning("Assignment already graded. Nothing happened.");
            yield break;
        }

        m_submitLoading.SetActive(true);
        m_submitButton.interactable = false;
        m_submitScoreDropdown.interactable = false;
        int grade = m_submitScoreDropdown.value - 1;
        grade = (grade == -1) ? m_selectedAssignment.grade : grade; // if 0, score is unassigned; use value in DB

        bool waitForDB = true;

        m_dbAssignment.UpdateAssignment(m_selectedAssignment.id, m_selectedAssignment.description, m_selectedAssignment.instructions, m_selectedAssignment.studentresponse, grade, m_selectedAssignment.datedue, DBAssignment.Status.GRADED, error =>
        {
            waitForDB = false;

            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateAssignment() error: {0}", error));
                return;
            }
        });

        while (waitForDB)
            yield return new WaitForEndOfFrame();

        waitForDB = true;

        // get the user profile to check badges
        EntityDBVitaProfile badgeCheckUserProfile = null;
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetUser(m_selectedAssignment.student, (profile, error) =>
        {
            waitForDB = false;

            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetUser() - {0} - error: {1}", m_selectedAssignment.student, error));
                return;
            }

            badgeCheckUserProfile = profile;
        });

        while (waitForDB)
            yield return new WaitForEndOfFrame();

        waitForDB = true;

        if (grade == 5)
        {
            if (!dbUser.DoesUserHaveBadge(badgeCheckUserProfile, "High Five"))
            {
                Debug.LogFormat("AddBadge() - {0}", "High Five");

                dbUser.AddBadge(m_selectedAssignment.student, DBUser.BadgeData.NewBadge("High Five"), (profile, error) => 
                {
                    waitForDB = false;

                    if (!string.IsNullOrEmpty(error))
                    {
                        PopUpDisplay.Instance.Display("Error", error);
                        return;
                    }

                    badgeCheckUserProfile = profile;
                });

                while (waitForDB)
                    yield return new WaitForEndOfFrame();
            }
        }

        //Remove this assignment's count from counters (One of assigned/late/submitted) so upon RecordAssignmentStatus(), "Graded" will get +1
        ClassAssignmentClass.ClassAssignmentsOverview co = m_classAssignmentsObj.GetClassAssignmentsOverviewFromAssignedStudent(m_selectedAssignment);
        if (m_selectedAssignment.status == (int)DBAssignment.Status.ASSIGNED)
        {
            if (VitaGlobals.TicksToDateTime(m_selectedAssignment.datedue) < DateTime.Now)
            {
                co.LateNum--;
            }
            else
            {
                co.AssignedNum--;
            }
        }
        else if (m_selectedAssignment.status == (int)DBAssignment.Status.SUBMITTED)
        {
            co.SubmittedNum--;
        }

        m_selectedAssignment.grade = grade;
        m_selectedAssignment.status = (int)DBAssignment.Status.GRADED;
        RecordAssignmentStatus(m_classAssignmentsObj.GetClassAssignmentsOverviewFromAssignedStudent(m_selectedAssignment), m_selectedAssignment);
        m_classAssignmentsObj.GetClassStudent(m_selectedAssignment).StudentWidget.GetComponent<AGGuiAssignmentStudent>().SetStatus(m_selectedAssignment);
        UpdateClassInfo(co);
        UpdateUIStates();
    } 

    public void BtnSearch()
    {
        Debug.Log("BtnSearch()");
    }

    public void OnClickClassListItem(string className) { ClassAssignmentClass.ClassAssignmentsOverview classObj = m_classAssignmentsObj.GetClassByName(className); OnClickClassListItem(classObj); }
    public void OnClickClassListItem(ClassAssignmentClass.ClassAssignmentsOverview classObj)
    {
        //Turn off assignment selection if need be
        if (m_selectedAssignmentTemplate != null)
        {
            if (!m_classAssignmentsObj.ClassContainsAssignment(classObj.ClassName, m_selectedAssignmentTemplate.AssignmentName))
            {
                foreach (ClassAssignmentClass.ClassAssignmentsOverview o in m_classAssignmentsObj.m_classAssignmentsOverview)
                {
                    foreach (ClassAssignmentClass.ClassAssignment a in o.Assignments)
                    {
                        if (a.AssignmentName == m_selectedAssignmentTemplate.AssignmentName)
                        {
                            Toggle assignmentToggle = VHUtils.FindChildRecursive(a.AssignmentWidget, "TextAssignmentNum").GetComponent<Toggle>();
                            m_assignmentsToggleGrp.allowSwitchOff = true;
                            assignmentToggle.isOn = false;
                            m_assignmentsToggleGrp.allowSwitchOff = false;
                            m_selectedAssignmentTemplate = null;
                            break;
                        }
                    }
                }

                //Turn off student selection when turning off assignment selection
                if (m_selectedAssignment != null)
                {
                    foreach (ClassAssignmentClass.ClassAssignmentsOverview o in m_classAssignmentsObj.m_classAssignmentsOverview)
                    {
                        foreach (ClassAssignmentClass.ClassAssignment a in o.Assignments)
                        {
                            foreach (ClassAssignmentClass.ClassStudent s in a.AssignedStudents)
                            {
                                Toggle studentToggle = VHUtils.FindChildRecursive(s.StudentWidget, "TextStudentName").GetComponent<Toggle>();
                                if (studentToggle.isOn)
                                {
                                    m_studentsToggleGrp.allowSwitchOff = true;
                                    studentToggle.isOn = false;
                                    m_studentsToggleGrp.allowSwitchOff = false;
                                    m_selectedAssignment = null;
                                    break;
                                }
                            }

                        }
                    }
                }
            }
        }

        if (m_selectedAssignmentTemplate == null && m_selectedAssignment == null)
        {
            UpdateClassInfo(classObj);
        }
        UpdateUIStates();
    }

    void UpdateClassInfo(ClassAssignmentClass.ClassAssignmentsOverview classObj)
    {
        m_assignedNumText.GetComponent<Text>().text = classObj.AssignedNum.ToString();
        m_lateNumText.GetComponent<Text>().text = classObj.LateNum.ToString();
        m_submittedNumText.GetComponent<Text>().text = classObj.SubmittedNum.ToString();
        m_gradedNumText.GetComponent<Text>().text = classObj.GradedNum.ToString();

        //Clear assignment & student info
        if (m_selectedAssignmentTemplate == null)
        {
            m_assignmentNameText.GetComponent<Text>().text = "--";
            m_assignmentDescriptionText.GetComponent<Text>().text = "--";
            m_assignmentInstructionsText.GetComponent<Text>().text = "--";
            m_dueDateText.GetComponent<Text>().text = "--";
        }
        if (m_selectedAssignment == null)
        {
            m_studentNameText.GetComponent<Text>().text = "--";
            m_studentResponseText.GetComponent<Text>().text = "--";
            m_studentGradeText.GetComponent<Text>().text = "--";
        }
    }

    void UpdateAssignmentInfo(EntityDBVitaClassHomeworkAssigment assignment)
    {
        m_assignmentNameText.GetComponent<Text>().text = assignment.name;
        m_assignmentDescriptionText.GetComponent<Text>().text = assignment.description;
        m_assignmentInstructionsText.GetComponent<Text>().text = assignment.instructions;
        m_dueDateText.GetComponent<Text>().text = VitaGlobals.TicksToString(assignment.datedue);
        m_submitScoreDropdown.value = assignment.grade + 1;

        //Clear student info
        if (m_selectedAssignment == null)
        {
            m_studentNameText.GetComponent<Text>().text = "--";
            m_studentResponseText.GetComponent<Text>().text = "--";
            m_studentGradeText.GetComponent<Text>().text = "--";
            m_submitScoreDropdown.value = 0;
        }
    }

    public void OnClickAssignmentListItem(EntityDBVitaClassHomeworkAssigment assignment)
    {
        m_selectedAssignmentTemplate = new ClassAssignmentClass.ClassAssignment { AssignmentName = assignment.name };
        m_selectedAssignmentTemplate = m_classAssignmentsObj.GetClassAssignmentFromAssignedStudent(assignment); //This seems to take some time, hence the previous line

        //Update class & assignment info
        ClassAssignmentClass.ClassAssignmentsOverview co = m_classAssignmentsObj.GetClassAssignmentsOverviewFromAssignedStudent(assignment);
        GameObject classWidget = co.ClassWidget;
        Toggle classToggle = VHUtils.FindChildRecursive(classWidget, "TextClass").GetComponent<Toggle>();
        if (classToggle.isOn) classToggle.onValueChanged.Invoke(true);
        classToggle.isOn = true;

        //If the currently selected student assignment isn't the overall specified assignment, clear student selection
        if (m_selectedAssignment != null)
        {
            if (!m_classAssignmentsObj.AssignmentContainsStudent(m_selectedAssignmentTemplate.AssignmentName, m_selectedAssignment))
            {
                ClassAssignmentClass.ClassStudent cs = m_classAssignmentsObj.GetClassStudent(m_selectedAssignment);
                if (cs == null)
                {
                    Debug.LogWarningFormat("Assignment ({0}) contains student, but cannot grab the widget object.", m_selectedAssignment.name);
                }
                else
                {
                    m_studentsToggleGrp.allowSwitchOff = true;
                    cs.StudentWidget.GetComponentInChildren<Toggle>().isOn = false;
                    m_studentsToggleGrp.allowSwitchOff = false;
                }
                m_selectedAssignment = null;
            }
        }
        else
        {
        }

        UpdateClassInfo(co);
        UpdateAssignmentInfo(assignment);
        UpdateUIStates();
    }

    public void OnClickStudentListItem(EntityDBVitaClassHomeworkAssigment assignment)
    {
        m_selectedAssignmentTemplate = m_classAssignmentsObj.GetClassAssignmentFromAssignedStudent(assignment);
        m_selectedAssignment = assignment;

        //Update class & assignment info
        GameObject classWidget = m_classAssignmentsObj.GetClassAssignmentsOverviewFromAssignedStudent(assignment).ClassWidget;
        Toggle classToggle = VHUtils.FindChildRecursive(classWidget, "TextClass").GetComponent<Toggle>();
        if (classToggle.isOn)       classToggle.onValueChanged.Invoke(true);
        else                        classToggle.isOn = true;

        GameObject assignmentWidget = m_classAssignmentsObj.GetClassAssignmentFromAssignedStudent(assignment).AssignmentWidget;
        Toggle assignmentToggle = VHUtils.FindChildRecursive(assignmentWidget, "TextAssignmentNum").GetComponent<Toggle>();
        if (assignmentToggle.isOn)  assignmentToggle.onValueChanged.Invoke(true);
        else                        assignmentToggle.isOn = true;

        //Info
        UpdateClassInfo(m_classAssignmentsObj.GetClassAssignmentsOverviewFromAssignedStudent(assignment));
        UpdateAssignmentInfo(assignment);
        m_studentNameText.GetComponent<Text>().text = m_students.Find(item => item.username == assignment.student).name;
        m_studentResponseText.GetComponent<Text>().text = assignment.studentresponse;
        m_studentGradeText.GetComponent<Text>().text = assignment.grade.ToString();
        UpdateUIStates();
    }

    void UpdateUIStates()
    {
        m_submitScoreDropdown.interactable = false;
        m_submitButton.interactable = false;
        m_submitLoading.SetActive(false);
        VitaGlobalsUI.m_unsavedChanges = false;

        if (m_selectedAssignment != null)
        {
            if(m_selectedAssignment.status != (int)DBAssignment.Status.GRADED)
            {
                m_submitScoreDropdown.interactable = true;
                m_submitButton.interactable = true;
            }
        }

        UpdateFilteredList();
    }

    public void OnValueChangeFilter()
    {
        UpdateFilteredList();
    }

    void UpdateFilteredList()
    {
        foreach (ClassAssignmentClass.ClassStudent s in m_classAssignmentsObj.GetAllClassStudents())
        {
            //If all filters are off, show everything
            if (!m_filterAssignedToggle.isOn && !m_filterLateToggle.isOn && !m_filterSubmittedToggle.isOn && !m_filterGradedToggle.isOn)
            {
                s.StudentWidget.SetActive(true);
                continue;
            }

            if      (GetAssignmentStatusAG(s.StudentAssignment) == AGGuiAssignmentStudent.AssignmentStatus.Assigned)    s.StudentWidget.SetActive(m_filterAssignedToggle.isOn);
            else if (GetAssignmentStatusAG(s.StudentAssignment) == AGGuiAssignmentStudent.AssignmentStatus.Late)        s.StudentWidget.SetActive(m_filterLateToggle.isOn);
            else if (GetAssignmentStatusAG(s.StudentAssignment) == AGGuiAssignmentStudent.AssignmentStatus.Submitted)   s.StudentWidget.SetActive(m_filterSubmittedToggle.isOn);
            else if (GetAssignmentStatusAG(s.StudentAssignment) == AGGuiAssignmentStudent.AssignmentStatus.Graded)      s.StudentWidget.SetActive(m_filterGradedToggle.isOn);
        }
    }
    #endregion

    #region Private functions
    IEnumerator UpdateClassAssignmentList()
    {
        //Grab classes and assignments and wait for DB interactions to be done
        m_classAssignmentsObj = new ClassAssignmentClass(); //Reset whole class-assignments struct
        m_assignmentsList = new List<EntityDBVitaClassHomeworkAssigment>();
        waitForAllAssignments = true;
        waitForAllClasses = true;
        yield return GetStudents();
        GetAllClasses();
        GetAllAssignments();

        m_classesAssignmentLoading.SetActive(true);
        while (waitForAllAssignments || waitForAllClasses)  yield return new WaitForEndOfFrame();
        m_classesAssignmentLoading.SetActive(false);

        PopulateScrollList();
    }

    void GetAllClasses()
    {
        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
        dbClass.GetAllClasses((classes, error) =>
        {
            waitForAllClasses = true; //Start waiting
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllClasses() error: {0}", error));
                return;
            }

            foreach (EntityDBVitaClass c in classes)
            {
                EntityDBVitaClass classObj = c;
                if (c.organization != VitaGlobals.m_loggedInProfile.organization)   continue;
                if (c.teacher != VitaGlobals.m_loggedInProfile.username)            continue; //Not this teacher

                //Add class
                ClassAssignmentClass.ClassAssignmentsOverview newClassObj = new ClassAssignmentClass.ClassAssignmentsOverview { ClassName = classObj.classname, AssignedNum = 0, LateNum = 0, SubmittedNum = 0, GradedNum = 0 };
                m_classAssignmentsObj.m_classAssignmentsOverview.Add(newClassObj);
            }
            waitForAllClasses = false; //Done waiting
        });
    }

    void GetAllAssignments()
    {
        m_dbAssignment.GetAllAssignments((assignments, error1) =>
        {
            waitForAllAssignments = true; //Start waiting
            foreach (EntityDBVitaClassHomeworkAssigment a in assignments)
            {
                if (!string.IsNullOrEmpty(error1))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("GetAllAssignmentsInClass() error: {0}", error1));
                    return;
                }

                //Weed out assignments
                if (a.organization != VitaGlobals.m_loggedInProfile.organization)   continue; //Not this organization
                if (a.teacher != VitaGlobals.m_loggedInProfile.username)            continue; //Not this teacher
                if (a.status == 0)                                                  continue; //Template; aka non-assigned
                if (m_students.Find(item => item.username == a.student) == null)    continue; //Archived/non-existing student

                //Debug.Log("<color=#FF0066>Checking name: " + a.name + ", student: " + a.student + ", teacher: "+a.teacher+",class: " + a.classname + ", org: "+a.organization+", due: "+VitaGlobals.TicksToString(a.datedue)+"</color>");
                m_assignmentsList.Add(a);
            }
            waitForAllAssignments = false; //Done waiting
        });
    }

    IEnumerator GetStudents()
    {
        yield return null;

        bool waitingForStudents = true;
        m_students = new List<EntityDBVitaProfile>();
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.GetAllStudentsInOrganization(VitaGlobals.m_loggedInProfile.organization, (users, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllUsers() error: {0}", error));
                return;
            }

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

    AGGuiAssignmentStudent.AssignmentStatus GetAssignmentStatusAG(EntityDBVitaClassHomeworkAssigment assignment)
    {
        AGGuiAssignmentStudent.AssignmentStatus returnStatus = AGGuiAssignmentStudent.AssignmentStatus.Template;

        if (assignment.status == (int)DBAssignment.Status.ASSIGNED)
        {
            if (VitaGlobals.TicksToDateTime(assignment.datedue) < DateTime.Now)
            {
                returnStatus = AGGuiAssignmentStudent.AssignmentStatus.Late;
            }
            else
            {
                returnStatus = AGGuiAssignmentStudent.AssignmentStatus.Assigned;
            }
        }
        else if (assignment.status == (int)DBAssignment.Status.SUBMITTED)   returnStatus = AGGuiAssignmentStudent.AssignmentStatus.Submitted;
        else if (assignment.status == (int)DBAssignment.Status.GRADED)      returnStatus = AGGuiAssignmentStudent.AssignmentStatus.Graded;

        return returnStatus;
    }

    void PopulateScrollList()
    {
        //Add classes
        //foreach (ClassAssignmentClass.ClassAssignmentsOverview c in m_classAssignmentsObj.m_classAssignmentsOverview)
        for (int i = 0; i < m_classAssignmentsObj.m_classAssignmentsOverview.Count; i++ )
        {
            GameObject classItem = VitaGlobalsUI.AddWidgetToList(m_classListItem, m_classAssignmentStudentListContent, m_classListItem.name, "TextClass", "Class: " + m_classAssignmentsObj.m_classAssignmentsOverview[i].ClassName);
            m_classAssignmentsObj.m_classAssignmentsOverview[i].ClassWidget = classItem;
            classItem.GetComponentInChildren<Toggle>().group = m_classesToggleGrp;
            ClassAssignmentClass.ClassAssignmentsOverview classInDelegate = m_classAssignmentsObj.m_classAssignmentsOverview[i];
            classItem.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => { if (value) OnClickClassListItem(classInDelegate); });
        }

        //Add assignments
        foreach (EntityDBVitaClassHomeworkAssigment a in m_assignmentsList)
        {
            ClassAssignmentClass.ClassAssignmentsOverview classObj = m_classAssignmentsObj.GetClassByName(a.classname);
            if (classObj == null)
            {
                Debug.Log("Class for this assignment does not exist, or does not belong to this teacher; Skipping.");
                continue;
            }

            EntityDBVitaClassHomeworkAssigment assignmentEntry = a;
            ClassAssignmentClass.ClassAssignment classAssignmentObj = null;

            //Create new assignment entry if it isn't already in the class
            if (!m_classAssignmentsObj.ClassContainsAssignment(classObj.ClassName, a.name))
            {
                GameObject assignmentItem = VitaGlobalsUI.AddWidgetToList(m_assignmentListItem, VHUtils.FindChildRecursive(classObj.ClassWidget, "Content"), m_assignmentListItem.name, "TextAssignmentNum", "Assignment: " + a.name);
                assignmentItem.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => { if (value) OnClickAssignmentListItem(assignmentEntry); });
                assignmentItem.GetComponentInChildren<Toggle>().group = m_assignmentsToggleGrp;

                classAssignmentObj = new ClassAssignmentClass.ClassAssignment
                {
                    AssignmentName = a.name,
                    AssignmentWidget = assignmentItem,
                    AssignedStudents = new List<ClassAssignmentClass.ClassStudent>() { }
                };

                classObj.Assignments.Add(classAssignmentObj);
            }
            else
            {
                classAssignmentObj = m_classAssignmentsObj.GetClassAssignmentByName(classObj.ClassName, a.name);
            }

            //Add the assigned student entry (StudentWidget to be added in the next section)
            classAssignmentObj.AssignedStudents.Add(new ClassAssignmentClass.ClassStudent { StudentName = a.student, StudentWidget = null, StudentAssignment = a });
        }

        //Add students and update class assignment info counters
        foreach (ClassAssignmentClass.ClassAssignmentsOverview classroom in m_classAssignmentsObj.m_classAssignmentsOverview)
        {
            foreach (ClassAssignmentClass.ClassAssignment classAss in classroom.Assignments)
            {
                foreach (ClassAssignmentClass.ClassStudent assEntry in classAss.AssignedStudents)
                {
                    EntityDBVitaClassHomeworkAssigment assignmentEntryStudent = assEntry.StudentAssignment;
                    GameObject studentWidget = VitaGlobalsUI.AddWidgetToList(m_studentListItem, VHUtils.FindChildRecursive(classAss.AssignmentWidget, "Content"), m_studentListItem.name, "TextStudentName", m_students.Find(item => item.username == assignmentEntryStudent.student).name);
                    studentWidget.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => { if (value) OnClickStudentListItem(assignmentEntryStudent); });
                    studentWidget.GetComponentInChildren<Toggle>().group = m_studentsToggleGrp;
                    studentWidget.GetComponent<AGGuiAssignmentStudent>().SetStatus(assignmentEntryStudent);
                    assEntry.StudentWidget = studentWidget;
                    RecordAssignmentStatus(classroom, assignmentEntryStudent);
                }
            }
        }

        UpdateUIStates();
    }

    /// <summary>
    /// Sets Assigned/Late/Submitted/Graded info
    /// </summary>
    /// <param name="classObj"></param>
    /// <param name="assignment"></param>
    void RecordAssignmentStatus(ClassAssignmentClass.ClassAssignmentsOverview classObj, EntityDBVitaClassHomeworkAssigment assignment)
    {
        if      (GetAssignmentStatusAG(assignment) == AGGuiAssignmentStudent.AssignmentStatus.Assigned)     classObj.AssignedNum++;
        else if (GetAssignmentStatusAG(assignment) == AGGuiAssignmentStudent.AssignmentStatus.Late)         classObj.LateNum++;
        else if (GetAssignmentStatusAG(assignment) == AGGuiAssignmentStudent.AssignmentStatus.Submitted)    classObj.SubmittedNum++;
        else if (GetAssignmentStatusAG(assignment) == AGGuiAssignmentStudent.AssignmentStatus.Graded)       classObj.GradedNum++;

        ////Debug.Log(string.Format("RecordAssignmentStatus({0}, {1})", classObj.ClassName, assignment.name));
        //if (assignment.status == (int)DBAssignment.Status.ASSIGNED)
        //{
        //    // Compare ticks; Make sure to do else-if to check against late vs. graded etc.
        //    if (assignment.datedue.Contains("/"))
        //    {
        //        Debug.LogWarning(assignment.datedue.ToString() + " skipping wrong date format. This should be prevented in future versions. (Joe)");
        //        return;
        //    }

        //    if (VitaGlobals.TicksToDateTime(assignment.datedue) < DateTime.Now)
        //    {
        //        classObj.LateNum++;
        //    }
        //    else
        //    {
        //        classObj.AssignedNum++;
        //    }
        //}
        //else if (assignment.status == (int)DBAssignment.Status.SUBMITTED)   classObj.SubmittedNum++;
        //else if (assignment.status == (int)DBAssignment.Status.GRADED)      classObj.GradedNum++;
    }
    #endregion
}
