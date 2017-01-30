using UnityEngine;
using System.Collections;

public class AudioCapturer_RecordingDevice : AudioCapturer
{
    #region Variables
    public RecordingDevice m_RecordingDevice;
    #endregion

    #region Functions
    public override void Awake()
    {
        base.Awake();

        m_RecordingDevice.AddOnStartedRecordingCallback(OnStartedRecording);
        m_RecordingDevice.AddOnFinishedRecordingCallback(OnFinishedRecording);
        m_RecordingDevice.AddOnRecordingEnabled(OnRecordingEnabled);
    }

    void OnStartedRecording(AudioStream stream)
    {
        m_vhmsg.SendVHMsg("acquireSpeech startUtterance mic");

        if (m_RecordingDevice.IsContiniouslyStreaming)
        {
            CaptureAudioTextFromStream(stream);
        }
    }

    void OnFinishedRecording(AudioStream stream)
    {
        m_vhmsg.SendVHMsg("acquireSpeech stopUtterance mic");
        ++m_SpeechUserID;
        m_DefaultRecognizer.Recognize(stream.Clip);
    }

    void OnRecordingEnabled(bool enabled)
    {
        if (m_vhmsg != null)
        {
            m_vhmsg.SendVHMsg("acquireSpeech " + (enabled ? "startSession" : "stopSession"));
        }
        else
        {
            Debug.LogWarning("No vhmsg manager found in the scene.  Recording device can't send messages to acquireSpeech");
        }
    }
    #endregion
}
