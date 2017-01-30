using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

public class DBOrganization : MonoBehaviour
{
    void Start()
    {
    }


    void Update()
    {
    }


    public delegate void CreateOrganizationDelegate(string error);

    public void CreateOrganization(string orgname, string firstname, string lastname, string email, string phone, string localAdminUser, string localAdminPassword, CreateOrganizationDelegate callback)
    {
        EntityDBVitaOrganization profile = new EntityDBVitaOrganization(orgname, firstname, lastname, email, phone, localAdminUser, localAdminPassword);

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.AddEntity<EntityDBVitaOrganization>(profile, (result, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            // each org has a built-in local admin user that's attached to the org.  Create that here
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.CreateUser(profile.username, profile.password, profile.name, string.Format("{0} {1}", firstname, lastname), DBUser.AccountType.ORGADMIN, error2 =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    public IEnumerator AddOrganizations(List<EntityDBVitaOrganization> orgs)
    {
        // TODO: Add bulk Add() function

        // this function is used for backup/restore purposes.  In normal operation, use Create() instead

        int waitCount = orgs.Count;
        foreach (var org in orgs)
        {
            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaOrganization>(org, (result, error) =>
            {
                waitCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("AddOrganizations() failed - {0}", org.name);
                        return;
                    }
                }
            });
        }

        while (waitCount > 0)
            yield return new WaitForEndOfFrame();
    }

    public delegate void GetOrganizationDelegate(EntityDBVitaOrganization org, string error);

    public void GetOrganization(string name, GetOrganizationDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();

        vitaDynamoDB.GetEntity<EntityDBVitaOrganization>(name, null, null, (result, error) =>
        {
            EntityDBVitaOrganization org = (EntityDBVitaOrganization)result;

            if (string.IsNullOrEmpty(error))
            {
                org.FixNullLists();
            }

            if (callback != null)
                callback(org, error);
        });
    }

    public delegate void GetAllOrganizationsDelegate(List<EntityDBVitaOrganization> organizations, string error);

    public void GetAllOrganizations(GetAllOrganizationsDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetAllEntities<EntityDBVitaOrganization>(EntityDBVitaOrganization.tableName, null, (result, error)=>
        {
            List<EntityDBVitaOrganization> orgs = null;
            if (string.IsNullOrEmpty(error))
            {
                orgs = new List<EntityDBVitaOrganization>((List<EntityDBVitaOrganization>)result);
                orgs.Sort((a, b) => a.name.CompareTo(b.name));
                orgs.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(orgs, error);
        });
    }

    public delegate void DeleteOrganizationDelegate(string error);

    public void DeleteOrganization(string orgName, DeleteOrganizationDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.DeleteEntity<EntityDBVitaOrganization>(orgName, null, (result, error) =>
        {
            if (callback != null)
                callback(error);
        });
    }

    public void DeleteOrganizationAndData(string orgName, DeleteOrganizationDelegate callback)
    {
        StartCoroutine(DeleteOrganizationAndDataInternal(orgName, callback));
    }

    IEnumerator DeleteOrganizationAndDataInternal(string orgName, DeleteOrganizationDelegate callback)
    {
        bool error = false;
        bool waitForCallback = true;

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntities<EntityDBVitaClass>(EntityDBVitaClass.tableName, "organization-index", "organization", orgName, null, (list1, error1) =>
        {
            waitForCallback = false;
            if (!string.IsNullOrEmpty(error1))
            {
                error = true;
                if (callback != null)
                    callback(error1);

                return;
            }

            List<EntityDBVitaClass> classItems = (List<EntityDBVitaClass>)list1;
            if (classItems.Count > 0)
            {
                waitForCallback = true;

                vitaDynamoDB.BatchDelete<EntityDBVitaClass>(classItems, (obj2, error2) =>
                {
                    waitForCallback = false;

                    if (!string.IsNullOrEmpty(error2))
                    {
                        error = true;
                        if (callback != null)
                            callback(error2);

                        return;
                    }
                });
            }
        });

        while (waitForCallback)
            yield return new WaitForEndOfFrame();

        if (error)
            yield break;

        error = false;
        waitForCallback = true;


        vitaDynamoDB.GetEntities<EntityDBVitaClassHomeworkAssigment>(EntityDBVitaClassHomeworkAssigment.tableName, "organization-index", "organization", orgName, null, (list3, error3) =>
        {
            waitForCallback = false;

            if (!string.IsNullOrEmpty(error3))
            {
                error = true;
                if (callback != null)
                    callback(error3);

                return;
            }

            List<EntityDBVitaClassHomeworkAssigment> classAssigmentItems = (List<EntityDBVitaClassHomeworkAssigment>)list3;
            if (classAssigmentItems.Count > 0)
            {
                waitForCallback = true;

                vitaDynamoDB.BatchDelete<EntityDBVitaClassHomeworkAssigment>(classAssigmentItems, (obj4, error4) =>
                {
                    waitForCallback = false;

                    if (!string.IsNullOrEmpty(error4))
                    {
                        error = true;
                        if (callback != null)
                            callback(error4);

                        return;
                    }
                });
            }
        });

        while (waitForCallback)
            yield return new WaitForEndOfFrame();

        if (error)
            yield break;

        error = false;
        waitForCallback = true;


        vitaDynamoDB.GetEntities<EntityDBVitaProfile>(VitaDynamoDB.TableNameEntityDBVitaProfile, "organization-index", "organization", orgName, null, (list5, error5) =>
        {
            waitForCallback = false;

            if (!string.IsNullOrEmpty(error5))
            {
                error = true;
                if (callback != null)
                    callback(error5);

                return;
            }

            List<EntityDBVitaProfile> profileItems = (List<EntityDBVitaProfile>)list5;
            if (profileItems.Count > 0)
            {
                waitForCallback = true;

                vitaDynamoDB.BatchDelete<EntityDBVitaProfile>(profileItems, (obj6, error6) =>
                {
                    waitForCallback = false;

                    if (!string.IsNullOrEmpty(error6))
                    {
                        error = true;
                        if (callback != null)
                            callback(error6);

                        return;
                    }
                });
            }
        });

        while (waitForCallback)
            yield return new WaitForEndOfFrame();

        if (error)
            yield break;

        error = false;
        waitForCallback = true;


        vitaDynamoDB.GetEntities<EntityDBVitaStudentSession>("VitaStudentSession", "organization-index", "organization", orgName, null, (listB, errorB) =>
        {
            waitForCallback = false;

            if (!string.IsNullOrEmpty(errorB))
            {
                error = true;
                if (callback != null)
                    callback(errorB);

                return;
            }

            List<EntityDBVitaStudentSession> studentSessionItems = (List<EntityDBVitaStudentSession>)listB;
            if (studentSessionItems.Count > 0)
            {
                waitForCallback = true;

                vitaDynamoDB.BatchDelete<EntityDBVitaStudentSession>(studentSessionItems, (objC, errorC) =>
                {
                    waitForCallback = false;

                    if (!string.IsNullOrEmpty(errorC))
                    {
                        error = true;
                        if (callback != null)
                            callback(errorC);

                        return;
                    }
                });
            }
        });

        while (waitForCallback)
            yield return new WaitForEndOfFrame();

        if (error)
            yield break;

        error = false;
        waitForCallback = true;


        DeleteOrganization(orgName, (errorD) =>
        {
            if (callback != null)
                callback(errorD);
        });
    }

    public delegate void ArchiveOrganizationDelegate(string error);

    public void ArchiveOrganization(string orgName, ArchiveOrganizationDelegate callback)
    {
        GetOrganization(orgName, (org, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            org.archived = 1;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaOrganization>(org, (result, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    public delegate void ReinstateOrganizationDelegate(string error);

    public void ReinstateOrganization(string orgName, ReinstateOrganizationDelegate callback)
    {
        GetOrganization(orgName, (org, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            org.archived = 0;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaOrganization>(org, (result, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    public delegate void UpdateOrganizationNameDelegate(string error);

    public void UpdateOrganizationName(string oldName, string newName, UpdateOrganizationNameDelegate callback)
    {
        GetOrganization(oldName, (org, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            org.name = newName;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaOrganization>(org, (result2, error2) =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    if (callback != null)
                        callback(error2);

                    return;
                }

                vitaDynamoDB.DeleteEntity<EntityDBVitaProfile>(oldName, null, (result3, error3) =>
                {
                    if (callback != null)
                        callback(error3);
                });
            });
        });
    }


    public delegate void UpdateOrganizationDelegate(string error);

    [Obsolete("This overload does not save account expiration date")]
    public void UpdateOrganization(string name, string firstname, string lastname, string email, string phone, string username, string password, UpdateOrganizationDelegate callback)
    {
        UpdateOrganization(name, firstname, lastname, email, phone, username, password, "", callback);
    }

    public void UpdateOrganization(string name, string firstname, string lastname, string email, string phone, string username, string password, string accexpire, UpdateOrganizationDelegate callback)
    {
        GetOrganization(name, (org, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            org.firstname = firstname;
            org.lastname = lastname;
            org.email = email;
            org.phone = phone;
            org.username = username;
            org.password = password;
            org.accexpire = accexpire;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaOrganization>(org, (result, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }
}
