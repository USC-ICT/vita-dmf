using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugInfo : MonoBehaviour
{
    [Serializable]
    public class UnityCloudBuildManifest
    {
        // https://build.cloud.unity3d.com/support/guides/manifest/

        // {"cloudBuildTargetName":"android","buildNumber":41,"scmCommitId":"7347","scmBranch":"/core/Monticello","buildStartTime":"3/15/2016 1:08:07 AM","projectId":"ictfromunityads32798/monticello","bundleId":"edu.usc.ict.monticello","unityVersion":"5.3.3f1"}

        public string cloudBuildTargetName;  // The name of the project build target that was built. Currently, this will correspond to the platform, as either "default-web�, �default-ios�, or �default-android".
        public int buildNumber;  // The Unity Cloud Build number corresponding to this build
        public string scmCommitId;          // Commit or changelist built by UCB
        public string scmBranch;  // Name of the branch that was built
        public string buildStartTime;  // The UTC timestamp when the build process was started
        public string projectId;  // The UCB project identifier
        public string bundleId;  // (iOS and Android only) The bundleIdentifier configured in Unity Cloud Build
        public string unityVersion;  // The version of Unity used by UCB to create the build
        public string xcodeVersion;  // (iOS only) The version of XCode used to build the project
    }


    #region Constants

    public enum InfoMode
    {
        Off,
        General,
        Build,
        System,

        NUM_MODES
    }

    enum TextType
    {
        // General
        Frame,
        Resolution,
        SceneName,
        QualitySetting,
        CameraPosition,
        CameraRotation,
        NUM_GENERAL_TYPES,

        // Build
        UnityVersion,
        Platform,
        StreamingAssetsPath,
        CurrentPath,
        DataPath,
        PersistentDataPath,
        FullPath,
        CloudProjectId,
        CloudBuildTargetName,
        BuildNumber,
        CommitId,
        BuildStartTime,
        xCodeVersion,
        NUM_BUILD_TYPES,

        // System
        OperatingSystem,
        Processor,
        Memory,
        GfxDeviceId,
        GfxVendorId,
        GfxVersion,
        GfxMemory,
        ShaderLevel,
        Shadows,
        DeviceUniqueId,
        DeviceName,
        DeviceModel,
        DeviceType,
        Username,
        IP,
        NUM_SYSTEM_TYPES
    }

    #endregion


    #region Variables
    public float m_TextScreenHeightPct = 0.03f;
    public FpsCounter m_fpsCounter;
    public Camera m_Camera;
    public KeyCode m_ToggleKey = KeyCode.None;
    public Vector2 m_ScreenPosition = Vector2.zero;  // in pixels
    public Color m_color = Color.white;
    public float m_lowFpsThreshold = 30;
    public string m_svnWorkingFolder = "";  // a relative path is expected here, or just "".  This will get expanded to Application.dataPath + m_svnWorkingFolder.
    public List<string> m_additionalLinesPersistent = new List<string>();
    public List<string> m_additionalLinesPerFrame = new List<string>();

    float m_CachedTextScreenHeightPct;
    Vector2 m_CachedScreenResolution = new Vector2();
    InfoMode m_mode = InfoMode.Off;
    [NonSerialized] public UnityCloudBuildManifest m_unityCloudBuildManifest;
    Image m_BackgroundImage;
    string m_localIp = "unknown";



    Dictionary<TextType, Text> m_Labels = new Dictionary<TextType, Text>();
    VerticalLayoutGroup[] m_Tabs = new VerticalLayoutGroup[(int)InfoMode.NUM_MODES];
    #endregion

    #region Properties
    VerticalLayoutGroup OffTab { get { return m_Tabs[(int)InfoMode.Off]; } }
    VerticalLayoutGroup GeneralTab { get { return m_Tabs[(int)InfoMode.General]; } }
    VerticalLayoutGroup BuildTab { get { return m_Tabs[(int)InfoMode.Build]; } }
    VerticalLayoutGroup SystemTab { get { return m_Tabs[(int)InfoMode.System]; } }
    #endregion


    #region Functions

    void Awake()
    {
        if (!VHUtils.IsWebGL())
        {
            // on some windows machines, an exception is thrown.  Unsure why.  For now, just catch it and move on, not important.
            try
            {
                foreach (var ip in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        m_localIp = ip.ToString();
                        break;
                    }
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Debug.LogWarningFormat("DebugInfo.Awake() - SocketException caught when trying to resolve ip address: {0}", e);
            }
        }

#pragma warning disable 219
        // this property has a noticeable delay (as of 5.3.6f1) on the first time it's called.  Seems to be cached internally on subsequent calls. Pre-compute it in Awake() so that it's not visible.
        string unused = SystemInfo.deviceUniqueIdentifier;
#pragma warning restore 219

        GetBuildInfo();
        Canvas canvas = uGuiUtils.CreateCanvas("DebugInfoCanvas", this.gameObject, 100);
        m_BackgroundImage = uGuiUtils.CreateImage("Background", canvas.transform);
        m_BackgroundImage.enabled = false;
        m_BackgroundImage.rectTransform.pivot = new Vector2(0.5f, 1);
        m_BackgroundImage.raycastTarget = false;

        //bg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        //bg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height * ScreenPct);
        m_BackgroundImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, m_ScreenPosition.x, Screen.width);
        m_BackgroundImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, m_ScreenPosition.y, Screen.height);
        //bg.rectTransform.anchorMin = new Vector2(0, 1);
        //bg.rectTransform.anchorMax = Vector2.one;
        uGuiUtils.StretchToParent(m_BackgroundImage.rectTransform);

        // Off Tab
        CreateTab(InfoMode.Off, m_BackgroundImage.transform);

        // General Tab
        CreateTab(InfoMode.General, m_BackgroundImage.transform);

        float generalHeight = m_TextScreenHeightPct;//m_TextScreenHeightPct / (float)(TextType.NUM_GENERAL_TYPES + m_additionalLinesPersistent.Count);
        CreateText(TextType.Frame, GeneralTab.transform, "", generalHeight);
        CreateText(TextType.Resolution, GeneralTab.transform, "", generalHeight);
        CreateText(TextType.SceneName, GeneralTab.transform, "", generalHeight);
        CreateText(TextType.QualitySetting, GeneralTab.transform, "", generalHeight);
        CreateText(TextType.CameraPosition, GeneralTab.transform, "", generalHeight);
        CreateText(TextType.CameraRotation, GeneralTab.transform, "", generalHeight);

        for (int i = 0; i < m_additionalLinesPersistent.Count; i++)
        {
            uGuiUtils.CreateLayoutText("AdditionalLine" + i.ToString(), GeneralTab.transform, m_additionalLinesPersistent[i], Color.white, 1, generalHeight);
        }

        // Build Tab
        CreateTab(InfoMode.Build, m_BackgroundImage.transform);

        float buildHeight = m_TextScreenHeightPct;//m_TextScreenHeightPct / (float)TextType.NUM_BUILD_TYPES;
        CreateText(TextType.UnityVersion, BuildTab.transform, string.Format("Unity Version: {0}", Application.unityVersion), buildHeight);
        CreateText(TextType.Platform, BuildTab.transform, string.Format("Platform: {0}", Application.platform), buildHeight);
        if (!VHUtils.IsWebGL())
        {
            CreateText(TextType.StreamingAssetsPath, BuildTab.transform, string.Format("App.streamingAssetsPath - '{0}'", Application.streamingAssetsPath), buildHeight);
            CreateText(TextType.CurrentPath, BuildTab.transform, string.Format("Dir.GetCurrentDirectory() - '{0}'", System.IO.Directory.GetCurrentDirectory()), buildHeight);
            CreateText(TextType.DataPath, BuildTab.transform, string.Format("App.dataPath - '{0}'", Application.dataPath), buildHeight);
            CreateText(TextType.PersistentDataPath, BuildTab.transform, string.Format("App.persistantDataPath - '{0}'", Application.persistentDataPath), buildHeight);
            CreateText(TextType.FullPath, BuildTab.transform, string.Format("Path.GetFullPath('.') - '{0}'", System.IO.Path.GetFullPath(".")), buildHeight);
        }

        if (!string.IsNullOrEmpty(Application.cloudProjectId))
        {
            CreateText(TextType.CloudProjectId, BuildTab.transform, string.Format("cloudProjectId: {0}", Application.cloudProjectId), buildHeight);
        }
        if (!string.IsNullOrEmpty(m_unityCloudBuildManifest.cloudBuildTargetName))
        {
            CreateText(TextType.CloudBuildTargetName, BuildTab.transform, string.Format("cloudBuildTargetName: {0}", m_unityCloudBuildManifest.cloudBuildTargetName), buildHeight);
        }
        if (m_unityCloudBuildManifest.buildNumber != 0)
        {
            CreateText(TextType.BuildNumber, BuildTab.transform, string.Format("buildNumber: {0}", m_unityCloudBuildManifest.buildNumber), buildHeight);
        }
        if (!string.IsNullOrEmpty(m_unityCloudBuildManifest.scmCommitId))
        {
            CreateText(TextType.CommitId, BuildTab.transform, string.Format("scmCommitId: {0}", m_unityCloudBuildManifest.scmCommitId), buildHeight);
        }
        if (!string.IsNullOrEmpty(m_unityCloudBuildManifest.buildStartTime))
        {
            CreateText(TextType.BuildStartTime, BuildTab.transform, string.Format("buildStartTime: {0}", m_unityCloudBuildManifest.buildStartTime), buildHeight);
        }
        if (!string.IsNullOrEmpty(m_unityCloudBuildManifest.xcodeVersion))
        {
            CreateText(TextType.xCodeVersion, BuildTab.transform, string.Format("xcodeVersion: {0}", m_unityCloudBuildManifest.xcodeVersion), buildHeight);
        }

        // System Tab
        CreateTab(InfoMode.System, m_BackgroundImage.transform);

        float systemHeight = m_TextScreenHeightPct;//m_TextScreenHeightPct / (float)TextType.NUM_SYSTEM_TYPES;
        CreateText(TextType.OperatingSystem, SystemTab.transform, string.Format("{0}", SystemInfo.operatingSystem), systemHeight);
        CreateText(TextType.Processor, SystemTab.transform, string.Format("{0} x {1}", SystemInfo.processorCount, SystemInfo.processorType), systemHeight);
        CreateText(TextType.Memory, SystemTab.transform, string.Format("Mem: {0:f1}gb", SystemInfo.systemMemorySize / 1000.0f), systemHeight);
        CreateText(TextType.GfxDeviceId, SystemTab.transform, string.Format("{0} - deviceID: {1}", SystemInfo.graphicsDeviceName, SystemInfo.graphicsDeviceID), systemHeight);
        CreateText(TextType.GfxVendorId, SystemTab.transform, string.Format("{0} - vendorID: {1}", SystemInfo.graphicsDeviceVendor, SystemInfo.graphicsDeviceVendorID), systemHeight);
        CreateText(TextType.GfxVersion, SystemTab.transform, string.Format("{0}", SystemInfo.graphicsDeviceVersion), systemHeight);
        CreateText(TextType.GfxMemory, SystemTab.transform, string.Format("VMem: {0}mb", SystemInfo.graphicsMemorySize), systemHeight);
        CreateText(TextType.ShaderLevel, SystemTab.transform, string.Format("Shader Level: {0:f1}", SystemInfo.graphicsShaderLevel / 10.0f), systemHeight);

        #if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
        CreateText(TextType.Shadows, SystemTab.transform, string.Format("Shadows:{0} RT:{1} FX:{2}", SystemInfo.supportsShadows ? "y" : "n", SystemInfo.supportsRenderTextures ? "y" : "n", SystemInfo.supportsImageEffects ? "y" : "n"), systemHeight);
        #else
        CreateText(TextType.Shadows, SystemTab.transform, string.Format("Shadows:{0} RT:{1} FX:{2} MT:{3}", SystemInfo.supportsShadows ? "y" : "n", SystemInfo.supportsRenderTextures ? "y" : "n", SystemInfo.supportsImageEffects ? "y" : "n", SystemInfo.graphicsMultiThreaded ? "y" : "n"), systemHeight);
        #endif

        CreateText(TextType.DeviceUniqueId, SystemTab.transform, string.Format("deviceUniqueIdentifier: {0}", SystemInfo.deviceUniqueIdentifier), systemHeight);
        CreateText(TextType.DeviceName, SystemTab.transform, string.Format("deviceName: {0}", SystemInfo.deviceName), systemHeight);
        CreateText(TextType.DeviceModel, SystemTab.transform, string.Format("deviceModel: {0}", SystemInfo.deviceModel), systemHeight);
        CreateText(TextType.DeviceType, SystemTab.transform, string.Format("deviceType: {0}", SystemInfo.deviceType), systemHeight);
        CreateText(TextType.Username, SystemTab.transform, string.Format("UserName: {0}", System.Environment.UserName), systemHeight);
        CreateText(TextType.IP, SystemTab.transform, string.Format("IP: {0}", m_localIp), systemHeight);

        for (int i = 0; i < m_Tabs.Length; i++)
        {
            m_Tabs[i].gameObject.SetActive(false);
        }
        OffTab.gameObject.SetActive(true);

        ResizeFonts();
    }

    VerticalLayoutGroup CreateTab(InfoMode tab, Transform parent)
    {
        m_Tabs[(int)tab] = uGuiUtils.CreateLayoutGroup<VerticalLayoutGroup>(tab.ToString(), parent);
        m_Tabs[(int)tab].childForceExpandHeight = false;
        Image image = m_Tabs[(int)tab].gameObject.AddComponent<Image>();
        image.sprite = m_BackgroundImage.sprite;
        image.color = m_BackgroundImage.color;
        image.rectTransform.pivot = new Vector2(0.5f, 1f);
        ContentSizeFitter fitter = m_Tabs[(int)tab].gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        RectTransform layoutRect = m_Tabs[(int)tab].GetComponent<RectTransform>();
        uGuiUtils.StretchToParent(layoutRect);
        return m_Tabs[(int)tab];
    }

    Text CreateText(TextType id, Transform parent, string text, float screenPctTall)
    {
        Text label = null;
        int splitLineCount = 0;

        if (!m_Labels.ContainsKey(id))
        {
            label = uGuiUtils.CreateLayoutText(id.ToString(), parent, text, Color.white, 1, screenPctTall);
            m_Labels.Add(id, label);
            label.gameObject.SetActive(false);
            return label;
        }
        else
        {
            label = m_Labels[id];

            for (int i = 0; i < label.transform.parent.childCount; i++)
            {
                GameObject child = label.transform.parent.GetChild(i).gameObject;
                if (child.activeSelf && child.name == id.ToString())
                {
                    splitLineCount += 1;
                }
            }
        }

        LayoutElement layout = label.GetComponent<LayoutElement>();
        uGuiUtils.SetPreferredLayout(layout, 1, screenPctTall);
        label.fontSize = (int)(layout.preferredHeight * 0.7f);

        text = uGuiUtils.InsertBreaks(text, label.font, label.fontSize, (int)((float)Screen.width * 0.9f));
        string[] newLineSeperatedLines = text.Split('\n');

        if (splitLineCount != newLineSeperatedLines.Length)
        {
            for (int i = 0; i < label.transform.parent.childCount; i++)
            {
                GameObject child = label.transform.parent.GetChild(i).gameObject;
                if (child.activeSelf && child.name == id.ToString())
                {
                    Destroy(child);
                }
            }

            BreakUpLabel(label, newLineSeperatedLines, screenPctTall);
        }
        else
        {
            int lineCounter = 0;
            for (int i = 0; i < label.transform.parent.childCount; i++)
            {
                GameObject child = label.transform.parent.GetChild(i).gameObject;
                if (child.activeSelf && child.name == id.ToString())
                {
                    child.GetComponent<Text>().text = newLineSeperatedLines[lineCounter++];
                }
            }
        }

        label.gameObject.SetActive(false);
        return label;
    }

    void BreakUpLabel(Text label, string[] newLineSeperatedLines, float screenPctTall)
    {
        for (int i = 0; i < newLineSeperatedLines.Length; i++)
        {
            Text splitLabel = uGuiUtils.CreateLayoutText(label.name, label.transform.parent, newLineSeperatedLines[i], Color.white, 1, screenPctTall);
            uGuiUtils.SetPreferredLayout(splitLabel.GetComponent<LayoutElement>(), 1, screenPctTall);
            splitLabel.fontSize = label.fontSize;
            splitLabel.transform.SetSiblingIndex(label.transform.GetSiblingIndex() + 1 + i);
        }
    }

    void SetTextColor(TextType id, Color c)
    {
        Text label = m_Labels[id];
        for (int i = 0; i < label.transform.parent.childCount; i++)
        {
            GameObject child = label.transform.parent.GetChild(i).gameObject;
            if (child.name == id.ToString())
            {
                child.GetComponent<Text>().color = c;
            }
        }
    }

    void ResizeFonts()
    {
        m_CachedTextScreenHeightPct = m_TextScreenHeightPct;
        m_CachedScreenResolution.x = Screen.width;
        m_CachedScreenResolution.y = Screen.height;

        foreach (KeyValuePair<TextType, Text> kvp in m_Labels)
        {
            CreateText(kvp.Key, kvp.Value.transform.parent, kvp.Value.text, m_TextScreenHeightPct);
        }
    }

    void Start()
    {
        if (m_fpsCounter == null)
        {
            m_fpsCounter = GameObject.FindObjectOfType<FpsCounter>();
        }
    }


    void Update()
    {
        bool nextModeSet = false;
        if (Input.GetKeyDown(m_ToggleKey))
        {
            NextMode();
            nextModeSet = true;
        }

        switch (m_mode)
        {
        case InfoMode.General:
            OnGUIGeneralInfo();
            break;

        case InfoMode.Build:
            OnGUIBuildInfo();
            break;

        case InfoMode.System:
            OnGUISystemInfo();
            break;
        }

        m_BackgroundImage.rectTransform.anchoredPosition = m_ScreenPosition;
        if (m_TextScreenHeightPct != m_CachedTextScreenHeightPct
            || (int)m_CachedScreenResolution.x != Screen.width || (int)m_CachedScreenResolution.y != Screen.height)
        {
            ResizeFonts();
        }

        if (nextModeSet)
        {
            ResizeFonts();
        }
    }


    public void NextMode()
    {
        m_Tabs[(int)m_mode].gameObject.SetActive(false);
        m_mode = (InfoMode)VHMath.IncrementWithRollover((int)m_mode, (int)InfoMode.NUM_MODES);
        m_Tabs[(int)m_mode].gameObject.SetActive(true);
        ResizeFonts();
        //m_BackgroundImage.enabled = m_mode != InfoMode.Off;
    }


    public void SetMode(InfoMode mode)
    {
        m_mode = mode;
    }


    void GetBuildInfo()
    {
        // try and get version info from the resource file generated by Unity Cloud build server.
        // ref: https://build.cloud.unity3d.com/support/guides/manifest/
        // if that doesn't exist, try and get version info from svn.
        // otherwise, fill with some default info

        m_unityCloudBuildManifest = null;

        string versionText = "";

        var unityCloudBuildManifestText = (TextAsset)Resources.Load("UnityCloudBuildManifest.json");
        if (unityCloudBuildManifestText != null)
        {
            try
            {
                m_unityCloudBuildManifest = JsonUtility.FromJson<UnityCloudBuildManifest>(unityCloudBuildManifestText.text);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        else
        {
            // either built locally (not Unity Cloud), or run from editor via svn sandbox

            // check to see if folder is a svn working copy
            versionText = VHUtils.GetSVNRevision(Application.dataPath + m_svnWorkingFolder);
        }

        if (m_unityCloudBuildManifest == null)
        {
            m_unityCloudBuildManifest = new UnityCloudBuildManifest();
            m_unityCloudBuildManifest.scmCommitId = versionText;
            m_unityCloudBuildManifest.unityVersion = Application.unityVersion;
        }
    }


    void OnGUIGeneralInfo()
    {
        float fps = 0;
        float averageFps = 0;
        if (m_fpsCounter)
        {
            fps = m_fpsCounter.Fps;
            averageFps = m_fpsCounter.AverageFps;
        }

        Camera camera = m_Camera;
        if (camera == null)
            camera = Camera.main;



        float fpsColor = Mathf.Min(1.0f, averageFps / m_lowFpsThreshold);
        //m_Labels[TextType.Frame].text = string.Format("T: {0:f2} F: {1} AVG: {2:f0} FPS: {3:f2}", Time.time, Time.frameCount, averageFps, fps);
        CreateText(TextType.Frame, m_Labels[TextType.Frame].transform.parent, string.Format("T: {0:f2} F: {1} AVG: {2:f0} FPS: {3:f2}", Time.time, Time.frameCount, averageFps, fps), m_TextScreenHeightPct);
        SetTextColor(TextType.Frame, new Color(1, fpsColor, fpsColor));

        CreateText(TextType.Resolution, m_Labels[TextType.Frame].transform.parent, string.Format("{0}x{1}x{2} ({3}) {4:f0}dpi", Screen.width, Screen.height, Screen.currentResolution.refreshRate,
            VHUtils.GetCommonAspectText((float)Screen.width / Screen.height), Screen.dpi), m_TextScreenHeightPct);

        CreateText(TextType.SceneName, m_Labels[TextType.SceneName].transform.parent, string.Format("{0}", VHUtils.SceneManagerActiveSceneName()), m_TextScreenHeightPct);
        CreateText(TextType.QualitySetting, m_Labels[TextType.QualitySetting].transform.parent, string.Format("{0} - {1}", QualitySettings.names[QualitySettings.GetQualityLevel()], QualitySettings.activeColorSpace), m_TextScreenHeightPct);

        if (camera != null)
        {
            Transform camTrans = camera.transform;
            string pos = string.Format("Cam Pos ({0}): {1:f2} {2:f2} {3:f2}", camera.name, camTrans.position.x, camTrans.position.y, camTrans.position.z);
            CreateText(TextType.CameraPosition, m_Labels[TextType.CameraPosition].transform.parent, pos, m_TextScreenHeightPct);
            string rot = string.Format("Cam Rot (xyz): {0:f2} {1:f2} {2:f2}", camTrans.rotation.eulerAngles.x, camTrans.rotation.eulerAngles.y, camTrans.rotation.eulerAngles.z);
            CreateText(TextType.CameraRotation, m_Labels[TextType.CameraRotation].transform.parent, rot, m_TextScreenHeightPct);
        }
    }


    void OnGUIBuildInfo()
    {
        // IN CASE WE NEED THESE LATER
        //if (!string.IsNullOrEmpty(m_unityCloudBuildManifest.scmBranch))            GUILayout.Label(string.Format("scmBranch: {0}", m_unityCloudBuildManifest.scmBranch), m_guiLabel);
        //if (!string.IsNullOrEmpty(m_unityCloudBuildManifest.projectId))            GUILayout.Label(string.Format("projectId: {0}", m_unityCloudBuildManifest.projectId), m_guiLabel);
        //if (!string.IsNullOrEmpty(m_unityCloudBuildManifest.bundleId))             GUILayout.Label(string.Format("bundleId: {0}", m_unityCloudBuildManifest.bundleId), m_guiLabel);
        //if (!string.IsNullOrEmpty(m_unityCloudBuildManifest.unityVersion))         GUILayout.Label(string.Format("unityVersion: {0}", m_unityCloudBuildManifest.unityVersion), m_guiLabel);
    }


    void OnGUISystemInfo()
    {
        // IN CASE WE NEED THESE LATER
        //GUILayout.Label(string.Format("{0}", SystemInfo.npotSupport), m_guiLabel);   // What NPOTSupport support does GPU provide? (Read Only)
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportedRenderTargetCount), m_guiLabel);   // How many simultaneous render targets (MRTs) are supported? (Read Only)
        //GUILayout.Label(string.Format("{0}", SystemInfo.supports3DTextures), m_guiLabel);   // Are 3D (volume) textures supported? (Read Only)
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsAccelerometer), m_guiLabel);   // Is an accelerometer available on the device?
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsComputeShaders), m_guiLabel);   // Are compute shaders supported? (Read Only)
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsGyroscope), m_guiLabel);   // Is a gyroscope available on the device?
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsInstancing), m_guiLabel);   // Is GPU draw call instancing supported? (Read Only)
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsLocationService), m_guiLabel);   // Is the device capable of reporting its location?
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsRenderToCubemap), m_guiLabel);   // Are cubemap render textures supported? (Read Only)
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsSparseTextures), m_guiLabel);   // Are sparse textures supported? (Read Only)
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsStencil), m_guiLabel);   // Is the stencil buffer supported? (Read Only)
        //GUILayout.Label(string.Format("{0}", SystemInfo.supportsVibration), m_guiLabel);   // Is the device capable of providing the user haptic feedback by vibration?
    }

    #endregion
}
