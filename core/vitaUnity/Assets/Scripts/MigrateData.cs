using Amazon.DynamoDBv2.DataModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

public class MigrateData : MonoBehaviour
{
    // Steps:
    // Copy original classes here, rename to match current version (only need to copy those that contain changes)
    // Make your changes to db classes
    // Fill out Convert functions appropriately
    // Increment VitaGlobals.m_versionPrefix
    // Click Upgrade DB button
    // Commit everything


    class DatabaseData
    {
        public List<EntityDBVitaOrganization> m_orgs = new List<EntityDBVitaOrganization>();
        public List<EntityDBVitaProfile> m_users = new List<EntityDBVitaProfile>();
        public List<EntityDBVitaClass> m_classes = new List<EntityDBVitaClass>();
        public List<EntityDBVitaStudentSession> m_sessions = new List<EntityDBVitaStudentSession>();
        public List<EntityDBVitaClassHomeworkAssigment> m_assignments = new List<EntityDBVitaClassHomeworkAssigment>();
    }

    IEnumerator WriteData(DatabaseData data)
    {
        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        yield return StartCoroutine(dbOrg.AddOrganizations(data.m_orgs));

        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        yield return StartCoroutine(dbUser.AddUsers(data.m_users));

        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
        yield return StartCoroutine(dbClass.AddClasses(data.m_classes));

        DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
        yield return StartCoroutine(dbSession.AddSessions(data.m_sessions));

        DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
        yield return StartCoroutine(dbAssignment.AddAssignments(data.m_assignments));

        Debug.LogWarningFormat("WriteData() finished");
    }


    public IEnumerator MigrateData_1_1_to_1_2()
    {
        DatabaseData_1_1 data_1_1 = new DatabaseData_1_1();
        yield return StartCoroutine(GetData_1_1(data_1_1));
        DatabaseData data = Convert_1_1_to_1_2(data_1_1);

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        yield return StartCoroutine(vitaDynamoDB.DeleteDatabase());
        yield return StartCoroutine(vitaDynamoDB.CreateDatabase());

        Debug.LogWarningFormat("MigrateData_1_1_to_1_2() - Waiting 30 secs for Tables to finish creating");
        yield return new WaitForSeconds(30);

        yield return StartCoroutine(WriteData(data));

        // update DB version to current version
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        dbUser.UpdateUser(VitaGlobals.m_versionUsername, "root", VitaGlobals.m_versionDB, DBUser.AccountType.STUDENT, VitaGlobals.CurrentTimeToString(), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("UpdateUser() - error: {0}", error); });

