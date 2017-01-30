using UnityEngine;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class VitaGlobals
{
    public enum RunMode
    {
        TwoMonitors,
        SplitScreen,
        SingleScreen,
    }

    public enum InterviewType
    {
        Interview,
        StudentPractice,
        Demo,
    }

    public static IntPtr m_windowHwnd = IntPtr.Zero;  // the Windows hwnd parameter which is used by many WinAPI functions.  this is not valid in editor.  It's assigned in Startup.Start()

    public static RunMode m_runMode = RunMode.TwoMonitors;
    public static bool m_isServer = true;      // is this process acting as the server?
    public static bool m_isGUIScreen = true;   // is this process showing the GUI screen or the VH screen?
    public static string m_versionPrefix = "1.0.2";
    public static string m_versionDB = m_versionPrefix + ".0";  // db version always has the last number as 0, and it's unused during version checking
    public static string m_version;               // initialized in MainMenu.Start()
    public static string m_versionText = "VITA";  // initialized in MainMenu.Start()
    public static string m_versionUsername = "version";
    public static string m_versionPassword = "version";
    public static string m_statusUsername = "status";
    public static string m_statusPassword = "status";
    public static string m_statusRunning = "running";
    public static string m_statusMaintenance = "maintenance";
    public const string Confirm = "Confirm";
    public const string CannotBeUndone = "This action cannot be undone.";

    public static InterviewType m_interviewType;

    // TODO - This is only valid on one monitor.  We need to sync this data with both processes
    public static EntityDBVitaProfile m_loggedInProfile;  // which user is currently logged in.

    public static string m_interviewStudent = "";  // which student is currently being interviewed
    public static string m_selectedSession = "";   // which session is selected when switching between menus

    public static MenuManager.Menu m_returnMenu = MenuManager.Menu.None;  // which menu should be activated upon return from the Interview mode?
    public static bool m_setReturnMenu = false;                           // set this to true when you want to return to a specific menu, using m_returnMenu above.


    public class LocalSessionInfo
    {
        public string sessionName;
        public List<DBSession.EventData> eventData = new List<DBSession.EventData>();
    }

    public static LocalSessionInfo m_localSessionInfo;


    public class CharacterInfo
    {
        public string prefab;
        public string displayName;
        public string locatorName;
        public string archetype;
    }

    public static readonly List<CharacterInfo> m_vitaCharacterInfo = new List<CharacterInfo>()
    {
        new CharacterInfo() { prefab = "ChrAlexPrefab",     displayName = "Alex Moreno",      locatorName = "LocStart", archetype = "Mle" },
        new CharacterInfo() { prefab = "ChrBarbaraPrefab",  displayName = "Barbara Wilson",   locatorName = "LocStart", archetype = "Fml" },
        new CharacterInfo() { prefab = "ChrGeorgePrefab",   displayName = "George Schneider", locatorName = "LocStart", archetype = "Mle" },
        new CharacterInfo() { prefab = "ChrKevinPrefab",    displayName = "Kevin Johnson",    locatorName = "LocStart", archetype = "Mle" },
        new CharacterInfo() { prefab = "ChrMariaPrefab",    displayName = "Maria Gonzales",   locatorName = "LocStart", archetype = "Fml" },
        new CharacterInfo() { prefab = "ChrMichellePrefab", displayName = "Michelle Lee",     locatorName = "LocStart", archetype = "Fml" },
    };


    public class BackgroundInfo
    {
        public string displayName;
        public string sceneName;
        public string cameraPrefix;
        public string lightGroupName;
        public bool   shownInMenu;
    }

    public static readonly List<BackgroundInfo> m_vitaBackgroundInfo = new List<BackgroundInfo>()
    {
        new BackgroundInfo() { displayName = "Break Room",       sceneName = "EnvBreakRoom01Scene",       cameraPrefix = "CamEnvBreakRoom01",       lightGroupName = "BreakRoom",       shownInMenu = true },
        new BackgroundInfo() { displayName = "Conference Room",  sceneName = "EnvConferenceRoom01Scene",  cameraPrefix = "CamEnvConferenceRoom01",  lightGroupName = "ConferenceRoom",  shownInMenu = true },
        new BackgroundInfo() { displayName = "Executive Office", sceneName = "EnvExecutiveOffice01Scene", cameraPrefix = "CamEnvExecutiveOffice01", lightGroupName = "ExecutiveOffice", shownInMenu = true },
        new BackgroundInfo() { displayName = "Hotel Lobby",      sceneName = "EnvHotelLobby01Scene",      cameraPrefix = "CamEnvHotelLobby01",      lightGroupName = "HotelLobby",      shownInMenu = true },
        new BackgroundInfo() { displayName = "Manager's Office", sceneName = "EnvManagerOffice01Scene",   cameraPrefix = "CamEnvManagerOffice01",   lightGroupName = "ManagersOffice",  shownInMenu = true },
        new BackgroundInfo() { displayName = "Restaurant",       sceneName = "EnvRestaurant01Scene",      cameraPrefix = "CamEnvRestaurant01",      lightGroupName = "Restaurant",      shownInMenu = true },
        new BackgroundInfo() { displayName = "Warehouse Office", sceneName = "EnvWarehouseOffice01Scene", cameraPrefix = "CamEnvWarehouseOffice01", lightGroupName = "WarehouseOffice", shownInMenu = true },
        new BackgroundInfo() { displayName = "Guide Room",       sceneName = "EnvGuideRoomScene",         cameraPrefix = "CamEnvRestaurant01",      lightGroupName = "Restaurant",      shownInMenu = false },
    };


    public enum VitaMoods                                         { SoftTouch,    Neutral,   Hostile };
    public static readonly string [] m_vitaMoods =                { "Soft-Touch", "Neutral", "Hostile" };
    public static readonly string [] m_vitaMoodsCutscenePrefix =  { "SoftTouch",  "Neutral", "Hostile" };

    public enum VitaScenarios                                     { SoftTouch,              Neutral,             Hostile };
    public static readonly string [] m_vitaScenarios =            { "Soft-Touch Interview", "Neutral Interview", "Hostile Interview" };    // Names of scenarios. Currently the same as 'moods' but it may expand in the future which is why it is separate.

    public enum VitaResponseTypes                                 { Primary,    Acknowledgement,    Answer,     BuyTime,    Distraction,    Elaboration,    Engagement,     Opening }
    public static readonly string [] m_vitaResponseTypes        = { "Primary",  "Acknowledgement",  "Answer",   "BuyTime",  "Distraction",  "Elaboration",  "Engagement",   "Opening" };

    public class MiasScore
    {
        List<int> m_scores;

        public MiasScore() : this(0, 0, 0, 0, 0) { }

        public MiasScore(List<int> scores) : this(scores[0], scores[1], scores[2], scores[3], scores[4]) { }

        public MiasScore(int firstImpression, int interviewResponse, int selfPromoting, int activeListening, int closing)
        {
            m_scores = new List<int>();
            m_scores.Add(firstImpression);
            m_scores.Add(interviewResponse);
            m_scores.Add(selfPromoting);
            m_scores.Add(activeListening);
            m_scores.Add(closing);
        }

        public int this[VitaMiasCategories i]
        {
            get { return m_scores[(int)i]; }
            set { m_scores[(int)i] = value; }
        }

        public List<int> ToList() { return new List<int>(m_scores); }
    }

    public enum VitaMiasCategories { FirstImpression, InterviewResponse, SelfPromoting, ActiveListening, Closing, Score }


    /*
    int m_vitaCurrentCharacter = 0;
    int m_vitaCurrentBackground = 0;
    int m_vitaCurrentMood = 1;
    */


    // various screen and monitor stats, as detected by Windows.  Initialized in Startup.StartupMonitors()
    public static int SM_CMONITORS;
    public static int SM_XVIRTUALSCREEN;
    public static int SM_YVIRTUALSCREEN;
    public static int SM_CXVIRTUALSCREEN;
    public static int SM_CYVIRTUALSCREEN;
    public static List<WindowsAPI.DEVMODE> DEVMODES = new List<WindowsAPI.DEVMODE>();
    public static int m_currentDEVMODE = 0;


    //UI
    public static Color m_uiNormalColor =           new Color(1f, 1f, 1f, 162f/255f);
    public static Color m_uiHighlightColor =        new Color(1f, 1f, 1f, 180f / 255f);
    public static Color m_uiHighlightYellowColor =  new Color(249f/255f, 202f/255f, 22f/255f, 180f/255f);


    public static bool DoesInputHaveFocus()
    {
        bool inputSelected = false;
        UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem)
        {
            GameObject currentSelected = eventSystem.currentSelectedGameObject;
            if (currentSelected)
            {
                inputSelected = currentSelected.GetComponent<UnityEngine.UI.InputField>() != null;
            }
        }

        return inputSelected;
    }

    public static bool VersionClientServerCompatible(string versionDB)
    {
        // client and db versions are compatible if the first 3 numbers are equal.
        // Last number can be different, which means a client update was pushed that didn't require a DB update
        // eg, 1.0.0.0 == 1.0.0.1234
        //     1.0.1.0 != 1.0.0.0

        string [] splitDB = versionDB.Split('.');
        string [] splitClient = m_version.Split('.');

        if (splitDB.Length >= 3 &&
            splitClient.Length >= 3)
        {
            if (splitDB[0] == splitClient[0] &&
                splitDB[1] == splitClient[1] &&
                splitDB[2] == splitClient[2])
                return true;
        }
        return false;
    }

    public static string CreateNiceSessionName(string ticksString)
    {
        string returnString = ticksString;
        try
        {
            DateTime sessionDateTime = TicksToDateTime(ticksString);
            returnString = "Session" + sessionDateTime.Month.ToString().PadLeft(2, '0') + sessionDateTime.Day.ToString().PadLeft(2, '0') + "_" + sessionDateTime.Hour.ToString().PadLeft(2, '0') + sessionDateTime.Minute.ToString().PadLeft(2, '0');
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("{0}", e.ToString());
        }

        return returnString;
    }

    public delegate void LogoutDelegate(string error);

    public static void Logout(LogoutDelegate callback)
    {
        DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
        dbSession.AddEvent("GLOBAL", VitaGlobals.m_loggedInProfile.username, DBSession.EventData.NewLogout(), error =>
        {
            VitaGlobals.m_loggedInProfile = null;

            if (callback != null)
                callback(error);
        });
    }

    public static Int64 CurrentTimeToTicks()
    {
        // output format of ToFileTimeUtc() (Int64)
        return DateTime.Now.ToFileTimeUtc();
    }

    public static string CurrentTimeToString()
    {
        // output format of ToFileTimeUtc() (Int64)
        return CurrentTimeToTicks().ToString();
    }

    public static Int64 MaxTimeToTicks()
    {
        // output format of ToFileTimeUtc() (Int64)
        return DateTime.MaxValue.Date.ToFileTimeUtc();
    }

    public static string MaxTimeToString()
    {
        // output format of ToFileTimeUtc() (Int64)
        return MaxTimeToTicks().ToString();
    }

    public static DateTime TicksToDateTime(Int64 ticks)
    {
        // input format of ToFileTimeUtc() (Int64)
        return DateTime.FromFileTimeUtc(ticks);
    }

    public static DateTime TicksToDateTime(string ticks)
    {
        // input format of ToFileTimeUtc() (Int64)
        return TicksToDateTime(Convert.ToInt64(ticks));
    }

    public static string TicksToString(Int64 ticks)
    {
        // input format of ToFileTimeUtc() (Int64)
        // output format of "MM/dd/yyyy"
        return TicksToDateTime(ticks).ToString("MM/dd/yyyy");
    }

    public static string TicksToString(string ticks)
    {
        // input format of ToFileTimeUtc() (Int64)
        // output format of "MM/dd/yyyy"
        return TicksToDateTime(ticks).ToString("MM/dd/yyyy");
    }

    public static DateTime StringToDateTime(string date)
    {
        // input format of 10/25/2016
        return Convert.ToDateTime(date);
    }

    public static Int64 StringToTicks(string date)
    {
        // input format of 10/25/2016
        // output format of ToFileTimeUtc() (Int64)
        return StringToDateTime(date).ToFileTimeUtc();
    }

    public static string StringToTicksString(string date)
    {
        // input format of 10/25/2016
        // output format of ToFileTimeUtc() (Int64)
        return StringToTicks(date).ToString();
    }
}
