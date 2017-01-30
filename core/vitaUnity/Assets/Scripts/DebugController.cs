using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Reflection;

public class DebugController : MonoBehaviour
{
    #region Variables

    public Texture2D m_whiteTexture;

    Main m_main;

    [NonSerialized] public bool m_showController = false;

    enum ControllerMenus             { DEBUG,        CHARACTER,   ENVIRONMENT,    CONFIG,   SOUNDS, LENGTH };
    string [] m_controllerMenuText = { "debug menu", "character", "environment", "config", "sounds" };
    ControllerMenus m_controllerMenuSelected = ControllerMenus.DEBUG;

    float m_timeSlider = 1.0f;
    int m_saccadeMode = 3;

    //string [] testUtteranceButton = { "z_2", "z_3", };
    //string [] testUtteranceName = { "z_viseme_test2", "z_viseme_test3", };
    //string [] testUtteranceText = { "", "", };
    //int testUtteranceSelected = 0;

    float m_sacaadeMultiplier = 1.0f;

    string [] m_mouthLayer = { "open", "W", "ShCh", "PBM", "FV", "wide", "tBack", "tTeeth", "tRoof" };
    float [] m_mouthSliders = new float [100];
    string [] m_expressionLayer = {
        "001_inner_brow_raiser_lf",
        "001_inner_brow_raiser_rt",
        "002_outer_brow_raiser_lf",
        "002_outer_brow_raiser_rt",
        "004_brow_lowerer_lf",
        "004_brow_lowerer_rt",
        "005_upper_lid_raiser",
        "006_cheek_raiser",
        "007_lid_tightener",
        "010_upper_lip_raiser",
        "012_lip_corner_puller",
        "014_dimpler",
        "015_lip_corner_depressor",
        "017_chin_raiser",
        "018_lip_pucker",
        "020_lip_stretcher",
        "023_lip_tightener",
        "024_lip_pressor",
        "025_lips_part",
        "026_jaw_drop",
        "045_blink_lf",
        "045_blink_rt", };
    float [] m_expressionSliders = new float [100];

    //Sounds variables
    AGUtteranceDataFilter m_uttDataFilterCmp = null;
    //int m_debugSoundsCharacterSelection = -1;
    //int m_debugSoundsScenarioSelection = -1;
    //bool[] m_debugSoundsDisposition = new bool[VitaGlobals.m_vitaMoods.Length];
    //bool[] m_debugSoundsResponseType = new bool[VitaGlobals.m_vitaResponseTypes.Length];
    List<AGUtteranceData> m_uttDataPrimaryReponses = new List<AGUtteranceData>();
    List<AGUtteranceData> m_uttDataAcknowledgementReponses = new List<AGUtteranceData>();
    List<AGUtteranceData> m_uttDataAnswerReponses = new List<AGUtteranceData>();
    List<AGUtteranceData> m_uttDataBuyTimeReponses = new List<AGUtteranceData>();
    List<AGUtteranceData> m_uttDataDistrationReponses = new List<AGUtteranceData>();
    List<AGUtteranceData> m_uttDataEngagementReponses = new List<AGUtteranceData>();
    List<AGUtteranceData> m_uttDataElaborationReponses = new List<AGUtteranceData>();
    List<AGUtteranceData> m_uttDataOpeningReponses = new List<AGUtteranceData>();

    #endregion

    
    void Start()
    {
        m_main = GameObject.Find("Main").GetComponent<Main>();
    }

    void Update()
    {
    }

