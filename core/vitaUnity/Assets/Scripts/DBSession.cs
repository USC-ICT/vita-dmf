using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

public class DBSession : MonoBehaviour
{
    public enum EventType
    {
        StartSession,
#if false
        StartTeacher,
        StartCharacter,
        StartEnvironment,
        StartDisposition,
#endif
        EndSession,
        PlayUtterance,
        StudentResponse,
        ScoreNote,
        ScoreNoteFinal,

        // GLOBAL events
        AccountCreation,
        Login,
        Logout,
        PracticeStarted,
        BadgeEarned,
        HomeworkSubmitted,
    }

    public enum SessionType
    {
        Interview,
        Practice,
    }

    public class EventData
    {
        public EventType type;
        public Int64 timeStamp;
        public SessionType sessionType;
        public string teacherUsername;
        public string character;
        public string environment;
        public string disposition;
        public string utteranceId;
        public string responseText;
        public List<int> miasScores;
        public string scoreNote;

#if false
        public EventType Type          { get { return type; } }
        public Int64 Timestamp         { get { return timeStamp; } }
        public SessionType SessionType { get { if (type != EventType.StartSession)     { Debug.LogError("EventData.SessionType - event is the wrong type!"); return SessionType.Interview; }  return sessionType; } }
        public string TeacherUsername  { get { if (type != EventType.StartTeacher)     { Debug.LogError("EventData.TeacherUsername - event is the wrong type!"); return ""; }  return teacherUsername; } }
        public string Character        { get { if (type != EventType.StartCharacter)   { Debug.LogError("EventData.Character - event is the wrong type!"); return ""; }  return character; } }
        public string Environment      { get { if (type != EventType.StartEnvironment) { Debug.LogError("EventData.Environment - event is the wrong type!"); return ""; }  return environment; } }
        public string Disposition      { get { if (type != EventType.StartDisposition) { Debug.LogError("EventData.Disposition - event is the wrong type!"); return ""; }  return disposition; } }
        public string UtteranceId      { get { if (type != EventType.PlayUtterance)    { Debug.LogError("EventData.UtteranceId - event is the wrong type!"); return ""; }  return utteranceId; } }
        public string ResponseText     { get { if (type != EventType.StudentResponse)  { Debug.LogError("EventData.ResponseText - event is the wrong type!"); return ""; }  return responseText; } }
        public List<int> Scores        { get { if (type != EventType.ScoreNote && type != EventType.ScoreNoteFinal)   { Debug.LogError("EventData.Scores - event is the wrong type!"); return null; }  return miasScores; } }
        public string ScoreNote        { get { if (type != EventType.ScoreNote && type != EventType.ScoreNoteFinal)   { Debug.LogError("EventData.ScoreNote - event is the wrong type!"); return ""; }  return scoreNote; } }
#endif

