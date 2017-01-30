using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LogCallbackHandler : MonoBehaviour
{
    protected List<Application.LogCallback> m_callbacks = new List<Application.LogCallback>();


    void Awake()
    {
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
        Application.RegisterLogCallback(LogCallback);
#else
        Application.logMessageReceived += LogCallback;
#endif
    }


    void OnDestroy()
    {
        Application.logMessageReceived -= LogCallback;
    }

    void Start()
    {
    }


    void LogCallback(string logString, string stackTrace, LogType type)
    {
        foreach (var callback in m_callbacks)
        {
            callback(logString, stackTrace, type);
        }
    }


    public void AddCallback(Application.LogCallback callback)
    {
        m_callbacks.Add(callback);
    }

    public void RemoveCallback(Application.LogCallback callback)
    {
        m_callbacks.Remove(callback);
    }
}
