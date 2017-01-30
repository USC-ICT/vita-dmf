using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelStudentAssignments : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
    [Serializable]
    public class WidgetObject
    {
        public string Name;
        public GameObject Widget;
        public EntityDBVitaClassHomeworkAssigment AssignmentEntry;
        public string UIStoredResponse; //The user input in response is stored in here as long as the user is still on this page. We use this to help check for modifications
    }

    List<WidgetObject> m_assignmentWidgets = new List<WidgetObject>();

    MenuManager m_menuManager;
    MenuManagerPanelStudentSideBar m_studentSideBar;
    GameObject m_headerName;
    GameObject m_assignmentEntryItem;
    ToggleGroup m_assignmentsToggleGrp;

    GameObject m_lateListGroup;
    GameObject m_assignedListGroup;
    GameObject m_submittedListGroup;
    GameObject m_gradedListGroup;

    GameObject m_assignmentNameText;
    GameObject m_dueDateText;
    GameObject m_instructionText;
    GameObject m_responseText;
    GameObject m_submitButton;
    Button m_saveChangesButton;
    GameObject m_submitLoadingIcon;
    GameObject m_saveChangesLoadingIcon;

    Text m_gradeText;

    EntityDBVitaClassHomeworkAssigment m_selectedAssignment;
    #endregion

    #region Menu init
    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.StudentAssignments);
        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");
        m_studentSideBar = VHUtils.FindChildRecursive(menu, "PanelStudentSideBarPrefab").GetComponent<MenuManagerPanelStudentSideBar>();

        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");
        m_assignmentEntryItem = VHUtils.FindChildRecursive(resources, "GuiButtonAssignmentPrefab");
        m_assignmentsToggleGrp = VHUtils.FindChildRecursive(menu, "AssignmentToggleGrp").GetComponent<ToggleGroup>();

        m_lateListGroup = VHUtils.FindChildRecursive(menu, "LateListGrp");
        m_assignedListGroup = VHUtils.FindChildRecursive(menu, "AssignedListGrp");
        m_submittedListGroup = VHUtils.FindChildRecursive(menu, "SubmittedListGrp");
        m_gradedListGroup = VHUtils.FindChildRecursive(menu, "GradedListGrp");

        m_assignmentNameText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssignmentNum");
        m_dueDateText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_AssignmentDate");
        m_instructionText = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "Scroll View_Instructions"), "GuiTextPrefab");
        m_responseText = VHUtils.FindChildRecursive(menu, "GuiInputScrollFieldPrefab_Response");

        m_gradeText = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Grade").GetComponent<Text>();

        m_submitButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Submit");
        m_saveChangesButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_SaveProgress").GetComponent<Button>();
        m_submitLoadingIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_Submit");
        m_saveChangesLoadingIcon = VHUtils.FindChildRecursive(menu, "GuiIconLoadingPrefab_SaveProgress");
    }

    public void OnMenuEnter()
    {
        m_studentSideBar.SideBarOnMenuEnter();
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        //Clear meu
        m_selectedAssignment = null;
        m_assignmentWidgets = new List<WidgetObject>();
        m_assignmentNameText.GetComponent<Text>().text = "--";
        m_dueDateText.GetComponent<Text>().text = "mm/dd/yyyy";
        m_instructionText.GetComponent<Text>().text = "--";
        m_responseText.GetComponent<InputField>().text = "--";
        m_gradeText.text = "--";
        m_submitButton.GetComponent<Button>().interactable = false;
        m_saveChangesButton.interactable = false;
        m_submitLoadingIcon.SetActive(false);
        m_saveChangesLoadingIcon.SetActive(false);

        RefreshAssignmentsList();
    }

    public void OnMenuExit()
    {
        m_studentSideBar.SideBarOnMenuExit();
    }
    #endregion

    #region UI Hooks
    void OnClickAssignmentListItem(GameObject assignmentItem, EntityDBVitaClassHomeworkAssigment assignment)
    {
        Debug.LogFormat(assignment.name);
        WidgetObject widgetObject = GetWidget(assignment.id, m_assignmentWidgets);
        m_selectedAssignment = assignment;
        m_assignmentNameText.GetComponent<Text>().text = assignment.name;
        m_dueDateText.GetComponent<Text>().text = VitaGlobals.TicksToString(assignment.datedue);
        m_instructionText.GetComponent<Text>().text = assignment.instructions;

        //Special UI Stored value
        m_responseText.GetComponent<AGGuiInputScrollField>().SetText(widgetObject.UIStoredResponse, delegate
        {
            if (m_selectedAssignment.status == (int)DBAssignment.Status.GRADED)
            {
                m_gradeText.text = assignment.grade.ToString();
            }
            else
            {
                m_gradeText.text = "--";
            }

            if (m_selectedAssignment.status == (int)DBAssignment.Status.ASSIGNED)
            {
                m_responseText.GetComponent<InputField>().interactable = true;
                m_submitButton.GetComponent<Button>().interactable = true;
                if (m_selectedAssignment.studentresponse != m_responseText.GetComponent<InputField>().text)
                {
                    m_saveChangesButton.interactable = true;
                }
            }
            else //Submitted/Graded will make this non-interactable
            {
                m_responseText.GetComponent<InputField>().interactable = false;
                m_submitButton.GetComponent<Button>().interactable = false;
                m_saveChangesButton.interactable = false;
            }
        });
        
    }

    void OnValueChangedResponse(string value)
    {
        if (m_selectedAssignment == null)
        {
            m_submitButton.GetComponent<Button>().interactable = false;
            m_saveChangesButton.interactable = false;
            return;
        }

        WidgetObject widgetObject = GetWidget(m_selectedAssignment.id, m_assignmentWidgets);
        GameObject widget = widgetObject.Widget;
        Text widgetText = VHUtils.FindChildRecursive(widget, "TextMain").GetComponent<Text>();
        string response = m_responseText.GetComponent<InputField>().text;
        widgetObject.UIStoredResponse = response;

        //Set submit button
        if (string.IsNullOrEmpty(response))
        {
            m_submitButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            m_submitButton.GetComponent<Button>().interactable = true;
        }

        //Set save button & check if assignment has changes
        if (m_selectedAssignment.studentresponse != widgetObject.UIStoredResponse)
        {
            m_saveChangesButton.interactable = true;
            if (widgetText.text.LastIndexOf(" * ") < 0)
            {
                widgetText.text = widgetText.text + " * ";
            }
        }
        else
        {
            m_saveChangesButton.interactable = false;
            if (widgetText.text.LastIndexOf(" * ") >= 0)
            {
                widgetText.text = widgetText.text.Substring(0, widgetText.text.Length - widgetText.text.LastIndexOf(" * "));
            }
        }

        Debug.LogFormat("OnValueChangedResponse() - {0} - {1}", response, m_submitButton.GetComponent<Button>().interactable);
    }

    public void BtnSubmitAssignment()
    {
        if (m_selectedAssignment == null)
            return;

        string response = m_responseText.GetComponent<InputField>().text;

        Debug.LogFormat("BtnSubmitAssignment() - {0} - {1}", m_selectedAssignment.name, response);

        DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
        m_submitLoadingIcon.SetActive(true);
        m_submitButton.GetComponent<Button>().interactable = false;
        m_saveChangesButton.GetComponent<Button>().interactable = false;
        dbAssignment.UpdateAssignmentStudentResponse(m_selectedAssignment, response, (error) => 
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateAssignment() error: {0}", error));
                return;
            }

            DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
            dbSession.AddEvent("GLOBAL", m_selectedAssignment.student, DBSession.EventData.NewHomeworkSubmitted(), error2 =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("AddEvent() error: {0}", error2));
                    return;
                }

                m_submitLoadingIcon.SetActive(false);
                RefreshAssignmentsList();
            });
        });
    }

    public void BtnSaveAssignment()
    {
        if (m_selectedAssignment == null)
            return;

        string response = m_responseText.GetComponent<InputField>().text;

        Debug.LogFormat("BtnSaveAssignment() - {0} - {1}", m_selectedAssignment.name, response);

        DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
        m_saveChangesLoadingIcon.SetActive(true);
        m_saveChangesButton.GetComponent<Button>().interactable = false;
        m_submitButton.GetComponent<Button>().interactable = false;
        dbAssignment.UpdateAssignmentStudentResponse(m_selectedAssignment, response, (DBAssignment.Status)m_selectedAssignment.status, (error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                PopUpDisplay.Instance.Display("Error", string.Format("UpdateAssignment() error: {0}", error));
                return;
            }

            m_saveChangesLoadingIcon.SetActive(false);
            m_submitButton.GetComponent<Button>().interactable = true;
            RefreshAssignmentsList();
        });
    }
    #endregion

    GameObject AddAssignmentItem(EntityDBVitaClassHomeworkAssigment assignment)
    {
        if (assignment.status == (int)DBAssignment.Status.TEMPLATE)
            return null;

        Debug.LogFormat("AddAssignmentItem({0})", assignment.name);

        GameObject assignmentItem = Instantiate(m_assignmentEntryItem);
        GameObject assignmentNameText = VHUtils.FindChildRecursive(assignmentItem, "TextMain");
        assignmentItem.SetActive(true);
        AGGuiAssignmentStudent assignmentCmp = assignmentItem.GetComponent<AGGuiAssignmentStudent>();
        m_assignmentWidgets.Add(new WidgetObject { Name = assignment.name, Widget = assignmentItem, AssignmentEntry = assignment, UIStoredResponse = assignment.studentresponse });

        if (assignment.status == (int)DBAssignment.Status.ASSIGNED)
        {
            if (VitaGlobals.TicksToDateTime(assignment.datedue) < DateTime.Now)
            {
                assignmentItem.transform.SetParent(m_lateListGroup.transform, false);
                assignmentCmp.SetStatus(AGGuiAssignmentStudent.AssignmentStatus.Late);
            }
            else
            {
                assignmentItem.transform.SetParent(m_assignedListGroup.transform, false);
                assignmentCmp.SetStatus(AGGuiAssignmentStudent.AssignmentStatus.Assigned);
            }
        }
        else if (assignment.status == (int)DBAssignment.Status.SUBMITTED)
        {
            assignmentItem.transform.SetParent(m_submittedListGroup.transform, false);
            assignmentCmp.SetStatus(AGGuiAssignmentStudent.AssignmentStatus.Submitted);
        }
        else if (assignment.status == (int)DBAssignment.Status.GRADED)
        {
            assignmentItem.transform.SetParent(m_gradedListGroup.transform, false);
            assignmentCmp.SetStatus(AGGuiAssignmentStudent.AssignmentStatus.Graded);
        }

        assignmentItem.GetComponent<Toggle>().group = m_assignmentsToggleGrp;
        assignmentItem.GetComponent<Toggle>().onValueChanged.AddListener((value) =>
        {
            if (value)
            {
                OnClickAssignmentListItem(assignmentItem, assignment);
            }
        });
        assignmentNameText.GetComponent<Text>().text = string.Format("{0}\n{1}", assignment.name, VitaGlobals.TicksToString(assignment.datedue));

        return assignmentItem;
    }

    WidgetObject GetWidget(string assignmentId, List<WidgetObject> list)
    {
        WidgetObject returnWidget = null;
        foreach (WidgetObject w in list)
        {
            if (w.AssignmentEntry.id == assignmentId)
            {
                returnWidget = w;
                break;
            }
        }

        return returnWidget;
    }

    void RefreshAssignmentsList()
    {
        VHUtils.DeleteChildren(m_lateListGroup.transform);
        VHUtils.DeleteChildren(m_assignedListGroup.transform);
        VHUtils.DeleteChildren(m_submittedListGroup.transform);
        VHUtils.DeleteChildren(m_gradedListGroup.transform);
        m_gradeText.text = "--";

        DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
        dbAssignment.GetAllAssignmentsForStudent(VitaGlobals.m_loggedInProfile.username, VitaGlobals.m_loggedInProfile.organization, (assignments, error) =>
        {
            m_assignmentWidgets = new List<WidgetObject>();
            foreach (var assignment in assignments)
            {
                AddAssignmentItem(assignment);
            }
        });
    }
}
