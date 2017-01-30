using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading;

public class DBAssignment : MonoBehaviour
{
    public enum Status
    {
        TEMPLATE = 0,
        ASSIGNED = 1,
        SUBMITTED = 2,
        GRADED = 3,
    }


    void Start()
    {
    }

    void Update()
    {
    }

    public delegate void CreateAssignmentDelegate(EntityDBVitaClassHomeworkAssigment assignment, string error);

    public void CreateAssignmentTemplate(string name, string description, string instructions, string organization, string teacher, CreateAssignmentDelegate callback)
    {
        CreateAssignment(name, description, instructions, organization, teacher, " ", " ", VitaGlobals.MaxTimeToString(), Status.TEMPLATE, callback);
    }

    public void CreateAssignment(string name, string description, string instructions, string organization, string teacher, string student, string classname, string duedate, CreateAssignmentDelegate callback)
    {
        CreateAssignment(name, description, instructions, organization, teacher, student, classname, duedate, Status.ASSIGNED, callback);
    }


    void CreateAssignment(string name, string description, string instructions, string organization, string teacher, string student, string classname, string duedate, Status status, CreateAssignmentDelegate callback)
    {
        // assert that duedate is in the right format.  This is because date is user-supplied.
        try                         { VitaGlobals.TicksToString(duedate); }
        catch (FormatException e)   { Debug.LogErrorFormat("Due date ({0}) not in correct format. Expecting ticks string. {1}", duedate.ToString(), e); }

        EntityDBVitaClassHomeworkAssigment assignment = new EntityDBVitaClassHomeworkAssigment(name, description, instructions, " ", -1, organization, teacher, student, classname, duedate, (int)status);

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.AddEntity<EntityDBVitaClassHomeworkAssigment>(assignment, (result, error) =>
        {
            if (callback != null)
                callback(assignment, error);
        });
    }