        Debug.LogWarningFormat("MigrateData_1_1_to_1_2() finished");
    }



    // 1.1  //////////////////////////

    public class EntityDBVitaProfile_1_1
    {
        // Use Hash and Range Key to Authenticate a user
        [DynamoDBHashKey]   // Hash key.
        public string           username        { get; set; }

        [DynamoDBRangeKey]  // Range Key
        public string           password        {get; set;}

        [DynamoDBProperty]
        public string           organization    {get; set;}

        [DynamoDBProperty]
        public int              type            { get; set; }  //0 --> root admin //1 --> organization admin  //2 -->teacher //3 -->student

        [DynamoDBProperty]
        public string           name            { get; set; }

        [DynamoDBProperty]
        public List<string>     badges          { get; set; }

        [DynamoDBProperty]
        public List<string>     unlocks         { get; set; }

        [DynamoDBProperty]
        public int              currency        { get; set; }

        [DynamoDBProperty]
        public List<string>     scores          { get; set; }

        [DynamoDBProperty]
        public string           createdate       {get;set;}

        [DynamoDBProperty]
        public string           expiredate      {get;set;}

        [DynamoDBProperty]
        public string           lastlogin       {get;set;}

        [DynamoDBProperty]
        public int              archived        {get; set;}

        [DynamoDBProperty]
        public List<string>     favorites       { get; set; }


        public static string tableName = "VitaProfile";

        public void FixNullLists()
        {
            // when you get a profile from the DB, and the list is empty, the variable is null.
            // for the user, we want an empty list, this makes the code a little cleaner
            // so check this and fix it up before sending it to the user

            if (this.favorites == null) this.favorites = new List<string>();
            if (this.badges == null)    this.badges = new List<string>();
            if (this.unlocks == null)   this.unlocks = new List<string>();
            if (this.scores == null)    this.scores = new List<string>();
        }
    }

    public delegate void GetAllUsersDelegate_1_1(List<EntityDBVitaProfile_1_1> users, string error);

    public void GetAllUsers_1_1(GetAllUsersDelegate_1_1 callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetAllEntities<EntityDBVitaProfile_1_1>(VitaDynamoDB.TableNameEntityDBVitaProfile, null, (result, error)=>
        {
            List<EntityDBVitaProfile_1_1> users = null;
            if (string.IsNullOrEmpty(error))
            {
                users = new List<EntityDBVitaProfile_1_1>((List<EntityDBVitaProfile_1_1>)result);
                users.Sort((a, b) => a.username.CompareTo(b.username));
                users.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(users, error);
        });
    }

    class DatabaseData_1_1
    {
        public List<EntityDBVitaOrganization> m_orgs = new List<EntityDBVitaOrganization>();
        public List<EntityDBVitaProfile_1_1> m_users = new List<EntityDBVitaProfile_1_1>();
        public List<EntityDBVitaClass> m_classes = new List<EntityDBVitaClass>();
        public List<EntityDBVitaStudentSession> m_sessions = new List<EntityDBVitaStudentSession>();
        public List<EntityDBVitaClassHomeworkAssigment> m_assignments = new List<EntityDBVitaClassHomeworkAssigment>();
    }

    IEnumerator GetData_1_1(DatabaseData_1_1 data)
    {
        bool waitForDB = true;
        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        dbOrg.GetAllOrganizations((orgs, error) =>
        {
            waitForDB = false;

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("GetAllOrganizations() error: {0}", error);
                return;
            }

            data.m_orgs = orgs;
        });

        while (waitForDB)
            yield return new WaitForEndOfFrame();


        waitForDB = true;
        GetAllUsers_1_1((users, error) =>
        {
            waitForDB = false;

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("GetAllUsers_1_1() error: {0}", error);
                return;
            }

            data.m_users = users;
        });

        while (waitForDB)
            yield return new WaitForEndOfFrame();


        waitForDB = true;
        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
        dbClass.GetAllClasses((classes, error) =>
        {
            waitForDB = false;

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("GetAllClasses() error: {0}", error);
                return;
            }

            data.m_classes = classes;
        });

        while (waitForDB)
            yield return new WaitForEndOfFrame();


        waitForDB = true;
        DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
        dbSession.GetAllSessions((sessions, error) =>
        {
            waitForDB = false;

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("GetAllSessions() error: {0}", error);
                return;
            }

            data.m_sessions = sessions;
        });

        while (waitForDB)
            yield return new WaitForEndOfFrame();


        waitForDB = true;
        DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
        dbAssignment.GetAllAssignments((assignments, error) =>
        {
            waitForDB = false;

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("GetAllAssignments() error: {0}", error);
                return;
            }

            data.m_assignments = assignments;
        });

        while (waitForDB)
            yield return new WaitForEndOfFrame();


        yield return new WaitForSeconds(1);

        Debug.LogWarningFormat("GetData_1_1() finished");
    }

    DatabaseData Convert_1_1_to_1_2(DatabaseData_1_1 data_1_1)
    {
        DatabaseData data = new DatabaseData();

        data.m_orgs = data_1_1.m_orgs;
        data.m_classes = data_1_1.m_classes;
        data.m_sessions = data_1_1.m_sessions;
        data.m_assignments = data_1_1.m_assignments;

        foreach (var user in data_1_1.m_users)
        {
            data.m_users.Add(new EntityDBVitaProfile() {
                username = user.username,
                password = user.password,
                organization = user.organization,
                type = user.type,
                name = user.name,
                badges = user.badges,
                unlocks = user.unlocks,
                currency = user.currency,
                scores = user.scores,
                createdate = user.createdate,
                expiredate = user.expiredate,
                lastlogin = user.lastlogin,
                archived = user.archived,
                favorites = user.favorites
            });
        }

        return data;
    }

    ////////////////////////////
}
