using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Reflection;

public class Main : MonoBehaviour
{
    GameObject m_menu;
    GameObject m_environment;
    GameObject m_character;
    DebugConsole m_debugConsole;
    DebugController m_debugController;

    public FreeMouseLook m_camera;
    public Texture2D m_whiteTexture;
    public MicrophoneRecorder m_Mic;
    public AudioStreamer m_Streamer;

    Vector3 m_StartingCameraPosition;
    Quaternion m_StartingCameraRotation;


    [NonSerialized] public int m_vitaCurrentCharacter = -1;
    int m_vitaCurrentBackground = 0;
    [NonSerialized] public int m_vitaCurrentMood = -1;
    //bool m_vitaHideMouse = false;

    [NonSerialized] public int m_micDetectionStatus = 0;   // 0 - Silence, 1 - Speaking, 2 - Cooldown
    [NonSerialized] public int m_turnTakingState = 0;   // 0 - AgentSpeaking, 1 - WaitForUserToStartSpeaking, 2 - UserSpeaking, 3 - WaitForAgentToStartSpeaking

    [NonSerialized] public float m_prevMicRecordingLevel = 0;

    bool m_waitForCutsceneFinished = false;
    Cutscene m_currentCutscene;


    void Start()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;


        int currentCharacter = PlayerPrefs.GetInt("vitaCurrentCharacter", -1);
        if (currentCharacter != -1)
            m_vitaCurrentCharacter = currentCharacter;

        int currentBackground = PlayerPrefs.GetInt("vitaCurrentBackground", -1);
        if (currentBackground != -1)
            m_vitaCurrentBackground = currentBackground;

        int currentMood = PlayerPrefs.GetInt("vitaCurrentMood", -1);
        if (currentMood != -1)
            m_vitaCurrentMood = currentMood;


#if false
        string currentSubjectNumber = PlayerPrefs.GetString("vitaCurrentSubjectNumber", "");
        if (currentSubjectNumber != "")
            m_vitaSubjectNumber = currentSubjectNumber;

        int currentRecordMovieFlag = PlayerPrefs.GetInt("vitaRecordMovieFlag", -1);
        if (currentRecordMovieFlag != -1)
            m_vitaRecordMovieFlag = currentRecordMovieFlag == 1;

        int currentManualResponseMode = PlayerPrefs.GetInt("vitaManualResponseMode", -1);
        if (currentManualResponseMode != -1)
            m_vitaManualResponseMode = currentManualResponseMode == 1;

        int currentLaptopModeFlag = PlayerPrefs.GetInt("vitaLaptopModeFlag", -1);
        if (currentLaptopModeFlag != -1)
            m_vitaLaptopModeFlag = currentLaptopModeFlag == 1;

        int currentLinearMode = PlayerPrefs.GetInt("vitaLinearMode", -1);
        if (currentLinearMode != -1)
            m_vitaLinearMode = currentLinearMode == 1;
#endif


#if false
        m_Streamer.AddOnStreamingSoundDetected(OnStreamAudioDetected);
        m_Streamer.AddOnStreamingSilenceDetected(OnStreamSilenceDetected);
        m_Streamer.AddOnStreamingFinished(OnStoppedStreaming);


        // 02/2015 - this throws an error if no mic is installed on the system.  so make sure it's the last block of code in Start()
        if (m_Mic != null)
        {
            m_Mic.AddOnStartedRecordingCallback(OnStartedStreaming);
            m_Mic.SetContinuousStreaming(true);
        }
        else
        {
            Debug.LogWarning("m_Mic variable is null. Silence detection won't work");
        }
#endif


        SpawnCharacter();

        // set up environment, including camera and lighting
        SetCamera(string.Format("{0}_{1}", VitaGlobals.m_vitaBackgroundInfo[m_vitaCurrentBackground].cameraPrefix, VitaGlobals.m_vitaCharacterInfo[m_vitaCurrentCharacter].archetype));
        SetLighting(VitaGlobals.m_vitaBackgroundInfo[m_vitaCurrentBackground].lightGroupName);

