using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Startup : MonoBehaviour
{
    //string m_x = "0";
    //string m_y = "0";

    GUIStyle m_guiLabel;


    void Awake()
    {
    }

    void Start()
    {
        GetHWND();

        StartCoroutine(StartupProcedure());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            VHUtils.ApplicationQuit();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            GameObject.FindObjectOfType<DebugInfo>().NextMode();
        }
    }

    void OnGUI()
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

        GUILayout.Label(string.Format("Loading..."), m_guiLabel);

        GUILayout.Space(10);

        GUILayout.Label(string.Format("isServer: {0}", VitaGlobals.m_isServer), m_guiLabel);
        GUILayout.Label(string.Format("isGUI: {0}", VitaGlobals.m_isGUIScreen), m_guiLabel);
        GUILayout.Label(string.Format("runMode: {0}", VitaGlobals.m_runMode), m_guiLabel);
        GUILayout.Label(string.Format("Connected: {0}", NetworkRelay.ConnectionEstablished), m_guiLabel);

        GUILayout.Space(10);

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

        GUILayout.Space(10);

        GUILayout.Label(string.Format("NetworkServer.active: {0}", UnityEngine.Networking.NetworkServer.active), m_guiLabel);
        GUILayout.Label(string.Format("NetworkServer.connections: {0}", UnityEngine.Networking.NetworkServer.connections.Count), m_guiLabel);
        GUILayout.Label(string.Format("NetworkServer.localConnections: {0}", UnityEngine.Networking.NetworkServer.localConnections.Count), m_guiLabel);
        GUILayout.Label(string.Format("NetworkServer.localClientActive: {0}", UnityEngine.Networking.NetworkServer.localClientActive), m_guiLabel);

        GUILayout.Space(10);

