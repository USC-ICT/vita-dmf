using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/*
    Created 2/25/2011 - Adam Reilly
    This class is designed to be the main entry/exit point of a Unity Project.
*/

/*
 * Unity Execution order, taken from http://unity3d.com/support/documentation/Manual/Execution%20Order.html
 *
 * -=Initialization=-
 * Awake() - once
 * OnEnable() - once, only called if the gameobject is active, just after the object is enabled
 * Start() - once
 *
 * -=Update=-
 * FixedUpdate() - every frame sometimes multiple times per frame. Called more frequently than update. Do all physics calculations here
 * Update() - every frame, once per frame
 * LateUpdate() - every frame, once per frame. All calculations in Update are completed before LateUpdate is called
 *
 * -=Coroutines=-
 * Executed after all Update functions. If you start a coroutine in LateUpdate, it will be called after LateUpdate, just before rendering
 *
 * -=Combined Execution order=-
 * So in conclusion, this is the execution order for any given script:

    * All Awake calls
    * All Start Calls
    * while (stepping towards variable delta time)
          o All FixedUpdate functions
          o Physics simulation
          o OnEnter/Exit/Stay trigger functions
          o OnEnter/Exit/Stay collision functions
    * Rigidbody interpolation applies transform.position and rotation
    * OnMouseDown/OnMouseUp etc. events
    * All Update functions
    * Animations are advanced, blended and applied to transform
    * All LateUpdate functions
    * Rendering
*/


public class VHMain : MonoBehaviour
{
    #region Constants
    protected const string VhMsgCommand = "vhmsg";
    protected const string ToggleFullScreenCommand = "toggle_fullscreen";
    protected const string SetResolutionCommand = "set_resolution";
    protected const string CutsceneCommand = "cutscene";
    #endregion

    #region Variables
    public DebugConsole m_Console;
    public string m_configIniFilename = "";
    public string m_procId;

    protected IniParser m_ConfigFile;  // used for reading config files
    protected int m_debugTextLineNumber = 0;  // reset this at the beginning of every OnGUI()

    protected bool m_displaySubtitles = false;
    protected bool m_displayUserDialogs = false;
    protected string m_subtitleText = "";
    protected string m_userDialogText = "";
    #endregion

    public bool DisplaySubtitles
    {
        get { return m_displaySubtitles; }
        set { m_displaySubtitles = value; }
    }

    public bool DisplayUserDialog
    {
        get { return m_displayUserDialogs; }
        set { m_displayUserDialogs = value; }
    }

    #region Unity Messages
    public virtual void Awake()
    {
        // this initializes the www system and prevents a delay on the loading of the first file
        new WWW("file://");
        Debug.Log("Unity Version: " + Application.unityVersion);
        Debug.Log("Platform: " + Application.platform);
        if (!VHUtils.IsWebGL())
        {
            Debug.Log(string.Format("Application.streamingAssetsPath - '{0}'", Application.streamingAssetsPath));
            Debug.Log(string.Format("Directory.GetCurrentDirectory() - '{0}'", Directory.GetCurrentDirectory()));
            Debug.Log(string.Format("Application.dataPath - '{0}'", Application.dataPath));
            Debug.Log(string.Format("Application.persistantDataPath - '{0}'", Application.persistentDataPath));
            Debug.Log(string.Format("Path.GetFullPath('.') - '{0}'", Path.GetFullPath(".")));
        }

        Application.runInBackground = true;
    }

