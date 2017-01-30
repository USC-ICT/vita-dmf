using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


public class SpeechRecognizer_MSSAPI : SpeechRecognizer
{
#if UNITY_IPHONE
    const string DLLIMPORT_NAME = "__Internal";
#else
    const string DLLIMPORT_NAME = "vhwrapper";
#endif

    class LibraryData
    {
        public string configuration; // "both", "release", "debug"
        public string architecture;  // "both", "x86", "x64"
        public string library;

        public LibraryData(string configuration, string architecture, string library) { this.configuration = configuration; this.architecture = architecture; this.library = library; }
    }

    static List<LibraryData> m_libraries = new List<LibraryData>()
    {
        // order does matter here.  Dependencies must be loaded first.  Libraries are freed in reverse order
        new LibraryData("both",  "both",  "vcruntime140.dll"),
        new LibraryData("both",  "both",  "msvcp140.dll"),
        new LibraryData("both",  "x86",   "dbghelp.dll"),
        new LibraryData("both",  "both",  "pthreadVSE2.dll"),
        new LibraryData("both",  "both",  "glew32.dll"),
        new LibraryData("both",  "both",  "OpenAL32.dll"),
        new LibraryData("both",  "both",  "wrap_oal.dll"),
        new LibraryData("both",  "both",  "alut.dll"),
        new LibraryData("both",  "both",  "libsndfile-1.dll"),
        new LibraryData("both",  "both",  "vhwrapper.dll"),
    };

    static List<KeyValuePair<string, IntPtr>> m_nativeDlls = new List<KeyValuePair<string, IntPtr>>();


    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool FreeLibrary(IntPtr hModule);


#if false
    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern IntPtr WRAPPER_MSSPEECH_Create();

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern bool WRAPPER_MSSPEECH_Init(IntPtr handle);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern bool WRAPPER_MSSPEECH_Free(IntPtr handle);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern bool WRAPPER_MSSPEECH_Recognize(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string waveFileName, StringBuilder buffer, int maxLen, int msFreqEnumVal);
#else
    static IntPtr WRAPPER_MSSPEECH_Create() { return new IntPtr(0); }
    static bool WRAPPER_MSSPEECH_Init(IntPtr handle) { return true; }
    static bool WRAPPER_MSSPEECH_Free(IntPtr handle) { return true; }
    static bool WRAPPER_MSSPEECH_Recognize(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string waveFileName, StringBuilder buffer, int maxLen, int msFreqEnumVal) { return true; }
#endif


    static void LoadLibraries()
    {
        if (!VHUtils.IsUnity5OrGreater())
            return;

        if (!VHUtils.IsWindows())
            return;

        if (m_nativeDlls.Count > 0)
            return;  // we've already called LoadLibraries()

        for (int i = 0; i < m_libraries.Count; i++)
        {
            var libraryEntry = m_libraries[i];
            string library = libraryEntry.library;
            string libraryArchitecture = libraryEntry.architecture;
            bool loadLibrary = false;

            if (VHUtils.Is64Bit() && (libraryArchitecture == "both" || libraryArchitecture == "x64"))
                loadLibrary = true;
            if (!VHUtils.Is64Bit() && (libraryArchitecture == "both" || libraryArchitecture == "x86"))
                loadLibrary = true;

            if (loadLibrary)
            {
                string path;
                if (VHUtils.Is64Bit() && VHUtils.IsEditor())
                    path = Path.GetFullPath(Application.dataPath + "/Plugins/x86_64/" + library);
                else
                    path = Path.GetFullPath(Application.dataPath + "/Plugins/" + library);

                IntPtr ptr = LoadLibraryInternal(path);

                m_nativeDlls.Add(new KeyValuePair<string,IntPtr>(path, ptr));
            }
        }
    }

    static IntPtr LoadLibraryInternal(string path)
    {
        IntPtr ptr = LoadLibrary(path);
        if (ptr == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            Debug.LogError(string.Format("Failed to load {1} (ErrorCode: {0})", errorCode, path));
        }
        else
        {
            //Debug.Log("Loaded: " + path);
        }
        return ptr;
    }

