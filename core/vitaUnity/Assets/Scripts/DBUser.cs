using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

public class DBUser : MonoBehaviour
{
    public class BadgeData
    {
        public Int64 TimeStamp;
        public string BadgeId;

        public static BadgeData NewBadge(string badgeId) { return new BadgeData() { BadgeId = badgeId, TimeStamp = VitaGlobals.CurrentTimeToTicks() }; }

        public static BadgeData ToBadge(string data) { return JsonUtility.FromJson<BadgeData>(data); }
        public static string ToJSONString(BadgeData data) { return JsonUtility.ToJson(data); }
    };

    public class FavoriteData
    {
        public Int64 TimeStamp;
        public string FavoriteId;

        public static FavoriteData NewFavorite(string favoriteId) { return new FavoriteData() { TimeStamp = VitaGlobals.CurrentTimeToTicks(), FavoriteId = favoriteId }; }

        public static FavoriteData ToFavorite(string data) { return JsonUtility.FromJson<FavoriteData>(data); }
        public static string ToJSONString(FavoriteData data) { return JsonUtility.ToJson(data); }
    }

    public class UnlockData
    {
        public Int64 TimeStamp;
        public string UnlockId;

        public static UnlockData NewUnlock(string unlockId) { return new UnlockData() {  TimeStamp = VitaGlobals.CurrentTimeToTicks(), UnlockId = unlockId }; }

        public static UnlockData ToUnlock(string data) { return JsonUtility.FromJson<UnlockData>(data); }
        public static string ToJSONString(UnlockData data) { return JsonUtility.ToJson(data); }
    }

    public class ScoreData
    {
        public Int64 TimeStamp;
        public string ScoreId;

        public static ScoreData NewScore(string scoreId) { return new ScoreData() {  TimeStamp = VitaGlobals.CurrentTimeToTicks(), ScoreId = scoreId }; }

        public static ScoreData ToScore(string data) { return JsonUtility.FromJson<ScoreData>(data); }
        public static string ToJSONString(ScoreData data) { return JsonUtility.ToJson(data); }
    }


    public enum AccountType
    {
        ROOT = 0,
        ORGADMIN = 1,
        TEACHER = 2,
        STUDENT = 3,
    };


   // bool m_addItemInProgress = false;
    bool m_removeItemInProgress = false;


    public class IntWrapper
    {
        public int Value { get; set; }
        public IntWrapper(int value) { Value = value; }
    }


    ArrayList AddFavInProgressList = new ArrayList();
    IntWrapper AddFavInProgressInx = null;
  
    ArrayList AddScoreInProgressList = new ArrayList();
    IntWrapper AddScoreInProgressInx = null;

    ArrayList AddUnlockInProgressList = new ArrayList();
    IntWrapper AddUnlockInProgressInx = null;

    ArrayList AddBadgeInProgressList = new ArrayList();
    IntWrapper AddBadgeInProgressInx = null;


    void Start()
    {
        AddFavInProgressList.Clear();
        AddFavInProgressInx = new IntWrapper(0);
        AddFavInProgressList.Add(false);
  
        AddScoreInProgressList.Clear();
        AddScoreInProgressInx =  new IntWrapper(0);;
        AddScoreInProgressList.Add(false);

        AddUnlockInProgressList.Clear();
        AddUnlockInProgressInx =  new IntWrapper(0);
        AddUnlockInProgressList.Add(false);

        AddBadgeInProgressList.Clear();
        AddBadgeInProgressInx =  new IntWrapper(0);
        AddBadgeInProgressList.Add(false);
    }


    void Update()
    {
    }


    public delegate void LoginDelegate(EntityDBVitaProfile profile, string error);

    public void Login(string username, string password, LoginDelegate callback)
    {
        string usernameLower = username.ToLower();

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntity<EntityDBVitaProfile>(usernameLower, password, null, (result, error) =>
        {
            EntityDBVitaProfile profile = null;
            if (string.IsNullOrEmpty(error))
            {
                if (result != null)
                {
                    profile = (EntityDBVitaProfile)result;
                    profile.FixNullLists();

                    if (!string.IsNullOrEmpty(error))
                    {
                        PopUpDisplay.Instance.Display("Error", error);
                    }
                }
            }

            if (callback != null)
                callback(profile, error);
        });
    }