    public virtual void Start()
    {
        DebugConsole console = DebugConsole.Get();
        if (console != null)
        {
            console.AddCommandCallback(VhMsgCommand, new DebugConsole.ConsoleCallback(HandleConsoleMessage));
            console.AddCommandCallback(ToggleFullScreenCommand, new DebugConsole.ConsoleCallback(HandleConsoleMessage));
            console.AddCommandCallback(SetResolutionCommand, new DebugConsole.ConsoleCallback(HandleConsoleMessage));
            console.AddCommandCallback(CutsceneCommand, new DebugConsole.ConsoleCallback(HandleConsoleMessage));
        }

        LoadConfigFile();

        VHMsgBase vhmsg = VHMsgBase.Get();
        if (vhmsg != null)
        {
            vhmsg.SubscribeMessage("renderer");
            vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageHandler));
        }

        if (VHUtils.HasCommandLineArgument(CutsceneCommand))
        {
            PlayCutscene(VHUtils.GetCommandLineArgumentValue(CutsceneCommand), true);
        }
    }

    public virtual void OnGUI()
    {
        if (DisplaySubtitles && !string.IsNullOrEmpty(m_subtitleText))
        {
            // subtitle text
            Rect subtitleTextPos = new Rect(0.5f, 0.8f, 0.8f, 0.25f);
            subtitleTextPos.x -= subtitleTextPos.width / 2;
            Rect subtitleTextBackdropPos = new Rect(subtitleTextPos);
            subtitleTextBackdropPos.x += 0.001f;
            subtitleTextBackdropPos.y -= 0.001f;

            var labelStyle = GUI.skin.GetStyle("Label");
            var previousAlignment = labelStyle.alignment;
            labelStyle.alignment = TextAnchor.UpperCenter;

            GUI.color = Color.black;
            VHGUI.Label(subtitleTextBackdropPos, m_subtitleText, labelStyle);
            GUI.color = Color.white;
            VHGUI.Label(subtitleTextPos, m_subtitleText, labelStyle, Color.yellow);

            labelStyle.alignment = previousAlignment;
        }

        if (DisplayUserDialog && !string.IsNullOrEmpty(m_userDialogText))
        {
            // user dialog text
            Rect userDialogTextPos = new Rect(0.5f, 0.2f, 0.8f, 0.25f);
            userDialogTextPos.x -= userDialogTextPos.width / 2;
            Rect m_UserDialogTextBackdropPos = new Rect(userDialogTextPos.x * Screen.width, userDialogTextPos.y * Screen.height, userDialogTextPos.width * Screen.width, userDialogTextPos.height * Screen.height);
            m_UserDialogTextBackdropPos.x += 1f;
            m_UserDialogTextBackdropPos.y -= 1f;

            GUI.color = Color.black;
            GUI.Label(m_UserDialogTextBackdropPos, m_userDialogText);
            GUI.color = Color.white;
            VHGUI.Label(userDialogTextPos, m_userDialogText, Color.white);
        }
    }

    public virtual void OnApplicationQuit()
    {
    }

    virtual protected void LoadConfigFile()
    {
        string configFileName = VHUtils.GetCommandLineArgumentValue("config");
        if (!String.IsNullOrEmpty(configFileName))
        {
            m_configIniFilename = configFileName;
        }

        if (!String.IsNullOrEmpty(m_configIniFilename))
        {
            configFileName = "Config/" + m_configIniFilename;
            m_ConfigFile = new IniParser(configFileName);
        }
    }

    #endregion


    #region Main Functionality

    protected virtual void HandleConsoleMessage(string commandEntered, DebugConsole console)
    {
        Vector2 vec2Data = Vector2.zero;
        string strData = string.Empty;
        if (commandEntered.IndexOf(ToggleFullScreenCommand) != -1)
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
        else if (commandEntered.IndexOf(SetResolutionCommand) != -1)
        {
            if (console.ParseVector2(commandEntered, ref vec2Data))
            {
                Screen.SetResolution((int)vec2Data.x, (int)vec2Data.y, Screen.fullScreen);
            }
            else
            {
                Debug.LogError("set_resolution requires a width and hieght parameter. Example: set_resolution 1024 768");
            }
        }
        else if (commandEntered.IndexOf(CutsceneCommand) != -1)
        {
            console.ParseSingleParameter(commandEntered, ref strData, CutsceneCommand.Length);
            PlayCutscene(strData, false);
        }
    }

    protected virtual void VHMsg_MessageHandler(object sender, VHMsgBase.Message message)
    {
        string [] splitargs = message.s.Split(" ".ToCharArray());

        if (splitargs.Length > 0)
        {
            if (splitargs[0] == "renderer")
            {
                string procid = "";
                if (splitargs.Length > 2)
                {
                    if (splitargs[1] == "id")
                    {
                        procid = splitargs[2];
                    }
                }

                if (!string.IsNullOrEmpty(m_procId) &&
                    !string.IsNullOrEmpty(procid) &&
                    procid != m_procId)
                {
                    return;
                }

                // remove the id from the message.  terribly inefficient, but I don't want to mess up the parsing code below
                if (!string.IsNullOrEmpty(procid))
                {
                    List<string> removeId = new List<string>(splitargs);
                    removeId.RemoveAt(1);
                    removeId.RemoveAt(1);
                    splitargs = removeId.ToArray();
                }

                if (splitargs.Length > 2 && splitargs[1] == "time")
                {
                    // 'renderer [id procid] time 1.0'

                    float timeScale;
                    if (float.TryParse(splitargs[2], out timeScale))
                    {
                         Time.timeScale = timeScale;
                    }
                }
                else if (splitargs.Length > 3 && splitargs[1] == "create")
                {
                    // 'renderer [id procid] create pawnName SmartbodyPawn'

                    string name = splitargs[2];
                    string objectType = splitargs[3];
                    UnityEngine.Object obj = Instantiate(Resources.Load(objectType), Vector3.zero, Quaternion.identity);
                    obj.name = name;

                    Debug.Log(string.Format("Object {0} ({1}) created!", name, objectType));
                }
                else if (splitargs.Length > 2 && splitargs[1] == "destroy")
                {
                    // 'renderer [id procid] destroy objectName'
                    DestroyGameObject(splitargs[2]);
                }
            }
            else if (splitargs[0] == "vrSpeech")
            {
                // vrSpeech interp user<somenumber> 1 1.0 normal <TEXT>
                // vrSpeech interp user0004 1 1.0 normal What's your name?

                // user is speaking into the mic
                if (splitargs.Length >= 2 && splitargs[1] == "interp")
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append('\"');
                    for (int i = 6; i < splitargs.Length; i++)
                    {
                        stringBuilder.Append(splitargs[i]);
                        stringBuilder.Append(' ');
                    }
                    stringBuilder.Insert(stringBuilder.Length - 1, '\"');
                    m_userDialogText = stringBuilder.ToString();
                }
            }
            else if (splitargs[0] == "vrSpeak" ||
                     splitargs[0] == "vrExpress")
            {
                /*
                vrExpress ChrBrad user 1346294984756-28-1 <?xml version="1.0" encoding="UTF-8" standalone="no" ?>
                <act>
                  <participant id="ChrBrad" role="actor" />
                  <fml>
                    <turn start="take" end="give" />
                    <affect type="neutral" target="addressee"></affect>
                    <culture type="neutral"></culture>
                    <personality type="neutral"></personality>
                  </fml>
                  <bml>
                    <speech id="sp1" ref="brad_self_firstbrad" type="application/ssml+xml">My first name is Brad.</speech>
                  </bml>
                </act>

                vrExpress ChrBrad user 1346294984756-28-1 <?xml version="1.0" encoding="UTF-8" standalone="no" ?> <act> <participant id="ChrBrad" role="actor" /> <fml> <turn start="take" end="give" /> <affect type="neutral" target="addressee"></affect> <culture type="neutral"></culture> <personality type="neutral"></personality> </fml> <bml> <speech id="sp1" ref="brad_self_firstbrad" type="application/ssml+xml">My first name is Brad.</speech> </bml> </act>
                */

                var args = VHMsgBase.SplitIntoOpArg(message.s);
                string subtitleText = ParseSpeechText(args.Value);
                if (!string.IsNullOrEmpty(subtitleText.Trim()))
                    m_subtitleText = subtitleText;
            }
            else if (splitargs[0] == "vrSpoke")
            {
                // vrSpoke ChrBrad user 1346294984756-28-1 My first name is Brad.
            }
        }
    }

    protected void DestroyGameObject(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Destroy(obj);
            Debug.Log(string.Format("Object '{0}' destroyed.", objectName));
        }
        else
        {
            Debug.Log(string.Format("Object '{0}' not found!", objectName));
        }
    }

    protected virtual void PlayCutscene(string cutsceneName, bool exitApplicationAfterwards)
    {
        Cutscene[] cutscenes = (Cutscene[])FindObjectsOfType(typeof(Cutscene));
        bool cutsceneFound = false;

        if (!string.IsNullOrEmpty(cutsceneName))
        {
            for (int i = 0; i < cutscenes.Length; i++)
            {
                if (cutsceneName == cutscenes[i].CutsceneName)
                {
                    cutscenes[i].Play();
                    if (exitApplicationAfterwards)
                    {
                        cutscenes[i].AddOnFinishedCutsceneCallback(FinishedStartupCutscene);
                    }
                    cutsceneFound = true;
                    break;
                }
            }
        }

        if (!cutsceneFound)
        {
            string cutscenNames = string.Empty;
            for (int i = 0; i < cutscenes.Length; i++)
            {
                cutscenNames += cutscenes[i].CutsceneName;
                cutscenNames += " ";
            }
            Debug.LogError(string.Format("Cutscene not found. Available cutscenes: {0}", cutscenNames));
        }
    }

    void FinishedStartupCutscene(Cutscene cutscene)
    {
        VHUtils.ApplicationQuit();
    }

    static string ParseSpeechText(string text)
    {
        int endOfSpeechIndex = text.IndexOf("</speech>");
        if (endOfSpeechIndex == -1)
        {
            // there is no speech text
            return "";
        }

        int startOfSpeechIndex = text.LastIndexOf('>', endOfSpeechIndex);
        if (startOfSpeechIndex == -1)
        {
            // broken xml tags
            return "";
        }

        return text.Substring(startOfSpeechIndex + 1, endOfSpeechIndex - startOfSpeechIndex - 1);
    }

    #endregion
}