        GameObject character = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_vitaCurrentCharacter].prefab);
        character.GetComponent<MecanimCharacter>().Gaze(m_camera.gameObject.name);


        m_menu = GameObject.Find("Canvas");
        m_environment = GameObject.Find("Scene");
        m_character = GameObject.Find("Characters");

        NetworkRelay.RemoveAllMessageCallbacks();
        NetworkRelay.AddMessageCallback(HandleNetworkMessage);

        m_debugConsole = GameObject.FindObjectOfType<DebugConsole>();
        m_debugController = GameObject.FindObjectOfType<DebugController>();

        StartCoroutine(StartMenus());

        // TODO - For BETA release, disable on screen log
        DebugOnScreenLog debugOnScreenLog = VHUtils.FindChildOfType<DebugOnScreenLog>(GameObject.Find("vhAssets"));
        debugOnScreenLog.gameObject.SetActive(false);
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PopUpDisplay.Instance.DisplayYesNo("Quit?", "Would you like to exit the VITA application and return to the desktop?", () => { NetworkRelay.SendNetworkMessage("quit"); }, null);
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            m_debugController.m_showController = !m_debugController.m_showController;
            //m_vitaHideMouse = GameObject.FindObjectOfType<DebugController>().m_showController ? false : true;
        }

        if (!m_debugConsole.DrawConsole && !VitaGlobals.DoesInputHaveFocus() && m_debugController.m_showController)
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                //m_vitaHideMouse = !m_vitaHideMouse;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                // reset camera position
                m_camera.transform.position = m_StartingCameraPosition;
                m_camera.transform.rotation = m_StartingCameraRotation;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                GameObject.FindObjectOfType<DebugInfo>().NextMode();
            }
        }

#if false
        float recordingLevel = m_Mic.GetRecordingVolumeLevel();
        recordingLevel = Math.Min(recordingLevel * 2.0f, 1.0f);
        if (recordingLevel > m_prevMicRecordingLevel)
        {
            m_prevMicRecordingLevel = recordingLevel;
        }
        else
        {
            m_prevMicRecordingLevel -= (Time.deltaTime * 1.5f);
            m_prevMicRecordingLevel = Math.Max(m_prevMicRecordingLevel, 0);
        }
#endif

        if (m_waitForCutsceneFinished)
        {
            if (m_currentCutscene.GetNextEvent() == null)
            {
                DictationRecognizer dictationRecognizer = GameObject.FindObjectOfType<DictationRecognizer>();
                dictationRecognizer.StartRecording();

                m_waitForCutsceneFinished = false;
                m_currentCutscene = null;
            }
        }

        m_camera.enabled = !m_debugConsole.DrawConsole;

        //Cursor.lockState = (m_camera.CameraRotationOn || m_vitaHideMouse) ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.lockState = m_camera.CameraRotationOn ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
    }


    void OnGUI()
    {
        {
#if false
            if (m_vitaLaptopShowMenu)
            {
                float buttonX =   0.0f / 1920;
                float buttonY =   0.0f / 1080;
                float buttonW = 193.0f / 1920;
                float buttonH = 970.0f / 1080;

                Rect r = new Rect(0, 0, buttonW, buttonH);
                GUI.color = new Color(1, 1, 1, m_vitaLaptopMenuGuiBackgroundAlpha);
                GUI.DrawTexture(VHGUI.ScaleToRes(ref r), m_vitaLaptopMenuGuiBackground);
                GUI.color = Color.white;

                buttonX =  27.0f / 1920;
                buttonY = 114.0f / 1080;
                buttonW = 139.0f / 1920;
                buttonH = (60.0f / 1080) * Screen.height;

                Rect rectArea = new Rect(buttonX, buttonY, buttonW, 1.0f);
                GUILayout.BeginArea(VHGUI.ScaleToRes(ref rectArea));

                GUILayout.BeginVertical();

                // this is inconvenient.  We draw the slider, but hide it, only so that we can get the rect.
                // we do this so that we can draw the box *under* the slider.  So, draw it once disabled to get the rect, then draw the box, then draw it again to make it visible.

                GUI.enabled = false;
                GUI.color = Color.clear;
                m_Streamer.m_SilenceThreshhold = GUILayout.HorizontalSlider(m_Streamer.m_SilenceThreshhold, 0, 1, m_vitaGuiSkin.customStyles[VitaGlobals.m_vitaGuiSkinCustomSliderLaptopSilent], m_vitaGuiSkin.customStyles[VitaGlobals.m_vitaGuiSkinCustomSliderThumbLaptopSilent], GUILayout.Height(VitaGlobals.VitaNormalizedScreenWidth(54)));
                GUI.enabled = true;
                GUI.color = Color.white;

                if (m_vitaManualResponseMode)
                {
                    GUI.enabled = false;
                    GUI.color = Color.clear;
                }

                if (!m_vitaManualResponseMode) GUI.color = new Color(113.0f/255.0f, 114.0f/255.0f, 114.0f/255.0f, 1);  // #717272
                Rect micLevelBack = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(micLevelBack, m_whiteTexture);
                if (!m_vitaManualResponseMode) GUI.color = Color.white;

                if (!m_vitaManualResponseMode) GUI.color = new Color(163.0f/255.0f, 163.0f/255.0f, 163.0f/255.0f, 1);  // #A3A3A3
                Rect micLevel = GUILayoutUtility.GetLastRect();
                micLevel.width = m_prevMicRecordingLevel * micLevel.width;
                GUI.DrawTexture(micLevel, m_whiteTexture);
                if (!m_vitaManualResponseMode) GUI.color = Color.white;

                m_Streamer.m_SilenceThreshhold = GUI.HorizontalSlider(GUILayoutUtility.GetLastRect(), m_Streamer.m_SilenceThreshhold, 0, 1, m_vitaGuiSkin.customStyles[VitaGlobals.m_vitaGuiSkinCustomSliderLaptopSilent], m_vitaGuiSkin.customStyles[VitaGlobals.m_vitaGuiSkinCustomSliderThumbLaptopSilent]);

                GUI.enabled = true;
                GUI.color = Color.white;

                GUILayout.EndVertical();

                GUILayout.EndArea();
            }
#endif
        }
    }


    IEnumerator StartMenus()
    {
        // we wait a bit for all Start()'s to run, then enable/disable the gameobject

        yield return new WaitForSeconds(0.1f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();


        if (VitaGlobals.m_runMode == VitaGlobals.RunMode.SingleScreen ||
            VitaGlobals.m_interviewType == VitaGlobals.InterviewType.StudentPractice)
        {
            ConfigureSingleScreenMode();
        }
        else
        {
            ConfigureTwoScreenMode();
        }


        string teacherUsername = "DEMO";
        string character = VitaGlobals.m_vitaCharacterInfo[m_vitaCurrentCharacter].prefab;
        string environment = VitaGlobals.m_vitaBackgroundInfo[m_vitaCurrentBackground].sceneName;
        string disposition = VitaGlobals.m_vitaMoods[m_vitaCurrentMood];

        if (VitaGlobals.m_interviewType != VitaGlobals.InterviewType.Demo)
        {
            if (VitaGlobals.m_loggedInProfile != null)
            {
                teacherUsername = VitaGlobals.m_loggedInProfile.username;
            }
        }

        DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
        VitaGlobals.m_localSessionInfo = new VitaGlobals.LocalSessionInfo();
        VitaGlobals.m_localSessionInfo.sessionName = dbSession.NewSessionName();
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewStartSession(DBSession.SessionType.Interview, teacherUsername, character, environment, disposition));
#if false
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewStartSession(DBSession.SessionType.Interview));
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewStartTeacher(teacherUsername));
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewStartCharacter(character));
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewStartEnvironment(environment));
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewStartDisposition(disposition));
#endif

        // play intro cutscene
        if (m_character.activeSelf)
        {
            // Alex_Startup_Neutral
            string characterStripped = character.Replace("Chr", "").Replace("Prefab", "");
            string dispositionPrefix = VitaGlobals.m_vitaMoodsCutscenePrefix[m_vitaCurrentMood];
            string cutsceneName = string.Format("{0}_Startup_{1}", characterStripped, dispositionPrefix);
            PlayCutscene(cutsceneName);
        }
    }


