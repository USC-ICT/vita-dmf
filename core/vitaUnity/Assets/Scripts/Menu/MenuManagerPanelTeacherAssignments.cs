using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Linq;

public class MenuManagerPanelTeacherAssignments : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
    [Serializable]
    public class WidgetObject
    {
        public string Name;
        public GameObject Widget;
    }

    List<WidgetObject> m_teacherWidgets = new List<WidgetObject>();
    List<EntityDBVitaProfile> m_students;
    List<WidgetObject> m_studentWidgets = new List<WidgetObject>();
    List<EntityDBVitaClass> m_classes;
    List<WidgetObject> m_classesWidgets = new List<WidgetObject>();
    List<WidgetObject> m_assignmentWidgets = new List<WidgetObject>();
    List<EntityDBVitaClassHomeworkAssigment> m_assignmentsAll;
    List<EntityDBVitaClassHomeworkAssigment> m_assignmentsOriginal;
    List<EntityDBVitaClassHomeworkAssigment> m_assignments;
    EntityDBVitaClassHomeworkAssigment m_selectedAssignment;
    string m_lastSelectedAssignmentId;

    MenuManager m_menuManager;
    GameObject m_headerName;
    GameObject m_classesStudentsListContent;
    GameObject m_assignmentsListContent;
    InputField m_assignmentNameText;
    GameObject m_assignmentDescriptionText;
    AGDropdownDate m_dueDate;
    GameObject m_instructionText;
    GameObject m_loadingAssignments;
    GameObject m_loadingClassesStudents;
    GameObject m_loadingAssign;
    GameObject m_loadingSave;
    GameObject m_loadingDelete;

    // Resources
    GameObject m_classEntryItem;
    GameObject m_studentEntryItem;
    GameObject m_assignedByEntryItem;
    GameObject m_assignmentEntryItem;
    GameObject m_assignedBySpacerItem; //This only for visual effects, giving some padding between "assigned by" widgets

    GameObject m_createAssignmentButton;
    GameObject m_duplicateAssignmentButton;
    GameObject m_deleteAssignmentButton;
    GameObject m_saveAssignmentButton;
    GameObject m_assignAssignmentButton;

    GameObject m_assignmentListSelectedItem;
    GameObject m_classListSelectedItem;
    ToggleGroup m_classListSelectionToggleGrp;
    #endregion

    #region Menu init
    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.TeacherAssignments);

        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");

        //Resources
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_classEntryItem = VHUtils.FindChildRecursive(resources, "GuiAssignmentClassEntryTogglePrefab");
        m_studentEntryItem = VHUtils.FindChildRecursive(resources, "GuiAssignmentStudentEntryPrefab");
        m_assignedByEntryItem = VHUtils.FindChildRecursive(resources, "GuiAssignmentAssignedByPrefab");
        m_assignmentEntryItem = VHUtils.FindChildRecursive(resources, "GuiAssignmentPrefab");
        m_assignedBySpacerItem = VHUtils.FindChildRecursive(resources, "GuiSpacerPrefab");

        //Scroll list
        GameObject m_classesStudentsList = VHUtils.FindChildRecursive(menu, "ScrollView_ClassesStudents");
        m_classesStudentsListContent = VHUtils.FindChildRecursive(m_classesStudentsList, "Content");
        GameObject m_assignmentsList = VHUtils.FindChildRecursive(menu, "ScrollView_Assignments");
        m_assignmentsListContent = VHUtils.FindChildRecursive(m_assignmentsList, "Content");
        

        m_assignmentNameText = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_AssignmentName").GetComponent<InputField>();
        m_assignmentDescriptionText = VHUtils.FindChildRecursive(menu, "GuiInputScrollFieldPrefab_Description");
        m_dueDate = VHUtils.FindChildRecursive(menu, "GuiDropdownDatePrefab_Due").GetComponent<AGDropdownDate>();
        m_instructionText = VHUtils.FindChildRecursive(menu, "GuiInputScrollFieldPrefab_Instructions");

        m_createAssignmentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_CreateNew");
        m_duplicateAssignmentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_DuplicateSelected");
        m_deleteAssignmentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Delete");
        m_saveAssignmentButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Save");
        m_assignAssignmentButton = VHUtils.FindChildRecursive(VHUtils.FindChild(menu, "MainSection"), "GuiButtonPrefab_Assign");

        m_loadingAssignments = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_Assignments");
        m_loadingClassesStudents = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_ClassesStudents");
        m_loadingAssign = VHUtils.FindChildRecursive(m_createAssignmentButton.gameObject, "LoadingIcon");
        m_loadingSave = VHUtils.FindChildRecursive(m_saveAssignmentButton.gameObject, "LoadingIcon");
        m_loadingDelete = VHUtils.FindChildRecursive(m_deleteAssignmentButton.gameObject, "LoadingIcon");

        m_classListSelectionToggleGrp = VHUtils.FindChildRecursive(menu, "ClassListToggleGrp").GetComponent<ToggleGroup>();
        m_lastSelectedAssignmentId = "";
    }

    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        VitaGlobalsUI.m_unsavedChanges = false;

        StartCoroutine(RefreshClassList());
        RefreshAssignmentsList();
        m_loadingAssign.SetActive(false);
        m_loadingSave.SetActive(false);
        m_loadingDelete.SetActive(false);
    }

    public void OnMenuExit()
    {
        VitaGlobalsUI.m_unsavedChanges = false;
    }
    #endregion

    #region UI Hooks
    public void BtnAssignAssignment()
    {
        var selected = GetSelectedAssignment();
        if (selected == null)
            return;

        if (m_classListSelectedItem == null)
            return;

        StartCoroutine(BtnAssignAssignmentsInternal());
    }

    IEnumerator BtnAssignAssignmentsInternal()
    {
        m_loadingAssign.SetActive(true);
        m_assignAssignmentButton.GetComponent<Button>().interactable = false;
        var selected = GetSelectedAssignment();
        GameObject classItemContent = VHUtils.FindChildRecursive(m_classListSelectedItem, "Content");
        GameObject[] studentsInSelectedClass = VHUtils.FindAllChildren(classItemContent);
        string classname = VHUtils.FindChildRecursive(m_classListSelectedItem, "TextClass").GetComponent<Text>().text;
        string duedate = m_dueDate.GetDate();
        int numberOfStudents = studentsInSelectedClass.Length;

        foreach (var studentItem in studentsInSelectedClass)
        {
            WidgetObject studentWidget = m_studentWidgets.Find(item => item.Widget == studentItem);
            Debug.LogFormat("CreateAssignment() - {0} {1} {2} {3} {4} {5} {6} {7}", selected.name, selected.description, selected.instructions, selected.organization, selected.teacher, studentWidget.Name, classname, duedate);

            DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
            dbAssignment.CreateAssignment(selected.name, selected.description, selected.instructions, selected.organization, selected.teacher, studentWidget.Name, classname, VitaGlobals.StringToTicksString(duedate), (id, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("CreateAssignment() error: {0}", error));
                    return;
                }
                numberOfStudents--;
            });
        }

        while (numberOfStudents > 0)
            yield return new WaitForEndOfFrame();

        RefreshAssignmentsList();
        m_assignAssignmentButton.GetComponent<Button>().interactable = true;
        m_loadingAssign.SetActive(false);
    }

    public void BtnCreateNewAssignment()
    {
        PopUpDisplay.Instance.DisplayOkCancelInput("Add Assignment", "Enter Assignment Name", (input) =>
        {
            string assignmentName = input;

            string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
            string loginUsername = VitaGlobals.m_loggedInProfile.username;

            //Note that this creates a new, but misleading GUID for this assignment
            EntityDBVitaClassHomeworkAssigment newAssignment = new EntityDBVitaClassHomeworkAssigment(assignmentName, "Description goes here...", "Instructions go here...", " ", -1, loginOrganization, loginUsername, " ", " ", VitaGlobals.MaxTimeToString(), (int)DBAssignment.Status.TEMPLATE);
            m_assignments.Add(newAssignment);
            m_selectedAssignment = newAssignment; //Won't assign "m_lastSelectedAssignmentId" right now, since this would be a false value

            // Add entry into "assigned by" list (if doesn't exist)
            GameObject assignedByItem = null;
            if (GetWidget(newAssignment.teacher, m_teacherWidgets) == null)
            {
                assignedByItem = VitaGlobalsUI.AddWidgetToList(m_assignedByEntryItem, m_assignmentsListContent, m_assignedByEntryItem + "_" + newAssignment.teacher, "TextAssignedBy", "Assigned by " + newAssignment.teacher);
                m_teacherWidgets.Add(new WidgetObject { Name = newAssignment.teacher, Widget = assignedByItem });
                GameObject spacerItem = Instantiate(m_assignedBySpacerItem);
                spacerItem.transform.SetParent(m_assignmentsListContent.transform);
                spacerItem.SetActive(true);
            }
            else
            {
                assignedByItem = GetWidget(newAssignment.teacher, m_teacherWidgets).Widget;
            }

            GameObject newAssignmentObj = VitaGlobalsUI.AddWidgetToList(m_assignmentEntryItem, VHUtils.FindChildRecursive(assignedByItem, "Content"), m_assignmentEntryItem.name + "_" + newAssignment.name, "TextAssignmentName", newAssignment.name);
            VHUtils.FindChildRecursive(newAssignmentObj, "TextAssignmentDescription").GetComponent<Text>().text = newAssignment.description;
            m_assignmentWidgets.Add(new WidgetObject { Name = newAssignment.name, Widget = newAssignmentObj });
            newAssignmentObj.GetComponent<Button>().onClick.AddListener(delegate
            {
                OnClickAssignmentListItem(newAssignmentObj, newAssignment);
            });

            EventSystem.current.SetSelectedGameObject(newAssignmentObj);
            newAssignmentObj.GetComponent<Button>().onClick.Invoke();

            UpdateUIButtonStates();
        }, null);
    }

    public void BtnDeleteAssignment()
    {
        PopUpDisplay.Instance.DisplayYesNo(VitaGlobals.Confirm, "Confirm delete assignment? This action cannot be undone.", delegate
        {
            if (m_selectedAssignment == null)
                return;

            Debug.LogFormat("DeleteAssignment() - {0} - {1}", m_selectedAssignment.name, m_selectedAssignment.id);

            //Catch freshly duplicated/new assignment that is to be deleted
            if (m_assignmentsOriginal.Find(item => item.id == m_selectedAssignment.id) == null)
            {
                m_assignments.Remove(m_assignments.Find(item => item.id == m_selectedAssignment.id));
                RefreshAssignmentsList();
                return;
            }

            DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
            dbAssignment.DeleteAssignment(m_selectedAssignment.id, error =>
            {
                m_loadingDelete.SetActive(true);
                SetUIButtonStates(false);
                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("DeleteAssignment() error: {0}", error));
                    return;
                }

                m_loadingDelete.SetActive(false);
                RefreshAssignmentsList();
            });
        }, null);
    }

    public void BtnDuplicateAssignment()
    {
        if (m_selectedAssignment == null) return;

        //Note that this creates a new, but misleading GUID for this assignment
        EntityDBVitaClassHomeworkAssigment newAssignment = new EntityDBVitaClassHomeworkAssigment(m_selectedAssignment);
        newAssignment.id = "DuplicatedAssignmentNoID";
        newAssignment.name = CreateUniqueAssignmentName(newAssignment.name);
        newAssignment.organization = VitaGlobals.m_loggedInProfile.organization;
        newAssignment.teacher = VitaGlobals.m_loggedInProfile.username;
        m_assignments.Add(newAssignment);
        m_selectedAssignment = newAssignment; //Won't assign "m_lastSelectedAssignmentId" right now, since this would be a false value

        // Add entry into "assigned by" list
        GameObject assignedByItem = GetWidget(newAssignment.teacher, m_teacherWidgets).Widget;

        //Create new widget
        GameObject newAssignmentObj = VitaGlobalsUI.AddWidgetToList(m_assignmentEntryItem, VHUtils.FindChildRecursive(assignedByItem, "Content"), m_assignmentEntryItem.name + "_" + newAssignment.name, "TextAssignmentName", newAssignment.name);
        VHUtils.FindChildRecursive(newAssignmentObj, "TextAssignmentDescription").GetComponent<Text>().text = newAssignment.description;
        m_assignmentWidgets.Add(new WidgetObject { Name = newAssignment.name, Widget = newAssignmentObj });
        newAssignmentObj.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnClickAssignmentListItem(newAssignmentObj, newAssignment);
        });

        EventSystem.current.SetSelectedGameObject(newAssignmentObj);
        newAssignmentObj.GetComponent<Button>().onClick.Invoke();

        UpdateUIButtonStates();
    }

    public void BtnSaveAssignment() { StartCoroutine(BtnSaveChangesInternal()); }
    IEnumerator BtnSaveChangesInternal()
    {
        m_loadingSave.SetActive(true);
        SetUIButtonStates(false);
        yield return StartCoroutine(BtnSaveChangesAssignmentName());
        yield return StartCoroutine(BtnSaveChangesAssignmentsNew());
        yield return StartCoroutine(BtnSaveChangesAssignmentInfo());

        m_loadingSave.SetActive(false);
        RefreshAssignmentsList(); //This will update UI button states
    }

    public void OnValueChangeAssignmentName()
    {
        Debug.Log("OnValueChangeAssignmentName()");

        var selected = GetSelectedAssignment();
        if (selected == null)
            return;

        //Prevent assignment same-names
        m_assignmentNameText.text = CreateUniqueAssignmentName(m_assignmentNameText.text);
        WidgetObject widgetObj = GetWidget(selected.name, m_assignmentWidgets);
        widgetObj.Name = m_assignmentNameText.text;
        VHUtils.FindChildRecursive(widgetObj.Widget, "TextAssignmentName").GetComponent<Text>().text = m_assignmentNameText.text;
        selected.name = m_assignmentNameText.text;

        UpdateUIButtonStates();
    }

    public void OnValueChangeAssignmentDueDate()
    {
        Debug.Log("OnValueChangeAssignmentDueDate()");

        var selected = GetSelectedAssignment();
        if (selected == null)
            return;

        selected.datedue = VitaGlobals.StringToTicksString(m_dueDate.GetDate());

        UpdateUIButtonStates();
    }

    public void OnValueChangeAssignmentDescription()
    {
        Debug.Log("OnValueChangeAssignmentDescription()");

        var selected = GetSelectedAssignment();
        if (selected == null)
            return;

        selected.description = m_assignmentDescriptionText.GetComponent<InputField>().text;

        UpdateUIButtonStates();
    }

    public void OnValueChangeAssignmentInstructions()
    {
        Debug.Log("OnValueChangeAssignmentInstructions()");

        var selected = GetSelectedAssignment();
        if (selected == null)
            return;

        selected.instructions = m_instructionText.GetComponent<InputField>().text;

        UpdateUIButtonStates();
    }

    public void OnValueChangeClassListItem(GameObject classItem, bool value)
    {
        GameObject classText = VHUtils.FindChildRecursive(classItem, "TextClass");
        string className = classText.GetComponent<Text>().text;
        Debug.LogFormat("OnValueChangeClassListItem() - {0}", className);

        //Set up toggle on/off
        if (value)  m_classListSelectedItem = classItem;
        else        m_classListSelectedItem = null;

        UpdateUIButtonStates();
    }

    public void OnClickAssignmentListItem(GameObject assignmentItem, EntityDBVitaClassHomeworkAssigment assignment)
    {
        Debug.LogFormat("OnClickAssignmentListItem() - {0}", assignment.name);

        //Reset last selection's color
        SetWidgetOriginalColors(m_assignmentListSelectedItem);
        if (m_classListSelectedItem != null)
        {
            VHUtils.FindChildRecursive(m_classListSelectedItem, "TextClass").GetComponent<Toggle>().isOn = false;
        }

        //Assign new selection
        m_assignmentListSelectedItem = assignmentItem;
        m_selectedAssignment = assignment;
        m_lastSelectedAssignmentId = assignment.id;

        //Update UI
        InputField m_assignmentNameInput = m_assignmentNameText;
        AGGuiInputScrollField m_assignmentDescriptionInput = m_assignmentDescriptionText.GetComponent<AGGuiInputScrollField>();
        AGGuiInputScrollField m_assignmentInstructionInput = m_instructionText.GetComponent<AGGuiInputScrollField>();
        m_assignmentNameInput.text = assignment.name;
        m_dueDate.SetDate(VitaGlobals.TicksToString(assignment.datedue));
        m_assignmentDescriptionInput.SetText(assignment.description, delegate 
        {
            m_assignmentInstructionInput.SetText(assignment.instructions, delegate
            {
                UpdateUIButtonStates();
                SetClassHighlightsPerAssigned();
                m_classListSelectionToggleGrp.SetAllTogglesOff();
            }); 
        });

        
    }

    EntityDBVitaClassHomeworkAssigment GetSelectedAssignment()
    {
        return m_selectedAssignment;
    }

    /// <summary>
    /// Sets all UI buttons' interactivity (ie. disable all interactions)
    /// </summary>
    /// <param name="enabled"></param>
    void SetUIButtonStates(bool enabled)
    {
        //m_assignmentNameText.interactable = enabled;
        m_assignmentDescriptionText.GetComponent<InputField>().interactable = enabled;
        m_instructionText.GetComponent<InputField>().interactable = enabled;
        m_saveAssignmentButton.GetComponent<Button>().interactable = enabled;
        m_createAssignmentButton.GetComponent<Button>().interactable = enabled;
        m_duplicateAssignmentButton.GetComponent<Button>().interactable = enabled;
        m_deleteAssignmentButton.GetComponent<Button>().interactable = enabled;
        m_saveAssignmentButton.GetComponent<Button>().interactable = enabled;
        m_assignAssignmentButton.GetComponent<Button>().interactable = enabled;
    }

    /// <summary>
    /// Shows/Hides "*" on assignment widget upon edits
    /// </summary>
    void UpdateUIAssignmentWidgetIfChanges()
    {
        //The function calling this checks if there is a selected assignment
        GameObject widget = GetWidget(GetSelectedAssignment().name, m_assignmentWidgets).Widget;
        Text widgetText = VHUtils.FindChildRecursive(widget, "TextAssignmentName").GetComponent<Text>();

        //Set indicator for editable templates
        VHUtils.FindChildRecursive(widget, "AssignmentEditable").SetActive(GetClassesAssignedToSelectedAssignment().Count == 0);

        //Changes
        if (AnyChangesMade(GetSelectedAssignment()))
        {
            if (widgetText.text.LastIndexOf(" * ") < 0)
            {
                widgetText.text = widgetText.text + " * ";
            }
        }
        else
        {
            if (widgetText.text.LastIndexOf(" * ") >= 0)
            {
                widgetText.text = widgetText.text.Substring(0, widgetText.text.Length - widgetText.text.LastIndexOf(" * "));
            }
        }
    }

    void UpdateUIButtonStates()
    {
        //Reset
        SetUIButtonStates(true);

        //Disable buttons if not this teacher's assignments
        if (m_selectedAssignment != null)
        {
            if (m_selectedAssignment.teacher != VitaGlobals.m_loggedInProfile.username)
            {
                SetUIButtonStates(false);
                m_createAssignmentButton.GetComponent<Button>().interactable = true;
                m_duplicateAssignmentButton.GetComponent<Button>().interactable = true;
                return;
            }
        }
        else
        {
            m_duplicateAssignmentButton.GetComponent<Button>().interactable = false;
            m_saveAssignmentButton.GetComponent<Button>().interactable = false;
            m_assignAssignmentButton.GetComponent<Button>().interactable = false;
        }

        bool changesMade = AnyChangesMade(m_selectedAssignment);

        //Assign assignment button
        if (m_classListSelectedItem == null || m_selectedAssignment == null)
        {
            m_assignAssignmentButton.GetComponent<Button>().interactable = false;
        }
        //else if (GetClassesAssignedToSelectedAssignment().Contains(GetWidget(m_classListSelectedItem, m_classesWidgets).Name)) //If selected class is already assigned
        //{
        //    m_assignAssignmentButton.GetComponent<Button>().interactable = false;
        //}
        else
        {
            m_assignAssignmentButton.GetComponent<Button>().interactable = !changesMade;
        }

        //Disable editing if assignment has been assigned. Making assignments editable after assigning isn't feasible for now, so this limitation is put in
        //Note that date can still be updated and then saved & assigned
        bool assignmentEditable = GetClassesAssignedToSelectedAssignment().Count == 0;
        m_assignmentNameText.interactable = assignmentEditable;
        m_assignmentDescriptionText.GetComponent<InputField>().interactable = assignmentEditable;
        m_instructionText.GetComponent<InputField>().interactable = assignmentEditable;
        //m_saveAssignmentButton.GetComponent<Button>().interactable = assignmentEditable;
        //if (!assignmentEditable) return;

        //Changes
        //m_deleteAssignmentButton.GetComponent<Button>().interactable = !changesMade;
        m_saveAssignmentButton.GetComponent<Button>().interactable = changesMade;
        VitaGlobalsUI.m_unsavedChanges = changesMade;

        UpdateUIAssignmentWidgetIfChanges();
    }
    #endregion

    #region Private functions
    string CreateUniqueAssignmentName(string assignmentName)
    {
        string assignmentNameBase = assignmentName;

        int duplicatedAssignmentCounter = 0;
        string copyCounterString = " (Copy)";

        //Prune " (Copy)"
        while (assignmentNameBase.Contains(copyCounterString))
        {
            assignmentNameBase = assignmentNameBase.Substring(0, assignmentNameBase.Length - copyCounterString.Length);
            duplicatedAssignmentCounter++;
        }
        //Prune " (Copy #)"
        if (assignmentNameBase.Contains(" (Copy ") && assignmentNameBase.LastIndexOf(")") == assignmentNameBase.Length-1)
        {
            Debug.LogWarning(assignmentNameBase);
            int startSubString = assignmentNameBase.LastIndexOf(" (Copy ") + " (Copy ".Length;
            string numString = assignmentNameBase.Substring(startSubString, assignmentNameBase.LastIndexOf(")") - startSubString);
            int num;
            if (int.TryParse(numString, out num)) //If this is a number (ie. Copy 2) instead of some other text (ie. Copy New Task)
            {
                assignmentNameBase = assignmentNameBase.Substring(0, assignmentNameBase.LastIndexOf(" (Copy "));
                duplicatedAssignmentCounter++;
            }
        }

        //Set up initial return name
        if (duplicatedAssignmentCounter == 0)       copyCounterString = "";
        else if (duplicatedAssignmentCounter == 1)  copyCounterString = " (Copy)";
        //else                                        copyCounterString = " (Copy " + duplicatedAssignmentCounter.ToString() + ")";
        string returnAssName = assignmentNameBase + copyCounterString;

        //Add counter if needed
        while (m_assignmentsOriginal.Find(item => item.name == returnAssName) != null || m_assignments.Find(item => item.name == returnAssName) != null)
        {
            copyCounterString = " (Copy " + duplicatedAssignmentCounter.ToString() + ")";

            //Count
            if (duplicatedAssignmentCounter == 1)
            {
                returnAssName = assignmentNameBase + " (Copy)";
            }
            else
            {
                returnAssName = assignmentNameBase + copyCounterString;
            }

            duplicatedAssignmentCounter++;
        }

        return returnAssName;
    }

    List<string> GetClassesAssignedToAssignment(EntityDBVitaClassHomeworkAssigment assignment)
    {
        List<string> assignedClasses = new List<string>();
        foreach (var assignmentInOrg in m_assignmentsAll)
        {
            if (string.IsNullOrEmpty(assignmentInOrg.classname))                continue;
            if (assignmentInOrg.status == (int)DBAssignment.Status.TEMPLATE)    continue;
            if (assignmentInOrg.name != assignment.name)                        continue;
            if (!assignedClasses.Contains(assignmentInOrg.classname))           assignedClasses.Add(assignmentInOrg.classname);
        }

        return assignedClasses;
    }

    List<string> GetClassesAssignedToSelectedAssignment()
    {
        return GetClassesAssignedToAssignment(m_selectedAssignment);
    }
    
    WidgetObject GetWidget(string name, List<WidgetObject> list)
    {
        WidgetObject returnWidget = null;
        foreach (WidgetObject w in list)
        {
            if (w.Name == name)
            {
                returnWidget = w;
                break;
            }
        }

        return returnWidget;
    }

    WidgetObject GetWidget(GameObject widget, List<WidgetObject> list)
    {
        WidgetObject returnWidget = null;
        foreach (WidgetObject w in list)
        {
            if (w.Widget == widget)
            {
                returnWidget = w;
                break;
            }
        }

        return returnWidget;
    }

    void SetClassHighlightsPerAssigned()
    {
        //Highlight selected assignment
        SetWidgetHighlightedColors(m_assignmentListSelectedItem);

        //Highlight affiliated classes
        m_classListSelectedItem = null;
        foreach (var anyClassWidget in m_classesWidgets)                SetWidgetClassHighlight(anyClassWidget.Widget, false);                              //Deselect all classes
        foreach (var assignedClass in GetClassesAssignedToSelectedAssignment()) 
        {
            if (GetWidget(assignedClass, m_classesWidgets) != null)     SetWidgetClassHighlight(GetWidget(assignedClass, m_classesWidgets).Widget, true);   //Highlight all assigned classes
        } 
    }

    void SetWidgetClassHighlight(GameObject widget, bool enabled)
    {
        if (widget == null) return;
        VHUtils.FindChildRecursive(widget, "ClassHighlight").SetActive(enabled);
    }

    void SetWidgetOriginalColors(GameObject widget)
    {
        if (widget == null)                         return;
        if (widget.GetComponent<Button>() == null)  return;

        ColorBlock newColorBlock = widget.GetComponent<Button>().colors;
        newColorBlock.normalColor = VitaGlobals.m_uiNormalColor;
        newColorBlock.highlightedColor = VitaGlobals.m_uiHighlightColor;
        widget.GetComponent<Button>().colors = newColorBlock;
    }

    void SetWidgetHighlightedColors(GameObject widget)
    {
        if (widget == null)                         return;
        if (widget.GetComponent<Button>() == null)  return;

        ColorBlock newColorBlock = widget.GetComponent<Button>().colors;
        newColorBlock.normalColor = VitaGlobals.m_uiHighlightYellowColor;
        newColorBlock.highlightedColor = VitaGlobals.m_uiHighlightYellowColor;
        widget.GetComponent<Button>().colors = newColorBlock;
    }

    IEnumerator RefreshClassList()
    {
        m_loadingClassesStudents.SetActive(true);
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_classesWidgets = new List<WidgetObject>();
        m_studentWidgets = new List<WidgetObject>();

        VHUtils.DeleteChildren(m_classesStudentsListContent.transform);

        yield return GetStudents();

        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
        dbClass.GetAllClassesInOrganization(loginOrganization, (classes, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllClasses() error: {0}", error));
                return;
            }

            m_classes = new List<EntityDBVitaClass>();

            //Debug.Log("Classes num: " + m_classes.Count.ToString());

            foreach (var c in classes)
            {
                Debug.Log("Class name: " + c.classname);
                if (c.teacher != VitaGlobals.m_loggedInProfile.username)    continue; //Not this teacher

                //Add class & students
                m_classes.Add(c);
                GameObject classItem = VitaGlobalsUI.AddWidgetToList(m_classEntryItem, m_classesStudentsListContent, m_classEntryItem.name + "_" + c.classname, "TextClass", c.classname);
                VHUtils.FindChildRecursive(classItem, "TextClass").GetComponent<Toggle>().group = m_classListSelectionToggleGrp;
                VHUtils.FindChildRecursive(classItem, "TextClass").GetComponent<Toggle>().onValueChanged.AddListener((value) => { OnValueChangeClassListItem(classItem, value); });
                m_classesWidgets.Add(new WidgetObject { Name = c.classname, Widget = classItem });
                foreach (var s in c.students)
                {
                    EntityDBVitaProfile studentProfile = m_students.Find(item => item.username == s);
                    if (studentProfile == null) //Archived student
                        continue;
                    GameObject studentWidget = VitaGlobalsUI.AddWidgetToList(m_studentEntryItem, VHUtils.FindChildRecursive(classItem, "Content"), m_studentEntryItem.name + "_" + s, "TextStudentName", studentProfile.name);
                    m_studentWidgets.Add(new WidgetObject { Name = studentProfile.username, Widget = studentWidget });
                }
            }

            if (m_classes.Count > 0)
            {
                GameObject [] classesContent = VHUtils.FindAllChildren(m_classesStudentsListContent);

                int selectedIdx = 0;
                EventSystem.current.SetSelectedGameObject(classesContent[selectedIdx]);
            }
            m_loadingClassesStudents.SetActive(false);
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

    /// <summary>
    /// Clear, repopulate assignments from database data.
    ///
    /// This can be used any time the assignments list needs updating/refreshing
    /// </summary>
    /// <param name="m_classes"></param>
    void RefreshAssignmentsList()
    {
        m_loadingAssignments.SetActive(true);
        m_teacherWidgets = new List<WidgetObject>();
        m_assignmentWidgets = new List<WidgetObject>();
        EntityDBVitaClassHomeworkAssigment selectedAssignment = null;
        if (m_selectedAssignment != null)
            selectedAssignment = m_selectedAssignment;

        Debug.LogFormat("RefreshAssignmentsList() - {0}", selectedAssignment == null ? "" : selectedAssignment.name);

        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;

        m_selectedAssignment = null;
        m_classListSelectedItem = null;

        VHUtils.DeleteChildren(m_assignmentsListContent.transform);
        m_assignmentNameText.text = "Assignment ##";
        m_assignmentDescriptionText.GetComponent<InputField>().text = "--";
        m_dueDate.SetDate(DateTime.Today.ToShortDateString());
        m_instructionText.GetComponent<InputField>().text = "--";
        m_createAssignmentButton.GetComponent<Button>().interactable = true;
        m_deleteAssignmentButton.GetComponent<Button>().interactable = false;
        m_saveAssignmentButton.GetComponent<Button>().interactable = true;
        m_assignAssignmentButton.GetComponent<Button>().interactable = true;

        //Add a "My templates" first, so it always stays at the top
        GameObject myAssignedByItem = VitaGlobalsUI.AddWidgetToList(m_assignedByEntryItem, m_assignmentsListContent, m_assignedByEntryItem + "_" + VitaGlobals.m_loggedInProfile.username, "TextAssignedBy", "My Templates");
        m_teacherWidgets.Add(new WidgetObject { Name = VitaGlobals.m_loggedInProfile.username, Widget = myAssignedByItem });
        GameObject mySpacerItem = Instantiate(m_assignedBySpacerItem);
        mySpacerItem.transform.SetParent(m_assignmentsListContent.transform);
        mySpacerItem.SetActive(true);

        DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
        dbAssignment.GetAllAssignmentsInOrganization(loginOrganization, (assignments, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("GetAllAssignments() error: {0}", error));
                return;
            }

            //Sort by teacher name then by assignment name
            m_assignmentsAll = assignments.OrderBy(a => a.teacher).ThenBy(a => a.name).ToList();
            m_assignmentsOriginal = new List<EntityDBVitaClassHomeworkAssigment>();

            foreach (var assignment in m_assignmentsAll)
            {
                if (assignment.status == (int)DBAssignment.Status.TEMPLATE)
                {
                    m_assignmentsOriginal.Add(assignment);

                    Debug.LogFormat("GetAllClassAssignments() {0} {1} {2} {3}", assignment.classname, assignment.name, assignment.description, assignment.instructions);
                }
            }

            // make a copy
            m_assignments = new List<EntityDBVitaClassHomeworkAssigment>();
            m_assignmentsOriginal.ForEach(i => m_assignments.Add(new EntityDBVitaClassHomeworkAssigment(i)));

            foreach (EntityDBVitaClassHomeworkAssigment assignment in m_assignments)
            {
                EntityDBVitaClassHomeworkAssigment a = assignment;

                //Add entry into "assigned by" list (if doesn't exist)
                GameObject assignedByItem = null;
                if (GetWidget(a.teacher, m_teacherWidgets) == null)
                {
                    string assignedByTeacherName = (a.teacher == VitaGlobals.m_loggedInProfile.username) ? "My Templates" : a.teacher;
                    assignedByItem = VitaGlobalsUI.AddWidgetToList(m_assignedByEntryItem, m_assignmentsListContent, m_assignedByEntryItem + "_" + a.teacher, "TextAssignedBy", assignedByTeacherName);
                    m_teacherWidgets.Add(new WidgetObject { Name = a.teacher, Widget = assignedByItem });
                    GameObject spacerItem = Instantiate(m_assignedBySpacerItem);
                    spacerItem.transform.SetParent(m_assignmentsListContent.transform);
                    spacerItem.SetActive(true);
                }
                else
                {
                    assignedByItem = GetWidget(a.teacher, m_teacherWidgets).Widget;
                }

                GameObject assignmentItem = VitaGlobalsUI.AddWidgetToList(m_assignmentEntryItem, VHUtils.FindChildRecursive(assignedByItem, "Content"), m_assignmentEntryItem.name + "_" + a.name, "TextAssignmentName", a.name);
                VHUtils.FindChildRecursive(assignmentItem, "TextAssignmentDescription").GetComponent<Text>().text = a.description;
                m_assignmentWidgets.Add(new WidgetObject { Name = a.name, Widget = assignmentItem });
                VHUtils.FindChildRecursive(assignmentItem, "AssignmentEditable").SetActive(GetClassesAssignedToAssignment(assignment).Count == 0); //Editable indicator
                assignmentItem.GetComponent<Button>().onClick.AddListener(delegate
                {
                    OnClickAssignmentListItem(assignmentItem, a);
                });

                Debug.LogFormat("Adding {0} {1} {2}", a.name, a.description, assignedByItem.GetComponent<Text>().text);
            }

            //If there was a last selection, try to perserve it even on update
            if (m_assignments.Find(item => item.id == m_lastSelectedAssignmentId) != null)
            {
                EntityDBVitaClassHomeworkAssigment lastAssignment = m_assignments.Find(item => item.id == m_lastSelectedAssignmentId);
                GameObject lastAssignmentWidget = GetWidget(lastAssignment.name, m_assignmentWidgets).Widget;
                EventSystem.current.SetSelectedGameObject(lastAssignmentWidget);
                lastAssignmentWidget.GetComponent<Button>().onClick.Invoke();
            }
            //Select first assignment, which will update UI button states as well
            else if (m_assignmentWidgets.Count > 0)
            {
                //If the logged in teacher has a template, then select it, otherwise, select any other
                Button[] myAssignedByButtons = myAssignedByItem.GetComponentsInChildren<Button>();
                if (myAssignedByButtons.Length > 0)
                {
                    EventSystem.current.SetSelectedGameObject(myAssignedByButtons[0].gameObject);
                    myAssignedByButtons[0].onClick.Invoke();
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(m_assignmentWidgets[0].Widget);
                    m_assignmentWidgets[0].Widget.GetComponent<Button>().onClick.Invoke();
                }
            }
            m_loadingAssignments.SetActive(false);
        });
    }

    bool AssignmentEquals(EntityDBVitaClassHomeworkAssigment originalAssignment, EntityDBVitaClassHomeworkAssigment newAssignment)
    {
        return /*originalAssignment.name == newAssignment.name &&*/    // ignore name since we special case it
               originalAssignment.description == newAssignment.description &&
               originalAssignment.instructions == newAssignment.instructions &&
               originalAssignment.datedue == newAssignment.datedue;
    }

    /// <summary>
    /// Check all assignments
    /// </summary>
    /// <returns></returns>
    bool AnyChangesMade()
    {
        if (m_assignmentsOriginal.Count != m_assignments.Count)
        {
            //Debug.LogWarningFormat("AnyChangesMade() - count");
            return true;
        }

        for (int i = 0; i < m_assignments.Count; i++)
        {
            if (AnyChangesMade(m_assignments[i])) return true;
        }

        return false;
    }

    /// <summary>
    /// Check a single assignment
    /// </summary>
    /// <param name="assignment"></param>
    /// <returns></returns>
    bool AnyChangesMade(EntityDBVitaClassHomeworkAssigment assignment)
    {
        if (assignment == null)                                         return false;
        EntityDBVitaClassHomeworkAssigment assignmentOriginal = m_assignmentsOriginal.Find(item => item.id == assignment.id);
        EntityDBVitaClassHomeworkAssigment assignmentCurrent = m_assignments.Find(item => item.id == assignment.id);

        if (assignmentOriginal == null || assignmentCurrent == null)    return true;
        if (assignmentOriginal.name != assignmentCurrent.name)          return true;
        if (!AssignmentEquals(assignmentOriginal, assignmentCurrent))   return true;
        return false;
    }

    List<int> GetAssignmentsWithChangedName()
    {
        List<int> assignmentIdxs = new List<int>();

        for (int i = 0; i < m_assignments.Count; i++)
        {
            if (i < m_assignmentsOriginal.Count)
            {
                // look if the username of the teacher has changed
                if (m_assignmentsOriginal[i].name != m_assignments[i].name)
                {
                    assignmentIdxs.Add(i);
                }
            }
        }

        return assignmentIdxs;
    }

    List<int> GetAssignmentsWithChangedInfo()
    {
        List<int> assignmentIdxs = new List<int>();

        for (int i = 0; i < m_assignments.Count; i++)
        {
            if (i < m_assignmentsOriginal.Count)
            {
                // look for changes made in existing teachers
                if (!AssignmentEquals(m_assignmentsOriginal[i], m_assignments[i]))
                {
                    assignmentIdxs.Add(i);
                }
            }
        }

        return assignmentIdxs;
    }

    List<int> GetAssignmentsNewlyAdded()
    {
        List<int> assignmentIdxs = new List<int>();

        for (int i = 0; i < m_assignments.Count; i++)
        {
            // look for new assignments added
            if (i >= m_assignmentsOriginal.Count)
            {
                assignmentIdxs.Add(i);
            }
        }

        return assignmentIdxs;
    }

    IEnumerator BtnSaveChangesAssignmentName()
    {
        List<int> assignmentIdxs = GetAssignmentsWithChangedName();
        int nameChangeCount = assignmentIdxs.Count;

        foreach (int i in assignmentIdxs)
        {
            Debug.LogFormat("BtnSaveChanges() - changing assignment name - {0}", m_assignments[i].name);

            DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
            dbAssignment.UpdateName(m_assignmentsOriginal[i].id, m_assignments[i].name, error =>
            {
                nameChangeCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateName() error: {0}", error));
                    return;
                }
            });
        }

        while (nameChangeCount > 0)
            yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// Saves all assignments that are not new (aka assignment has entry in both m_assignmentsOriginal and m_assignments
    /// </summary>
    /// <returns></returns>
    IEnumerator BtnSaveChangesAssignmentInfo()
    {
        List<int> assignmentIdxs = GetAssignmentsWithChangedInfo();
        int assignmentChangeCount = assignmentIdxs.Count;

        foreach (int i in assignmentIdxs)
        {
            Debug.LogFormat("BtnSaveChanges() - updating assignment with changes - {0}", m_assignments[i].name);
            //Debug.LogWarning(string.Format("Info: {0}, {1}, {2}, {3}, {4}, {5}, {6}", m_assignments[i].id, m_assignments[i].description, m_assignments[i].instructions, m_assignments[i].studentresponse, m_assignments[i].grade, m_assignments[i].datedue, (DBAssignment.Status)m_assignments[i].status));

            DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
            dbAssignment.UpdateAssignment(m_assignments[i].id, m_assignments[i].description, m_assignments[i].instructions, m_assignments[i].studentresponse, m_assignments[i].grade, m_assignments[i].datedue, (DBAssignment.Status)m_assignments[i].status, error =>
            {
                assignmentChangeCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateAssignment() error: {0}", error));
                    return;
                }
            });
        }

        while (assignmentChangeCount > 0)
            yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// This detects if a new assignment is created based on m_assignmentsOriginal vs m_assignments, where if an assignment appears on m_assignments isn't in m_assignmentsOriginal, it is considered a new assignment, and will be saved as template.
    /// </summary>
    /// <returns></returns>
    IEnumerator BtnSaveChangesAssignmentsNew()
    {
        List<int> assignmentIdxs = GetAssignmentsNewlyAdded();
        int assignmentsNewCount = assignmentIdxs.Count;

        //NOTE: IDs do not update causing errors if saving by foreach(int i in assignmentIdx)
        if (assignmentsNewCount > 1)
            Debug.LogWarning("Known error: Multiple new templates will cause an error upon saving. Templates will save, but their info might be off (Joe)");
        foreach (int i in assignmentIdxs)
        {
            Debug.LogFormat("BtnSaveChanges() - creating new assignment - {0}", m_assignments[i].name);

            DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
            int index = i;
            dbAssignment.CreateAssignmentTemplate(m_assignments[i].name, m_assignments[i].description, m_assignments[i].instructions, m_assignments[i].organization, m_assignments[i].teacher, (newAssignmentTemplate, error) =>
            {
                //Adding the returned assignment so GUID would update, so we can properly save other properties like duedate
                m_assignmentsOriginal.Add(newAssignmentTemplate);
                m_assignments[index].id = newAssignmentTemplate.id;
                m_lastSelectedAssignmentId = newAssignmentTemplate.id;

                assignmentsNewCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("CreateAssignmentTemplate() error: {0}", error));
                    return;
                }
            });
        }

        while (assignmentsNewCount > 0)
            yield return new WaitForEndOfFrame();
    }
    #endregion
}

