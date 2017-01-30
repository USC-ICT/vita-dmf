using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class MenuManager : MonoBehaviour
{
    public interface IMenuManagerInterface
    {
        void OnMenuEnter();
        void OnMenuExit();
    }

    public enum Menu
    {
        DemoAbout,
        DemoBadges,
        DemoConfigure,
        DemoPracticeSession,
        DemoUnlocks,
        DMFAdminHubAboutVita,
        DMFAdminHubArchive,
        DMFAdminHub,
        Exit,
        InterviewCompletion,
        InterviewRecord,
        InterviewScreen,
        LocalAdminHubAboutVita,
        LocalAdminHubClasses,
        LocalAdminHubExport,
        LocalAdminHubStudentArchives,
        LocalAdminHubStudents,
        LocalAdminHubTeacherArchives,
        LocalAdminHubTeachers,
        Login,
        StudentAboutVita,
        StudentAssignments,
        StudentBadges,
        StudentMias,
        StudentPracticeSession,
        StudentSessionConfiguration,
        StudentUnlocks,
        TeacherAbout,
        TeacherAssignments,
        TeacherClasses,
        TeacherConfigure,
        TeacherProfile,
        TeacherReview,
        TeacherStudents,

        None,
    }

    class MenuData
    {
        public string prefabName;
        //public Menu menu;
        public GameObject gameObject;
    }

    List<MenuData> m_menus = new List<MenuData>()
    {
        new MenuData() { prefabName = "PanelDemoAboutPrefab" },
        new MenuData() { prefabName = "PanelDemoBadgesPrefab" },
        new MenuData() { prefabName = "PanelDemoConfigurePrefab" },
        new MenuData() { prefabName = "PanelDemoPracticeSessionPrefab" },
        new MenuData() { prefabName = "PanelDemoUnlocksPrefab" },
        new MenuData() { prefabName = "PanelDMFAdminHubAboutVitaPrefab" },
        new MenuData() { prefabName = "PanelDMFAdminHubArchivePrefab" },
        new MenuData() { prefabName = "PanelDMFAdminHubPrefab" },
        new MenuData() { prefabName = "PanelExitPrefab" },
        new MenuData() { prefabName = "PanelInterviewCompletionPrefab" },
        new MenuData() { prefabName = "PanelInterviewRecordPrefab" },
        new MenuData() { prefabName = "PanelInterviewScreenPrefab" },
        new MenuData() { prefabName = "PanelLocalAdminHubAboutVitaPrefab" },
        new MenuData() { prefabName = "PanelLocalAdminHubClassesPrefab" },
        new MenuData() { prefabName = "PanelLocalAdminHubExportPrefab" },
        new MenuData() { prefabName = "PanelLocalAdminHubStudentArchivesPrefab" },
        new MenuData() { prefabName = "PanelLocalAdminHubStudentsPrefab" },
        new MenuData() { prefabName = "PanelLocalAdminHubTeacherArchivesPrefab" },
        new MenuData() { prefabName = "PanelLocalAdminHubTeachersPrefab" },
        new MenuData() { prefabName = "PanelLoginPrefab" },
        new MenuData() { prefabName = "PanelStudentAboutVitaPrefab" },
        new MenuData() { prefabName = "PanelStudentAssignmentsPrefab" },
        new MenuData() { prefabName = "PanelStudentBadgesPrefab" },
        new MenuData() { prefabName = "PanelStudentMiasPrefab" },
        new MenuData() { prefabName = "PanelStudentPracticeSessionPrefab" },
        new MenuData() { prefabName = "PanelStudentSessionConfigurationPrefab" },
        new MenuData() { prefabName = "PanelStudentUnlocksPrefab" },
        new MenuData() { prefabName = "PanelTeacherAboutVitaPrefab" },
        new MenuData() { prefabName = "PanelTeacherAssignmentsPrefab" },
        new MenuData() { prefabName = "PanelTeacherClassesPrefab" },
        new MenuData() { prefabName = "PanelTeacherConfigurePrefab" },
        new MenuData() { prefabName = "PanelTeacherProfilePrefab" },
        new MenuData() { prefabName = "PanelTeacherReviewPrefab" },
        new MenuData() { prefabName = "PanelTeacherStudentsPrefab" },
    };

    List<string> m_menusInterview = new List<string>()
    {
        "PanelInterviewScreenPrefab",
        "PanelStudentPracticeSessionPrefab",
    };


    GameObject m_canvas;
    Menu m_currentMenu = Menu.None;

    [NonSerialized] public Cutscene [] m_cutscenes;


    public GameObject Canvas { get { return m_canvas; } }
    public Menu CurrentMenu { get { return m_currentMenu; } }
    public GameObject CurrentMenuGameObject { get { return m_menus[(int)CurrentMenu].gameObject; } }


    void Awake()
    {
        m_canvas = GameObject.Find("Canvas");

        // disable canvas until all prefabs have been instantiated, and Start() called.  See StartInitialMenu()
        m_canvas.GetComponent<Canvas>().enabled = false;

        for (int i = 0; i < m_menus.Count; i++)
        {
            if (IsInterviewMode() &&
                !m_menusInterview.Exists(name => name == m_menus[i].prefabName))
            {
                continue;
            }

            GameObject menu = (GameObject)Instantiate(Resources.Load(m_menus[i].prefabName));
            menu.name = menu.name.Replace("(Clone)", "");
            menu.transform.SetParent(m_canvas.transform, false);

            m_menus[i].gameObject = menu;
        }
    }

    void Start()
    {
        m_cutscenes = GameObject.FindObjectsOfType<Cutscene>();
        Array.Sort<Cutscene>(m_cutscenes, (a, b) => a.name.CompareTo(b.name));

        StartCoroutine(StartInitialMenu());
    }

    public void ChangeMenu(Menu menu)
    {
        if (m_currentMenu != Menu.None)
        {
            m_menus[(int)m_currentMenu].gameObject.GetComponent<IMenuManagerInterface>().OnMenuExit();
        }

        for (int i = 0; i < m_menus.Count; i++)
        {
            if (m_menus[i].gameObject)
                m_menus[i].gameObject.SetActive(false);
        }

        if (menu != Menu.None)
        {
            m_menus[(int)menu].gameObject.SetActive(true);
            m_currentMenu = menu;

            m_menus[(int)m_currentMenu].gameObject.GetComponent<IMenuManagerInterface>().OnMenuEnter();
        }
    }

    public GameObject GetMenuGameObject(Menu menu)
    {
        return m_menus[(int)menu].gameObject;
    }

    public GameObject GetCommonMenuGameObject()
    {
        return VHUtils.FindChild(m_canvas, "PanelCommonPrefab");
    }

    IEnumerator StartInitialMenu()
    {
        // since this is done at startup, we want to make sure all menus run their Start() first

        yield return new WaitForSeconds(0.05f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // re-enable canvas after disabling it in Awake()
        m_canvas.GetComponent<Canvas>().enabled = true;

        Menu menu = Menu.Login;

        if (IsInterviewMode())
        {
            if (VitaGlobals.m_runMode == VitaGlobals.RunMode.SingleScreen ||
                VitaGlobals.m_interviewType == VitaGlobals.InterviewType.StudentPractice)
            {
                // HACK - We can't disable PanelCommonPrefab completely because it contains the popup dialogs, so we disable the specific elements.
                // this is a nasty hack, though esp if the contents of PanelCommonPrefab change
                {
                    GameObject panelCommon = GetCommonMenuGameObject();
                    //panelCommon.SetActive(false);
                    panelCommon.GetComponent<Image>().enabled = false;
                    foreach (var child in VHUtils.FindAllChildren(panelCommon))
                        child.SetActive(false);
                }
                menu = Menu.StudentPracticeSession;
            }
            else
            {
                menu = Menu.InterviewScreen;
            }
        }

        if (VitaGlobals.m_setReturnMenu)
        {
            VitaGlobals.m_setReturnMenu = false;
            menu = VitaGlobals.m_returnMenu;
        }

        ChangeMenu(menu);
    }

    bool IsInterviewMode()
    {
        return VHUtils.SceneManagerActiveSceneName().StartsWith("Env");
    }
}