#if false
    void OnStartedStreaming(AudioStream stream)
    {
        Debug.Log("OnStartedStreaming");
        if (m_Streamer != null)
        {
            m_Streamer.Stream(stream, null);
        }
        else
        {
            Debug.LogWarning("m_Streamer variable is null. Silence detection won't work");
        }
    }

    void OnStoppedStreaming()
    {
        //Debug.Log("OnStoppedStreaming");
    }

    void OnStreamAudioDetected()
    {
        //Debug.Log("OnStreamAudioDetected");
        //VHMsgBase.Get().SendVHMsg("vrSpeech start utt29 captain");

        m_micDetectionStatus = 1;

        if (m_vitaManualResponseMode)
            return;

        if (m_turnTakingState == 1)
        {
            m_turnTakingState = 2;
        }
    }

    void OnStreamSilenceDetected()
    {
        //Debug.Log("OnStreamSilenceDetected");
        //VHMsgBase.Get().SendVHMsg("vrSpeech finished-speaking utt29");

        m_micDetectionStatus = 0;

        if (m_vitaManualResponseMode)
            return;

        if (m_turnTakingState == 2)
        {
            m_turnTakingState = 3;

            VitaPlayNextQuestion();
        }
    }
#endif


    public void SwitchScreens()
    {
        VitaGlobals.m_isGUIScreen = !VitaGlobals.m_isGUIScreen;

        m_menu.SetActive(VitaGlobals.m_isGUIScreen);
        m_environment.SetActive(!VitaGlobals.m_isGUIScreen);
        m_character.SetActive(!VitaGlobals.m_isGUIScreen);
    }

    public void PlayCutscene(string cutsceneName)
    {
        if (m_character.activeSelf)
        {
            GameObject cutscenes = GameObject.Find("Cutscenes");
            GameObject cutsceneObj = VHUtils.FindChildRecursive(cutscenes, cutsceneName);
            Cutscene cutscene = cutsceneObj.GetComponent<Cutscene>();
            cutscene.Play();

            DictationRecognizer dictationRecognizer = GameObject.FindObjectOfType<DictationRecognizer>();
            dictationRecognizer.StopRecording();

            m_waitForCutsceneFinished = true;
            m_currentCutscene = cutscene;
        }

        //if (VitaGlobals.m_localSessionInfo != null)
        //    VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewPlayUtterance(cutsceneName));
    }

    void HandleNetworkMessage(object sender, string message)
    {
        string [] splitargs = message.Split(' ');
        string opcode = splitargs[0];

        Debug.LogFormat(opcode);

        if (opcode == "quit")
        {
            VHUtils.ApplicationQuit();
        }
        else if (opcode == "switchscreens")
        {
            SwitchScreens();
        }
        else if (opcode == "playcutscene")
        {
            string cutsceneName = splitargs[1];
            PlayCutscene(cutsceneName);
        }
        else if (opcode == "dictationresult")
        {
            if (!m_character.activeSelf)
            {
                // record an interview event
                string text = message.Substring("dictationresult ".Length);
                MenuManagerPanelInterviewScreen panelInterviewScreen = GameObject.FindObjectOfType<MenuManagerPanelInterviewScreen>();
                if (panelInterviewScreen != null)
                    panelInterviewScreen.LogEvent_Student(text);
            }
        }
        else if (opcode == "endinterview")
        {
            VHUtils.SceneManagerLoadScene("ConfigurationNew");
        }
    }

    public void SpawnCharacter()
    {
        // spawn character and place in correct location

        GameObject characters = GameObject.Find("Characters");

        GameObject [] charactersChildren = VHUtils.FindAllChildren(characters);
        foreach (var child in charactersChildren)
        {
            Destroy(child);
        }

        MecanimManager mecanimManager = GameObject.FindObjectOfType<MecanimManager>();
        mecanimManager.RemoveAllCharacters();


        string prefabName = VitaGlobals.m_vitaCharacterInfo[m_vitaCurrentCharacter].prefab;
        UnityEngine.Object character = Instantiate(Resources.Load(prefabName));
        character.name = prefabName;
        GameObject gameObject = (GameObject)character;
        gameObject.transform.parent = characters.transform;

        MecanimCharacter mecanimCharacter = gameObject.GetComponent<MecanimCharacter>();
        mecanimManager.AddCharacter(mecanimCharacter);

        GameObject locator = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_vitaCurrentCharacter].locatorName);
        gameObject.transform.position = locator.transform.position;
        gameObject.transform.rotation = locator.transform.rotation;
    }

    public void SetCamera(string name)
    {
        GameObject environmentGlobals = GameObject.Find("EnvironmentGlobals");
        GameObject cameras = VHUtils.FindChild(environmentGlobals, "Cameras");

        foreach (var backgroundInfo in VitaGlobals.m_vitaBackgroundInfo)
        {
            var cameraPrefix = backgroundInfo.cameraPrefix;

            string cameraFmlString = cameraPrefix + "_Fml";
            GameObject cameraFml = VHUtils.FindChild(cameras, cameraFmlString);
            if (cameraFml != null)
            {
                cameraFml.SetActive(false);
            }

            string cameraMleString = cameraPrefix + "_Mle";
            GameObject cameraMle = VHUtils.FindChild(cameras, cameraMleString);
            if (cameraMle != null)
            {
                cameraMle.SetActive(false);
            }
        }

        string cameraString = name;
        GameObject cameraObj = VHUtils.FindChild(cameras, cameraString);
        cameraObj.SetActive(true);

        m_camera = cameraObj.GetComponent<FreeMouseLook>();

        m_StartingCameraPosition = m_camera.transform.position;
        m_StartingCameraRotation = m_camera.transform.rotation;
    }

    public void SetLighting(string namedSet)
    {
        AGLightSettingsVita lightSettings = GameObject.FindObjectOfType<AGLightSettingsVita>();
        lightSettings.UseSetNamed(namedSet);
    }

    void ConfigureTwoScreenMode()
    {
        // two screen mode has the gui on one screen, environment+character on another

        m_menu.SetActive(VitaGlobals.m_isGUIScreen);
        m_environment.SetActive(!VitaGlobals.m_isGUIScreen);
        m_character.SetActive(!VitaGlobals.m_isGUIScreen);

        // need to change to the right menu.  This is handled in MenuManager.StartInitialMenu()
    }

    void ConfigureSingleScreenMode()
    {
        // single screen mode has the gui (outline) and environment+character on one screen. 
        // If there's another screen running, that one is blank

        m_menu.SetActive(VitaGlobals.m_isGUIScreen);
        m_environment.SetActive(VitaGlobals.m_isGUIScreen);
        m_character.SetActive(VitaGlobals.m_isGUIScreen);

        // need to change to the right menu.  This is handled in MenuManager.StartInitialMenu()
    }
}