    void OnGUI()
    {
        {
            if (m_showController)
            {
#if false
                float buttonX = 0;
                float buttonY = 0;
                float buttonW = 150.0f / 1920;

                if (m_main.m_vitaLaptopShowMenu)
                    buttonX = 238.0f / 1920;

                float startX = buttonX + buttonW;
                Rect rectArea = new Rect(startX, buttonY, 1.0f - startX, 1.0f);
                GUILayout.BeginArea(VHGUI.ScaleToRes(ref rectArea));
                GUILayout.BeginVertical(GUI.skin.box);

                GUI.contentColor = Color.white;
                if (m_main.m_vitaLinearMode)
                {
                    GUILayout.Label(String.Format(@"Current Disposition: {0}", VitaGlobals.m_vitaMoods[m_main.m_vitaCurrentMood]));
                    GUILayout.Label(String.Format(@"Current Question ({0}/{1}): ""{2}""", m_main.m_interviewCurrentQuestion + 1, m_main.m_interviewSequence.Count, VitaGlobals.m_vitaInterviewQuestionData[m_main.m_interviewSequence[m_main.m_interviewCurrentQuestion]].text));
                    if (m_main.m_interviewCurrentQuestion + 1 < m_main.m_interviewSequence.Count)
                        GUILayout.Label(String.Format(@"Next Question ({0}/{1}): ""{2}""", m_main.m_interviewCurrentQuestion + 2, m_main.m_interviewSequence.Count, VitaGlobals.m_vitaInterviewQuestionData[m_main.m_interviewSequence[m_main.m_interviewCurrentQuestion + 1]].text));
                }
                else
                {
                    GUILayout.Label(String.Format(@"Current Question ({0}/{1}): {2} - ""{3}""", 0, 0, m_main.m_vitaBranchingCurrentNode.m_data.id, m_main.m_vitaBranchingCurrentNode.m_data.text));
                    foreach (var child in m_main.m_vitaBranchingCurrentNode.m_children)
                        GUILayout.Label(String.Format(@"Next Question ({0}/{1}): {2} - ""{3}""", 0, 0, child.m_data.id, child.m_data.text));
                }
                GUI.contentColor = Color.white;

                GUILayout.EndVertical();
                GUILayout.EndArea();
#endif
            }
        }


        if (m_showController)
        {
            float buttonX = 0;
            float buttonY = 0;
            //float buttonH = 20;
            float buttonW = 550.0f / 1920;

            //if (m_main.m_vitaLaptopShowMenu)
            //    buttonX = 238.0f / 1920;

            //if (VHUtils.IsAndroid())
            //    buttonH = 80;

            Rect rectArea = new Rect(buttonX, buttonY, buttonW, 1.0f);
            GUILayout.BeginArea(VHGUI.ScaleToRes(ref rectArea));

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label(VitaGlobals.m_versionText);
            GUILayout.Label(string.Format("isServer: {0}", VitaGlobals.m_isServer));
            GUILayout.Label(string.Format("isGUI: {0}", VitaGlobals.m_isGUIScreen));
            GUILayout.Label(string.Format("Connected: {0}", NetworkRelay.ConnectionEstablished));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(20))) { m_controllerMenuSelected = (ControllerMenus)VHMath.DecrementWithRollover((int)m_controllerMenuSelected, m_controllerMenuText.Length); }
            if (GUILayout.Button(m_controllerMenuText[(int)m_controllerMenuSelected])) { m_controllerMenuSelected = (ControllerMenus)VHMath.IncrementWithRollover((int)m_controllerMenuSelected, m_controllerMenuText.Length); }
            if (GUILayout.Button(">", GUILayout.Width(20))) { m_controllerMenuSelected = (ControllerMenus)VHMath.IncrementWithRollover((int)m_controllerMenuSelected, m_controllerMenuText.Length); }
            GUILayout.EndHorizontal();