    static void FreeLibraries()
    {
        if (!VHUtils.IsUnity5OrGreater())
            return;

        if (!VHUtils.IsWindows())
            return;

        // free in reverse order
        for (int i = m_nativeDlls.Count - 1; i >= 0; i--)
        {
            KeyValuePair<string, IntPtr> entry = m_nativeDlls[i];

            FreeLibrary(entry.Value);

            //Debug.Log(string.Format("FreeLibrary({0} - {1}) - {2}", entry.Key, entry.Value, ret));
        }

        m_nativeDlls.Clear();
    }

    static IntPtr Create()
    {
        //LoadLibraries();

        return WRAPPER_MSSPEECH_Create();
    }

    static bool Init(IntPtr sbmID)
    {
        return WRAPPER_MSSPEECH_Init(sbmID);
    }

    static bool Free(IntPtr sbmID)
    {
        bool ret = WRAPPER_MSSPEECH_Free(sbmID);

        FreeLibraries();

        return ret;
    }

    static bool Recognize(IntPtr sbmID, string waveFileName, StringBuilder buffer, int maxLen, int msFreqEnumVal)
    {
        return WRAPPER_MSSPEECH_Recognize(sbmID, waveFileName, buffer, maxLen, msFreqEnumVal);
    }

    public enum Frequency
    {
        Mono_11k,
        Mono_22k,
        Mono_44k,
        Stereo_11k,
        Stereo_22k,
        Stereo_44k,
    }


    #region Variables
    public int m_MaxUtteranceLength = 256;
    public Frequency m_AudioFrequency = Frequency.Mono_44k;
    bool m_Initialized = false;
    IntPtr m_Id = new IntPtr( -1 );
    #endregion

    #region Properties
    public Frequency AudioFrequency
    {
        get { return m_AudioFrequency; }
        set { m_AudioFrequency = value; }
    }
    #endregion


    #region Functions
    void Awake()
    {
    }

    void Start()
    {
        m_Id = Create();
        if (m_Id != new IntPtr(-1))
        {
            if (!Init(m_Id))
            {
                Debug.LogError("WRAPPER_MSSPEECH_Init failed");
            }
            else
            {
                Debug.Log("MS Speech Initialized");
                m_Initialized = true;
            }
        }
        else
        {
            Debug.LogError("WRAPPER_MSSPEECH_Create failed");
        }
    }

    void OnDestroy()
    {
        if (m_Id != new IntPtr(-1))
        {
            Free(m_Id);
        }
    }

    protected override void PerformRecognition(AudioClip clip)
    {
        if (!m_Initialized)
        {
            return;
        }

        //if (clip.frequency != 22050)
        //{
        //    Debug.LogWarning(string.Format("Microphone Recorder frequency is not set to {0}. The results given from MS Speech Recognition won't be good. Select the microphone recorder gameobject and change the frequency to {0}", 22050));
        //}

        string wavePath = VHFile.GetStreamingAssetsPath() + "Flac/testwav.wav";
        AudioConverter.ConvertClipToWav(clip, wavePath);
        StringBuilder utteranceHolder = new StringBuilder(m_MaxUtteranceLength);
        if (!Recognize(m_Id, wavePath, utteranceHolder, m_MaxUtteranceLength, GetConvertedFrequency(m_AudioFrequency)))
        {
            Debug.LogError("failed to recognize");
        }

        List<RecognizerResult> recognizerResults = new List<RecognizerResult>();
        recognizerResults.Add(new RecognizerResult(utteranceHolder.ToString()));
        DispatchResults(recognizerResults);
    }

    int GetConvertedFrequency(Frequency freq)
    {
        // look at sap51.h to see how this mapping works
        int msFreq = 0;

        switch (freq)
        {
            case Frequency.Mono_11k:
                msFreq = 10; //SPSF_11kHz16BitMono
                break;

            case Frequency.Mono_22k:
                msFreq = 22; //SPSF_22kHz16BitMono
                break;

            case Frequency.Mono_44k:
                msFreq = 34; //SPSF_44kHz16BitMono
                break;

            case Frequency.Stereo_11k:
                msFreq = 11; // SPSF_11kHz16BitStereo
                break;

            case Frequency.Stereo_22k:
                msFreq = 23; // SPSF_22kHz16BitStereo
                break;

            case Frequency.Stereo_44k:
                 msFreq = 35; // SPSF_22kHz16BitStereo
                break;

            default:
                msFreq = 22; //SPSF_22kHz16BitMono
                break;
        }

        return msFreq;
    }
    #endregion
}