        public static EventData NewStartSession(SessionType sessionType, string teacherUsername, string character, string environment, string disposition) { return new EventData() { type = EventType.StartSession, timeStamp = VitaGlobals.CurrentTimeToTicks(), sessionType = sessionType, teacherUsername = teacherUsername, character = character, environment = environment, disposition = disposition }; }
#if false
        public static EventData NewStartSession(SessionType sessionType) { return new EventData() { type = EventType.StartSession, timeStamp = VitaGlobals.CurrentTimeToTicks(), sessionType = sessionType }; }
        public static EventData NewStartTeacher(string teacherUsername) { return new EventData() { type = EventType.StartTeacher, timeStamp = VitaGlobals.CurrentTimeToTicks(), teacherUsername = teacherUsername }; }
        public static EventData NewStartCharacter(string character) { return new EventData() { type = EventType.StartCharacter, timeStamp = VitaGlobals.CurrentTimeToTicks(), character = character }; }
        public static EventData NewStartEnvironment(string environment) { return new EventData() { type = EventType.StartEnvironment, timeStamp = VitaGlobals.CurrentTimeToTicks(), environment = environment }; }
        public static EventData NewStartDisposition(string disposition) { return new EventData() { type = EventType.StartDisposition, timeStamp = VitaGlobals.CurrentTimeToTicks(), disposition = disposition }; }
#endif
        public static EventData NewEndSession() { return new EventData() { type = EventType.EndSession, timeStamp = VitaGlobals.CurrentTimeToTicks() }; }
        public static EventData NewPlayUtterance(string utteranceId) { return new EventData() { type = EventType.PlayUtterance, timeStamp = VitaGlobals.CurrentTimeToTicks(), utteranceId = utteranceId }; }
        public static EventData NewStudentResponse(string responseText) { return new EventData() { type = EventType.StudentResponse, timeStamp = VitaGlobals.CurrentTimeToTicks(), responseText = responseText }; }
        public static EventData NewScoreNote(List<int> miasScores, string scoreNote) { return new EventData() { type = EventType.ScoreNote, timeStamp = VitaGlobals.CurrentTimeToTicks(), miasScores = new List<int>(miasScores), scoreNote = scoreNote }; }
        public static EventData NewScoreNoteFinal(List<int> miasScores, string scoreNote) { return new EventData() { type = EventType.ScoreNoteFinal, timeStamp = VitaGlobals.CurrentTimeToTicks(), miasScores = new List<int>(miasScores), scoreNote = scoreNote }; }
        public static EventData NewAccountCreation() { return new EventData() { type = EventType.AccountCreation, timeStamp = VitaGlobals.CurrentTimeToTicks() }; }
        public static EventData NewLogin() { return new EventData() { type = EventType.Login, timeStamp = VitaGlobals.CurrentTimeToTicks() }; }
        public static EventData NewLogout() { return new EventData() { type = EventType.Logout, timeStamp = VitaGlobals.CurrentTimeToTicks() }; }
        public static EventData NewPracticeStarted() { return new EventData() { type = EventType.PracticeStarted, timeStamp = VitaGlobals.CurrentTimeToTicks() }; }
        public static EventData NewBadgeEarned(/*string badge*/) { return new EventData() { type = EventType.BadgeEarned, timeStamp = VitaGlobals.CurrentTimeToTicks() }; }
        public static EventData NewHomeworkSubmitted(/*string homework*/) { return new EventData() { type = EventType.HomeworkSubmitted, timeStamp = VitaGlobals.CurrentTimeToTicks() }; }

        public static EventData ToEvent(string data) { return JsonUtility.FromJson<EventData>(data); }
        public static string ToJSONString(EventData data) { return JsonUtility.ToJson(data); }
    }


   // bool m_sessionOpInProgress = false;


    ArrayList AddItemInProgressList = new ArrayList();
    int AddItemInProgressInx = 0;


    void Start()
    {
        AddItemInProgressList.Clear();
        AddItemInProgressInx = 0;     
        AddItemInProgressList.Add(false);  
    }


    void Update()
    {
    }


    public string NewSessionName()
    {
        string time = VitaGlobals.CurrentTimeToString();
        string guid = Guid.NewGuid().ToString();
        return string.Format("{0}_{1}", time, guid);
    }


    public delegate void AddEventFunction(string sessionName, string student, List<string> eventData, SessionAddEventDelegate callback);

    IEnumerator WaitListSyncDynamoDB(AddEventFunction func, string sessionName, string student, List<string> eventData, SessionAddEventDelegate callback)
    {
        //  while (m_sessionOpInProgress)
        while (AddItemInProgressList.Count > AddItemInProgressInx && (bool)AddItemInProgressList[AddItemInProgressInx] )
        {
            yield return new WaitForEndOfFrame();
        }

        func(sessionName, student, eventData, callback);
    }


    public delegate void SessionCreateDelegate(string error);

    public void CreateSession(string sessionName, string username, string teacher, string organization, SessionCreateDelegate callback)
    {
       // m_sessionOpInProgress = true;

        EntityDBVitaStudentSession s = new EntityDBVitaStudentSession(username, sessionName, teacher, organization, true);

        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.AddEntity<EntityDBVitaStudentSession>(s, (result, error) =>
        {
          //  m_sessionOpInProgress = false;
            if (callback != null)
                callback(error);
        });
    }


    public void CreateSession(EntityDBVitaStudentSession sessionItem, SessionCreateDelegate callback)
    {
       // m_sessionOpInProgress = true;
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.AddEntity<EntityDBVitaStudentSession>(sessionItem, (result, error) =>
        {
           // m_sessionOpInProgress = false;
            if (callback != null)
                callback(error);
        });
    }

