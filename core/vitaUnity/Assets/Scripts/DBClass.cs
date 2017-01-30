using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

public class DBClass : MonoBehaviour
{

    //bool m_addItemInProgress = false;
    bool m_removeItemInProgress = false;


    ArrayList AddItemInProgressList = new ArrayList();
    int AddItemInProgressInx = 0;
  


    void Start()
    {
        AddItemInProgressList.Clear();
        AddItemInProgressInx = 0;     
        AddItemInProgressList.Add(false);          
    }


    void Update()
    {
    }


    public delegate void CreateClassDelegate(string error);

    public void CreateClass(string className, string organization, CreateClassDelegate callback)
    {
        EntityDBVitaClass profile = new EntityDBVitaClass(className, organization);

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.AddEntity<EntityDBVitaClass>(profile, (result, error) =>
        {
            if (callback != null)
                callback(error);
        });
    }

    public IEnumerator AddClasses(List<EntityDBVitaClass> classes)
    {
        // TODO: Add bulk Add() function

        // this function is used for backup/restore purposes.  In normal operation, use Create() instead

        int waitCount = classes.Count;
        foreach (var c in classes)
        {
            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaClass>(c, (result, error) =>
            {
                waitCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("AddClasses() failed - {0}", c.classname);
                        return;
                    }
                }
            });
        }

        while (waitCount > 0)
            yield return new WaitForEndOfFrame();
    }

    public delegate void GetClassDelegate(EntityDBVitaClass c, string error);

    public void GetClass(string className, GetClassDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntity<EntityDBVitaClass>(className, "", null, (result, error) =>
        {
            EntityDBVitaClass c = (EntityDBVitaClass)result;

            if (string.IsNullOrEmpty(error))
            {
                c.FixNullLists();
            }

            if (callback != null)
                callback(c, error);
        });
    }

    public delegate void GetAllClassesDelegate(List<EntityDBVitaClass> classes, string error);

    public void GetAllClasses(GetAllClassesDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetAllEntities<EntityDBVitaClass>(EntityDBVitaClass.tableName, null, (result, error) =>
        {
            List<EntityDBVitaClass> classes = new List<EntityDBVitaClass>();

            if (string.IsNullOrEmpty(error))
            {
                classes = (List<EntityDBVitaClass>)result;

                classes.Sort((a, b) => a.classname.CompareTo(b.classname));
                classes.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(classes, error);
        });
    }

    public void GetAllClassesInOrganization(string organization, GetAllClassesDelegate callback)
    {
        GetAllClasses((list, error) =>
        {
            List<EntityDBVitaClass> classes = new List<EntityDBVitaClass>();

            if (string.IsNullOrEmpty(error))
            {
                list.ForEach((a) => { if (a.organization == organization) classes.Add(a); });
            }

            if (callback != null)
                callback(classes, error);
        });
    }

    public delegate void DeleteClassDelegate(string error);

    public void DeleteClass(string className, DeleteClassDelegate callback)
    {
        GetClass(className, (c, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            DeleteClass(className, c.teacher, callback);
        });
    }

    public void DeleteClass(string className, string teacher, DeleteClassDelegate callback)
    {
        Debug.LogFormat("DeleteClass() - {0} - {1}", className, teacher);

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.DeleteEntity<EntityDBVitaClass>(className,"", (result, error2) =>
        {
            if (callback != null)
                callback(error2);
        });
    }

    public delegate void UpdateClassnameDelegate(string error);

    public void UpdateClassname(string oldName, string newName, UpdateClassnameDelegate callback)
    {
#if false
        GetClass(oldName, (c, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            //Debug.LogFormat("UpdateUserName() - {0} - {1} - {2} - {3}", profile.username, profile.password, profile.organization, profile.type);

            DeleteClass(oldName, error2 =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    if (callback != null)
                        callback(error2);

                    return;
                }

                c.classname = newName;

                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                vitaDynamoDB.AddEntity<EntityDBVitaProfile>(c, (result, error3) =>
                {
                    if (callback != null)
                        callback(error3);
                });
            });
        });
#else
        Debug.LogWarningFormat("TODO - UpdateClassname() - this function needs to be implemented.  Currently does nothing");

        if (callback != null)
            callback("");
