using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

public class DebugControllerMenu : MonoBehaviour
{
    [NonSerialized] public bool m_showController = false;

    enum ControllerMenus             { DEBUG,        DATABASE,   DATABASE_HW,   DATABASE_SES,       DATABASE_PROF,  AWS_LAMBDA,     MENU,   ASR,   CONFIG,   LENGTH };
    string [] m_controllerMenuText = { "debug menu", "database", "database-hw", "database-session", "database-profile", "aws-lambda",  "menu", "asr", "config", };

    ControllerMenus m_controllerMenuSelected = ControllerMenus.DEBUG;

    GUIStyle m_guiLabel;

    float m_timeSlider = 1.0f;

    //MainMenu m_main;
    GameObject m_menu;
    GameObject m_panelLogin;
    GameObject m_panelDemoPracticeSession;
    GameObject m_panelDemoUnlocks;
    GameObject m_panelDemoBadges;
    GameObject m_panelDemoAbout;
    GameObject m_panelDemoConfigure;


   

    int m_interviewEnvironmentSelected = 1; 
    int m_interviewCharacterSelected = 5;
    int m_interviewMoodSelected = 1;

    string m_panelLoginUsername = "New User Name";
    string m_panelLoginPassword = "Password";
    string m_panelLoginUserOrganization = "Existing Organization";
    string m_panelLoginOrganization = "TestOrganization";
    string m_panelLoginClass = "TestClass";
    string m_panelLoginClassOrganization = "Class Organization";

    string m_panelClassHomeworkHomeworkName     = "Class Homework Name";
    string m_panelClassHomeworkClassName        = "TestClass";

    string m_panelStudentHomeworkHomeworkName     = "Homework Name";
    string m_panelStudentHomeworkCStudentName     = "dmf-student";

    string m_idenityPoolId = "";

    string m_SessionName      = "Student Session Name Test";
    string m_SessionUserName  = "dmf-studen";
    string m_SessionEvent     = "test event 1";

    string m_OrganizationName = "ict";

    string m_StudentProfileame =  "dmf-student";
    string m_StudentItemData  =  "item1";

    int m_panelLoginType = 0;
    string [] m_panelLoginTypeNames = new string [] { "root", "local", "teach", "stud" };

    bool m_resolutionFullscreen;
    Vector2 m_resolutionListScrollPosition = Vector2.zero;


    string RemoveStudentName = "dmf-studen";
    string RemoveClassName   = "dmf101"; 

    string m_VitaTable = "VitaOrganization";


    string m_SessionNameToExport = "GLOBAL";

    void Start()
    {
        //m_main = GameObject.Find("Main").GetComponent<MainMenu>();
        m_menu = GameObject.Find("Canvas");
        m_panelLogin = VHUtils.FindChild(m_menu, "PanelLoginPrefab");
        m_panelDemoPracticeSession = VHUtils.FindChild(m_menu, "PanelDemoPracticeSessionPrefab");
        m_panelDemoUnlocks = VHUtils.FindChild(m_menu, "PanelDemoUnlocksPrefab");
        m_panelDemoBadges = VHUtils.FindChild(m_menu, "PanelDemoBadgesPrefab");
        m_panelDemoAbout = VHUtils.FindChild(m_menu, "PanelDemoAboutPrefab");
        m_panelDemoConfigure = VHUtils.FindChild(m_menu, "PanelDemoConfigurePrefab");


        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        m_idenityPoolId = vitaDynamoDB.IdentityPoolId;
    }


    void Update()
    {
    }


    void OnGUI()
    {
        if (m_showController)
        {
            if (m_guiLabel == null)
            {
                m_guiLabel = new GUIStyle(GUI.skin.label);
                m_guiLabel.padding = new RectOffset(0, 0, 0, 0);
            }


            float buttonX = 0;
            float buttonY = 0;
            float buttonW = 240;

            GUILayout.BeginArea(new Rect(buttonX, buttonY, buttonW, Screen.height));

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label(VitaGlobals.m_versionText, m_guiLabel);
            GUILayout.Label(string.Format("isServer: {0}", VitaGlobals.m_isServer), m_guiLabel);
            GUILayout.Label(string.Format("isGUI: {0}", VitaGlobals.m_isGUIScreen), m_guiLabel);
            GUILayout.Label(string.Format("runMode: {0}", VitaGlobals.m_runMode), m_guiLabel);
            GUILayout.Label(string.Format("Connected: {0}", NetworkRelay.ConnectionEstablished), m_guiLabel);

#if false
            GUILayout.Label("Display functions");

            if (GUILayout.Button("Activate 2nd Monitor"))
            {
                Debug.Log(string.Format("UICamera.targetDisplay: {0}", uiCamera.GetComponent<Camera>().targetDisplay));

                if (Display.displays.Length > 1)
                {
                    Debug.Log(string.Format("Activating Display 2"));
                    Display.displays[1].Activate();
                }
                else
                {
                    Debug.Log(string.Format("System only has {0} displays", Display.displays.Length));
                }
            }

            int renderWidth = 1024;
            int renderHeight = 768;

            if (GUILayout.Button(string.Format("Activate 2nd - {0}x{1}", renderWidth, renderHeight)))
            {
                Debug.Log(string.Format("UICamera.targetDisplay: {0}", uiCamera.GetComponent<Camera>().targetDisplay));

                if (Display.displays.Length > 1)
                {
                    Debug.Log(string.Format("Activating Display 2"));
                    Display.displays[1].Activate(renderWidth, renderHeight, 0);
                }
                else
                {
                    Debug.Log(string.Format("System only has {0} displays", Display.displays.Length));
                }
            }

            if (GUILayout.Button("Activate Game Camera"))
            {
                gameCamera.SetActive(true);
                gameCamera.GetComponent<Camera>().targetDisplay = 1;
            }

            if (GUILayout.Button("Swap displays"))
            {
                if (Display.displays.Length > 1)
                {
                    if (uiCamera.GetComponent<Camera>().targetDisplay == 0)
                    {
                        uiCamera.GetComponent<Camera>().targetDisplay = 1;
                        //uiCanvas.GetComponent<Canvas>().targetDisplay = 1;
                        gameCamera.GetComponent<Camera>().targetDisplay = 0;
                    }
                    else
                    {
                        uiCamera.GetComponent<Camera>().targetDisplay = 0;
                        //uiCanvas.GetComponent<Canvas>().targetDisplay = 0;
                        gameCamera.GetComponent<Camera>().targetDisplay = 1;
                    }
                }
                else
                {
                    Debug.Log(string.Format("System only has {0} displays", Display.displays.Length));
                }
            }

            if (GUILayout.Button("SetTargetBuffers()"))
            {
                int targetDisplay = uiCamera.GetComponent<Camera>().targetDisplay;
                uiCamera.GetComponent<Camera>().SetTargetBuffers(Display.displays[targetDisplay].colorBuffer, Display.displays[targetDisplay].depthBuffer);
            }

            if (GUILayout.Button(string.Format("SetRenderingResolution({0}, {1})", renderWidth, renderHeight)))
            {
                int targetDisplay = uiCamera.GetComponent<Camera>().targetDisplay;

                Debug.LogWarning(string.Format("{0} - {1} x {2}", targetDisplay, renderWidth, renderHeight));

                Display.displays[targetDisplay].SetRenderingResolution(renderWidth, renderHeight);
            }

            if (GUILayout.Button("Report systemWidth/Height"))
            {
                int targetDisplay = uiCamera.GetComponent<Camera>().targetDisplay;
                Debug.LogWarning(string.Format("{0} x {1} - {0} x {1}", Display.displays[targetDisplay].systemWidth, Display.displays[targetDisplay].systemHeight, Display.displays[targetDisplay].renderingWidth, Display.displays[targetDisplay].renderingHeight));
            }
#endif

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(20))) { m_controllerMenuSelected = (ControllerMenus)VHMath.DecrementWithRollover((int)m_controllerMenuSelected, m_controllerMenuText.Length); }
            if (GUILayout.Button(m_controllerMenuText[(int)m_controllerMenuSelected])) { m_controllerMenuSelected = (ControllerMenus)VHMath.IncrementWithRollover((int)m_controllerMenuSelected, m_controllerMenuText.Length); }
            if (GUILayout.Button(">", GUILayout.Width(20))) { m_controllerMenuSelected = (ControllerMenus)VHMath.IncrementWithRollover((int)m_controllerMenuSelected, m_controllerMenuText.Length); }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (m_controllerMenuSelected == ControllerMenus.DEBUG)
            {
                OnGUIDebug();
            }
            else if (m_controllerMenuSelected == ControllerMenus.DATABASE)
            {
                OnGUIDatabase();
            }
            else if (m_controllerMenuSelected == ControllerMenus.DATABASE_HW)
            {
                OnGUIDatabaseHW();
            }
            else if (m_controllerMenuSelected == ControllerMenus.DATABASE_SES)
            {
                OnGUIDatabaseSES();
            }
            else if (m_controllerMenuSelected == ControllerMenus.DATABASE_PROF)
            {
                OnGUIDatabasePROF();
            }

