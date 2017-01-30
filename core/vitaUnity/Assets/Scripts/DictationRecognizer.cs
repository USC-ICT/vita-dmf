using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

public class DictationRecognizer : MonoBehaviour
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public UnityEngine.Windows.Speech.DictationRecognizer m_dictationRecognizer;
#endif

    bool m_isRecording = false;

    bool m_errorMessageSent = false;
    string m_errorMessage = "This system is not configured properly to use Speech Recognition";


    void Start()
    {
        if (VHUtils.IsWindows10OrGreater())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            m_dictationRecognizer = new UnityEngine.Windows.Speech.DictationRecognizer();

            m_dictationRecognizer.DictationResult += (text, confidence) =>
            {
                Debug.LogFormat("Dictation result: {0}", text);

                // TODO - make this a callback to let the caller control what happens

                NetworkRelay.SendNetworkMessage(string.Format("dictationresult {0}", text));
            };

            m_dictationRecognizer.DictationHypothesis += (text) =>
            {
                Debug.LogFormat("Dictation hypothesis: {0}", text);
            };

            m_dictationRecognizer.DictationComplete += (completionCause) =>
            {
                if (completionCause != UnityEngine.Windows.Speech.DictationCompletionCause.Complete)
                {
                    Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);

                    if (!m_errorMessageSent)
                    {
                        m_errorMessageSent = true;
                        NetworkRelay.SendNetworkMessage(string.Format("dictationresult {0}", m_errorMessage));
                    }
                }
            };

            m_dictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);

                if (!m_errorMessageSent)
                {
                    m_errorMessageSent = true;
                    NetworkRelay.SendNetworkMessage(string.Format("dictationresult {0}", m_errorMessage));
                }
            };

            m_dictationRecognizer.InitialSilenceTimeoutSeconds = 999;
#endif
        }
    }


    void Update()
    {
    }

    public void StartRecording()
    {
        if (VHUtils.IsWindows10OrGreater())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (!m_isRecording)
            {
                m_isRecording = true;
                m_dictationRecognizer.Start();
            }
#endif
        }
        else
        {
            if (!m_errorMessageSent)
            {
                m_errorMessageSent = true;
                NetworkRelay.SendNetworkMessage(string.Format("dictationresult {0}", m_errorMessage));
            }
        }
    }

    public void StopRecording()
    {
        if (VHUtils.IsWindows10OrGreater())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (m_isRecording)
            {
                m_isRecording = false;
                m_dictationRecognizer.Stop();
            }
#endif
        }
    }

#if false
    void OnGUIASR()
    {
        if (VHUtils.IsWindows10OrGreater())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            GUILayout.Label(string.Format("PhraseRecogntionSystem.isSupported: {0}", UnityEngine.Windows.Speech.PhraseRecognitionSystem.isSupported));
            GUILayout.Label(string.Format("PhraseRecogntionSystem.status: {0}", UnityEngine.Windows.Speech.PhraseRecognitionSystem.Status));
            if (GUILayout.Button("PhraseRecogntionSystem.Restart()"))
            {
                UnityEngine.Windows.Speech.PhraseRecognitionSystem.Restart();
            }

            if (GUILayout.Button("PhraseRecogntionSystem.Stop()"))
            {
                UnityEngine.Windows.Speech.PhraseRecognitionSystem.Shutdown();
            }

            GUILayout.Label(string.Format("Dictation.status: {0}", m_dictationRecognizer.Status));
            GUILayout.Label(string.Format("Dictation.AutoSilenceTimeoutSeconds: {0}", m_dictationRecognizer.AutoSilenceTimeoutSeconds));
            GUILayout.Label(string.Format("Dictation.InitialSilenceTimeoutSeconds: {0}", m_dictationRecognizer.InitialSilenceTimeoutSeconds));

            if (GUILayout.Button("Dictation Recognizer Setup"))
            {
                m_dictationRecognizer.DictationResult += (text, confidence) =>
                {
                    Debug.LogFormat("Dictation result: {0}", text);
                };

                m_dictationRecognizer.DictationHypothesis += (text) =>
                {
                    Debug.LogFormat("Dictation hypothesis: {0}", text);
                };

                m_dictationRecognizer.DictationComplete += (completionCause) =>
                {
                    if (completionCause != UnityEngine.Windows.Speech.DictationCompletionCause.Complete)
                        Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
                };

                m_dictationRecognizer.DictationError += (error, hresult) =>
                {
                    Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
                };

                m_dictationRecognizer.Start();
            }

            if (GUILayout.Button("Dictation Recognizer Stop"))
            {
                m_dictationRecognizer.Stop();
            }

            if (GUILayout.Button("Dictation Recognizer Dispose"))
            {
                m_dictationRecognizer.Dispose();
            }

            GUILayout.Space(10);
#endif
        }
    }
#endif
}