#endif
    }

    delegate void AddItemFunction(string name, string item,  AddStudentToClassDelegate callback);

    IEnumerator WaitTillAddItemDone(AddItemFunction func, string username, string item,  AddStudentToClassDelegate callback)
    {
        //while (m_addItemInProgress)
        while (AddItemInProgressList.Count > AddItemInProgressInx && (bool)AddItemInProgressList[AddItemInProgressInx] )
        {
            yield return new WaitForEndOfFrame();
        }
        func(username, item, callback);
    }



    public delegate void AddStudentToClassDelegate(string error);

    public void AddStudentToClass(string className, string student, AddStudentToClassDelegate callback)
    {
        //if (m_addItemInProgress)
        if(AddItemInProgressList.Count > AddItemInProgressInx && (bool)AddItemInProgressList[AddItemInProgressInx])
        {
            StartCoroutine(WaitTillAddItemDone(AddStudentToClass, className, student, callback));
            AddItemInProgressList.Add(false);
            return;
        }

        AddItemInProgressList[AddItemInProgressInx] = true;
        //m_addItemInProgress = true; 


        GetClass(className, (ob, getAllError) =>
        {

            if (!string.IsNullOrEmpty(getAllError))
            {
                //m_addItemInProgress = false;
                    
                AddItemInProgressInx++;

                AddItemInProgressList[AddItemInProgressInx -1] = true;

                if(AddItemInProgressList.Count <= AddItemInProgressInx)  AddItemInProgressList.Add(false);


                if (callback != null) callback(getAllError);

                return;
            }

            EntityDBVitaClass c = ob;

            if(c.students.Contains(student))
            {
                //m_addItemInProgress = false;

                AddItemInProgressInx++;

                AddItemInProgressList[AddItemInProgressInx -1] = true;

                if(AddItemInProgressList.Count <= AddItemInProgressInx)  AddItemInProgressList.Add(false);

                Debug.Log("Student already exists : " + student);
            }
            else
            {
                c.students.Add(student);
                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                vitaDynamoDB.AddEntity<EntityDBVitaClass>(c, (result, error) =>
                {
                    //m_addItemInProgress = false;

                    AddItemInProgressInx++;

                    AddItemInProgressList[AddItemInProgressInx -1] = true;

                    if(AddItemInProgressList.Count <= AddItemInProgressInx)  AddItemInProgressList.Add(false);

                    if (callback != null) callback(error);
                 });
             }

        });
    }

    public delegate void UpdateClassDelegate(string error);

    public void UpdateClassTeacher(string className, string teacher, UpdateClassDelegate callback)
    {
        GetClass(className, (c, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            EntityDBVitaClass newClass = c;
            newClass.teacher = teacher;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaClass>(newClass, (result, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    delegate void RemoveItemFunction(string username, string item, bool removeAll, ItemDelegate callback);

    IEnumerator WaitTillRemoveItemDone(RemoveItemFunction func, string username, string item, bool removeAll, ItemDelegate callback)
    {
        while (m_removeItemInProgress)
        {
            yield return new WaitForEndOfFrame();
        }
        func(username, item,  removeAll, callback);
    }


    public delegate void ItemDelegate(string error);

    public void RemoveStudentFromClass(string classname, string studentName, ItemDelegate callback)
    {
        RemoveStudentFromClass(classname, studentName, false, callback);
    }

    public void RemoveAllStudentsFromClass(string classname, ItemDelegate callback)
    {
        RemoveStudentFromClass(classname, " ", true, callback);
    }

    void RemoveStudentFromClass(string classname, string studentName,  bool removeAll, ItemDelegate callback)
    {
        if (m_removeItemInProgress)
        {
            StartCoroutine(WaitTillRemoveItemDone(RemoveStudentFromClass, classname, studentName,  removeAll, callback));
            return;
        }

        m_removeItemInProgress = true;

        GetClass(classname, (cl, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                m_removeItemInProgress = false;

                if (callback != null)
                    callback(error);

                return;
            }

            EntityDBVitaClass objcl = cl;

           if (removeAll)
           {
                objcl.students = null;
           }
           else 
           {
                objcl.students.Remove(studentName);

                if(objcl.students.Count == 0)
                {
                    objcl.students = null;
                }
           }

           VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
           vitaDynamoDB.AddEntity<EntityDBVitaClass>(objcl, (result, error2) =>
           {
                m_removeItemInProgress = false;

                if (callback != null)
                    callback(error2);
           });
        });
    }

    public void RemoveStudentFromAllClasses(string studentName, string organization, ItemDelegate callback)
    {
        GetAllClassesInOrganization(organization, (list, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            List<EntityDBVitaClass> classes = new List<EntityDBVitaClass>();
            list.ForEach(c => { if (c.students.Contains(studentName)) { classes.Add(c); } });

            int classCount = classes.Count;

            if(classCount == 0)
            {
                if (callback != null)
                    callback("");

                return;
            }

            foreach (var c in classes)
            {
                RemoveStudentFromClass(c.classname, studentName, error2 =>
                {
                    classCount--;

                    if (!string.IsNullOrEmpty(error2))
                    {
                        if (callback != null)
                            callback(error2);

                        return;
                    }

                    if (classCount == 0)
                    {
                        if (callback != null)
                            callback(error2);

                        return;
                    }
                });
            }
        });
    }
}