#if false
        if (NetworkRelay.m_client != null)
        {
            GUILayout.Label(string.Format("NetworkClient.isConnected: {0}", NetworkRelay.m_client.isConnected));
            GUILayout.Label(string.Format("NetworkClient.serverIp: {0}", NetworkRelay.m_client.serverIp));
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Connect Server"))
        {
            NetworkRelay.ConnectServer();
        }

        if (GUILayout.Button("Connect Client"))
        {
            NetworkRelay.ConnectClient();
        }

        if (GUILayout.Button("Disconnect"))
        {
            NetworkRelay.Disconnect();
        }

        if (GUILayout.Button("Send Ack"))
        {
            NetworkRelay.SendNetworkMessage(string.Format("ack {0}", VitaGlobals.m_isServer ? "server" : "client"));
        }

        if (GUILayout.Button("Next Scene"))
        {
            VHUtils.SceneManagerLoadScene("ConfigurationNew");
        }
#endif

        if (GUILayout.Button("Quit"))
        {
            NetworkRelay.SendNetworkMessage("quit");
        }

        GUILayout.Space(10);

#if false
        m_x = GUILayout.TextField(m_x);
        m_y = GUILayout.TextField(m_y);
        if (GUILayout.Button("SetWindowPos"))
        {
            int x = Convert.ToInt32(m_x);
            int y = Convert.ToInt32(m_y);
            var hwnd = VitaGlobals.m_windowHwnd;
            WindowsAPI.WinAPI_SetWindowPos(hwnd, (IntPtr)WindowsAPI.SpecialWindowHandles.HWND_TOP, x, y, 0, 0, WindowsAPI.SetWindowPosFlags.SWP_SHOWWINDOW | WindowsAPI.SetWindowPosFlags.SWP_NOSIZE);
        }
#endif

        GUILayout.EndVertical();

        GUILayout.EndArea();
    }


    void GetHWND()
    {
        VitaGlobals.m_windowHwnd = new IntPtr(0);

        if (VHUtils.IsEditor())
            return;

        if (VHUtils.IsAndroid() || VHUtils.IsIOS() || VHUtils.IsWebGL())
            return;


        // tried both of these, but they are inconsistent, especially with multiple processes starting at the same time (for multi-window)
        //VitaGlobals.m_windowHwnd = WindowsAPI.WinAPI_GetForegroundWindow();
        //VitaGlobals.m_windowHwnd = WindowsAPI.WinAPI_GetActiveWindow();


        // the below was taken from here.  Seems to be more consistent than the other methods
        // http://matt.benic.us/post/88468666204/using-win32-api-to-get-specific-window-instance-in

        const string UnityWindowClassName = "UnityWndClass";

        uint threadId = WindowsAPI.WinAPI_GetCurrentThreadId();

        WindowsAPI.WinAPI_EnumThreadWindows(threadId, (hWnd, lParam) =>
        {
            string classText;
            WindowsAPI.WinAPI_GetClassName(hWnd, out classText);
            if (classText == UnityWindowClassName)
            {
                VitaGlobals.m_windowHwnd = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);
    }


    IEnumerator StartupProcedure()
    {
        // assumptions:
        // if server, start gui, place on left
        // if client, start black screen, for VH, place on right

        // if server, open connection
        // if client, look for server on coroutine.  when server starts up, connect

        // switch to windowed mode
        // get number of monitors
        // if monitors == 1
        //     get full desktop size (screen.currentresolution)
        //     if gui, place on left side, resize to take up half screen, keep in windowed mode
        //     if vh, place on right side, resize to take up half screen, keep in windowed mode
        // else
        //     get full desktop size
        //     get monitor #1 rect within virtual desktop
        //     get monitor #2 rect within virtual desktop
        //     if gui, place on monitor #1, resize to monitor #1 native, put in fullscreen
        //     if vh, place on monitor #2, resize to monitor #2 native, put in fullscreen

        VitaGlobals.m_runMode = VitaGlobals.RunMode.TwoMonitors;
        VitaGlobals.m_isServer = true;
        VitaGlobals.m_isGUIScreen = true;

        if (VHUtils.HasCommandLineArgument("vitaclient"))
        {
            VitaGlobals.m_isServer = false;
            VitaGlobals.m_isGUIScreen = false;
        }

        if (VHUtils.HasCommandLineArgument("vitasplitscreen"))
        {
            VitaGlobals.m_runMode = VitaGlobals.RunMode.SplitScreen;
        }

        if (VHUtils.HasCommandLineArgument("vitasinglescreen"))
        {
            VitaGlobals.m_runMode = VitaGlobals.RunMode.SingleScreen;
        }

        Debug.LogFormat("isServer: {0} - isGUI: {1} - runMode: {2}", VitaGlobals.m_isServer, VitaGlobals.m_isGUIScreen, VitaGlobals.m_runMode);


        yield return StartCoroutine(StartupMonitors());
        yield return StartCoroutine(StartupNetwork());

        NetworkRelay.RemoveAllMessageCallbacks();
        NetworkRelay.AddMessageCallback(HandleNetworkMessage);


        string sceneName = "ConfigurationNew";

        Debug.LogFormat("StartupProcedure() - loading {0}", sceneName);

        VHUtils.SceneManagerLoadScene(sceneName);
    }

    IEnumerator StartupMonitors()
    {
        if (VHUtils.IsAndroid() || VHUtils.IsIOS() || VHUtils.IsWebGL())
        {
            // early out for other platforms
            VitaGlobals.m_runMode = VitaGlobals.RunMode.SingleScreen;
            yield break;
        }

        Screen.SetResolution(800, 600, false);

        //yield return new WaitForSeconds(0.3f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();


        VitaGlobals.SM_CMONITORS = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CMONITORS);
        VitaGlobals.SM_XVIRTUALSCREEN = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_XVIRTUALSCREEN);
        VitaGlobals.SM_YVIRTUALSCREEN = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_YVIRTUALSCREEN);
        VitaGlobals.SM_CXVIRTUALSCREEN = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CXVIRTUALSCREEN);
        VitaGlobals.SM_CYVIRTUALSCREEN = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CYVIRTUALSCREEN);

        int numMonitors = VitaGlobals.SM_CMONITORS;

        //Debug.LogFormat("number of monitors: {0}", numMonitors);
        //Debug.LogFormat("Screen.currentResolution: {0}", Screen.currentResolution);
        //Debug.LogFormat("Sreen.dpi: {0}", Screen.dpi);

#if false
        // getting dpi reported by app.  Windows will lie to you if the app is not high-dpi aware
        {
            var dc = WindowsAPI.WinAPI_GetDC(WindowsAPI.WinAPI_GetActiveWindow());
            int logpixelsy = WindowsAPI.WinAPI_GetDeviceCaps(dc, (int)WindowsAPI.DeviceCap.LOGPIXELSY);
            Debug.LogFormat("LOGPIXELSY: {0}", logpixelsy);
            WindowsAPI.WinAPI_ReleaseDC(WindowsAPI.WinAPI_GetActiveWindow(), dc);
        }
