using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class AudioCapturer : MonoBehaviour
{
    #region Variables
    public SpeechRecognizer m_DefaultRecognizer;
    public SpeechRecognizer[] m_SpeechRecognizers;
    public AudioStreamer m_AudioStreamer;
    public VHMsgBase m_vhmsg;
    public string m_SpeechUserName = "captain";
    protected int m_SpeechUserID = 1;
    #endregion

    #region Functions
    public virtual void Awake()
    {
        if (m_vhmsg == null)
        {
            m_vhmsg = VHMsgBase.Get();
        }

        // error checking
        if (m_DefaultRecognizer != null)
        {
            if (m_SpeechRecognizers.Length == 0 || Array.Find<SpeechRecognizer>(m_SpeechRecognizers, sr => sr == m_DefaultRecognizer) == null)
            {
                Debug.LogWarning(string.Format("The default recognizer {0} is not listed in the m_SpeechRecognizers array", m_DefaultRecognizer.name));
                m_SpeechRecognizers = new SpeechRecognizer[1];
                m_SpeechRecognizers[0] = m_DefaultRecognizer;
            }
        }
        else
        {
            Debug.LogError(string.Format("No default recognizer found in AudioCapturer", name));
        }

        // setup callback for the recognizer for when it's finished recognizing
        for (int i = 0; i < m_SpeechRecognizers.Length; i++)
        {
            m_SpeechRecognizers[i].AddSpeechRecognitionFinishedCallback(OnReceivedRecognizerResults);
        }

        // setup audio streamer callbacks
        if (m_AudioStreamer != null)
        {
            m_AudioStreamer.AddOnStreamingSoundDetected(OnStreamingSoundDetected);
            m_AudioStreamer.AddOnStreamingSilenceDetected(OnStreamingSilenceDetected);
            m_AudioStreamer.AddOnStreamingFinished(OnStreamingFinished);
        }
        else
        {
            Debug.LogError(string.Format("m_AudioStreamer is null in gameobject {0}. Streaming recognition won't work", name));
        }

        if (m_DefaultRecognizer == null && m_SpeechRecognizers.Length > 0)
        {
            SetDefaultRecognizer(m_SpeechRecognizers[0]);
        }
    }

    void Start()
    {
    }

    /// <summary>
    /// Sets the recognizer that will be used to intrepret the caught audio data
    /// </summary>
    /// <param name="recognizer"></param>
    public void SetDefaultRecognizer(SpeechRecognizer recognizer)
    {
        m_DefaultRecognizer = recognizer;
    }

    /// <summary>
    /// Uses the default recognizer to interpret an audio file on the hard drive. This function
    /// won't work on the web or mobile
    /// </summary>
    /// <param name="absPath">The absolute path of the audio file</param>
    public void CaptureAudioTextFromHardDrive(string absPath)
    {
        StartCoroutine(CaptureAudioTextFromHardDriveCoroutine(absPath, null));
    }

    /// <summary>
    /// Uses the default recognizer to interpret an audio file on the hard drive. This function
    /// won't work on the web or mobile. This overload will also play the clip from the given audio source
    /// </summary>
    /// <param name="absPath">The absolute path of the audio file</param>
    public void CaptureAudioTextFromHardDrive(string absPath, AudioSource playSource)
    {
        StartCoroutine(CaptureAudioTextFromHardDriveCoroutine(absPath, playSource));
    }

    IEnumerator CaptureAudioTextFromHardDriveCoroutine(string absPath, AudioSource playSource)
    {
        string path = Path.GetFullPath(absPath);
        path = path.Replace("\\", "/");
        path = "file://" + path;
        WWW www = new WWW(path);
        yield return www;

        while (!www.audioClip)
        {
            yield return new WaitForEndOfFrame();
        }

        CaptureAudioTextFromClip(www.audioClip, playSource);
    }

    /// <summary>
    /// Uses the default recognizer to interpret an audio file that is located in a "Resources" folder inside
    /// of this unity project.
    /// </summary>
    /// <param name="resourcePath">The resource relative path WITHOUT file extension.</param>
    /// <returns></returns>
    public bool CaptureAudioTextFromResource(string resourcePath)
    {
        return CaptureAudioTextFromResource(resourcePath, null);
    }

    /// <summary>
    /// Uses the default recognizer to interpret an audio file that is located in a "Resources" folder inside
    /// of this unity project. This overload plays the clip from the given audio source
    /// </summary>
    /// <param name="resourcePath">The resource relative path WITHOUT file extension.</param>
    /// <returns></returns>
    public bool CaptureAudioTextFromResource(string resourcePath, AudioSource playSource)
    {
        bool success = false;
        AudioClip loadedClip = Resources.Load(resourcePath, typeof(AudioClip)) as AudioClip;
        if (loadedClip != null)
        {
            CaptureAudioTextFromClip(loadedClip, playSource);
            success = true;
        }
        else
        {
            Debug.LogError(string.Format("Failed to Resources.Load audio clip from path {0}", resourcePath));
        }

        return success;
    }

    /// <summary>
    /// Uses the default recognizer to interpret and audio clip
    /// </summary>
    /// <param name="clip"></param>
    public void CaptureAudioTextFromClip(AudioClip clip)
    {
        CaptureAudioTextFromClip(clip, null);
    }

    /// <summary>
    /// Uses the default recognizer to interpret and audio clip. This overload plays the clip from
    /// the given audio source
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="playSource"></param>
    public void CaptureAudioTextFromClip(AudioClip clip, AudioSource playSource)
    {
        if (playSource == null)
        {
            m_DefaultRecognizer.Recognize(clip);
        }
        else
        {
            m_DefaultRecognizer.PlayAndRecognize(clip, playSource);
        }
    }

    /// <summary>
    /// Uses the default recognizer to interpret a continuously running audio stream
    /// </summary>
    /// <param name="stream"></param>
    public void CaptureAudioTextFromStream(AudioStream stream)
    {
        if (m_AudioStreamer != null)
        {
            m_AudioStreamer.Stream(stream, m_DefaultRecognizer.Recognize);
        }
    }

    /// <summary>
    /// By passes capture and recognition and simply sends the given text string using vhmsg with vrSpeech opcodes
    /// </summary>
    /// <param name="text"></param>
    virtual public void SendCapturedText(string text)
    {
        m_vhmsg.SendVHMsg(string.Format("vrSpeech start user{0} user", m_SpeechUserID));
        m_vhmsg.SendVHMsg(string.Format("vrSpeech finished-speaking user{0}", m_SpeechUserID));
        m_vhmsg.SendVHMsg(string.Format("vrSpeech interp user{0} 1 1.0 normal {1}", m_SpeechUserID, text));
        m_vhmsg.SendVHMsg(string.Format("vrSpeech emotion user{0} 1 1.0 normal neutral", m_SpeechUserID));
        m_vhmsg.SendVHMsg(string.Format("vrSpeech tone user{0} 1 1.0 normal flat", m_SpeechUserID));
        m_vhmsg.SendVHMsg(string.Format("vrSpeech asr-complete user{0}", m_SpeechUserID));
    }

    #region Callbacks
    void OnStreamingSoundDetected()
    {
        //Debug.Log("OnStreamingSoundDetected");
        m_vhmsg.SendVHMsg(string.Format("vrSpeech start utt{0} {1}", m_SpeechUserID, m_SpeechUserName));
    }

    void OnStreamingSilenceDetected()
    {
        //Debug.Log("OnStreamingSilenceDetected");
        m_vhmsg.SendVHMsg(string.Format("vrSpeech asr-complete utt{0}", m_SpeechUserID));
        ++m_SpeechUserID;
    }

    void OnStreamingFinished()
    {

    }

    void OnReceivedRecognizerResults(SpeechRecognizer recognizer, List<SpeechRecognizer.RecognizerResult> results)
    {
        if (results.Count > 0)
        {
            SendCapturedText(results[0].m_Utterance);
        }
    }
    #endregion
    #endregion
}
