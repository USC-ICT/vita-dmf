using UnityEngine;
using System.Collections;

abstract public class RecordingDevice : MonoBehaviour
{
    #region Constants
    public delegate void OnStartedRecording(AudioStream stream);
    public delegate void OnFinishedRecording(AudioStream stream);
    public delegate void OnRecordingEnabled(bool isEnabled);
    #endregion

    #region Variables
    protected AudioStream m_Stream = new AudioStream();
    protected OnStartedRecording m_OnStartedRecordingCBs;
    protected OnFinishedRecording m_OnFinishedRecordingCBs;
    protected OnRecordingEnabled m_OnRecordingEnabledCBs;
    bool m_ContinuousStreaming = false;
    #endregion

    #region Properties
    public bool IsContiniouslyStreaming
    {
        get { return m_ContinuousStreaming; }
    }

    virtual public bool IsRecording
    {
        get { return false; }
    }
    #endregion

    #region Functions
    public virtual void OnEnable()
    {
        if (m_OnRecordingEnabledCBs != null)
        {
            m_OnRecordingEnabledCBs(true);
        }
    }

    public virtual void OnDisable()
    {
        if (m_OnRecordingEnabledCBs != null)
        {
            m_OnRecordingEnabledCBs(false);
        }
    }

    public void AddOnStartedRecordingCallback(OnStartedRecording cb)
    {
        m_OnStartedRecordingCBs += cb;
    }

    public void AddOnFinishedRecordingCallback(OnFinishedRecording cb)
    {
        m_OnFinishedRecordingCBs += cb;
    }

    public void AddOnRecordingEnabled(OnRecordingEnabled cb)
    {
        m_OnRecordingEnabledCBs += cb;
    }

    public virtual AudioStream StartRecording()
    {
        if (m_OnStartedRecordingCBs != null)
        {
            m_OnStartedRecordingCBs(m_Stream);
        }
        return m_Stream;
    }

    public virtual void StopRecording()
    {
        if (m_OnFinishedRecordingCBs != null)
        {
            m_OnFinishedRecordingCBs(m_Stream);
        }
    }

    public virtual float GetRecordingVolumeLevel() { return 0; }
    public virtual int GetNumRecordingDevices() { return 0; }
    public virtual void SetContinuousStreaming(bool tf) { m_ContinuousStreaming = tf; }
    public virtual string GetDeviceName() { return string.Empty; }
    public virtual void SetRecordingDevice(string deviceName) { }
    public virtual void SetRecordingDevice(int deviceIndex) { }
    #endregion
}