    public delegate void CreateUserDelegate(string error);

    public void CreateUser(string username, string password, string organization, string name, AccountType accountType, CreateUserDelegate callback)
    {
        // TODO - needs to have an organization parameter.  If anything other than ROOT, there needs to be a server check if organization exists, and fail if it doesn't.

        string usernameLower = username.ToLower();

        EntityDBVitaProfile profile = new EntityDBVitaProfile(usernameLower, password, organization, name, (int)accountType);

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.AddEntity<EntityDBVitaProfile>(profile, (result, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            // each user has a 'GLOBAL' session to contain global events not related to a specific session
            DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
            dbSession.CreateSession("GLOBAL", usernameLower, "NONE", profile.organization, error2 =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    if (callback != null)
                        callback(error2);

                    return;
                }

                dbSession.AddEvent("GLOBAL", usernameLower, DBSession.EventData.NewAccountCreation(), error3 =>
                {
                    if (callback != null)
                        callback(error3);
                });
            });
        });
    }

    public IEnumerator AddUsers(List<EntityDBVitaProfile> users)
    {
        // TODO: Add bulk Add() function

        // this function is used for backup/restore purposes.  In normal operation, use Create() instead

        int waitCount = users.Count;
        foreach (var user in users)
        {
            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaProfile>(user, (result, error) =>
            {
                waitCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("AddUsers() failed - {0}", user.username);
                        return;
                    }
                }
            });
        }

        while (waitCount > 0)
            yield return new WaitForEndOfFrame();
    }


    public delegate void GetUserDelegate(EntityDBVitaProfile profile, string error);

    public void GetUser(string username, GetUserDelegate callback)
    {
        string usernameLower = username.ToLower();

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntities<EntityDBVitaProfile>(VitaDynamoDB.TableNameEntityDBVitaProfile, "username-index", "username", usernameLower, null, (list, error) =>
        {
            List<EntityDBVitaProfile> users = (List<EntityDBVitaProfile>)list;

            EntityDBVitaProfile profile = null;
            string result = "";
            if (!string.IsNullOrEmpty(error))
            {
                result = ":ERROR: " + error;
            }
            else if (users.Count > 1)
            {
                result = ":ERROR: Multiple accounts";
            }
            else if (users.Count == 0)
            {
                result = ":ERROR: No profile found";
            }
            else
            {
                profile = users[0];
                profile.FixNullLists();
            }

            if (callback != null)
                callback(profile, result);
        });
    }

    public delegate void GetAllUsersDelegate(List<EntityDBVitaProfile> users, string error);

    public void GetAllUsers(GetAllUsersDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetAllEntities<EntityDBVitaProfile>(VitaDynamoDB.TableNameEntityDBVitaProfile, null, (result, error)=>
        {
            List<EntityDBVitaProfile> users = null;
            if (string.IsNullOrEmpty(error))
            {
                users = new List<EntityDBVitaProfile>((List<EntityDBVitaProfile>)result);
                users.Sort((a, b) => a.username.CompareTo(b.username));
                users.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(users, error);
        });
    }

    public void GetAllUsersInOrganization(string organization, GetAllUsersDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntities<EntityDBVitaProfile>(VitaDynamoDB.TableNameEntityDBVitaProfile, "organization-index", "organization", organization, null, (result, error) =>
        {
            List<EntityDBVitaProfile> users =null;
            if (string.IsNullOrEmpty(error))
            {
                users = new List<EntityDBVitaProfile>((List<EntityDBVitaProfile>)result);            
                users.Sort((a, b) => a.username.CompareTo(b.username));
                users.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(users, error);
        });
    }

    public void GetAllTeachersInOrganization(string organization, GetAllUsersDelegate callback)
    {
        GetAllUsersInOrganization(organization, (users, error) =>
        {
            List<EntityDBVitaProfile> teachers = null;
            if (string.IsNullOrEmpty(error))
            {
                teachers = new List<EntityDBVitaProfile>();
                foreach (var user in users)
                {
                    if (user.type == (int)AccountType.TEACHER)
                        teachers.Add(user);
                }
            }

            if (callback != null)
                callback(teachers, error);
        });
    }

    public void GetAllStudentsInOrganization(string organization, GetAllUsersDelegate callback)
    {
        GetAllUsersInOrganization(organization, (users, error) =>
        {
            List<EntityDBVitaProfile> students = null;
            if (string.IsNullOrEmpty(error))
            {
                students = new List<EntityDBVitaProfile>();
                foreach (var user in users)
                {
                    if (user.type == (int)AccountType.STUDENT)
                        students.Add(user);
                }
            }

            if (callback != null)
                callback(students, error);
        });
    }

    public delegate void DeleteUserDelegate(string error);

    public void DeleteUser(string username, DeleteUserDelegate callback)
    {
        string usernameLower = username.ToLower();

        GetUser(usernameLower, (profile, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.DeleteEntity<EntityDBVitaProfile>(profile.username, profile.password, (result, error2) =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    if (callback != null)
                        callback(error2);

                    return;
                }

                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.DeleteAllSessionsForUser(usernameLower, (error3) =>
                {
                    if (!string.IsNullOrEmpty(error3))
                    {
                        if (callback != null)
                            callback(error3);

                        return;
                    }

                    DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
                    dbAssignment.DeleteAllAssignmentsForStudent(usernameLower, profile.organization, (error4) =>
                    {
                        if (!string.IsNullOrEmpty(error4))
                        {
                            if (callback != null)
                                callback(error4);

                            return;
                        }

                        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
                        dbClass.RemoveStudentFromAllClasses(usernameLower, profile.organization, (error5) =>
                        {
                            if (callback != null)
                                callback(error5);
                        });
                    });
                });
            });
        });
    }

    void DeleteUserOnly(string username, DeleteUserDelegate callback)
    {
        string usernameLower = username.ToLower();

        // this only deletes the user, not the session data.  This is useful for changing the password, possibly other cases
        GetUser(usernameLower, (profile, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.DeleteEntity<EntityDBVitaProfile>(profile.username, profile.password, (result, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    public delegate void ArchiveUserDelegate(string error);

    public void ArchiveUser(string username, ArchiveUserDelegate callback)
    {
        string usernameLower = username.ToLower();

        GetUser(usernameLower, (profile, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            //Debug.LogFormat("ArchiveUser() - {0} - {1} - {2} - {3}", profile.username, profile.password, profile.organization, profile.type);

            profile.archived = 1;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaProfile>(profile, (result, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    public delegate void ReinstateUserDelegate(string error);

    public void ReinstateUser(string username, ReinstateUserDelegate callback)
    {
        string usernameLower = username.ToLower();

        GetUser(usernameLower, (profile, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            //Debug.LogFormat("ReinstateUser() - {0} - {1} - {2} - {3}", profile.username, profile.password, profile.organization, profile.type);

            profile.archived = 0;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaProfile>(profile, (result, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }


    public delegate void UpdateUserNameDelegate(string error);

    public void UpdateUserName(string oldName, string newName, UpdateUserNameDelegate callback)
    {
        string oldNameLower = oldName.ToLower();
        string newNameLower = newName.ToLower();

        GetUser(oldNameLower, (profile, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            //Debug.LogFormat("UpdateUserName() - {0} - {1} - {2} - {3}", profile.username, profile.password, profile.organization, profile.type);

            DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
            dbSession.GetAllSessionsForUser(oldNameLower, (sessions, error2) =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    if (callback != null)
                        callback(error2);

                    return;
                }

                DeleteUser(oldNameLower, error3 =>
                {
                    if (!string.IsNullOrEmpty(error3))
                    {
                        if (callback != null)
                            callback(error3);

                        return;
                    }

                    profile.username = newNameLower;

                    VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                    vitaDynamoDB.AddEntity<EntityDBVitaProfile>(profile, (result, error4) =>
                    {
                        if (!string.IsNullOrEmpty(error4))
                        {
                            if (callback != null)
                                callback(error4);

                            return;
                        }

                        int count = sessions.Count;

                        foreach (var item in sessions)
                        {
                            item.username = newNameLower;

                            dbSession.CreateSession(item, (error5) =>
                            {
                                count--;

                                if (!string.IsNullOrEmpty(error5))
                                {
                                    if (callback != null)
                                        callback(error5);

                                    return;
                                }

                                if (count == 0)
                                {
                                    if (callback != null)
                                        callback(error4);
                                }
                            });
                        }
                    });
                });
            });
        });
    }

    public delegate void UpdateUserPasswordDelegate(string error);

    public void UpdateUserPassword(string username, string password, UpdateUserPasswordDelegate callback)
    {
        string usernameLower = username.ToLower();

        GetUser(usernameLower, (profile, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            //Debug.LogFormat("UpdateUserPassword() - {0} - {1} - {2} - {3}", profile.username, profile.password, profile.organization, profile.type);

            DeleteUserOnly(usernameLower, error2 =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    if (callback != null)
                        callback(error2);

                    return;
                }

                profile.password = password;

                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                vitaDynamoDB.AddEntity<EntityDBVitaProfile>(profile, (result, error3) =>
                {
                    if (callback != null)
                        callback(error3);
                });
            });
        });
    }

    public delegate void UpdateUserDelegate(string error);

    public void UpdateUserLastLogin(EntityDBVitaProfile user, string lastlogin, UpdateUserDelegate callback)
    {
        UpdateUser(user.username, user.organization, user.name, (AccountType)user.type, lastlogin, callback);
    }

    public void UpdateUser(string username, string organization, string name, AccountType accountType, string lastlogin, UpdateUserDelegate callback)
    {
        string usernameLower = username.ToLower();

        GetUser(usernameLower, (profile, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            //Debug.LogFormat("UpdateUser() - {0} - {1} - {2} - {3}", profile.username, profile.password, profile.organization, profile.type);

            profile.organization = organization;
            profile.name = name;
            profile.type = (int)accountType;
            profile.lastlogin = lastlogin;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaProfile>(profile, (result, error2) =>
            {
                if (callback != null)
                    callback(error2);
            });
        });
    }

    public delegate void ItemDelegate(EntityDBVitaProfile profile, string error);

    public void AddFavorite(string username, FavoriteData item, ItemDelegate callback )
    {
        AddItem(username, FavoriteData.ToJSONString(item), "FavoriteData", callback);
    }

    public void AddScore(string username, ScoreData item, ItemDelegate callback)
    {
        AddItem(username, ScoreData.ToJSONString(item), "ScoreData", callback);
    }

    public void AddUnlock(string username, UnlockData item, ItemDelegate callback)
    {
        AddItem(username, UnlockData.ToJSONString(item), "UnlockData", callback);
    }

    public void AddBadge(string username, BadgeData item, ItemDelegate callback)
    {
        AddItem(username, BadgeData.ToJSONString(item), "BadgeData", (profile, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(profile, error);

                return;
            }

            DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
            dbSession.AddEvent("GLOBAL", username, DBSession.EventData.NewBadgeEarned(), error2 =>
            {
                if (callback != null)
                    callback(profile, error2);
            });
        });
    }


    public void RemoveFavorite(string username, FavoriteData item, ItemDelegate callback)
    {
        RemoveItem(username, FavoriteData.ToJSONString(item), "FavoriteData", callback);
    }

    public void RemoveScore(string username, ScoreData item, ItemDelegate callback)
    {
        RemoveItem(username, ScoreData.ToJSONString(item), "ScoreData", callback);
    }

    public void RemoveUnlock(string username, UnlockData item, ItemDelegate callback)
    {
        RemoveItem(username, UnlockData.ToJSONString(item), "UnlockData", callback);
    }

    public void RemoveBadge(string username, BadgeData item, ItemDelegate callback)
    {
        RemoveItem(username, BadgeData.ToJSONString(item), "BadgeData", callback);
    }

    public void RemoveAllFavorites(string username, ItemDelegate callback)
    {
        RemoveAllItems(username, "FavoriteData", callback);
    }

    public void RemoveAllScores(string username, ItemDelegate callback)
    {
        RemoveAllItems(username, "ScoreData", callback);
    }

    public void RemoveAllUnlocks(string username, ItemDelegate callback)
    {
        RemoveAllItems(username, "UnlockData", callback);
    }

    public void RemoveAllBadges(string username, ItemDelegate callback)
    {
        RemoveAllItems(username, "BadgeData", callback);
    }

    delegate void AddItemFunction(string username, string item, string type, ItemDelegate callback);

    IEnumerator WaitTillAddItemDone(AddItemFunction func, string username, string item, string type, ItemDelegate callback)
    {

        ArrayList AddItemInProgressList = null;

        IntWrapper AddItemInProgressInx = null;

        switch (type)
         {
            case "BadgeData": 
            {
                AddItemInProgressList = AddBadgeInProgressList;
                AddItemInProgressInx = AddBadgeInProgressInx;
            }
            break;
            case "FavoriteData":
            {
                AddItemInProgressList = AddFavInProgressList;
                AddItemInProgressInx = AddFavInProgressInx;
             }
             break;
             case "UnlockData": 
             {
                AddItemInProgressList = AddUnlockInProgressList;
                AddItemInProgressInx = AddUnlockInProgressInx;
             }
             break;
             case "ScoreData": 
             {
                AddItemInProgressList = AddScoreInProgressList;
                AddItemInProgressInx = AddScoreInProgressInx;
             }
             break;                    
             default: break;
            }


        while (AddItemInProgressList.Count > AddItemInProgressInx.Value && (bool)AddItemInProgressList[AddItemInProgressInx.Value] )
        {
            yield return new WaitForEndOfFrame();
        }
        func(username, item, type, callback);
    }

    void AddItem(string username, string item, string type, ItemDelegate callback)
    {

        ArrayList AddItemInProgressList = null;

        IntWrapper AddItemInProgressInx = null;

        switch (type)
         {
            case "BadgeData": 
            {
                AddItemInProgressList = AddBadgeInProgressList;
                AddItemInProgressInx = AddBadgeInProgressInx;
            }
            break;
            case "FavoriteData":
            {
                AddItemInProgressList = AddFavInProgressList;
                AddItemInProgressInx = AddFavInProgressInx;
             }
             break;
             case "UnlockData": 
             {
                AddItemInProgressList = AddUnlockInProgressList;
                AddItemInProgressInx = AddUnlockInProgressInx;
             }
             break;
             case "ScoreData": 
             {
                AddItemInProgressList = AddScoreInProgressList;
                AddItemInProgressInx = AddScoreInProgressInx;
             }
             break;                    
             default: break;
            }


        if(AddItemInProgressList.Count > AddItemInProgressInx.Value && (bool)AddItemInProgressList[AddItemInProgressInx.Value])
        {
            StartCoroutine(WaitTillAddItemDone(AddItem, username, item, type, callback));
            AddItemInProgressList.Add(false);

            return;
        }

        //m_addItemInProgress = true;
        AddItemInProgressList[AddItemInProgressInx.Value] = true;

        string usernameLower = username.ToLower();

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        GetUser(usernameLower, (user, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
              //  m_addItemInProgress = false;

                 AddItemInProgressInx.Value++;

                AddItemInProgressList[AddItemInProgressInx.Value -1] = true;

                if(AddItemInProgressList.Count <= AddItemInProgressInx.Value)  AddItemInProgressList.Add(false);

                if (callback != null)
                    callback(null, error);

                return;
            }

            switch (type)
            {
                case "BadgeData": user.badges.Add(item); break;
                case "FavoriteData": user.favorites.Add(item); break;
                case "UnlockData": user.unlocks.Add(item); break;
                case "ScoreData": user.scores.Add(item); break;
                default: break;
            }

            vitaDynamoDB.AddEntity<EntityDBVitaProfile>(user, (result, error2) =>
            {
               // m_addItemInProgress = false;

                AddItemInProgressInx.Value++;

                AddItemInProgressList[AddItemInProgressInx.Value -1] = true;

                if(AddItemInProgressList.Count <= AddItemInProgressInx.Value)  AddItemInProgressList.Add(false);

                if (callback != null)
                    callback(user, error2);
            });
        });
    }

    delegate void RemoveItemFunction(string username, string item, string type, bool removeAll, ItemDelegate callback);

    IEnumerator WaitTillRemoveItemDone(RemoveItemFunction func, string username, string item, string type, bool removeAll, ItemDelegate callback)
    {
        while (m_removeItemInProgress)
        {
            yield return new WaitForEndOfFrame();
        }
        func(username, item, type, removeAll, callback);
    }

    void RemoveAllItems(string username, string type, ItemDelegate callback)
    {
        RemoveItem(username, "", type, true, callback);
    }

    void RemoveItem(string username, string item, string type, ItemDelegate callback)
    {
        RemoveItem(username, item, type, false, callback);
    }

    void RemoveItem(string username, string item, string type, bool removeAll, ItemDelegate callback)
    {
        if (m_removeItemInProgress)
        {
            StartCoroutine(WaitTillRemoveItemDone(RemoveItem, username, item, type, removeAll, callback));
            return;
        }

        m_removeItemInProgress = true;

        string usernameLower = username.ToLower();

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        GetUser(usernameLower, (user, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                m_removeItemInProgress = false;

                if (callback != null)
                    callback(null, error);

                return;
            }

            switch (type)
            {
                case "BadgeData":    if (removeAll) user.badges = null; else user.badges.Remove(item); break;
                case "FavoriteData": if (removeAll) user.favorites = null; else user.favorites.Remove(item); break;
                case "UnlockData":   if (removeAll) user.unlocks = null; else user.unlocks.Remove(item); break;
                case "ScoreData":    if (removeAll) user.scores = null; else user.scores.Remove(item); break;
                default: break;
            }

            vitaDynamoDB.AddEntity<EntityDBVitaProfile>(user, (result, error2) =>
            {
                m_removeItemInProgress = false;

                user.FixNullLists();

                if (callback != null)
                    callback(user, error2);
            });
        });
    }

    public bool DoesUserHaveBadge(EntityDBVitaProfile user, string badgeName)
    {
        // this function does not hit the DB

        foreach (var badgeString in user.badges)
        {
            var badge = BadgeData.ToBadge(badgeString);
            if (badge.BadgeId == badgeName)
                return true;
        }

        return false;
    }

    public delegate void ExportStudentsDataByOrganizationDelegate(string studentCSV, string sessionCSV, string error);

    public void ExportStudentsDataByOrganization(string org, ExportStudentsDataByOrganizationDelegate callback)
    {
        GetAllStudentsInOrganization(org, (studentsList, error) =>
        {
            string studentsCSV = "";
            string sessionsCSV = "";

            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(studentsCSV, sessionsCSV, error);

                return;
            }

            studentsCSV += "[username]"     +  ","
                         + "[password]"     +  ","
                         + "[organization]" +  ","
                         + "[type]"         +  ","
                         + "[name]"          +  ","
                         + "[badges]"        +  ","
                         + "[unlocks]"       +  ","
                         + "[currency]"      +  ","
                         + "[score]"         +  ","
                         + "[createdate]"    +  ","
                         + "[expiredate]"    +  ","
                         + "[archived]"      +  ","
                         + "[favorites]"     +  ","
                         + "\n";

            foreach (var item in studentsList)
            {
                studentsCSV += item.username         +  ","
                             + item.password         +  ","
                             + item.organization     +  ","
                             + item.type             +  ","
                             + item.name             +  ","
                             + string.Join("|", item.badges.ToArray()) +  ","
                             + string.Join("|", item.unlocks.ToArray()) +  ","
                             + item.currency         +  ","
                             + string.Join("|", item.scores.ToArray()) +  ","
                             + item.createdate       +  ","
                             + item.expiredate       +  ","
                             + item.lastlogin        +  ","
                             + item.archived         +  ","
                             + string.Join("|", item.favorites.ToArray()) +  ","
                             + "\n";
            }

            //Debug.Log("Students Exported Data: " + studentsCSV);

            DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
            dbSession.GetAllSessionsInOrganization(org, (sessionList, error2) =>
            {
                if (!string.IsNullOrEmpty(error2))
                {
                    if (callback != null)
                        callback(studentsCSV, sessionsCSV, error2);

                    return;
                }

                sessionsCSV += "[sessionname]"  +  ","
                             + "[username]"     +  ","
                             + "[organization]" +  ","
                             + "[active]"       +  ","
                             + "[teacher]"      +  ","
                             + "[events]"       +  ","
                             + "\n";

                foreach (var item in sessionList)
                {
                    sessionsCSV += item.sessionname   +  ","
                                 + item.username      +  ","
                                 + item.organization  +  ","
                                 + item.active        +  ","
                                 + item.teacher       +  ","
                                 + string.Join("|", item.events.ToArray()) +  ","
                                 + "\n";
                }

                //Debug.Log("Sessions Exported Data: " + sessionsCSV);

                if (callback != null)
                    callback(studentsCSV, sessionsCSV, error2);
            });
        });
    }
}
