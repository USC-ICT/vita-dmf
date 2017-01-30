using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;

public class MainMenu : MonoBehaviour
{
    GameObject m_menu;
    DebugConsole m_debugConsole;
    DebugControllerMenu m_debugController;


    void Start()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;


        DebugInfo debugInfo = GameObject.FindObjectOfType<DebugInfo>();
        VitaGlobals.m_version = string.Format("{0}.{1}", VitaGlobals.m_versionPrefix, string.IsNullOrEmpty(debugInfo.m_unityCloudBuildManifest.scmCommitId) ? "0" : debugInfo.m_unityCloudBuildManifest.scmCommitId);
        VitaGlobals.m_versionText = string.Format("VITA - {0} - {1}", VitaGlobals.m_version, VitaGlobals.m_isServer ? "server" : "client");

        if (!VHUtils.IsEditor())
        {
            var hwnd = VitaGlobals.m_windowHwnd;
            if (hwnd != new IntPtr(0))
                WindowsAPI.WinAPI_SetWindowText(hwnd, VitaGlobals.m_versionText);
        }


        m_menu = GameObject.Find("Canvas");

        m_debugConsole = GameObject.FindObjectOfType<DebugConsole>();
        m_debugController = GameObject.FindObjectOfType<DebugControllerMenu>();


        NetworkRelay.RemoveAllMessageCallbacks();
        NetworkRelay.AddMessageCallback(HandleNetworkMessage);

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
        }

        if (!m_debugConsole.DrawConsole && !VitaGlobals.DoesInputHaveFocus() && m_debugController.m_showController)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                GameObject.FindObjectOfType<DebugInfo>().NextMode();
            }
        }
    }


    void OnGUI()
    {
    }


    IEnumerator StartMenus()
    {
        // we wait a bit for all Start()'s to run, then enable/disable the gameobject

        yield return new WaitForSeconds(0.1f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();


        m_menu.SetActive(VitaGlobals.m_isGUIScreen);
    }


    void VitaStartInterview()
    {
#if false
        PlayerPrefs.SetInt("vitaCurrentCharacter", m_vitaCurrentCharacter);
        PlayerPrefs.SetInt("vitaCurrentBackground", m_vitaCurrentBackground);
        PlayerPrefs.SetInt("vitaCurrentMood", m_vitaCurrentMood);
        PlayerPrefs.SetString("vitaCurrentSubjectNumber", m_vitaSubjectNumber);
        PlayerPrefs.SetInt("vitaRecordMovieFlag", m_vitaRecordMovieFlag ? 1 : 0);
        PlayerPrefs.SetInt("vitaManualResponseMode", m_vitaManualResponseMode ? 1 : 0);
        PlayerPrefs.SetInt("vitaLaptopModeFlag", m_vitaLaptopModeFlag ? 1 : 0);
        PlayerPrefs.SetInt("vitaLinearMode", m_vitaLinearMode ? 1 : 0);

        string level = VitaGlobals.m_vitaBackgroundsSceneNames[m_vitaCurrentBackground];
        VHUtils.SceneManagerLoadScene(level);
#endif
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
            VitaGlobals.m_isGUIScreen = !VitaGlobals.m_isGUIScreen;

            m_menu.SetActive(VitaGlobals.m_isGUIScreen);
        }
        else if (opcode == "startinterview")
        {
            string environment = splitargs[1];
            string character = splitargs[2];
            string disposition = splitargs[3];
            string student = splitargs[4];
            student = student.Replace("~"," ");
            VitaGlobals.InterviewType interviewType = (VitaGlobals.InterviewType)Convert.ToInt32(splitargs[5]);

            PlayerPrefs.SetInt("vitaCurrentCharacter",  VitaGlobals.m_vitaCharacterInfo.FindIndex(c => c.prefab == character));
            PlayerPrefs.SetInt("vitaCurrentBackground", VitaGlobals.m_vitaBackgroundInfo.FindIndex(b => b.sceneName == environment));
            PlayerPrefs.SetInt("vitaCurrentMood", Array.IndexOf<string>(VitaGlobals.m_vitaMoods, disposition));

            VitaGlobals.m_interviewStudent = student;
            VitaGlobals.m_interviewType = interviewType;

            VHUtils.SceneManagerLoadScene(environment);
        }
    }
}
