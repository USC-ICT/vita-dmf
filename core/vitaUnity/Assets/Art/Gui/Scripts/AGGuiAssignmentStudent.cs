using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class AGGuiAssignmentStudent : MonoBehaviour
{
    [Serializable]
    public class AssignmentStatusOptions
    {
        public string Name;
        public AssignmentStatus Status;
        public Sprite Sprite;
    }

    public enum AssignmentStatus
    {
        Template,
        Assigned,
        Submitted,
        Graded,
        Late
    }

    public Image m_assignmentIcon;
    public List<AssignmentStatusOptions> m_assignmentStatuses = new List<AssignmentStatusOptions>();

    /// <summary>
    /// This overload checks the assignment's duedate for status
    /// </summary>
    /// <param name="assignment"></param>
    public void SetStatus(EntityDBVitaClassHomeworkAssigment assignment)
    {
        AssignmentStatus assStatus = AssignmentStatus.Template;

        if (assignment.status == (int)DBAssignment.Status.ASSIGNED)
        {
            // Compare ticks; Make sure to do else-if to check against late vs. graded etc.
            if (assignment.datedue.Contains("/"))
            {
                Debug.LogWarning(assignment.datedue.ToString() + " skipping wrong date format. This should be prevented in future versions. (Joe)");
                return;
            }

            if (VitaGlobals.TicksToDateTime(assignment.datedue) < DateTime.Now)
            {
                assStatus = AssignmentStatus.Late;
            }
            else
            {
                assStatus = AssignmentStatus.Assigned;
            }
        }
        else if (assignment.status == (int)DBAssignment.Status.SUBMITTED)   assStatus = AssignmentStatus.Submitted;
        else if (assignment.status == (int)DBAssignment.Status.GRADED)      assStatus = AssignmentStatus.Graded;

        SetStatus(assStatus);
    }

    public void SetStatus(AssignmentStatus status)
    {
        Sprite s = null;

        foreach (AssignmentStatusOptions stat in m_assignmentStatuses)
        {
            if (stat.Status == status)
            {
                s = stat.Sprite;
                break;
            }
        }

        m_assignmentIcon.sprite = s;
    }
}