            if (m_controllerMenuSelected == ControllerMenus.DEBUG)
            {
                OnGUIDebug();
            }
            else if (m_controllerMenuSelected == ControllerMenus.CHARACTER)
            {
                OnGUICharacter();
            }
            else if (m_controllerMenuSelected == ControllerMenus.ENVIRONMENT)
            {
                OnGUIEnvironment();
            }
            else if (m_controllerMenuSelected == ControllerMenus.CONFIG)
            {
                OnGUIConfig();
            }
            else if (m_controllerMenuSelected == ControllerMenus.SOUNDS)
            {
                OnGUISounds();
            }

#if false
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(testUtteranceButton[testUtteranceSelected], GUILayout.Height(buttonH)))
            {
                testUtteranceSelected++;
                testUtteranceSelected = testUtteranceSelected % testUtteranceButton.Length;
            }
            if (GUILayout.Button("Test Utt", GUILayout.Height(buttonH)))    { m_sbm.SBPlayAudio(VitaGlobals.m_vitaCharacters[m_main.m_vitaCurrentCharacter], testUtteranceName[testUtteranceSelected], testUtteranceText[testUtteranceSelected]); }
            GUILayout.EndHorizontal();

            m_main.m_Streamer.m_SilenceThreshhold = GUILayout.HorizontalSlider(m_main.m_Streamer.m_SilenceThreshhold, 0, 1);
            GUILayout.Label(string.Format("SilenceThresh: {0:f2}", m_main.m_Streamer.m_SilenceThreshhold));

            if (m_main.m_micDetectionStatus == 0)      GUILayout.Label("Mic: Silence");
            else if (m_main.m_micDetectionStatus == 1) GUILayout.Label("Mic: Speaking");
            else if (m_main.m_micDetectionStatus == 2) GUILayout.Label("Mic: Cooldown");

            if (m_main.m_turnTakingState == 0)      GUILayout.Label("Turn: AgentSpeaking");
            else if (m_main.m_turnTakingState == 1) GUILayout.Label("Turn: WaitForUser");
            else if (m_main.m_turnTakingState == 2) GUILayout.Label("Turn: UserSpeaking");
            else if (m_main.m_turnTakingState == 3) GUILayout.Label("Turn: WaitForAgent");