    public IEnumerator AddAssignments(List<EntityDBVitaClassHomeworkAssigment> assignments)
    {
        // TODO: Add bulk Add() function

        // this function is used for backup/restore purposes.  In normal operation, use Create() instead

        int waitCount = assignments.Count;
        foreach (var assignment in assignments)
        {
            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaClassHomeworkAssigment>(assignment, (result, error) =>
            {
                waitCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("AddOrganizations() failed - {0}", assignment.name);
                        return;
                    }
                }
            });
        }

        while (waitCount > 0)
            yield return new WaitForEndOfFrame();
    }

    public delegate void GetAssignmentDelegate(EntityDBVitaClassHomeworkAssigment assignment, string error);

    public void GetAssignment(string assignmentId, GetAssignmentDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntity<EntityDBVitaClassHomeworkAssigment>(assignmentId, "", null, (result, error) =>
        {
            EntityDBVitaClassHomeworkAssigment assignment = (EntityDBVitaClassHomeworkAssigment)result;

            if (string.IsNullOrEmpty(error))
            {
                assignment.FixNullLists();
            }

            if (callback != null)
                callback(assignment, error);
        });
    }

    public delegate void GetAssignmentsDelegate(List<EntityDBVitaClassHomeworkAssigment> assignments, string error);

    public void GetAllAssignments(GetAssignmentsDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetAllEntities<EntityDBVitaClassHomeworkAssigment>(EntityDBVitaClassHomeworkAssigment.tableName, null, (list, error) =>
        {
            List<EntityDBVitaClassHomeworkAssigment> assignments = new List<EntityDBVitaClassHomeworkAssigment>();

            if (string.IsNullOrEmpty(error))
            {
                assignments = (List<EntityDBVitaClassHomeworkAssigment>)list;

                assignments.Sort((a, b) => a.name.CompareTo(b.name));
                assignments.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(assignments, error);
        });
    }

    public void GetAllAssignmentsInOrganization(string organization, GetAssignmentsDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntities<EntityDBVitaClassHomeworkAssigment>(EntityDBVitaClassHomeworkAssigment.tableName, "organization-index", "organization", organization, null, (list, error) =>
        {
            List<EntityDBVitaClassHomeworkAssigment> assignments = new List<EntityDBVitaClassHomeworkAssigment>();

            if (string.IsNullOrEmpty(error))
            {
                assignments = (List<EntityDBVitaClassHomeworkAssigment>)list;

                assignments.Sort((a, b) => a.name.CompareTo(b.name));
                assignments.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(assignments, error);
        });
    }

    public void GetAllAssignmentsForStudent(string username, string organization, GetAssignmentsDelegate callback)
    {
        GetAllAssignmentsInOrganization(organization, (list, error) =>
        {
            List<EntityDBVitaClassHomeworkAssigment> assignments = new List<EntityDBVitaClassHomeworkAssigment>();

            if (string.IsNullOrEmpty(error))
            {
                list.ForEach((a) => { if (a.student == username) assignments.Add(a); });
            }

            if (callback != null)
                callback(assignments, error);
        });
    }

    public delegate void UpdateAssignmentDelegate(string error);

    public void UpdateName(string assignmentId, string newName, UpdateAssignmentDelegate callback)
    {
        GetAssignment(assignmentId, (assignment, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            assignment.name = newName;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaClassHomeworkAssigment>(assignment, (result2, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    public void UpdateAssignmentStudentResponse(EntityDBVitaClassHomeworkAssigment assignment, string studentresponse, UpdateAssignmentDelegate callback)
    {
        UpdateAssignmentStudentResponse(assignment, studentresponse, Status.SUBMITTED, callback);
    }

    public void UpdateAssignmentStudentResponse(EntityDBVitaClassHomeworkAssigment assignment, string studentresponse, Status status, UpdateAssignmentDelegate callback)
    {
        UpdateAssignment(assignment.id, assignment.description, assignment.instructions, studentresponse, assignment.grade, assignment.datedue, status, callback);
    }

    public void UpdateAssignmentGrade(EntityDBVitaClassHomeworkAssigment assignment, int grade, UpdateAssignmentDelegate callback)
    {
        UpdateAssignment(assignment.id, assignment.description, assignment.instructions, assignment.studentresponse, grade, assignment.datedue, DBAssignment.Status.GRADED, callback);
    }

    public void UpdateAssignment(string assignmentId, string description, string instructions, string studentresponse, int grade, string datedue, Status status, UpdateAssignmentDelegate callback)
    {
        GetAssignment(assignmentId, (assignment, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            assignment.description = description;
            assignment.instructions = instructions;
            assignment.studentresponse = studentresponse;
            assignment.grade = grade;
            assignment.datedue = datedue;
            assignment.status = (int)status;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaClassHomeworkAssigment>(assignment, (result2, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    public delegate void DeleteAssignmentDelegate(string error);

    public void DeleteAssignment(string assignmentId, DeleteAssignmentDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.DeleteEntity<EntityDBVitaClassHomeworkAssigment>(assignmentId, null, (result, error) =>
        {
            if (callback != null)
                callback(error);
        });
    }

    public delegate void DeleteAllAssignmentsDelegate(string error);

    public void DeleteAllAssignmentsForStudent(string username, string organization, DeleteAllAssignmentsDelegate callback)
    {
        GetAllAssignmentsForStudent(username, organization, (assignments, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            int assignmentCount = assignments.Count;

            if(assignmentCount == 0)
            {
                if (callback != null)
                    callback("");

                return;
            }

            foreach (var assignment in assignments)
            {
                DeleteAssignment(assignment.id, (error2) =>
                {
                    assignmentCount--;

                    if (!string.IsNullOrEmpty(error2))
                    {
                        if (callback != null)
                            callback(error2);
                    }

                    if (assignmentCount == 0)
                    {
                        if (callback != null)
                            callback(error2);
                    }
                });
            }
        });
    }


    public void GetAllAssignmentsInClass(string className, GetAssignmentsDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntities<EntityDBVitaClassHomeworkAssigment>(EntityDBVitaClassHomeworkAssigment.tableName, "classname-index", "classname", className, null, (list, error) =>
        {
            List<EntityDBVitaClassHomeworkAssigment> assignments = new List<EntityDBVitaClassHomeworkAssigment>();

            if (string.IsNullOrEmpty(error))
            {
                assignments = (List<EntityDBVitaClassHomeworkAssigment>)list;

                assignments.Sort((a, b) => a.name.CompareTo(b.name));
                assignments.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(assignments, error);
        });
    }


    public delegate void ChangeAssigmentsTeacherDelegate(string error);
    public void ChangeAssigmentsTeacher(string className , string newTeacher, ChangeAssigmentsTeacherDelegate callback)
    {

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();

        GetAllAssignmentsInClass(className ,(assignments, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            assignments.ForEach(a =>a.teacher = newTeacher);

           // StartCoroutine(AddAssignments(assignments));

            
            vitaDynamoDB.BatchAdd<EntityDBVitaClassHomeworkAssigment>(assignments, (par, error2) =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    if (callback != null)
                        callback(error2);

                    return;
                }
            });


        });

    }
}
