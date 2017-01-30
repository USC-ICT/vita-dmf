using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelInterviewRecord : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
    DBSession m_dbSession;

    GameObject m_headerName;
    GameObject m_eventLogContent;
    Text m_field_studentName;
    Text m_field_interviewer;
    Text m_field_evaluator;
    Text m_field_disposition;
    Text m_field_environment;
    Text m_field_sessionName;
    Text m_field_sessionLength;
    Text m_field_date;

    RectTransform m_suggestedScore_firstImpression;
    RectTransform m_suggestedScore_interviewResponses;
    RectTransform m_suggestedScore_selfPromoting;
    RectTransform m_suggestedScore_activeListening;
    RectTransform m_suggestedScore_closing;
    Text m_field_responseScore;

    RectTransform m_finalScore_firstImpression;
    RectTransform m_finalScore_interviewResponses;
    RectTransform m_finalScore_selfPromoting;
    RectTransform m_finalScore_activeListening;
    RectTransform m_finalScore_closing;
    InputField m_field_scoringNote;

    GameObject m_dialogTeacherPrefab;
    GameObject m_dialogStudentPrefab;
    GameObject m_dialogDataPrefab;
    //GameObject m_dialogAlarmPrefab;

    public GameObject m_characterUtterancesPrefab;
    GameObject m_characterUtterancesPrefabInstance = null;
    AGUtteranceDataFilter m_chrUttCmp;

    EntityDBVitaStudentSession m_selectedSessionEntity;
    #endregion

    void Start()
    {
        MenuManager m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.InterviewRecord);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");

        m_headerName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Names");
        m_eventLogContent = VHUtils.FindChildRecursive(menu, "Content_EventLog");
        m_field_studentName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_StudentName").GetComponent<Text>();
        m_field_interviewer = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Interviewer").GetComponent<Text>();
        m_field_evaluator = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Evaluator").GetComponent<Text>();
        m_field_disposition = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Disposition").GetComponent<Text>();
        m_field_environment = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Environment").GetComponent<Text>();
        m_field_sessionName = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_SessionName").GetComponent<Text>();
        m_field_sessionLength = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_SessionLength").GetComponent<Text>();
        m_field_date = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_Date").GetComponent<Text>();

        m_suggestedScore_firstImpression = VHUtils.FindChildRecursive(menu, "Image_FirstImpressions").GetComponent<RectTransform>();
        m_suggestedScore_interviewResponses = VHUtils.FindChildRecursive(menu, "Image_InterviewResponses").GetComponent<RectTransform>();
        m_suggestedScore_selfPromoting = VHUtils.FindChildRecursive(menu, "Image_SelfPromoting").GetComponent<RectTransform>();
        m_suggestedScore_activeListening = VHUtils.FindChildRecursive(menu, "Image_ActiveListening").GetComponent<RectTransform>();
        m_suggestedScore_closing = VHUtils.FindChildRecursive(menu, "Image_Closing").GetComponent<RectTransform>();
        m_field_responseScore = VHUtils.FindChildRecursive(menu, "GuiTextPrefab_ResponseScore").GetComponent<Text>();

        m_finalScore_firstImpression = VHUtils.FindChildRecursive(menu, "Image_FirstImpressionsFinal").GetComponent<RectTransform>();
        m_finalScore_interviewResponses = VHUtils.FindChildRecursive(menu, "Image_InterviewResponsesFinal").GetComponent<RectTransform>();
        m_finalScore_selfPromoting = VHUtils.FindChildRecursive(menu, "Image_SelfPromotingFinal").GetComponent<RectTransform>();
        m_finalScore_activeListening = VHUtils.FindChildRecursive(menu, "Image_ActiveListeningFinal").GetComponent<RectTransform>();
        m_finalScore_closing = VHUtils.FindChildRecursive(menu, "Image_ClosingFinal").GetComponent<RectTransform>();
        m_field_scoringNote = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Note").GetComponent<InputField>();

        //Event log prefabs
        m_dialogTeacherPrefab = VHUtils.FindChildRecursive(resources, "GuiDialogTeacherPrefab");
        m_dialogStudentPrefab = VHUtils.FindChildRecursive(resources, "GuiDialogStudentPrefab");
        m_dialogDataPrefab = VHUtils.FindChildRecursive(resources, "GuiDialogDataPrefab");
        //m_dialogAlarmPrefab = VHUtils.FindChildRecursive(resources, "GuiDialogAlarmPrefab");
    }

    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        ClearMenu();

        m_dbSession = GameObject.FindObjectOfType<DBSession>();
        m_dbSession.GetSession(VitaGlobals.m_interviewStudent, VitaGlobals.m_selectedSession, (session, error) =>
        {
            ////Error check
            m_selectedSessionEntity = session;
            if (m_selectedSessionEntity == null)
            {
                Debug.LogError("Current session is null");
            }

            //Create character utterance instance
            if (m_characterUtterancesPrefabInstance == null)
            {
                m_characterUtterancesPrefabInstance = Instantiate(m_characterUtterancesPrefab);
            }
            m_chrUttCmp = m_characterUtterancesPrefabInstance.GetComponent<AGUtteranceDataFilter>();

            //Get all users for nice names, calling delegate
            UpdateSessionInfo();
        });
    }

    public void OnMenuExit()
    {
    }

    #region UI Hooks
    public void BtnHome()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnBack()
    {
        GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu.Login);
    }

    public void BtnExitRecord()
    {
        if (VitaGlobals.m_returnMenu != MenuManager.Menu.None)
        {
            MenuManager.Menu returnMenu = VitaGlobals.m_returnMenu;
            VitaGlobals.m_returnMenu = MenuManager.Menu.None;
            GameObject.FindObjectOfType<MenuManager>().ChangeMenu(returnMenu);
        }
    }

    public void BtnExportPdf()
    {
        List<AGUtteranceDataFilter.AGUtteranceDictionary> utteranceDictionary = m_chrUttCmp.GetUtteranceDictionary();

        DBSession dbSession = GameObject.FindObjectOfType<DBSession>();
        string sessionDataText = dbSession.ExportSessionToText(m_selectedSessionEntity, utteranceDictionary, m_field_studentName.text, m_field_evaluator.text);
        string sessionDataHTML = dbSession.ExportSessionToHtml(m_selectedSessionEntity, utteranceDictionary, m_field_studentName.text, m_field_evaluator.text);

        Debug.Log(sessionDataText);

        string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        DateTime firstEventDateTime = m_selectedSessionEntity.events.Count > 0 ? VitaGlobals.TicksToDateTime(DBSession.EventData.ToEvent(m_selectedSessionEntity.events[0]).timeStamp) : DateTime.Now;
        string sessionName = firstEventDateTime.ToString("yyyyMMdd_HHmm");

        string filenameText = string.Format("{0}\\{1}_Session_{2}.txt", folder, VitaGlobals.m_interviewStudent, sessionName);
        string filenameHTML = string.Format("{0}\\{1}_Session_{2}.html", folder, VitaGlobals.m_interviewStudent, sessionName);
        string filenamePDF  = string.Format("{0}\\{1}_Session_{2}.pdf", folder, VitaGlobals.m_interviewStudent, sessionName);

        System.IO.File.WriteAllText(filenameText, sessionDataText);
        System.IO.File.WriteAllText(filenameHTML, sessionDataHTML);
        ConvertToPDF(filenameHTML, filenamePDF);

        string text = string.Format("Data was successfully exported.  You will find the file here:\n{0}\n\n Hit OK to bring up the folder.", filenameHTML);

        PopUpDisplay.Instance.DisplayOkCancel("Export Success", text, delegate { System.Diagnostics.Process.Start(folder); }, null);
    }

    void ConvertToPDF(string htmlFilename, string pdfFilename)
    {
        // \Assets\StreamingAssets\wkhtmltopdf.exe ict-ed_ses_131250996081078985_50a687f6-c201-48d1-ab57-d14c1180a013_2016-12-01_21-06-23.html ict-ed_ses_131250996081078985_50a687f6-c201-48d1-ab57-d14c1180a013_2016-12-01_21-06-23.pdf

        string wkhtmltopdf = Application.streamingAssetsPath + "/wkhtmltopdf/wkhtmltopdf.exe";
        string arguments = string.Format(@"""{0}"" ""{1}""", htmlFilename, pdfFilename);

        Debug.LogFormat("{0} {1}", wkhtmltopdf, arguments);

        System.Diagnostics.Process.Start(wkhtmltopdf, arguments);
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Function to instantiate widgets into a list's content object.
    /// </summary>
    /// <param name="m_widget"></param>
    /// <param name="m_listContent"></param>
    /// <param name="m_textObjName"></param>
    /// <param name="m_widgetDisplayString"></param>
    /// <returns></returns>
    GameObject AddWidgetToList(GameObject m_widget, GameObject m_listContent, string m_widgetName, string m_textObjName, string m_widgetDisplayString)
    {
        //Debug.LogFormat("AddWidgetToList({0})", m_widgetDisplayString);

        GameObject widgetGo = Instantiate(m_widget);
        GameObject widgetText = VHUtils.FindChildRecursive(widgetGo, m_textObjName);
        widgetGo.name = m_widgetName;
        widgetGo.SetActive(true);
        widgetGo.transform.SetParent(m_listContent.transform, false);
        widgetText.GetComponent<Text>().text = m_widgetDisplayString;
        return widgetGo;
    }

    List<int> CalculateMiasScore(List<List<int>> scores)
    {
        List<int> returnScore = new List<int>() { -1, -1, -1, -1, -1 };

        //Per MIAS category
        for (int i = 0; i < 5; i++)
        {
            int scoreTotal = 0;
            int numberOfScores = 0;

            //Add it all up
            foreach (List<int> score in scores)
            {
                if (score[i] != -1)
                {
                    scoreTotal += score[i];
                    numberOfScores += 1;
                }
            }

            //Average it out
            if (numberOfScores > 0)
            {
                returnScore[i] = (int)((float)scoreTotal / (float)numberOfScores);
            }
        }

        //Debug.LogFormat("Returning scores: {0}, {1}, {2}, {3}, {4}", returnScore[0], returnScore[1], returnScore[2], returnScore[3], returnScore[4]);
        return returnScore;
    }

    void ClearMenu()
    {
        VHUtils.DeleteChildren(m_eventLogContent.transform);
        m_field_studentName.text = "";
        m_field_interviewer.text = "";
        m_field_evaluator.text = "";
        m_field_disposition.text = "";
        m_field_environment.text = "";
        m_field_sessionName.text = "";
        m_field_sessionLength.text = "";
        m_field_date.text = "";
        m_field_scoringNote.text = "";
        List<RectTransform> m_scoreBarGraphs = new List<RectTransform>() { m_suggestedScore_firstImpression, m_suggestedScore_interviewResponses, m_suggestedScore_selfPromoting, m_suggestedScore_activeListening, m_suggestedScore_closing };
        List<RectTransform> m_scoreFinalBarGraphs = new List<RectTransform>() { m_finalScore_firstImpression, m_finalScore_interviewResponses, m_finalScore_selfPromoting, m_finalScore_activeListening, m_finalScore_closing };
        for (int i = 0; i < 5; i++)
        {
            m_scoreBarGraphs[i].localScale = new Vector3(0, m_scoreBarGraphs[i].localScale.y, m_scoreBarGraphs[i].localScale.z);
            m_scoreFinalBarGraphs[i].localScale = new Vector3(0, m_scoreBarGraphs[i].localScale.y, m_scoreBarGraphs[i].localScale.z);
        }
    }

    string CreateNiceSessionName(string ticksString)
    {
        string returnString = ticksString;
        try
        {
            DateTime sessionDateTime = VitaGlobals.TicksToDateTime(ticksString);
            returnString = "Session" + sessionDateTime.Month.ToString().PadLeft(2, '0') + sessionDateTime.Day.ToString().PadLeft(2, '0') + "_" + sessionDateTime.Hour.ToString().PadLeft(2, '0') + sessionDateTime.Minute.ToString().PadLeft(2, '0');
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("{0}", e.ToString());
        }

        return returnString;
    }

    void UpdateSessionInfo() { StartCoroutine(UpdateSessionInfoInternal()); }
    IEnumerator UpdateSessionInfoInternal()
    {
        #region Session Info
        //Session information
        //Get start & end interview event, which holds session info
        DBSession.EventData startEvent = null;
        DBSession.EventData endEvent = null;
        foreach (var eventString in m_selectedSessionEntity.events)
        {
            var sessionEvent = DBSession.EventData.ToEvent(eventString);

            if (sessionEvent.type == DBSession.EventType.StartSession)
            {
                startEvent = sessionEvent;
            }
            else if (sessionEvent.type == DBSession.EventType.EndSession)
            {
                endEvent = sessionEvent;
            }

            if (startEvent != null && endEvent != null)
            {
                break;
            }
        }

        string m_sessionLength = "--";
        if (endEvent == null)
        {
            Debug.LogWarning("No duration; Session did not end");
        }
        else
        {
            TimeSpan m_sessionTimeSpan = VitaGlobals.TicksToDateTime(endEvent.timeStamp).Subtract(VitaGlobals.TicksToDateTime(startEvent.timeStamp));
            m_sessionLength = string.Format("{0}h {1}m", m_sessionTimeSpan.Hours, m_sessionTimeSpan.Minutes);

        }

        string m_studentName = VitaGlobals.m_interviewStudent;
        string m_interviewerName = startEvent.character;
        string m_evaluatorName = startEvent.teacherUsername;
        string m_envName = startEvent.environment;
        bool studentProfileFound = false;
        bool teacherProfileFound = false;

        //Get all nice names
        //Teacher & student names
        DBUser m_dbUser = GameObject.FindObjectOfType<DBUser>();
        m_dbUser.GetUser(startEvent.teacherUsername, (user, error) =>
        {
            if (user == null)   m_evaluatorName = "(Deleted) " + startEvent.teacherUsername;
            else                m_evaluatorName = user.name;
            teacherProfileFound = true;
        });
        m_dbUser.GetUser(m_studentName, (user, error) =>
        {
            if (user == null)   m_studentName = "(Deleted) " + m_studentName;
            else                m_studentName = user.name;
            studentProfileFound = true;
        });
        while (!teacherProfileFound || !studentProfileFound)
            yield return new WaitForEndOfFrame();

        //Character name
        foreach (VitaGlobals.CharacterInfo chrInfo in VitaGlobals.m_vitaCharacterInfo)
        {
            if (startEvent.character == chrInfo.prefab)
            {
                m_interviewerName = chrInfo.displayName;
                break;
            }
        }

        //Env name
        foreach (VitaGlobals.BackgroundInfo bgInfo in VitaGlobals.m_vitaBackgroundInfo)
        {
            if (startEvent.environment == bgInfo.sceneName)
            {
                m_envName = bgInfo.displayName;
                break;
            }
        }

        //Fill in info
        m_field_studentName.text = m_studentName;
        m_field_interviewer.text = m_interviewerName;
        m_field_evaluator.text = m_evaluatorName;
        m_field_disposition.text = startEvent.disposition;
        m_field_environment.text = m_envName;
        m_field_sessionName.text = CreateNiceSessionName(startEvent.timeStamp.ToString());
        m_field_sessionLength.text = m_sessionLength;
        m_field_date.text = VitaGlobals.TicksToString(startEvent.timeStamp);
        #endregion

        #region Event Log
        //Fill event log with session events
        List<AGUtteranceDataFilter.AGUtteranceDictionary> m_utteranceDictionary = m_chrUttCmp.GetUtteranceDictionary();
        List<List<int>> m_miasScores = new List<List<int>>();
        List<int> m_miasScoreFinal = new List<int>();
        List<RectTransform> m_scoreBarGraphs = new List<RectTransform>() { m_suggestedScore_firstImpression, m_suggestedScore_interviewResponses, m_suggestedScore_selfPromoting, m_suggestedScore_activeListening, m_suggestedScore_closing };
        List<RectTransform> m_scoreFinalBarGraphs = new List<RectTransform>() { m_finalScore_firstImpression, m_finalScore_interviewResponses, m_finalScore_selfPromoting, m_finalScore_activeListening, m_finalScore_closing };
        m_field_scoringNote.text = "No final score or note given.";

        //Create list and sort by timestamp
        List<DBSession.EventData> eventDataList = new List<DBSession.EventData>();
        foreach (var eventString in m_selectedSessionEntity.events)
        {
            eventDataList.Add(DBSession.EventData.ToEvent(eventString));
        }
        eventDataList.Sort((x, y) => DateTime.Compare(VitaGlobals.TicksToDateTime(x.timeStamp), VitaGlobals.TicksToDateTime(y.timeStamp)));

        foreach (DBSession.EventData eventData in eventDataList)
        {
            //Debug.LogWarning(eventString);
            //var eventData = DBSession.EventData.ToEvent(eventString);

            //Teacher responses
            if (eventData.type == DBSession.EventType.PlayUtterance)
            {
                //Go through dictionary to check for utterance text
                foreach (AGUtteranceDataFilter.AGUtteranceDictionary entry in m_utteranceDictionary)
                {
                    if (eventData.utteranceId == entry.utteranceName)
                    {
                        AddWidgetToList(m_dialogTeacherPrefab, m_eventLogContent, "DialogTeacherPrefab_" + eventData.utteranceId, "Text", entry.utteranceText);
                        break;
                    }
                }
            }
            else if (eventData.type == DBSession.EventType.StudentResponse)
            {
                AddWidgetToList(m_dialogStudentPrefab, m_eventLogContent, "DialogStudentPrefab", "Text", eventData.responseText);
            }
            else if (eventData.type == DBSession.EventType.ScoreNote)
            {
                string s0 = eventData.miasScores[0] == -1 ? "-" : eventData.miasScores[0].ToString();
                string s1 = eventData.miasScores[1] == -1 ? "-" : eventData.miasScores[1].ToString();
                string s2 = eventData.miasScores[2] == -1 ? "-" : eventData.miasScores[2].ToString();
                string s3 = eventData.miasScores[3] == -1 ? "-" : eventData.miasScores[3].ToString();
                string s4 = eventData.miasScores[4] == -1 ? "-" : eventData.miasScores[4].ToString();
                string note = string.Format("F[ {0} ]    I[ {1} ]    S[ {2} ]    A[ {3} ]    C[ {4} ]\n{5}", s0, s1, s2, s3, s4, eventData.scoreNote);
                AddWidgetToList(m_dialogDataPrefab, m_eventLogContent, "DialogDataPrefab", "Text", note);
                m_miasScores.Add(eventData.miasScores);
            }
            else if (eventData.type == DBSession.EventType.ScoreNoteFinal)
            {
                //Final score
                m_miasScoreFinal = eventData.miasScores;
                for (int i = 0; i < 5; i++)
                {
                    m_scoreFinalBarGraphs[i].localScale = new Vector3(((float)m_miasScoreFinal[i] + 1f) / 5f, m_scoreBarGraphs[i].localScale.y, m_scoreBarGraphs[i].localScale.z);
                }
                m_field_scoringNote.text = eventData.scoreNote;
            }
        }
        #endregion

        #region Scores
        //Recommended score
        List<int> m_miasScoreRecommended = CalculateMiasScore(m_miasScores);
        for (int i = 0; i < 5; i++)
        {
            m_scoreBarGraphs[i].localScale = new Vector3(((float)m_miasScoreRecommended[i] + 1f) / 5f, m_scoreBarGraphs[i].localScale.y, m_scoreBarGraphs[i].localScale.z);
        }
        m_field_responseScore.text = ""; //Currently we do not implement response score, so this is unused
        #endregion
    }
    #endregion
}