#endif

#if false
        // getting monitor info
        {
            int x = 0;
            int y = 0;
            var hwndMonitor1 = WindowsAPI.WinAPI_MonitorFromPoint(new WindowsAPI.POINT(x, y), WindowsAPI.MonitorOptions.MONITOR_DEFAULTTONEAREST);
            WindowsAPI.MonitorInfoEx monitor1Info = new WindowsAPI.MonitorInfoEx();
            monitor1Info.Init();
            WindowsAPI.WinAPI_GetMonitorInfo(hwndMonitor1, ref monitor1Info);
            Debug.Log(monitor1Info.Monitor);
        }
#endif

        if (VitaGlobals.m_runMode == VitaGlobals.RunMode.SingleScreen)
        {
            // early out for student mode (ie, single process)
            Resolution native = Screen.resolutions[Screen.resolutions.Length - 1];
            Screen.SetResolution(native.width, native.height, true);
            yield break;
        }

        if (VitaGlobals.m_runMode == VitaGlobals.RunMode.SplitScreen)
        {
            numMonitors = 1;
        }

        if (numMonitors == 1)
        {
            // use the single monitor, and split the screen in half vertically.  Put each process on each side of the screen.

            Resolution currentResolution = Screen.currentResolution;

            int x = 0;
            int y = 0;
            int width = currentResolution.width / 2;
            int height = currentResolution.height;

            if (!VitaGlobals.m_isGUIScreen)
            {
                x = currentResolution.width / 2;
                y = 0;
                width = currentResolution.width / 2;
                height = currentResolution.height;
            }

            //Debug.LogFormat("sm_x: {0}", WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_XVIRTUALSCREEN));
            //Debug.LogFormat("sm_y: {0}", WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_YVIRTUALSCREEN));
            //Debug.LogFormat("virtual: {0} {1} {2} {3}", x, y, width, height);

            //Debug.LogFormat("SetWindowPos() - {0} {1}", x, y);

            if (!VHUtils.IsEditor())
            {
                var hwnd = VitaGlobals.m_windowHwnd;
                WindowsAPI.WinAPI_SetWindowPos(hwnd, (IntPtr)WindowsAPI.SpecialWindowHandles.HWND_TOP, x, y, 0, 0, WindowsAPI.SetWindowPosFlags.SWP_SHOWWINDOW | WindowsAPI.SetWindowPosFlags.SWP_NOSIZE);
            }

            //yield return new WaitForSeconds(0.3f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Debug.LogFormat("SetResolution() - {0} {1}", width, height);

            Screen.SetResolution(width, height, false);

            //yield return new WaitForSeconds(0.3f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Debug.LogFormat("SetWindowPos() - {0} {1}", x, y);

            if (!VHUtils.IsEditor())
            {
                var hwnd = VitaGlobals.m_windowHwnd;
                WindowsAPI.WinAPI_SetWindowPos(hwnd, (IntPtr)WindowsAPI.SpecialWindowHandles.HWND_TOP, x, y, 0, 0, WindowsAPI.SetWindowPosFlags.SWP_SHOWWINDOW | WindowsAPI.SetWindowPosFlags.SWP_NOSIZE);
            }

            //yield return new WaitForSeconds(0.3f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
        else
        {
            // put each window 'fullscreen' on each monitor.  This is a fake fullscreen, with the window maximized with no window border
            // we do this because DirectX fullscreen can't be done with 2 processes running simultaneously

#if false
            int x = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_XVIRTUALSCREEN);
            int y = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_YVIRTUALSCREEN);
            int width = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CXVIRTUALSCREEN);
            int height = WindowsAPI.WinAPI_GetSystemMetrics(WindowsAPI.SystemMetric.SM_CYVIRTUALSCREEN);

            Debug.LogFormat("virtual: {0} {1} {2} {3}", x, y, width, height);
#endif