            else if (m_controllerMenuSelected == ControllerMenus.AWS_LAMBDA)
            {
                OnGUIAWSLambda();
            }
            else if (m_controllerMenuSelected == ControllerMenus.MENU)
            {
                OnGUIMenu();
            }
            else if (m_controllerMenuSelected == ControllerMenus.ASR)
            {
                OnGUIASR();
            }
            else if (m_controllerMenuSelected == ControllerMenus.CONFIG)
            {
                OnGUIConfig();
            }

            GUILayout.EndVertical();

            GUILayout.EndArea();
        }
    }

    void OnGUIDebug()
    {
        if (GUILayout.Button("Switch screens"))
        {
            NetworkRelay.SendNetworkMessage("switchscreens");
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(20))) { m_interviewEnvironmentSelected = VHMath.DecrementWithRollover(m_interviewEnvironmentSelected, VitaGlobals.m_vitaBackgroundInfo.Count); }
        if (GUILayout.Button(VitaGlobals.m_vitaBackgroundInfo[m_interviewEnvironmentSelected].sceneName)) { m_interviewEnvironmentSelected = VHMath.IncrementWithRollover(m_interviewEnvironmentSelected, VitaGlobals.m_vitaBackgroundInfo.Count); }
        if (GUILayout.Button(">", GUILayout.Width(20))) { m_interviewEnvironmentSelected = VHMath.IncrementWithRollover(m_interviewEnvironmentSelected, VitaGlobals.m_vitaBackgroundInfo.Count); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(20))) { m_interviewCharacterSelected = VHMath.DecrementWithRollover(m_interviewCharacterSelected, VitaGlobals.m_vitaCharacterInfo.Count); }
        if (GUILayout.Button(VitaGlobals.m_vitaCharacterInfo[m_interviewCharacterSelected].prefab)) { m_interviewCharacterSelected = VHMath.IncrementWithRollover(m_interviewCharacterSelected, VitaGlobals.m_vitaCharacterInfo.Count); }
        if (GUILayout.Button(">", GUILayout.Width(20))) { m_interviewCharacterSelected = VHMath.IncrementWithRollover(m_interviewCharacterSelected, VitaGlobals.m_vitaCharacterInfo.Count); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(20))) { m_interviewMoodSelected = VHMath.DecrementWithRollover(m_interviewMoodSelected, VitaGlobals.m_vitaMoods.Length); }
        if (GUILayout.Button(VitaGlobals.m_vitaMoods[m_interviewMoodSelected])) { m_interviewMoodSelected = VHMath.IncrementWithRollover(m_interviewMoodSelected, VitaGlobals.m_vitaMoods.Length); }
        if (GUILayout.Button(">", GUILayout.Width(20))) { m_interviewMoodSelected = VHMath.IncrementWithRollover(m_interviewMoodSelected, VitaGlobals.m_vitaMoods.Length); }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Start Interview"))
        {
            string environment = VitaGlobals.m_vitaBackgroundInfo[m_interviewEnvironmentSelected].sceneName;
            string character = VitaGlobals.m_vitaCharacterInfo[m_interviewCharacterSelected].prefab;
            string disposition = VitaGlobals.m_vitaMoods[m_interviewMoodSelected];
            NetworkRelay.SendNetworkMessage(string.Format("startinterview {0} {1} {2} {3} {4}", environment, character, disposition, "name", (int)VitaGlobals.InterviewType.Interview));
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Quit"))
        {
            NetworkRelay.SendNetworkMessage("quit");
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("DMF Admin Login"))    { DebugUsernamePasswordLogin("dmfadmin", "password"); }
        if (GUILayout.Button("Local Admin Login"))  { DebugUsernamePasswordLogin("dmf-localadmin", "password"); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Teacher Login"))  { DebugUsernamePasswordLogin("dmf-teacher", "password"); }
        if (GUILayout.Button("Student Login"))  { DebugUsernamePasswordLogin("dmf-student", "password"); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("ICT Admin"))  { DebugUsernamePasswordLogin("ict-localadmin", "password"); }
        if (GUILayout.Button("ICT Grace"))  { DebugUsernamePasswordLogin("ict-grace", "password"); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("ICT Arno"))  { DebugUsernamePasswordLogin("ict-arno", "password"); }
        if (GUILayout.Button("ICT Ed"))    { DebugUsernamePasswordLogin("ict-ed", "password"); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fake Admin"))  { DebugUsernamePasswordLogin("fake-localadmin", "password"); }
        if (GUILayout.Button("Fake Teach1")) { DebugUsernamePasswordLogin("fake-teacher-1", "password"); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fake Teach2")) { DebugUsernamePasswordLogin("fake-teacher-2", "password"); }
        if (GUILayout.Button("Fake Teach3")) { DebugUsernamePasswordLogin("fake-teacher-3", "password"); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fake New")) { DebugUsernamePasswordLogin("fake-student-new-1", "password"); }
        if (GUILayout.Button("Fake Beg")) { DebugUsernamePasswordLogin("fake-student-beginner-1", "password"); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fake Int")) { DebugUsernamePasswordLogin("fake-student-intermediate-1", "password"); }
        if (GUILayout.Button("Fake Exp")) { DebugUsernamePasswordLogin("fake-student-expert-1", "password"); }
        GUILayout.EndHorizontal();
    }

    void OnGUIDatabase()
    {
        if (m_panelLogin.activeSelf)
        {
            GUILayout.Space(10);

            GUILayout.Label(m_idenityPoolId);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("ICT"))
            {
                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                vitaDynamoDB.IdentityPoolId = VitaDynamoDB.IdentityPool_ICT.IdentityPoolId;
                vitaDynamoDB.CognitoPoolRegion = VitaDynamoDB.IdentityPool_ICT.CognitoPoolRegion;
                vitaDynamoDB.DynamoRegion = VitaDynamoDB.IdentityPool_ICT.DynamoRegion;

                m_idenityPoolId = vitaDynamoDB.IdentityPoolId;
                vitaDynamoDB.ClearIdentityData();
            }

            if (GUILayout.Button("ED"))
            {
                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                vitaDynamoDB.IdentityPoolId = VitaDynamoDB.IdentityPool_ED.IdentityPoolId;
                vitaDynamoDB.CognitoPoolRegion = VitaDynamoDB.IdentityPool_ED.CognitoPoolRegion;
                vitaDynamoDB.DynamoRegion = VitaDynamoDB.IdentityPool_ED.DynamoRegion;

                m_idenityPoolId = vitaDynamoDB.IdentityPoolId;
                vitaDynamoDB.ClearIdentityData();
            }

            if (GUILayout.Button("DMF"))
            {
                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                vitaDynamoDB.IdentityPoolId = VitaDynamoDB.IdentityPool_DMF.IdentityPoolId;
                vitaDynamoDB.CognitoPoolRegion = VitaDynamoDB.IdentityPool_DMF.CognitoPoolRegion;
                vitaDynamoDB.DynamoRegion = VitaDynamoDB.IdentityPool_DMF.DynamoRegion;

                m_idenityPoolId = vitaDynamoDB.IdentityPoolId;
                vitaDynamoDB.ClearIdentityData();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("Delete Database"))
            {
                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                StartCoroutine(vitaDynamoDB.DeleteDatabase());
            }

            if (GUILayout.Button("Create Database"))
            {
                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
                StartCoroutine(vitaDynamoDB.CreateDatabase());
            }

            if (GUILayout.Button("Create Test Data"))
            {
                CreateTestData();
            }

            if (GUILayout.Button("Upgrade DB 1.1 to 1.2"))
            {
                MigrateData migrate = this.gameObject.AddComponent<MigrateData>();
                StartCoroutine(migrate.MigrateData_1_1_to_1_2());
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Running"))
            {
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.UpdateUser(VitaGlobals.m_statusUsername, "root", VitaGlobals.m_statusRunning, DBUser.AccountType.STUDENT, VitaGlobals.CurrentTimeToString(), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("UpdateUser() - error: {0}", error); });
            }

            if (GUILayout.Button("Maintenance"))
            {
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.UpdateUser(VitaGlobals.m_statusUsername, "root", VitaGlobals.m_statusMaintenance, DBUser.AccountType.STUDENT, VitaGlobals.CurrentTimeToString(), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("UpdateUser() - error: {0}", error); });
            }

            GUILayout.EndHorizontal();


            GUILayout.Space(10);

            m_panelLoginOrganization = GUILayout.TextField(m_panelLoginOrganization);

            if (GUILayout.Button("Create Organization"))
            {
                DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
                dbOrg.CreateOrganization(m_panelLoginOrganization, "First", "Last", "example@example.com", "800-123-4567", m_panelLoginOrganization + "-admin", "password", error =>
                {
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogErrorFormat("CreateOrganization() - error: {0}", error);
                });
            }

            if (GUILayout.Button("Delete Organization"))
            {
                DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
                dbOrg.DeleteOrganizationAndData(m_panelLoginOrganization, error =>
                {
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogErrorFormat("DeleteOrganizationAndData() - error: {0}", error);
                });
            }

            GUILayout.Space(10);

            m_panelLoginUsername = GUILayout.TextField(m_panelLoginUsername);
            m_panelLoginPassword = GUILayout.TextField(m_panelLoginPassword);
            m_panelLoginUserOrganization = GUILayout.TextField(m_panelLoginUserOrganization);
            m_panelLoginType = GUILayout.Toolbar(m_panelLoginType, m_panelLoginTypeNames);

            if (GUILayout.Button("Create User"))
            {
                DBUser.AccountType type = DBUser.AccountType.ROOT;
                type = (DBUser.AccountType)m_panelLoginType;
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.CreateUser(m_panelLoginUsername, m_panelLoginPassword, m_panelLoginUserOrganization, "", type, error =>
                {
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogErrorFormat("CreateUser() - error: {0}", error);
                });
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Delete User"))
            {
                //DBUser.AccountType type = DBUser.AccountType.ROOT;
                //type = (DBUser.AccountType)m_panelLoginType;
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.DeleteUser(m_panelLoginUsername,  error =>
                {
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogErrorFormat("DeleteUser() - error: {0}", error);
                });
            }

            GUILayout.Space(10);


            if (GUILayout.Button("List Organizations"))
            {
                DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
                dbOrg.GetAllOrganizations((orgs, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("GetAllOrganizations() error: {0}", error);
                        return;
                    }

                    foreach (var org in orgs)
                    {
                        Debug.LogFormat("{0} - {1} - {2} - {3} - {4} - {5} - {6}", org.name, org.firstname, org.lastname, org.email, org.phone, org.acccreated, org.archived);
                    }
                });
            }



            if (GUILayout.Button("Create 10 Bulck Organizations"))
            {
                //DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();

                List<EntityDBVitaOrganization> TestOrg = new List<EntityDBVitaOrganization>();

                for(int i =0; i<10; i++)
                {
                    TestOrg.Add( new EntityDBVitaOrganization(System.Guid.NewGuid().ToString(),System.Guid.NewGuid().ToString(),System.Guid.NewGuid().ToString(),System.Guid.NewGuid().ToString(),System.Guid.NewGuid().ToString(),System.Guid.NewGuid().ToString(),System.Guid.NewGuid().ToString()));
                }

                VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();

                         vitaDynamoDB.BatchAdd<EntityDBVitaOrganization>(TestOrg,(res, err)=>
                        {
                            if(!string.IsNullOrEmpty(err))
                            {
                                Debug.LogError("Error Adding Bulk Organization");
                            }
                            else
                            {
                               Debug.Log("Organizations Added ");
                            }
                        });
            }




            if (GUILayout.Button("Export dmf101 class"))
            {

                //List<string> ProfileFiledsList = new List<string>();
                //List<string> ProfileList = new List<string>();
/*
                ExportClassData("dmf101", ProfileFiledsList, (user, result) =>
                {
                   // We have an error
                   if( !string.IsNullOrEmpty(result))
                   {
                        Debug.Log("result");
                   }
                   else
                   {
                        Debug.LogFormat("{0} - {1} - {2} ", user.username, user.favorites.Aggregate((x,y) =>  x + " , " + y));
                   }

                });+
                */
            }

            if (GUILayout.Button("Get All Teachers in ICT organization"))
            {
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.GetAllTeachersInOrganization("ict", (users, result) =>
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        Debug.LogError(result);
                        return;
                    }

                    foreach (var user in users)
                    {
                        Debug.LogFormat("{0} - {1} - {2} - {3} - {4} - {5}", user.username, user.password, user.organization, user.type, user.name, user.archived);
                    }
                });
            }

            if (GUILayout.Button("List Users"))
            {
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.GetAllUsers((users, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("GetAllUsers() error: {0}", error);
                        return;
                    }

                    foreach (var user in users)
                    {
                        Debug.LogFormat("{0} - {1} - {2} - {3} - {4} - {5}", user.username, user.password, user.organization, user.type, user.name, user.archived);
                    }
                });
            }

            if (GUILayout.Button("List Classes"))
            {
                DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
                dbClass.GetAllClasses((classes, error) =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("GetAllClasses() error: {0}", error);
                        return;
                    }

                    foreach (var c in classes)
                    {
                        Debug.LogFormat("{0} - {1} - studentCount: {2}", c.classname, c.organization, c.students.Count);
                    }
                });
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("DMF Admin Login"))
            {
                UnityEngine.UI.InputField username = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_UserName").GetComponent<UnityEngine.UI.InputField>();
                UnityEngine.UI.InputField password = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_Password").GetComponent<UnityEngine.UI.InputField>();
                username.text = "dmfadmin";
                password.text = "password";
            }

            if (GUILayout.Button("Local Admin Login"))
            {
                UnityEngine.UI.InputField username = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_UserName").GetComponent<UnityEngine.UI.InputField>();
                UnityEngine.UI.InputField password = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_Password").GetComponent<UnityEngine.UI.InputField>();
                username.text = "dmf-localadmin";
                password.text = "password";
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Teacher Login"))
            {
                UnityEngine.UI.InputField username = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_UserName").GetComponent<UnityEngine.UI.InputField>();
                UnityEngine.UI.InputField password = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_Password").GetComponent<UnityEngine.UI.InputField>();
                username.text = "dmf-teacher";
                password.text = "password";
            }

            if (GUILayout.Button("Student Login"))
            {
                UnityEngine.UI.InputField username = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_UserName").GetComponent<UnityEngine.UI.InputField>();
                UnityEngine.UI.InputField password = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_Password").GetComponent<UnityEngine.UI.InputField>();
                username.text = "dmf-student";
                password.text = "password";
            }

            GUILayout.EndHorizontal();
        }
    }



    void OnGUIDatabaseSES()
    {
        if (m_panelLogin.activeSelf)
        {
            GUILayout.Space(10);

            m_SessionName        = GUILayout.TextField(m_SessionName);
            m_SessionUserName    = GUILayout.TextField(m_SessionUserName);

            if (GUILayout.Button("Create Session"))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.CreateSession(m_SessionName, m_SessionUserName, "Teacher", "Organization", error =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("SessionCreate() error: {0}", error);
                    }
                });
            }


            m_OrganizationName = GUILayout.TextField(m_OrganizationName);
            if (GUILayout.Button("Get Organization Session "))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.GetAllSessionsInOrganization(m_OrganizationName, (org,error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogErrorFormat("GetSessionsForOrganization() - error: {0}", error);
                    else
                    {
                        Debug.Log("Total Session for Organization : " + m_OrganizationName + " " + org.Count);
                        if(org.Count > 0)
                        {
                            foreach(var item in org)
                            {
                                Debug.Log("Session Name : "  + item.sessionname);
                            }
                        }
                     }
                });
            }

            GUILayout.Space(10);

            m_SessionEvent = GUILayout.TextField(m_SessionEvent);

            if (GUILayout.Button("Add Event Play Uttarance To Session"))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.AddEvent(m_SessionName, m_SessionUserName,DBSession.EventData.NewPlayUtterance(m_SessionEvent), error =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddEventToSession() error: {0}", error);
                    }
                });
            }

            if (GUILayout.Button("Add Global Event Play Uttarance "))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.AddEvent(m_SessionName, m_SessionUserName, DBSession.EventData.NewPlayUtterance(m_SessionEvent), error =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("GlobalEventAdd() error: {0}", error);
                    }
                });
            }

            if (GUILayout.Button("Print All Session"))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.GetAllSessions((sessions, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("SessionGetAll() error: {0}", error);
                    }

                    foreach (var s in sessions)
                    {
                        Debug.LogFormat("{0} - {1} ", s.username, s.sessionname);
                    }
                });
            }

            if (GUILayout.Button("Print Global Events"))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.GetGlobalSession(m_SessionUserName, (session, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("GlobalEventsGet() error: {0}", error);
                    }

                    foreach(var item in session.events)
                    {
                        Debug.Log("Event : " + item);
                    }
                });
            }

            if (GUILayout.Button("Delete All Sessions"))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.DeleteAllSessions((error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("GlobalEventsGet() error: {0}", error);
                    }
                    else
                    {
                       Debug.Log("Session Deleted");
                    }
                });
            }

            if (GUILayout.Button("List ict-ed Sessions"))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.GetAllSessions((sessions, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("SessionGetAll() error: {0}", error);
                    }

                    foreach (var s in sessions)
                    {
                        if (s.username == "ict-ed")
                        {
                            Debug.LogFormat("{0} - {1}", s.username, s.sessionname);
                            foreach (var e in s.events)
                            {
                                Debug.LogFormat("   {0}", e);
                            }
                        }
                    }
                });
            }

            m_SessionNameToExport = GUILayout.TextField(m_SessionNameToExport);

            if (GUILayout.Button("Export Session Name  Data"))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.ExportSessionToText("ict-ed", m_SessionNameToExport, null, "Ed Fast", "Grace Benn", (sessionsCSV, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("Exporting Data() error: {0}", error);
                    }

                    string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                   
                    string filename = folder + "\\" + "ict-ed" + "_session_" + m_SessionNameToExport + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + ".csv";
                    System.IO.File.WriteAllText(filename, sessionsCSV);
                });
            }
            if (GUILayout.Button("Export Session Name  Data HTML"))
            {
                DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
                dbSession.ExportSessionToHtml("ict-ed", m_SessionNameToExport, null, "Ed Fast", "Grace Benn", (sessionsCSV, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("Exporting Data() error: {0}", error);
                    }

                    string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                   
                    string filename = folder + "\\" + "ict-ed" + "_session_" + m_SessionNameToExport + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + ".html";
                    System.IO.File.WriteAllText(filename, sessionsCSV);
                });
            }
        }
    }


    void OnGUIAWSLambda()
    {
        if (m_panelLogin.activeSelf)
        {
            GUILayout.Space(10);

            m_VitaTable  = GUILayout.TextField(m_VitaTable);

            if (GUILayout.Button("Get All Data"))
            {
                StartCoroutine(ExecuteAwsLambad("https://vgr03yzbyl.execute-api.us-east-1.amazonaws.com/prod/GetAllUsers?TableName=" + m_VitaTable));
            }                     

            GUILayout.Space(10);
        }

    }

    IEnumerator ExecuteAwsLambad(string cmd) 
    {
        WWW www = new WWW(cmd);
        yield return www;
        if(www.text.Contains("ERROR"))
        {
            Debug.LogError(www.text);
        }
        else
        {           
            Debug.Log(www.text);
        }
    }

    void OnGUIDatabasePROF()
    {
        if (m_panelLogin.activeSelf)
        {
            GUILayout.Space(10);

            m_StudentProfileame  = GUILayout.TextField(m_StudentProfileame);
            m_StudentItemData    = GUILayout.TextField(m_StudentItemData);


            if (GUILayout.Button("Get Profile"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.GetUser(m_StudentProfileame, (user, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                    else
                    {

                        Debug.Log("User name :" +  user.username);
                        Debug.Log("Favorites :" +  string.Join("," , user.favorites.ToArray()));
                    }
                });
            }



            if (GUILayout.Button("Add Badge"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.AddBadge(m_StudentProfileame, DBUser.BadgeData.NewBadge(m_StudentItemData), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                });
            }

            if (GUILayout.Button("Remove Badge"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.RemoveBadge(m_StudentProfileame,  DBUser.BadgeData.NewBadge(m_StudentItemData), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                });
            }
           


            if (GUILayout.Button("Add Favorite"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.AddFavorite(m_StudentProfileame,  DBUser.FavoriteData.NewFavorite(m_StudentItemData), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                });
            }

            if (GUILayout.Button("Remove Favorite"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.RemoveFavorite(m_StudentProfileame,  DBUser.FavoriteData.NewFavorite(m_StudentItemData), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                });
            }


            if (GUILayout.Button("Remove All Favorite"))
            {
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.RemoveAllFavorites(m_StudentProfileame, (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogErrorFormat("RemoveFavorite() - error: {0}", error);
                });
            }

            if (GUILayout.Button("Add Unlock"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.AddUnlock(m_StudentProfileame,  DBUser.UnlockData.NewUnlock(m_StudentItemData), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                });
            }

            if (GUILayout.Button("Remove Unlock"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.RemoveUnlock(m_StudentProfileame,  DBUser.UnlockData.NewUnlock(m_StudentItemData), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                });
            }


            if (GUILayout.Button("Add Score"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.AddScore(m_StudentProfileame,  DBUser.ScoreData.NewScore(m_StudentItemData), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                });
            }

            if (GUILayout.Button("Remove Score"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.RemoveScore(m_StudentProfileame,  DBUser.ScoreData.NewScore(m_StudentItemData), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("AddBadge() error: {0}", error);
                    }
                });
            }

            if (GUILayout.Button("Export Student Data"))
            {
                DBUser dbProfile = GameObject.FindObjectOfType<DBUser>();
                dbProfile.ExportStudentsDataByOrganization(m_StudentProfileame, (studentsCSV, sessionsCSV, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                       Debug.LogErrorFormat("Exporting Data() error: {0}", error);
                    }

                    string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string filename = folder + "\\" + m_StudentProfileame + "_students_" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + ".csv";
                    System.IO.File.WriteAllText(filename, studentsCSV);

                    filename = folder + "\\" + m_StudentProfileame + "_sessions_" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + ".csv";
                    System.IO.File.WriteAllText(filename, sessionsCSV);
                });
            }

            GUILayout.Space(10);
        }
    }




    void OnGUIDatabaseHW()
    {
        if (m_panelLogin.activeSelf)
        {
            GUILayout.Space(10);

            m_panelLoginClass = GUILayout.TextField(m_panelLoginClass);
            m_panelLoginClassOrganization = GUILayout.TextField(m_panelLoginClassOrganization);

            if (GUILayout.Button("Create Class"))
            {
                DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
                dbClass.CreateClass(m_panelLoginClass, m_panelLoginClassOrganization, error =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("CreateClass() error: {0}", error);
                    }
                });
            }

            GUILayout.Space(10);

            m_panelClassHomeworkHomeworkName = GUILayout.TextField(m_panelClassHomeworkHomeworkName);
            m_panelClassHomeworkClassName    =  GUILayout.TextField(m_panelClassHomeworkClassName);

            if (GUILayout.Button("Create Class Homework"))
            {
                DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();
                dbAssignment.CreateAssignment(m_panelClassHomeworkHomeworkName, "Assignment Description goes here", "Assignment instructions...", m_panelLoginClassOrganization, "TeacherName", "Student", m_panelClassHomeworkClassName, VitaGlobals.MaxTimeToString(), (assignment, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("CreateAssignment() error: {0}", error);
                    }
                });
            }

            GUILayout.Space(10);


            m_panelStudentHomeworkHomeworkName = GUILayout.TextField(m_panelStudentHomeworkHomeworkName);
            m_panelStudentHomeworkCStudentName    =  GUILayout.TextField(m_panelStudentHomeworkCStudentName);

            if (GUILayout.Button("Create Student Homework"))
            {
                //CreateClassHomework(m_panelStudentHomeworkHomeworkName, m_panelStudentHomeworkCStudentName);
            }

            GUILayout.Space(10);


            RemoveClassName   =  GUILayout.TextField(RemoveClassName);
            RemoveStudentName = GUILayout.TextField(RemoveStudentName);

            if (GUILayout.Button("Remove Student From Class"))
            {
                DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
                dbClass.RemoveStudentFromClass(RemoveClassName, RemoveStudentName, (error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("RemoveStudentFromClass() error: {0}", error);
                    }
                    else
                    {
                         Debug.Log("RemoveStudentFromClass " +  RemoveStudentName);
                    }
                    
                });
            }

        }
    }

    void OnGUIMenu()
    {
        if (m_panelDemoPracticeSession.activeSelf ||
            m_panelDemoUnlocks.activeSelf ||
            m_panelDemoBadges.activeSelf ||
            m_panelDemoAbout.activeSelf ||
            m_panelDemoConfigure.activeSelf)
        {
            if (GUILayout.Button("Home"))
            {
                if (m_panelDemoPracticeSession.activeSelf) GameObject.FindObjectOfType<MenuManagerPanelDemoPracticeSession>().BtnHome();
                else if (m_panelDemoUnlocks.activeSelf)    GameObject.FindObjectOfType<MenuManagerPanelDemoUnlocks>().BtnHome();
                else if (m_panelDemoBadges.activeSelf)     GameObject.FindObjectOfType<MenuManagerPanelDemoBadges>().BtnHome();
                else if (m_panelDemoAbout.activeSelf)      GameObject.FindObjectOfType<MenuManagerPanelDemoAbout>().BtnHome();
                else if (m_panelDemoConfigure.activeSelf)  GameObject.FindObjectOfType<MenuManagerPanelDemoConfigure>().BtnHome();
            }

            if (GUILayout.Button("Back"))
            {
                if (m_panelDemoPracticeSession.activeSelf) GameObject.FindObjectOfType<MenuManagerPanelDemoPracticeSession>().BtnBack();
                else if (m_panelDemoUnlocks.activeSelf)    GameObject.FindObjectOfType<MenuManagerPanelDemoUnlocks>().BtnBack();
                else if (m_panelDemoBadges.activeSelf)     GameObject.FindObjectOfType<MenuManagerPanelDemoBadges>().BtnBack();
                else if (m_panelDemoAbout.activeSelf)      GameObject.FindObjectOfType<MenuManagerPanelDemoAbout>().BtnBack();
                else if (m_panelDemoConfigure.activeSelf)  GameObject.FindObjectOfType<MenuManagerPanelDemoConfigure>().BtnBack();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("PracticeSession"))
            {
                MenuManagerPanelDemoSideBar menuManager = GameObject.FindObjectOfType<MenuManagerPanelDemoSideBar>();
                menuManager.BtnPracticeSession();
            }

            if (GUILayout.Button("Unlocks"))
            {
                MenuManagerPanelDemoSideBar menuManager = GameObject.FindObjectOfType<MenuManagerPanelDemoSideBar>();
                menuManager.BtnUnlocks();
            }

            if (GUILayout.Button("Badges"))
            {
                MenuManagerPanelDemoSideBar menuManager = GameObject.FindObjectOfType<MenuManagerPanelDemoSideBar>();
                menuManager.BtnBadges();
            }

            if (GUILayout.Button("About"))
            {
                MenuManagerPanelDemoSideBar menuManager = GameObject.FindObjectOfType<MenuManagerPanelDemoSideBar>();
                menuManager.BtnAbout();
            }

            if (GUILayout.Button("Configure (?)"))
            {
                MenuManagerPanelDemoSideBar menuManager = GameObject.FindObjectOfType<MenuManagerPanelDemoSideBar>();
                menuManager.BtnConfigure();
            }
        }
        else if (GameObject.FindObjectOfType<MenuManager>().CurrentMenu == MenuManager.Menu.StudentBadges)
        {
            GUILayout.Label(string.Format("Badge: {0}", VitaGlobals.m_loggedInProfile.badges.Count));

            if (GUILayout.Button("Add Badge"))
            {
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.AddBadge(VitaGlobals.m_loggedInProfile.username, DBUser.BadgeData.NewBadge("Consistency"), (profile, error) => 
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("AddBadge() - error: {0}", error);
                        return;
                    }

                    VitaGlobals.m_loggedInProfile = profile;
                });
            }

            if (GUILayout.Button("Remove Badge"))
            {
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.RemoveBadge(VitaGlobals.m_loggedInProfile.username, DBUser.BadgeData.NewBadge("Consistency"), (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("RemoveBadge() - error: {0}", error);
                        return;
                    }

                    VitaGlobals.m_loggedInProfile = profile;
                });
            }

            if (GUILayout.Button("Remove All Badges"))
            {
                DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
                dbUser.RemoveAllBadges(VitaGlobals.m_loggedInProfile.username, (profile, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogErrorFormat("RemoveAllBadges() - error: {0}", error);

                    VitaGlobals.m_loggedInProfile = profile;
                });
            }
        }
    }

    void OnGUIASR()
    {
        if (VHUtils.IsWindows10OrGreater())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            GUILayout.Label(string.Format("PhraseRecogntionSystem.isSupported: {0}", UnityEngine.Windows.Speech.PhraseRecognitionSystem.isSupported), m_guiLabel);
            GUILayout.Label(string.Format("PhraseRecogntionSystem.status: {0}", UnityEngine.Windows.Speech.PhraseRecognitionSystem.Status), m_guiLabel);

            DictationRecognizer dictationRecognizer = GameObject.FindObjectOfType<DictationRecognizer>();
            GUILayout.Label(string.Format("Dictation.status: {0}", dictationRecognizer.m_dictationRecognizer.Status), m_guiLabel);
            GUILayout.Label(string.Format("Dictation.AutoSilenceTimeoutSeconds: {0}", dictationRecognizer.m_dictationRecognizer.AutoSilenceTimeoutSeconds), m_guiLabel);
            GUILayout.Label(string.Format("Dictation.InitialSilenceTimeoutSeconds: {0}", dictationRecognizer.m_dictationRecognizer.InitialSilenceTimeoutSeconds), m_guiLabel);

            if (GUILayout.Button("Dictation Recognizer Start"))
            {
                dictationRecognizer.StartRecording();
            }

            if (GUILayout.Button("Dictation Recognizer Stop"))
            {
                dictationRecognizer.StopRecording();
            }

            GUILayout.Space(10);
#endif
        }
    }

    void OnGUIConfig()
    {
        GUILayout.Label(string.Format("Screen.curRes: {0}", Screen.currentResolution), m_guiLabel);
        GUILayout.Label(string.Format("Screen.res[high]: {0}", Screen.resolutions.Length > 0 ? Screen.resolutions[Screen.resolutions.Length - 1].ToString() : ""), m_guiLabel);
        GUILayout.Label(string.Format("{0}x{1}x{2} ({3}) {4:f0}dpi", Screen.width, Screen.height, Screen.currentResolution.refreshRate, VHUtils.GetCommonAspectText((float)Screen.width / Screen.height), Screen.dpi), m_guiLabel);
        GUILayout.Label(string.Format("sm_xy: {0}x{1}", VitaGlobals.SM_XVIRTUALSCREEN, VitaGlobals.SM_YVIRTUALSCREEN), m_guiLabel);
        GUILayout.Label(string.Format("sm_cxy: {0}x{1}", VitaGlobals.SM_CXVIRTUALSCREEN, VitaGlobals.SM_CYVIRTUALSCREEN), m_guiLabel);
        GUILayout.Label(string.Format("sm_cmonitors: {0}", VitaGlobals.SM_CMONITORS), m_guiLabel);
        GUILayout.Label(string.Format("devmodes: {0}, current: {1}", VitaGlobals.DEVMODES.Count, VitaGlobals.m_currentDEVMODE), m_guiLabel);
        for (int i = 0; i < VitaGlobals.DEVMODES.Count; i++)
        {
            var devmode = VitaGlobals.DEVMODES[i];
            GUILayout.Label(string.Format("  {0}: {1}x{2} : {3}x{4}", i, devmode.dmPosition.x, devmode.dmPosition.y, devmode.dmPelsWidth, devmode.dmPelsHeight), m_guiLabel);
        }

        VitaGlobals.m_runMode = (VitaGlobals.RunMode)GUILayout.SelectionGrid((int)VitaGlobals.m_runMode, new string[] { VitaGlobals.RunMode.TwoMonitors.ToString(), VitaGlobals.RunMode.SplitScreen.ToString(), VitaGlobals.RunMode.SingleScreen.ToString() }, 3);

        if (GUILayout.Button("Toggle Stats"))
        {
            GameObject.FindObjectOfType<DebugInfo>().NextMode();
        }

        if (GUILayout.Button("Toggle Console"))
        {
            GameObject.FindObjectOfType<DebugConsole>().ToggleConsole();
        }

        if (GUILayout.Button("Toggle OnScreenLog"))
        {
            DebugOnScreenLog debugOnScreenLog = VHUtils.FindChildOfType<DebugOnScreenLog>(GameObject.Find("vhAssets"));
            debugOnScreenLog.gameObject.SetActive(!debugOnScreenLog.gameObject.activeSelf);
            Debug.LogWarning(string.Format(@"DebugOnScreenLog turned {0}", debugOnScreenLog.gameObject.activeSelf ? "On" : "Off"));
        }

        GUILayout.Space(5);

        bool resolutionFullscreen = GUILayout.Toggle(m_resolutionFullscreen, "Fullscreen");
        if (m_resolutionFullscreen != resolutionFullscreen)
        {
            m_resolutionFullscreen = resolutionFullscreen;
            Screen.SetResolution(Screen.width, Screen.height, m_resolutionFullscreen);
        }

        m_resolutionListScrollPosition = GUILayout.BeginScrollView(m_resolutionListScrollPosition, GUILayout.Height(100));

        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (GUILayout.Button(string.Format(@"{0}x{1}", Screen.resolutions[i].width, Screen.resolutions[i].height)))
            {
                Screen.SetResolution(Screen.resolutions[i].width, Screen.resolutions[i].height, m_resolutionFullscreen);
            }
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Move Window Upper Left"))
        {
            if (!VHUtils.IsEditor())
            {
                var hwnd = VitaGlobals.m_windowHwnd;
                //ShowWindow(hwnd, SW_MAXIMIZE);
                //if (Screen.currentResolution.width == Screen.width)  // compare desktop width (current monitor) to current window width.

                int x = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_XVIRTUALSCREEN);
                int y = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_YVIRTUALSCREEN);

                if (!VHUtils.IsEditor())
                    WindowsAPI.WinAPI_SetWindowPos(hwnd, (IntPtr)WindowsAPI.SpecialWindowHandles.HWND_TOP, x, y, 0, 0, WindowsAPI.SetWindowPosFlags.SWP_SHOWWINDOW | WindowsAPI.SetWindowPosFlags.SWP_NOSIZE);
            }
        }

        if (GUILayout.Button("Move Window Lower Right"))
        {
            if (!VHUtils.IsEditor())
            {
                var hwnd = VitaGlobals.m_windowHwnd;

                int x = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_XVIRTUALSCREEN);
                int y = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_YVIRTUALSCREEN);
                int width = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CXVIRTUALSCREEN);
                int height = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CYVIRTUALSCREEN);

                int posX = (x + width) - Screen.width;
                int posY = (y + height) - Screen.height;

                Debug.LogFormat("Lower Right: {0}x{1} - {2}x{3} - {4}x{5} - {6}x{7}",
                    x, y,
                    width, height,
                    posX, posY,
                    Screen.width, Screen.height);

                if (!VHUtils.IsEditor())
                    WindowsAPI.WinAPI_SetWindowPos(hwnd, (IntPtr)WindowsAPI.SpecialWindowHandles.HWND_TOP, posX, posY, 0, 0, WindowsAPI.SetWindowPosFlags.SWP_SHOWWINDOW | WindowsAPI.SetWindowPosFlags.SWP_NOSIZE);
            }

            // SM_CX/YVIRTUALSCREEN
            //Display.
            Debug.LogFormat("cx: {0}", WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CXVIRTUALSCREEN));
            Debug.LogFormat("cy: {0}", WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CYVIRTUALSCREEN));
            Debug.LogFormat("x: {0}", WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_XVIRTUALSCREEN));
            Debug.LogFormat("y: {0}", WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_YVIRTUALSCREEN));
        }

        float newTimeSlider = GUILayout.HorizontalSlider(m_timeSlider, 0.01f, 3);
        if (m_timeSlider != newTimeSlider)
        {
            m_timeSlider = newTimeSlider;
            Time.timeScale = m_timeSlider;
        }
        GUILayout.Label(string.Format("Time: {0}", m_timeSlider), m_guiLabel);
    }

    void DebugUsernamePasswordLogin(string username, string password)
    {
        UnityEngine.UI.InputField usernameInput = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_UserName").GetComponent<UnityEngine.UI.InputField>();
        UnityEngine.UI.InputField passwordInput = VHUtils.FindChildRecursive(m_panelLogin, "GiuInputPrefab_Password").GetComponent<UnityEngine.UI.InputField>();
        usernameInput.text = username;
        passwordInput.text = password;
    }

    public void CreateTestData()
    {
        DBOrganization dbOrg = GameObject.FindObjectOfType<DBOrganization>();
        DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
        DBClass dbClass = GameObject.FindObjectOfType<DBClass>();
        DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
        DBAssignment dbAssignment = GameObject.FindObjectOfType<DBAssignment>();

        dbOrg.CreateOrganization("root", " ", " ", " ", " ", "root-admin", "password", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateOrganization() - error: {0}", error); });
        dbUser.CreateUser("dmfadmin", "password", "root", "DMF Admin", DBUser.AccountType.ROOT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });

        dbUser.CreateUser(VitaGlobals.m_versionUsername, VitaGlobals.m_versionPassword, "root", VitaGlobals.m_versionDB, DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
        dbUser.CreateUser(VitaGlobals.m_statusUsername, VitaGlobals.m_statusPassword, "root", VitaGlobals.m_statusRunning, DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });

#if true
        dbOrg.CreateOrganization("dmf", "Dan", "Marino", "noreply@dmf.org", "800-DOLPHIN", "dmf-localadmin", "password", orgError =>
        {
            if (!string.IsNullOrEmpty(orgError)) { Debug.LogErrorFormat("CreateOrganization() - error: {0}", orgError); return; }

            dbUser.CreateUser("dmf-teacher", "password", "dmf", "DMF Teacher", DBUser.AccountType.TEACHER, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("dmf-teacher1", "password", "dmf", "DMF Teacher #10", DBUser.AccountType.TEACHER, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("dmf-teacher2", "password", "dmf", "DMF Teacher #11", DBUser.AccountType.TEACHER, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("dmf-teacher3", "password", "dmf", "DMF Teacher #12", DBUser.AccountType.TEACHER, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("dmf-student", "password", "dmf", "DMF Student", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("dmf-student21", "password", "dmf", "DMF Student #21", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("dmf-student22", "password", "dmf", "DMF Student #22", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("dmf-student23", "password", "dmf", "DMF Student #23", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });

            dbClass.CreateClass("dmf101", "dmf", error =>
            {
                if (!string.IsNullOrEmpty(error)) { Debug.LogErrorFormat("CreateClass() - error: {0}", error); return; }

                dbClass.AddStudentToClass("dmf101", "dmf-student", error2 => { if (!string.IsNullOrEmpty(error2)) Debug.LogErrorFormat("AddStudentToClass() - error: {0}", error2); });
            });

            dbClass.CreateClass("dmf102", "dmf", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateClass() - error: {0}", error); });
            dbClass.CreateClass("dmf103", "dmf", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateClass() - error: {0}", error); });
        });
#endif

        dbOrg.CreateOrganization("ict", "Randy", "Hill", "noreply@ict.usc.edu", "800-ICT-ROCKS", "ict-localadmin", "password", orgError =>
        {
            if (!string.IsNullOrEmpty(orgError)) { Debug.LogErrorFormat("CreateOrganization() - error: {0}", orgError); return; }

            dbUser.CreateUser("ict-grace", "password", "ict", "ICT Grace (Teacher)", DBUser.AccountType.TEACHER, userError =>
            {
                if (!string.IsNullOrEmpty(userError)) { Debug.LogErrorFormat("CreateUser() - error: {0}", userError); return; }

                dbUser.AddFavorite("ict-grace", DBUser.FavoriteData.NewFavorite("Acknowledgement_2002"), (profile, error) => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddFavorite() - error: {0}", error); });
                dbUser.AddFavorite("ict-grace", DBUser.FavoriteData.NewFavorite("Acknowledgement_2002"), (profile, error) => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddFavorite() - error: {0}", error); });
            });

            dbUser.CreateUser("ict-ed", "password", "ict", "ICT Ed (Student)", DBUser.AccountType.STUDENT, userError =>
            {
                if (!string.IsNullOrEmpty(userError)) { Debug.LogErrorFormat("CreateUser() - error: {0}", userError); return; }

                string sessionName = dbSession.NewSessionName();
                Debug.LogFormat(sessionName);
                dbSession.CreateSession(sessionName, "ict-ed", "ict-grace", "ict", sessionError =>
                {
                    if (!string.IsNullOrEmpty(sessionError)) { Debug.LogErrorFormat("CreateSession() - error: {0}", sessionError); return; }

                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewStartSession(DBSession.SessionType.Interview, "ict-grace", "ChrAlexPrefab", "Restaurant", "Neutral"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
#if false
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewStartSession(DBSession.SessionType.Interview), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewStartTeacher("ict-grace"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewStartCharacter("ChrAlexPrefab"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewStartEnvironment("Restaurant"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewStartDisposition("Neutral"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
#endif
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewPlayUtterance("Alex_Neutral_01E"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewStudentResponse("Hi, it's going pretty good"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewPlayUtterance("Alex_Neutral_02E"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewScoreNote(new List<int>() { 5, 1, 2, -1, 0 }, "Excellent first impression!"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewPlayUtterance("Alex_Neutral_03E"), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewScoreNoteFinal(new List<int>() { 3, 3, 3, 3, 3 }, "Overall, pretty average."), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });  System.Threading.Thread.Sleep(10);
                    dbSession.AddEvent(sessionName, "ict-ed", DBSession.EventData.NewEndSession(), error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddEvent() - error: {0}", error); });
                });
            });

            dbUser.CreateUser("ict-arno", "password", "ict", "ICT Arno (Student)", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("ict-matt", "password", "ict", "ICT Matt (Student)", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });

            dbClass.CreateClass("ivh", "ict", classError =>
            {
                if (!string.IsNullOrEmpty(classError)) { Debug.LogErrorFormat("CreateClass() - error: {0}", classError); return; }

                dbClass.UpdateClassTeacher("ivh", "ict-grace", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("UpdateClass() - error: {0}", error); });
                dbClass.AddStudentToClass("ivh", "ict-ed", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddStudentToClass() - error: {0}", error); });
                dbClass.AddStudentToClass("ivh", "ict-arno", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddStudentToClass() - error: {0}", error); });

                dbAssignment.CreateAssignmentTemplate("HW #1", "Talk to Alex",     "Have a conversation with Alex",      "ict",   "ict-grace", (assignment, error) => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateAssignment() - error: {0}", error); });
                dbAssignment.CreateAssignmentTemplate("HW #2", "Talk to Michelle", "Have a conversation with Michelle",  "ict",   "ict-grace", (assignment, error) => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateAssignment() - error: {0}", error); });
                dbAssignment.CreateAssignmentTemplate("HW #3", "Talk to Kevin",    "Have a conversation with Kevin",     "ict",   "ict-grace", (assignment, error) => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateAssignment() - error: {0}", error); });
            });

            dbClass.CreateClass("ag", "ict", classError =>
            {
                if (!string.IsNullOrEmpty(classError)) { Debug.LogErrorFormat("CreateClass() - error: {0}", classError); return; }

                dbClass.AddStudentToClass("ag", "ict-matt", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddStudentToClass() - error: {0}", error); });
            });
        });


        dbOrg.CreateOrganization("usc", "Randolph", "Hall", "noreply@usc.edu", "800-USC-TRJN", "usc-localadmin", "password", orgError =>
        {
            if (!string.IsNullOrEmpty(orgError)) { Debug.LogErrorFormat("CreateOrganization() - error: {0}", orgError); return; }

            dbClass.CreateClass("viterbi", "usc", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateClass() - error: {0}", error); });
        });

        dbOrg.CreateOrganization("fake", "Fake", "Contact", "noreply@example.com", "800-555-FAKE", "fake-localadmin", "password", orgError =>
        {
            if (!string.IsNullOrEmpty(orgError)) { Debug.LogErrorFormat("CreateOrganization() - error: {0}", orgError); return; }

            dbUser.CreateUser("fake-teacher-1", "password", "fake", "Fake Teacher 1", DBUser.AccountType.TEACHER, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("fake-teacher-2", "password", "fake", "Fake Teacher 2", DBUser.AccountType.TEACHER, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("fake-teacher-3", "password", "fake", "Fake Teacher 3", DBUser.AccountType.TEACHER, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });

            dbUser.CreateUser("fake-student-new-1", "password", "fake", "Fake New 1", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("fake-student-beginner-1", "password", "fake", "Fake Beginner 1", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("fake-student-beginner-2", "password", "fake", "Fake Beginner 2", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("fake-student-intermediate-1", "password", "fake", "Fake Intermediate 1", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("fake-student-intermediate-2", "password", "fake", "Fake Intermediate 2", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });
            dbUser.CreateUser("fake-student-expert-1", "password", "fake", "Fake Expert 1", DBUser.AccountType.STUDENT, error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateUser() - error: {0}", error); });

            dbClass.CreateClass("fake-class-1", "fake", classError =>
            {
                if (!string.IsNullOrEmpty(classError)) { Debug.LogErrorFormat("CreateClass() - error: {0}", classError); return; }

                dbClass.UpdateClassTeacher("fake-class-1", "fake-teacher-1", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("UpdateClass() - error: {0}", error); });
                dbClass.AddStudentToClass("fake-class-1", "fake-student-new-1", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddStudentToClass() - error: {0}", error); });
                dbClass.AddStudentToClass("fake-class-1", "fake-student-beginner-1", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddStudentToClass() - error: {0}", error); });
                dbClass.AddStudentToClass("fake-class-1", "fake-student-intermediate-1", error => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("AddStudentToClass() - error: {0}", error); });

                //dbAssignment.CreateAssignmentTemplate("HW #1", "Talk to Alex",     "Have a conversation with Alex",      "ict",   "ict-grace", (assignment, error) => { if (!string.IsNullOrEmpty(error)) Debug.LogErrorFormat("CreateAssignment() - error: {0}", error); });
            });
        });

        Debug.LogWarningFormat("CreateTestData() finished");
    }
}