#endif

            GUILayout.EndVertical();

            GUILayout.EndArea();


            {
                // background
                Rect micFillBar = new Rect(0.94f, 0.25f, 0.03f, 0.70f);
                GUI.color = new Color(0, 0, 1, 1);
                VHGUI.DrawTexture(micFillBar, m_whiteTexture);
                GUI.color = Color.white;

                // current recording level
                Rect micLevel = micFillBar;
                GUI.color = new Color(1, 0, 0, 1);
                micLevel.height = m_main.m_prevMicRecordingLevel * micLevel.height;
                micLevel.y = micFillBar.y + (micFillBar.height - micLevel.height);
                VHGUI.DrawTexture(micLevel, m_whiteTexture);
                GUI.color = Color.white;

#if false
                // silence threshold
                Rect silenceLevel = micFillBar;
                GUI.color = Color.white;
                silenceLevel.height = 0.005f;
                silenceLevel.width += 0.015f;
                silenceLevel.x -= (0.015f / 2);
                silenceLevel.y = silenceLevel.y + ((1 - m_main.m_Streamer.m_SilenceThreshhold) * micFillBar.height) - (silenceLevel.height / 2);
                VHGUI.DrawTexture(silenceLevel, m_whiteTexture);
                GUI.color = Color.white;
#endif
            }


            Time.timeScale = m_timeSlider;
        }
    }

    void OnGUIDebug()
    {
        if (GUILayout.Button("Switch Screens"))
        {
            if (NetworkRelay.ConnectionEstablished)
            {
                NetworkRelay.SendNetworkMessage("switchscreens");
            }
            else
            {
                GameObject.FindObjectOfType<Main>().SwitchScreens();
            }
        }

        if (GUILayout.Button("End Interview"))
        {
            NetworkRelay.SendNetworkMessage("endinterview");
        }

        if (GUILayout.Button("Quit"))
        {
            NetworkRelay.SendNetworkMessage("quit");
        }

        GUILayout.Space(10);

        if (VHUtils.IsWindows10OrGreater())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            GUILayout.Label(string.Format("PhraseRecogntionSystem.isSupported: {0}", UnityEngine.Windows.Speech.PhraseRecognitionSystem.isSupported));
            GUILayout.Label(string.Format("PhraseRecogntionSystem.status: {0}", UnityEngine.Windows.Speech.PhraseRecognitionSystem.Status));

            DictationRecognizer dictationRecognizer = GameObject.FindObjectOfType<DictationRecognizer>();
            GUILayout.Label(string.Format("Dictation.status: {0}", dictationRecognizer.m_dictationRecognizer.Status));
            GUILayout.Label(string.Format("Dictation.AutoSilenceTimeoutSeconds: {0}", dictationRecognizer.m_dictationRecognizer.AutoSilenceTimeoutSeconds));
            GUILayout.Label(string.Format("Dictation.InitialSilenceTimeoutSeconds: {0}", dictationRecognizer.m_dictationRecognizer.InitialSilenceTimeoutSeconds));

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

    void OnGUICharacter()
    {
        for (int i = 0; i < VitaGlobals.m_vitaCharacterInfo.Count; i++)
        {
            if (GUILayout.Button(VitaGlobals.m_vitaCharacterInfo[i].displayName))
            {
                m_main.m_vitaCurrentCharacter = i;
                m_main.SpawnCharacter();
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Gaze:");
        if (GUILayout.Button("Camera"))
        {
            GameObject character = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_main.m_vitaCurrentCharacter].prefab);
            MecanimCharacter mecanim = character.GetComponent<MecanimCharacter>();
            Debug.Log(m_main.m_camera.gameObject.name);
            mecanim.Gaze(m_main.m_camera.gameObject.name);
        }

        if (GUILayout.Button("Off"))
        {
            GameObject character = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_main.m_vitaCurrentCharacter].prefab);
            MecanimCharacter mecanim = character.GetComponent<MecanimCharacter>();
            mecanim.StopGaze(0.5f);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Saccades:");

        int prevMode = m_saccadeMode;
        m_saccadeMode = GUILayout.SelectionGrid(m_saccadeMode, new string[] { "Listen", "Talk", "Think", "Off" }, 4);
        if (prevMode != m_saccadeMode)
        {
            GameObject character = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_main.m_vitaCurrentCharacter].prefab);
            MecanimCharacter mecanim = character.GetComponent<MecanimCharacter>();
            CharacterDefines.SaccadeType saccadeType = (CharacterDefines.SaccadeType)m_saccadeMode;
            mecanim.GetComponent<SaccadeController>().enabled = saccadeType != CharacterDefines.SaccadeType.End;
            mecanim.SetSaccadeBehaviour((CharacterDefines.SaccadeType)m_saccadeMode);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            string newText = GUILayout.TextField(m_sacaadeMultiplier.ToString("F2"), GUILayout.Width(50));
            m_sacaadeMultiplier = Convert.ToSingle(newText);

            float newSlider = GUILayout.HorizontalSlider(m_sacaadeMultiplier, 0, 1);
            if (newSlider != m_sacaadeMultiplier)
            {
                m_sacaadeMultiplier = newSlider;
                GameObject character = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_main.m_vitaCurrentCharacter].prefab);
                MecanimCharacter mecanim = character.GetComponent<MecanimCharacter>();
                mecanim.GetComponent<SaccadeController>().MagnitudeScaler = m_sacaadeMultiplier;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Locators:");
        for (int i = 0; i < VitaGlobals.m_vitaCharacterInfo.Count; i++)
        {
            if (GUILayout.Button(VitaGlobals.m_vitaCharacterInfo[i].locatorName))
            {
                GameObject character = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_main.m_vitaCurrentCharacter].prefab);

                GameObject locator = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[i].locatorName);
                character.transform.position = locator.transform.position;
                character.transform.rotation = locator.transform.rotation;
            }
        }

        GUILayout.Label("Face:");
        for (int i = 0; i < m_mouthLayer.Length; i++)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(string.Format(@"{0}:", m_mouthLayer[i]), GUILayout.Width(50));

            float newSlider = GUILayout.HorizontalSlider(m_mouthSliders[i], 0, 1);
            if (newSlider != m_mouthSliders[i])
            {
                m_mouthSliders[i] = newSlider;

                //SmartbodyManager.Get().PythonCommand(string.Format(@"scene.command('char {0} viseme au_{1} {2}')", opponentName, i, newSlider));
                GameObject character = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_main.m_vitaCurrentCharacter].prefab);
                MecanimCharacter mecanim = character.GetComponent<MecanimCharacter>();
                mecanim.SetFloatParam(m_mouthLayer[i], newSlider);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(5);
        for (int i = 0; i < m_expressionLayer.Length; i++)
        {
            GUILayout.Label(string.Format(@"{0}:", m_expressionLayer[i]));

            float newSlider = GUILayout.HorizontalSlider(m_expressionSliders[i], 0, 1);
            if (newSlider != m_expressionSliders[i])
            {
                m_expressionSliders[i] = newSlider;

                GameObject character = GameObject.Find(VitaGlobals.m_vitaCharacterInfo[m_main.m_vitaCurrentCharacter].prefab);
                MecanimCharacter mecanim = character.GetComponent<MecanimCharacter>();
                mecanim.SetFloatParam(m_expressionLayer[i], newSlider);
            }
        }
    }

    void OnGUIEnvironment()
    {
        GUILayout.Label("Scenes:");
        for (int i = 0; i < VitaGlobals.m_vitaBackgroundInfo.Count; i++)
        {
            if (GUILayout.Button(VitaGlobals.m_vitaBackgroundInfo[i].displayName))
            {
                string environment = VitaGlobals.m_vitaBackgroundInfo[i].sceneName;

                PlayerPrefs.SetInt("vitaCurrentBackground", i);

                VHUtils.SceneManagerLoadScene(environment);
            }
        }

        GUILayout.Label("Cameras:");
        for (int i = 0; i < VitaGlobals.m_vitaBackgroundInfo.Count; i++)
        {
            string fml = VitaGlobals.m_vitaBackgroundInfo[i].cameraPrefix + "_Fml";
            if (GUILayout.Button(fml))
            {
                m_main.SetCamera(fml);
            }

            string mle = VitaGlobals.m_vitaBackgroundInfo[i].cameraPrefix + "_Mle";
            if (GUILayout.Button(VitaGlobals.m_vitaBackgroundInfo[i].cameraPrefix + "_Mle"))
            {
                m_main.SetCamera(mle);
            }
        }

        GUILayout.Label("Lighting:");
        for (int i = 0; i < VitaGlobals.m_vitaBackgroundInfo.Count; i++)
        {
            if (GUILayout.Button(VitaGlobals.m_vitaBackgroundInfo[i].lightGroupName))
            {
                m_main.SetLighting(VitaGlobals.m_vitaBackgroundInfo[i].lightGroupName);
            }
        }
    }

    void OnGUIConfig()
    {
        GUILayout.Space(5);

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

        m_timeSlider = GUILayout.HorizontalSlider(m_timeSlider, 0.01f, 3);
        GUILayout.Label(string.Format("Time: {0}", m_timeSlider));

        GUILayout.Space(5);

        if (GUILayout.Button(string.Format("{0}", QualitySettings.names[QualitySettings.GetQualityLevel()])))
        {
            QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel() + 1 >= QualitySettings.names.Length ? 0 : QualitySettings.GetQualityLevel() + 1);

            if (QualitySettings.names[QualitySettings.GetQualityLevel()] == "Fant-NoVsync")
            {
                Application.targetFrameRate = 0;
            }
            else
            {
                Application.targetFrameRate = 60;
            }
        }
    }

    void OnGUISounds()
    {
        if (GUILayout.Button("Switch Screens"))
        {
            NetworkRelay.SendNetworkMessage("switchscreens");
        }
        GUILayout.Label("Character: " + VitaGlobals.m_vitaCharacterInfo[m_main.m_vitaCurrentCharacter].prefab);
        GUILayout.Label("Scenario: " + VitaGlobals.m_vitaMoods[m_main.m_vitaCurrentMood]);
        if (GUILayout.Button("Refresh Query"))
        {
            m_uttDataFilterCmp = null;
        }

        if (m_uttDataFilterCmp == null)
        {
            m_uttDataFilterCmp = GameObject.FindObjectOfType<AGUtteranceDataFilter>();
            bool[] m_scenario = m_uttDataFilterCmp.GetScenarioBoolArrayByEnum((VitaGlobals.VitaScenarios)m_main.m_vitaCurrentMood);

            m_uttDataPrimaryReponses = m_uttDataFilterCmp.GetSounds(m_main.m_vitaCurrentCharacter, m_scenario,             m_uttDataFilterCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Primary));
            m_uttDataAcknowledgementReponses = m_uttDataFilterCmp.GetSounds(m_main.m_vitaCurrentCharacter, m_scenario,     m_uttDataFilterCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Acknowledgement));
            m_uttDataAnswerReponses = m_uttDataFilterCmp.GetSounds(m_main.m_vitaCurrentCharacter, m_scenario,              m_uttDataFilterCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Answer));
            m_uttDataBuyTimeReponses = m_uttDataFilterCmp.GetSounds(m_main.m_vitaCurrentCharacter, m_scenario,             m_uttDataFilterCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.BuyTime));
            m_uttDataDistrationReponses = m_uttDataFilterCmp.GetSounds(m_main.m_vitaCurrentCharacter, m_scenario,          m_uttDataFilterCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Distraction));
            m_uttDataElaborationReponses = m_uttDataFilterCmp.GetSounds(m_main.m_vitaCurrentCharacter, m_scenario,         m_uttDataFilterCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Elaboration));
            m_uttDataEngagementReponses = m_uttDataFilterCmp.GetSounds(m_main.m_vitaCurrentCharacter, m_scenario,          m_uttDataFilterCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Engagement));
            m_uttDataOpeningReponses = m_uttDataFilterCmp.GetSounds(m_main.m_vitaCurrentCharacter, m_scenario,             m_uttDataFilterCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Opening));
        }
        if (m_uttDataFilterCmp == null)
        {
            return;
        }

        GUILayout.EndArea();
        float soundColumnWidth = 250;
        Rect rectAreaColumn1 = new Rect(Screen.width - 100 - soundColumnWidth, 0, soundColumnWidth, Screen.height);
        Rect rectAreaColumn2 = new Rect(Screen.width - 100 - (soundColumnWidth * 2), 0, soundColumnWidth, Screen.height);
        //Rect rectAreaColumn3 = new Rect(Screen.width - 100 - (soundColumnWidth * 3), 0, soundColumnWidth, Screen.height);
        GUILayout.BeginArea(rectAreaColumn2);

        //Primary
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Primary");
        foreach (AGUtteranceData utterance in m_uttDataPrimaryReponses)
        {
            if (GUILayout.Button(utterance.gameObject.name))
            {
                NetworkRelay.SendNetworkMessage("playcutscene " + utterance.gameObject.name);
            }
        }
        GUILayout.EndVertical();

        //GUILayout.EndArea();
        //GUILayout.BeginArea(rectAreaColumn2);

        //Opening
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Opening");
        foreach (AGUtteranceData utterance in m_uttDataOpeningReponses)
        {
            if (GUILayout.Button(utterance.gameObject.name))
            {
                NetworkRelay.SendNetworkMessage("playcutscene " + utterance.gameObject.name);
            }
        }
        GUILayout.EndVertical();

        //Engagement
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Engagement");
        foreach (AGUtteranceData utterance in m_uttDataEngagementReponses)
        {
            if (GUILayout.Button(utterance.gameObject.name))
            {
                NetworkRelay.SendNetworkMessage("playcutscene " + utterance.gameObject.name);
            }
        }
        GUILayout.EndVertical();

        //Ackowledgements
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Acknowledgements");
        foreach (AGUtteranceData utterance in m_uttDataAcknowledgementReponses)
        {
            if (GUILayout.Button(utterance.gameObject.name))
            {
                NetworkRelay.SendNetworkMessage("playcutscene " + utterance.gameObject.name);
            }
        }
        GUILayout.EndVertical();

        GUILayout.EndArea();
        GUILayout.BeginArea(rectAreaColumn1);
        //Elaborations
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Elaborations");
        foreach (AGUtteranceData utterance in m_uttDataElaborationReponses)
        {
            if (GUILayout.Button(utterance.gameObject.name))
            {
                NetworkRelay.SendNetworkMessage("playcutscene " + utterance.gameObject.name);
            }
        }
        GUILayout.EndVertical();

        //Distrations
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Distractions");
        foreach (AGUtteranceData utterance in m_uttDataDistrationReponses)
        {
            if (GUILayout.Button(utterance.gameObject.name))
            {
                NetworkRelay.SendNetworkMessage("playcutscene " + utterance.gameObject.name);
            }
        }
        GUILayout.EndVertical();

        //Answer
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Answer");
        foreach (AGUtteranceData utterance in m_uttDataAnswerReponses)
        {
            if (GUILayout.Button(utterance.gameObject.name))
            {
                NetworkRelay.SendNetworkMessage("playcutscene " + utterance.gameObject.name);
            }
        }
        GUILayout.EndVertical();

        //BuyTime
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("BuyTime");
        foreach (AGUtteranceData utterance in m_uttDataBuyTimeReponses)
        {
            if (GUILayout.Button(utterance.gameObject.name))
            {
                NetworkRelay.SendNetworkMessage("playcutscene " + utterance.gameObject.name);
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
        GUILayout.BeginArea(rectAreaColumn1);

        ////This section can set up interactive filtering
        //GUILayout.Label("Character");
        //GUILayout.BeginHorizontal();
        //for (int i = 0; i < VitaGlobals.m_vitaCharacters.Length; i++)
        //{
        //    if (GUILayout.Toggle((m_debugSoundsCharacterSelection == i), VitaGlobals.m_vitaCharacters[i], GUILayout.Width(50)))
        //    {
        //        m_debugSoundsCharacterSelection = i;
        //        optionsUpdated = true;
        //    }
        //}
        //GUILayout.EndHorizontal();
        //GUILayout.Space(5);
        //GUILayout.Label("Scenario");
        //GUILayout.BeginHorizontal();
        //for (int i = 0; i < VitaGlobals.m_vitaMoods.Length; i++)
        //{
        //    if (GUILayout.Toggle((m_debugSoundsScenarioSelection == i), VitaGlobals.m_vitaMoods[i], GUILayout.Width(50)))
        //    {
        //        m_debugSoundsScenarioSelection = i;
        //        optionsUpdated = true;
        //    }
        //}
        //GUILayout.EndHorizontal();
        //GUILayout.Space(5);
        //GUILayout.Label("Disposition(s)");
        //GUILayout.BeginHorizontal();
        //for (int i = 0; i < VitaGlobals.m_vitaMoods.Length; i++)
        //{
        //    if (GUILayout.Toggle(m_debugSoundsDisposition[i], VitaGlobals.m_vitaMoods[i], GUILayout.Width(50)))
        //    {
        //        m_debugSoundsDisposition[i] = true;
        //        optionsUpdated = true;
        //    }
        //    else
        //    {
        //        m_debugSoundsDisposition[i] = false;
        //        optionsUpdated = true;
        //    }
        //}
        //GUILayout.EndHorizontal();
        //GUILayout.Space(5);
        //GUILayout.Label("Response Type(s)");
        //GUILayout.BeginHorizontal();
        //for (int i = 0; i < VitaGlobals.m_vitaResponseTypes.Length; i++)
        //{
        //    if (GUILayout.Toggle(m_debugSoundsResponseType[i], VitaGlobals.m_vitaResponseTypes[i], GUILayout.Width(50)))
        //    {
        //        m_debugSoundsResponseType[i] = true;
        //        optionsUpdated = true;
        //    }
        //    else
        //    {
        //        m_debugSoundsResponseType[i] = false;
        //        optionsUpdated = true;
        //    }
        //}
        //GUILayout.EndHorizontal();
        //GUILayout.Space(5);
    }
}