    public IEnumerator AddSessions(List<EntityDBVitaStudentSession> sessions)
    {
        // TODO: Add bulk Add() function

        // this function is used for backup/restore purposes.  In normal operation, use Create() instead

        int waitCount = sessions.Count;
        foreach (var session in sessions)
        {
            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaStudentSession>(session, (result, error) =>
            {
                waitCount--;

                if (!string.IsNullOrEmpty(error))
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogErrorFormat("AddOrganizations() failed - {0}", session.sessionname);
                        return;
                    }
                }
            });
        }

        while (waitCount > 0)
            yield return new WaitForEndOfFrame();
    }


    public delegate void SessionAddEventDelegate(string error);

    public void AddEvent(string sessionName, string student, DBSession.EventData eventData, SessionAddEventDelegate callback)
    {
        List<DBSession.EventData> events = new List<EventData>();
        events.Add(eventData);
        AddEvents(sessionName, student, events, callback);
    }

    public void AddEvents(string sessionName, string student, List<DBSession.EventData> eventData, SessionAddEventDelegate callback)
    {
        List<string> events = new List<string>();
        eventData.ForEach(e => events.Add(EventData.ToJSONString(e)));
        AddEvents(sessionName, student, events, callback);
    }

   
    public void AddGlobalEvent(string student, DBSession.EventData eventData, SessionAddEventDelegate callback)
    {
        AddEvent("GLOBAL", student, eventData, callback);
    }



    void AddEvents(string sessionName, string student, List<string> eventData, SessionAddEventDelegate callback)
    {
       // m_sessionOpInProgress = true;


        if(AddItemInProgressList.Count > AddItemInProgressInx && (bool)AddItemInProgressList[AddItemInProgressInx])
        {
            StartCoroutine(WaitListSyncDynamoDB(AddEvents, sessionName, student, eventData, callback));
            AddItemInProgressList.Add(false);
            return;
        }

        AddItemInProgressList[AddItemInProgressInx] = true;



        GetSession(student, sessionName, (session, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
               // m_sessionOpInProgress = false;

                 AddItemInProgressInx++;

                AddItemInProgressList[AddItemInProgressInx -1] = true;

                if(AddItemInProgressList.Count <= AddItemInProgressInx)  AddItemInProgressList.Add(false);

                if (callback != null)
                    callback(error);

                return;
            }

            if (session.events == null)
                session.events = new List<string>();

            foreach (var e in eventData)
            {
                if (session.events.Contains(e))
                {
                  //  m_sessionOpInProgress = false;

                  AddItemInProgressInx++;

                  AddItemInProgressList[AddItemInProgressInx -1] = true;

                  if(AddItemInProgressList.Count <= AddItemInProgressInx)  AddItemInProgressList.Add(false);

                    string error2 = "session event already exists : " + eventData;
                    if (callback != null)
                        callback(error2);

                    return;
                }
            }

            foreach (var e in eventData)
            {
                session.events.Add(e);
            }

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            vitaDynamoDB.AddEntity<EntityDBVitaStudentSession>(session, (result, error3) =>
            {
                // m_sessionOpInProgress = false;


                AddItemInProgressInx++;

                AddItemInProgressList[AddItemInProgressInx -1] = true;

                if(AddItemInProgressList.Count <= AddItemInProgressInx)  AddItemInProgressList.Add(false);

                if (callback != null)
                    callback(error3);
            });
        });
    }

 

    public delegate void GetAllSessionsDelegate(List<EntityDBVitaStudentSession> sessions, string error);

    public void GetAllSessions(GetAllSessionsDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetAllEntities<EntityDBVitaStudentSession>("VitaStudentSession", null, (result, error) =>
        {
            List<EntityDBVitaStudentSession> sessions = null;
            if (string.IsNullOrEmpty(error))
            {
                sessions = new List<EntityDBVitaStudentSession>((List<EntityDBVitaStudentSession>)result);
                sessions.Sort((a, b) => a.sessionname.CompareTo(b.sessionname));
                sessions.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(sessions, error);
        });
    }

    public void GetAllSessionsInOrganization(string organization, GetAllSessionsDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntities<EntityDBVitaStudentSession>(EntityDBVitaStudentSession.tableName, "organization-index", "organization", organization, null, (result, error) =>
        {
            List<EntityDBVitaStudentSession> sessions = null;
            if (string.IsNullOrEmpty(error))
            {
                sessions = new List<EntityDBVitaStudentSession>((List<EntityDBVitaStudentSession>)result);
                sessions.Sort((a, b) => a.sessionname.CompareTo(b.sessionname));
                sessions.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(sessions, error);
        });
    }

    public void GetAllSessionsForUser(string username, GetAllSessionsDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntities<EntityDBVitaStudentSession>(EntityDBVitaStudentSession.tableName, "username-index", "username", username, null, (result, error) =>
        {
            List<EntityDBVitaStudentSession> sessions =  new List<EntityDBVitaStudentSession>();
            if (string.IsNullOrEmpty(error))
            {
                sessions = new List<EntityDBVitaStudentSession>((List<EntityDBVitaStudentSession>)result);
                sessions.Sort((a, b) => a.sessionname.CompareTo(b.sessionname));
                sessions.ForEach((a) => a.FixNullLists());
            }

            if (callback != null)
                callback(sessions, error);
        });
    }

    public delegate void GetSessionDelegate(EntityDBVitaStudentSession session, string error);

    public void GetSession(string username, string sessionName, GetSessionDelegate callback)
    {
        VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
        vitaDynamoDB.GetEntity<EntityDBVitaStudentSession>(username, sessionName, null, (result, error) =>
        {
            EntityDBVitaStudentSession session = (EntityDBVitaStudentSession)result;
            if (string.IsNullOrEmpty(error) && session != null)
            {
                session.FixNullLists();
            }

            if (callback != null)
                callback(session, error);
        });
    }

    public void GetGlobalSession(string username, GetSessionDelegate callback)
    {
        GetSession(username, "GLOBAL", (session, error) =>
        {
            if (callback != null)
                callback(session, error);
        });
    }

    public delegate void DeleteSessionDelegate(string error);

    public void DeleteAllSessions(DeleteSessionDelegate callback)
    {
        GetAllSessions((sessions, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            int sessionCount = sessions.Count;

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            foreach (var item in sessions)
            {
                vitaDynamoDB.DeleteEntity<EntityDBVitaStudentSession>(item.username, item.sessionname, (result2, error2) =>
                {
                    sessionCount--;

                    if (!string.IsNullOrEmpty(error2))
                    {
                        if (callback != null)
                            callback(error2);
                    }

                    if (sessionCount == 0)
                    {
                        if (callback != null)
                            callback(error2);
                    }
                });
            }

            /*
            vitaDynamoDB.BatchDelete<EntityDBVitaStudentSession>(sessions, (ob, res2) =>
            {
                if (!string.IsNullOrEmpty(res2))
                {
                    if (callback != null) callback(res2);
                }
            });
            */
        });
    }

    public void DeleteAllSessionsForUser(string username, DeleteSessionDelegate callback)
    {
        GetAllSessionsForUser(username, (sessions, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(error);

                return;
            }

            int sessionCount = sessions.Count;

            if(sessionCount == 0)
            {
                if (callback != null)
                    callback("");

                return;
            }

            VitaDynamoDB vitaDynamoDB = GameObject.Find("AWS").GetComponent<VitaDynamoDB>();
            foreach (var item in sessions)
            {
                vitaDynamoDB.DeleteEntity<EntityDBVitaStudentSession>(item.username, item.sessionname, (result2, error2) =>
                {
                    sessionCount--;

                    if (!string.IsNullOrEmpty(error2))
                    {
                        if (callback != null)
                            callback(error2);
                    }

                    if (sessionCount == 0)
                    {
                        if (callback != null)
                            callback(error2);
                    }
                });
            }

            /*
            vitaDynamoDB.BatchDelete<EntityDBVitaStudentSession>(sessions, (ob, res2) =>
            {
                if (!string.IsNullOrEmpty(res2))
                {
                    if (callback != null) callback(res2);
                }
            });
            */
        });
    }


    public delegate void ExportSessionToTextDelegate(string exportedData, string error);

    public void ExportSessionToText(string username, string sessionName, List<AGUtteranceDataFilter.AGUtteranceDictionary> utteranceDictionary, string studentName, string teacherName, ExportSessionToTextDelegate callback)
    {
        ExportSessionToString(username, sessionName, false, utteranceDictionary, studentName, teacherName, callback);
    }

    public void ExportSessionToHtml(string username, string sessionName, List<AGUtteranceDataFilter.AGUtteranceDictionary> utteranceDictionary, string studentName, string teacherName, ExportSessionToTextDelegate callback)
    {
        ExportSessionToString(username, sessionName, true, utteranceDictionary, studentName, teacherName, callback);
    }

    public string ExportSessionToText(EntityDBVitaStudentSession session, List<AGUtteranceDataFilter.AGUtteranceDictionary> utteranceDictionary, string studentName, string teacherName)
    {
        return ExportSessionToString(session, false, utteranceDictionary, studentName, teacherName);
    }

    public string ExportSessionToHtml(EntityDBVitaStudentSession session, List<AGUtteranceDataFilter.AGUtteranceDictionary> utteranceDictionary, string studentName, string teacherName)
    {
        return ExportSessionToString(session, true, utteranceDictionary, studentName, teacherName);
    }

    void ExportSessionToString(string username, string sessionName, bool htmlFormat, List<AGUtteranceDataFilter.AGUtteranceDictionary> utteranceDictionary, string studentName, string teacherName, ExportSessionToTextDelegate callback)
    {
        GetSession(username, sessionName, (session, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (callback != null)
                    callback(null, error);

                return;
            }

            string sessionData = ExportSessionToString(session, htmlFormat, utteranceDictionary, studentName, teacherName);

            if (callback != null)
                callback(sessionData, error);
        });
    }

    string ExportSessionToString(EntityDBVitaStudentSession session, bool htmlFormat, List<AGUtteranceDataFilter.AGUtteranceDictionary> utteranceDictionary, string studentName, string teacherName)
    {
        Dictionary<EventType, string> eventColors = new Dictionary<EventType, string>()
        {
            { EventType.StartSession,    "000000" },
            { EventType.EndSession,      "000000" },
            { EventType.PlayUtterance,   "0099FF" },
            { EventType.StudentResponse, "37A428" },
            { EventType.ScoreNote,       "E46C0A" },
            { EventType.ScoreNoteFinal,  "E46C0A" },
        };

        string headerTitle = @"<p class=MsoNormal align=center style='text-align:center'>
            <b style='mso-bidi-font-weight:normal'>
            <span style='font-family:""Arial"",""sans-serif""'>
            {0}
            <o:p></o:p>
            </span>
            </b>
            </p>" + "\r\n";

        string headerEntry = @"<p class=MsoNormal style='tab-stops:1.5in'>
            <b style='mso-bidi-font-weight:normal'>
            <span style='font-family:""Arial"",""sans-serif""'>{0}</span>
            </b>
            <span style='font-family:""Arial"",""sans-serif""'><span style='mso-tab-count:1'></span>{1}<o:p></o:p></span>
            </p>" + "\r\n";

        string headerSeparator = @"<p class=MsoNormal style='border:none;mso-border-bottom-alt:solid windowtext 1.5pt;padding:0in;mso-padding-alt:0in 0in 1.0pt 0in'>
            <span style='font-family:""Arial"",""sans-serif""'>
            <o:p>
            &nbsp;
            </o:p>
            </span>
            </p>
            <hr>
            <p class=MsoNormal style='margin-left:2.0in;text-indent:-2.0in'>
            <o:p>
            &nbsp;
            </o:p>
            </p>" + "\r\n";

        string eventEntry = @"<p class=MsoNormal style='margin-left:2.0in;text-indent:-2.0in;tab-stops:67.5pt'><span style='font-family:""Arial"",""sans-serif""'>[{0}]</span><span style='mso-tab-count:1'></span>
            <span style='font-family:""Arial"",""sans-serif""'><span style='color:#{2}'>{1}<o:p></o:p></span></span>
            </p>" + "\r\n";

        if (!htmlFormat)
        {
            headerTitle = @"{0}" + "\r\n";
            headerEntry = @"{0} {1}" + "\r\n";
            headerSeparator = @"" + "\r\n\r\n";
            eventEntry = @"[{0}] {2}{1}" + "\r\n";

            foreach (var key in eventColors.Keys.ToList())
                eventColors[key] = "";
        }

#if false
            <p 
            class=MsoNormal
            align=center
            style='text-align:center'>
            <b style='mso-bidi-font-weight:normal'>
            <span style='font-family:"Arial","sans-serif"'>
            VITA SESSION LOG
            <o:p></o:p>
            </span>
            </b>
            </p>

            <p 
            class=MsoNormal 
            style='tab-stops:1.5in'>
            <b style='mso-bidi-font-weight:normal'>
            <span style='font-family:"Arial","sans-serif"'>
            Session:
            </span>
            </b>
            <span style='font-family:"Arial","sans-serif"'>
             
            <span style='mso-tab-count:1'>
                                
            </span>
            1206_0859
            <o:p></o:p>
            </span>
            </p>

            <p 
            class=MsoNormal
            style='tab-stops:1.5in'>
            <b style='mso-bidi-font-weight:normal'>
            <span style='font-family:"Arial","sans-serif"'>
            Date:
            </span>
            </b>
            <span style='font-family:"Arial","sans-serif"'>
             
            <span style='mso-tab-count:1'>
            </span>
            12/06/2016
            <o:p></o:p>
            </span>
            </p>




            <p class=MsoNormal style='border:none;mso-border-bottom-alt:solid windowtext 1.5pt;padding:0in;mso-padding-alt:0in 0in 1.0pt 0in'>
            <span style='font-family:"Arial","sans-serif"'>
            <o:p>
            &nbsp;
            </o:p>
            </span>
            </p>



            <p class=MsoNormal style='margin-left:2.0in;text-indent:-2.0in'>
            <o:p>
            &nbsp;
            </o:p>
            </p>


            <p class=MsoNormal style='margin-left:2.0in;text-indent:-2.0in'>
            [12/6/2016 11:27:09 <span class=GramE>AM</span>] 
            <b style='mso-bidi-font-weight:normal'>
            Session initiated.
            </b>
            </p>


            <p class=MsoNormal style='margin-left:2.0in;text-indent:-2.0in;tab-stops:67.5pt'>
            [12/6/2016 11:27:09 <span class=GramE>AM</span>]
            <span style='mso-tab-count:1'>
                          
            </span>
            <span style='color:#0099FF'>
            Maria: "Hey, thanks for coming in. How's it going?"
            <o:p></o:p>
            </span>
            </p>


            <p class=MsoNormal style='margin-left:2.0in;text-indent:-2.0in'>
            [12/6/2016 11:27:09 <span class=GramE>AM</span>] 
            <span style='mso-tab-count:1'>
                         
            </span>
            <span style='color:#E46C0A;mso-themecolor:accent6;mso-themeshade:191;mso-style-textfill-fill-color:#E46C0A;mso-style-textfill-fill-themecolor:accent6;mso-style-textfill-fill-alpha:100.0%;mso-style-textfill-fill-colortransforms:lumm=75000'>
            Final Scores: (3<span class=GramE>,2,0,1,0</span>) "Room for improvement"
            </span>
            </p>


            <p class=MsoNormal>
            [12/6/2016 11:27:09 <span class=GramE>AM</span>]
            <b style='mso-bidi-font-weight:normal'>
            Session concluded.
            </b>
            <o:p></o:p>
            </p>
#endif



        string sessionData = "";

        List<string> events = new List<string>(session.events);
        events.Sort((a, b) => EventData.ToEvent(a).timeStamp.CompareTo(EventData.ToEvent(b).timeStamp));


        string interviewer = "unknown";
        string environment = "unknown";
        string disposition = "unknown";

        foreach (string item in events)
        {
            EventData ev = EventData.ToEvent(item);

            if (ev.type == EventType.StartSession)
            {
                interviewer = ev.character;
                interviewer = VitaGlobals.m_vitaCharacterInfo.Find(c => c.prefab == interviewer).displayName;
                environment = ev.environment;
                disposition = ev.disposition;
            }
        }


        // header
        {
            string text = "";

            DateTime firstEventDateTime = events.Count > 0 ? VitaGlobals.TicksToDateTime(EventData.ToEvent(events[0]).timeStamp) : DateTime.Now;

            text = string.Format("VITA SESSION LOG");
            sessionData += string.Format(headerTitle, text);
            text = string.Format("{0}", firstEventDateTime.ToString("yyyyMMdd_HHmm"));
            sessionData += string.Format(headerEntry, "Session:", text);
            text = string.Format("{0}", firstEventDateTime.ToString("MM/dd/yyyy"));
            sessionData += string.Format(headerEntry, "Date:", text);
            text = string.Format("{0}", studentName);
            sessionData += string.Format(headerEntry, "Student:", text);
            text = string.Format("{0}", teacherName);
            sessionData += string.Format(headerEntry, "Evaluator:", text);
            text = string.Format("{0}", interviewer);
            sessionData += string.Format(headerEntry, "Interviewer:", text);
            text = string.Format("{0}", environment);
            sessionData += string.Format(headerEntry, "Environment:", text);
            text = string.Format("{0}", disposition);
            sessionData += string.Format(headerEntry, "Disposition:", text);

            sessionData += headerSeparator;
        }

        string boldStart = htmlFormat ? "<b>" : "";
        string boldEnd   = htmlFormat ? "</b>" : "";

        foreach (string item in events)
        {
            string itemText = "";

            EventData ev = EventData.ToEvent(item);

            string timestamp = VitaGlobals.TicksToDateTime(ev.timeStamp).ToString("MM'/'dd'/'yyyy hh':'mm':'ss tt");

            if (ev.type == DBSession.EventType.PlayUtterance)
            {
                string utterance = ev.utteranceId;
                if (utteranceDictionary != null)
                {
                    AGUtteranceDataFilter.AGUtteranceDictionary entry = utteranceDictionary.Find(i => i.utteranceName == ev.utteranceId);
                    if (entry != null)
                    {
                        utterance = entry.utteranceText.TrimEnd();
                    }
                }

                string interviewerFirst = interviewer.Split(' ')[0];  // only take the first name
                utterance = string.Format(@"{0}: ""{1}""", interviewerFirst, utterance);
                itemText = string.Format(eventEntry, timestamp, utterance, eventColors[ev.type]);
            }
            else if (ev.type == EventType.StudentResponse)
            {
                string text = string.Format(@"{0}: ""{1}""", studentName, ev.responseText);
                itemText = string.Format(eventEntry, timestamp, text, eventColors[ev.type]);
            }
            else if (ev.type == EventType.ScoreNote)
            {
                string s0 = ev.miasScores[0] == -1 ? "-" : ev.miasScores[0].ToString();
                string s1 = ev.miasScores[1] == -1 ? "-" : ev.miasScores[1].ToString();
                string s2 = ev.miasScores[2] == -1 ? "-" : ev.miasScores[2].ToString();
                string s3 = ev.miasScores[3] == -1 ? "-" : ev.miasScores[3].ToString();
                string s4 = ev.miasScores[4] == -1 ? "-" : ev.miasScores[4].ToString();
                string text = string.Format(@"Session Score Note: ({0}, {1}, {2}, {3}, {4}) ""{5}""", s0, s1, s2, s3, s4, ev.scoreNote);
                itemText = string.Format(eventEntry, timestamp, text, eventColors[ev.type]);
            }
            else if (ev.type == EventType.ScoreNoteFinal)
            {
                string s0 = ev.miasScores[0] == -1 ? "-" : ev.miasScores[0].ToString();
                string s1 = ev.miasScores[1] == -1 ? "-" : ev.miasScores[1].ToString();
                string s2 = ev.miasScores[2] == -1 ? "-" : ev.miasScores[2].ToString();
                string s3 = ev.miasScores[3] == -1 ? "-" : ev.miasScores[3].ToString();
                string s4 = ev.miasScores[4] == -1 ? "-" : ev.miasScores[4].ToString();
                string text = string.Format(@"Final Scores: ({0}, {1}, {2}, {3}, {4}) ""{5}""", s0, s1, s2, s3, s4, ev.scoreNote);
                itemText = string.Format(eventEntry, timestamp, text, eventColors[ev.type]);
            }
            else if (ev.type == EventType.StartSession)
            {
                string text = string.Format("{0}Session initiated.{1}", boldStart, boldEnd);
                itemText = string.Format(eventEntry, timestamp, text, eventColors[ev.type]);
            }
            else if (ev.type == EventType.EndSession)
            {
                string text = string.Format("{0}Session concluded.{1}", boldStart, boldEnd);
                itemText = string.Format(eventEntry, timestamp, text, eventColors[ev.type]);
            }

            sessionData += itemText;
        }

        return sessionData;
    }
}
