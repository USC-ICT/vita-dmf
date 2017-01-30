using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class SpeechRecognizer_PocketSphinx : SpeechRecognizer
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

    #region Constants
#if UNITY_IPHONE
    public const string DLLIMPORT_NAME = "__Internal";
#else
    public const string DLLIMPORT_NAME = "vhPocketSphinxWrapper";
#endif
    #endregion

    #region Dll Functions
    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern IntPtr WRAPPER_PS_CreatePS(bool releaseMode, [MarshalAs(UnmanagedType.LPStr)]string langModelPath, [MarshalAs(UnmanagedType.LPStr)]string dictPath, [MarshalAs(UnmanagedType.LPStr)]string hmmPath);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern bool WRAPPER_PS_Init(IntPtr psId);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern bool WRAPPER_PS_Free(IntPtr psId);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern int WRAPPER_PS_DecodeStream(IntPtr psId, [MarshalAs(UnmanagedType.LPStr)]string absFilePath, [MarshalAs(UnmanagedType.LPStr)]string uttId, long maxAmps);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern int WRAPPER_PS_StartUtterance(IntPtr psId, [MarshalAs(UnmanagedType.LPStr)]string uttId);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern int WRAPPER_PS_EndUtterance(IntPtr psId);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern int WRAPPER_PS_Process(IntPtr psId, short[] audioData, int samples, bool noSearch, bool processFullUtt);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    static extern bool WRAPPER_PS_GetHypothesis(IntPtr psId, StringBuilder utterance, int maxLength);
    #endregion

    #region Variables
    public string m_LanguageModelPath = "../../../../data/pocketsphinx/lm.arpa";
    public string m_DictionaryPath = "../../../../data/pocketsphinx/cmudict.0.7a_SPHINX_40";
    public string m_HMMPath = "../../../../data/pocketsphinx/wsj1";
    public int m_MaxUtteranceLength = 256;
    int m_UttId = 0;
    bool m_Initialized;
    IntPtr m_PSId;

    #endregion

    #region Functions
    void Awake()
    {
    }

    void Start()
    {
        m_PSId = WRAPPER_PS_CreatePS(true, FixFilePath(m_LanguageModelPath), FixFilePath(m_DictionaryPath), FixFilePath(m_HMMPath));
        if (m_PSId != new IntPtr(-1))
        {
            Debug.Log("Pocket Sphinx Id: " + m_PSId);
            if (!WRAPPER_PS_Init(m_PSId))
            {
                Debug.LogError("WRAPPER_PS_Init failed");
            }
            else
            {
                Debug.Log("Pocket Sphinx Initialized");
                m_Initialized = true;
            }
        }
        else
        {
            Debug.LogError("WRAPPER_PS_CreatePS failed");
        }
    }

    string FixFilePath(string path)
    {
        string retVal = path;
        return VHFile.GetStreamingAssetsPath() + retVal;
    }

    protected override void PerformRecognition(AudioClip clip)
    {
        Debug.Log("PerformRecognition");
        if (!m_Initialized)
        {
            return;
        }

        if (clip.frequency != 16000)
        {
            Debug.LogWarning("Microphone Recorder frequency is not set to 16000. The results given from pocket sphinx won't be good. Select the microphone recorder gameobject and change the frequency to 16000");
        }

        byte[] audioData = AudioConverter.ConvertClipToWav(clip, VHFile.GetStreamingAssetsPath() + "Flac/testwav.wav");

        // start utt
        string uttName = string.Format("utt{0}", m_UttId);
        if (WRAPPER_PS_StartUtterance(m_PSId, uttName) < 0)
        {
            Debug.LogError("SpeechRecognizer_PocketSphinx failed to start utterance");
        }

        // process the audio data
        short[] audioDataShort = ConvertByteToShorts(audioData);
        //short[] audioDataShort = Array.ConvertAll(audioData, b => (short)b);
        if (WRAPPER_PS_Process(m_PSId, audioDataShort, audioDataShort.Length, false, true) < 0)
        {
            Debug.LogError("SpeechRecognizer_PocketSphinx failed to process utterance");
        }

        // utterance finished
        if (WRAPPER_PS_EndUtterance(m_PSId) < 0)
        {
            Debug.LogError("SpeechRecognizer_PocketSphinx failed to stop utterance");
        }

        // get the text from the audio data
        StringBuilder utteranceHolder = new StringBuilder(m_MaxUtteranceLength);
        if (!WRAPPER_PS_GetHypothesis(m_PSId, utteranceHolder, m_MaxUtteranceLength))
        {
            //Debug.LogError("SpeechRecognizer_PocketSphinx failed to recognize");
        }
        List<RecognizerResult> recognizerResults = new List<RecognizerResult>();
        recognizerResults.Add(new RecognizerResult(utteranceHolder.ToString()));

        ++m_UttId;

        DispatchResults(recognizerResults);
    }

    void OnDestroy()
    {
        if (m_PSId != new IntPtr(-1))
        {
            WRAPPER_PS_Free(m_PSId);
        }
    }

    short[] ConvertByteToShorts(byte[] bytes)
    {
        short[] shortData = new short[bytes.Length / 2];
        Buffer.BlockCopy(bytes, 0, shortData, 0, bytes.Length);
        return shortData;
    }
    #endregion


#else  // UNITY_WIN

#endif // UNITY_WIN
}