#if false
            // find two different monitor hwnds by sampling pixels that we think are on different screens.

            var hwndMonitor1 = WindowsAPI.WinAPI_MonitorFromPoint(new WindowsAPI.POINT(x, y), WindowsAPI.MonitorOptions.MONITOR_DEFAULTTONEAREST);

            WindowsAPI.MonitorInfoEx monitor1Info = new WindowsAPI.MonitorInfoEx();
            monitor1Info.Init();
            WindowsAPI.WinAPI_GetMonitorInfo(hwndMonitor1, ref monitor1Info);

            Debug.Log(monitor1Info.Monitor);


            var hwndMonitor2 = WindowsAPI.WinAPI_MonitorFromPoint(new WindowsAPI.POINT(x + width, y + height), WindowsAPI.MonitorOptions.MONITOR_DEFAULTTONEAREST);

            WindowsAPI.MonitorInfoEx monitor2Info = new WindowsAPI.MonitorInfoEx();
            monitor2Info.Init();
            WindowsAPI.WinAPI_GetMonitorInfo(hwndMonitor2, ref monitor2Info);

            Debug.Log(monitor2Info.Monitor);
#endif

            VitaGlobals.DEVMODES = new List<WindowsAPI.DEVMODE>();

            {
                uint deviceId = 0;
                while (true)
                {
                    WindowsAPI.DISPLAY_DEVICE displayDevice = new WindowsAPI.DISPLAY_DEVICE();
                    displayDevice.Init();
                    bool retDevice = WindowsAPI.WinAPI_EnumDisplayDevices(null, deviceId, ref displayDevice, 0);
                    if (!retDevice)
                        break;

                    //Debug.LogFormat("{0}, {1}, {2}, {3}, {4}, {5}", deviceId, displayDevice.DeviceName, displayDevice.DeviceString, displayDevice.StateFlags, displayDevice.DeviceID, displayDevice.DeviceKey );

                    deviceId++;


                    WindowsAPI.DEVMODE devmode = new WindowsAPI.DEVMODE();
                    devmode.Init();
                    bool retSettings = WindowsAPI.WinAPI_EnumDisplaySettings(displayDevice.DeviceName, WindowsAPI.ENUM_CURRENT_SETTINGS, ref devmode);
                    if (!retSettings)
                        continue;

                    //Debug.LogFormat(devmode.dmDeviceName);
                    //Debug.LogFormat(devmode.dmPelsWidth.ToString());
                    //Debug.LogFormat(devmode.dmPelsHeight.ToString());
                    //Debug.LogFormat(devmode.dmBitsPerPel.ToString());
                    //Debug.LogFormat(devmode.dmPosition.x.ToString());
                    //Debug.LogFormat(devmode.dmPosition.y.ToString());
                    //Debug.LogFormat("-----------");

                    VitaGlobals.DEVMODES.Add(devmode);
                }
            }

            if (VitaGlobals.DEVMODES.Count < 2)
            {
                Debug.LogErrorFormat("Detected multiple monitors, but failed to find device info for two or more devices");
                yield break;
            }

            VitaGlobals.m_currentDEVMODE = 0;
            if (!VitaGlobals.m_isGUIScreen)
            {
                VitaGlobals.m_currentDEVMODE = 1;
            }

            int monitorX = VitaGlobals.DEVMODES[VitaGlobals.m_currentDEVMODE].dmPosition.x;
            int monitorY = VitaGlobals.DEVMODES[VitaGlobals.m_currentDEVMODE].dmPosition.y;
            //int monitorWidth = devModes[devModeNum].dmPelsWidth;
            //int monitorHeight = devModes[devModeNum].dmPelsHeight;

            //Debug.LogFormat("monitorX: {0}, monitorY: {1}, monitorWidth: {2}, monitorHeight: {3}", monitorX, monitorY, monitorWidth, monitorHeight);

            // remove the window border
            if (!VHUtils.IsEditor())
            {
                //     If you have changed certain window data using SetWindowLong, you must call SetWindowPos for the changes to
                //     take effect. Use the following combination for uFlags: SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |

                var hwnd = VitaGlobals.m_windowHwnd;

                Int64 style = (Int64)WindowsAPI.WinAPI_GetWindowLongPtr(hwnd, WindowsAPI.WindowLongFlags.GWL_STYLE);
                style &= (Int64)~(WindowsAPI.WindowStyles.WS_CAPTION | WindowsAPI.WindowStyles.WS_THICKFRAME | WindowsAPI.WindowStyles.WS_MINIMIZE | WindowsAPI.WindowStyles.WS_MAXIMIZE | WindowsAPI.WindowStyles.WS_SYSMENU);
                WindowsAPI.WinAPI_SetWindowLongPtr(hwnd, WindowsAPI.WindowLongFlags.GWL_STYLE, new IntPtr(style));

                Int64 exStyle = (Int64)WindowsAPI.WinAPI_GetWindowLongPtr(hwnd, WindowsAPI.WindowLongFlags.GWL_EXSTYLE);
                exStyle &= (Int64)~(WindowsAPI.WindowStyles.WS_EX_DLGMODALFRAME | WindowsAPI.WindowStyles.WS_EX_CLIENTEDGE | WindowsAPI.WindowStyles.WS_EX_STATICEDGE);
                WindowsAPI.WinAPI_SetWindowLongPtr(hwnd, WindowsAPI.WindowLongFlags.GWL_EXSTYLE, new IntPtr(exStyle));

                // we call SetWindowPos() below, so this isn't needed
                //SetWindowPos(hwnd, NULL, 0,0,0,0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOOWNERZORDER);
            }

            //yield return new WaitForSeconds(0.3f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (!VHUtils.IsEditor())
            {
                var hwnd = VitaGlobals.m_windowHwnd;
                WindowsAPI.WinAPI_SetWindowPos(hwnd, (IntPtr)WindowsAPI.SpecialWindowHandles.HWND_TOP, monitorX, monitorY, 0, 0, WindowsAPI.SetWindowPosFlags.SWP_SHOWWINDOW | WindowsAPI.SetWindowPosFlags.SWP_NOSIZE);
            }

            //yield return new WaitForSeconds(0.3f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Screen.SetResolution(monitor1Width, monitor1Height, true);

            if (!VHUtils.IsEditor())
            {
                var hwnd = VitaGlobals.m_windowHwnd;
                WindowsAPI.WinAPI_ShowWindow(hwnd, WindowsAPI.SW_MAXIMIZE);
            }

            //yield return new WaitForSeconds(0.3f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (!VHUtils.IsEditor())
            {
                // bring window to front
                // taken from http://stackoverflow.com/questions/916259/win32-bring-a-window-to-top

                var hwnd = VitaGlobals.m_windowHwnd;
                uint threadId = WindowsAPI.WinAPI_GetCurrentThreadId();
                uint dwCurID = WindowsAPI.WinAPI_GetWindowThreadProcessId(hwnd, IntPtr.Zero);
                WindowsAPI.WinAPI_AttachThreadInput(dwCurID, threadId, true);
                WindowsAPI.WinAPI_SetWindowPos(hwnd, (IntPtr)WindowsAPI.SpecialWindowHandles.HWND_TOPMOST, 0, 0, 0, 0, WindowsAPI.SetWindowPosFlags.SWP_NOSIZE | WindowsAPI.SetWindowPosFlags.SWP_NOMOVE);
                WindowsAPI.WinAPI_SetWindowPos(hwnd, (IntPtr)WindowsAPI.SpecialWindowHandles.HWND_NOTOPMOST, 0, 0, 0, 0, WindowsAPI.SetWindowPosFlags.SWP_NOSIZE | WindowsAPI.SetWindowPosFlags.SWP_NOMOVE);
                WindowsAPI.WinAPI_SetForegroundWindow(hwnd);
                WindowsAPI.WinAPI_AttachThreadInput(dwCurID, threadId, false);
                WindowsAPI.WinAPI_SetFocus(hwnd);
                WindowsAPI.WinAPI_SetActiveWindow(hwnd);
            }

            //yield return new WaitForSeconds(0.3f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator StartupNetwork()
    {
        if (VitaGlobals.m_isServer)
        {
            NetworkRelay.ConnectServer();

            float startTime = Time.time;
            while (Time.time < startTime + 30)
            {
                if (NetworkRelay.ConnectionEstablished)
                    break;

                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            float startTime = Time.time;
            while (Time.time < startTime + 30)
            {
                NetworkRelay.ConnectClient();
                yield return new WaitForSeconds(1.5f);

                if (NetworkRelay.ConnectionEstablished)
                    break;

                NetworkRelay.Disconnect();
                yield return new WaitForSeconds(1.5f);
            }
        }

        if (!NetworkRelay.ConnectionEstablished)
        {
            Debug.LogError("StartupNetwork() - Could not connect to server");
        }
    }

    void HandleNetworkMessage(object sender, string message)
    {
        string [] splitargs = message.Split(' ');
        string opcode = splitargs[0];

        Debug.LogFormat(opcode);

        if (opcode == "ack")
        {
            string acktype = splitargs[1];
            Debug.LogFormat("ACK received - {0}", acktype);
        }
        else if (opcode == "quit")
        {
            VHUtils.ApplicationQuit();
        }
    }
}
