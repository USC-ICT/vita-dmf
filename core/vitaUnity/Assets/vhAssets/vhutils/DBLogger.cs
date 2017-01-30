using UnityEngine;
using System.Collections;

public class DBLogger : MonoBehaviour
{
    [SerializeField] string m_serverUrl =  "https://vhtoolkitwww.ict.usc.edu/WebMQ/Backend/DBServer/index.php/api/weblogger/fireevent";

    [SerializeField] string m_projectName = "Default";
    [SerializeField] string m_sessionName = "Session";
    [SerializeField] string m_participantId = "Player";
    [SerializeField] string m_platform = "Windows";


    public string ServerUrl { get { return m_serverUrl; } set { m_serverUrl = value; } }
    public string ProjectName { get { return m_projectName; } set { m_projectName = value; } }
    public string SessionName { get { return m_sessionName; } set { m_sessionName = value; } }
    public string ParticipantId { get { return m_participantId; } set { m_participantId = value; } }
    public string Platform { get { return m_platform; } set { m_platform = value; } }


    public void FireEvent(string eventName, string eventData)
    {
        StartCoroutine(FireEventInternal(eventName, eventData));
    }

    IEnumerator FireEventInternal(string eventName, string eventData)
    {
        WWWForm Form = new WWWForm();
        Form.AddField("ProjectName",        ProjectName);
        Form.AddField("SessionName",        SessionName);
        Form.AddField("ParticipantName",    ParticipantId);
        Form.AddField("Platform",           Platform);
        Form.AddField("ev",                 eventName);
        Form.AddField("evData",             eventName + " " + eventData);

        WWW www = new WWW(ServerUrl, Form);

        //Debug.Log(serverURL);

        yield return www;

        while (!www.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.error != null)
        {
            Debug.LogWarningFormat("FAIL: {0}", www.error);
        }

        //Debug.Log("SUCCESS");
        Debug.LogFormat("SUCCESS: {0}", www.text);
    }
}
