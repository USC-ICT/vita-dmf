using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;

public class BMLEventHandler_Web: MonoBehaviour
{
    #region Variables
    public ICharacterController m_CharacterController;
    public Cutscene m_CutscenePrefab;
    //public SpeechBox_Web m_SpeechBox;
    public string m_AudioUrl = "https://vhtoolkitwww.ict.usc.edu/VHMsgAsp/Audio";
    BMLParser m_BMLParser;
    string vrSpokeMessage = "";
    string m_UtteranceNum = "";
    bool m_DownloadingUtterance;
    #endregion

    #region Properites
    public bool IsDownloadingUtterance
    {
        get { return m_DownloadingUtterance; }
    }
    #endregion

    #region Functions
    void Start()
    {
        m_BMLParser = new BMLParser(OnParsedBMLTiming, OnParsedVisemeTiming, OnParsedBMLEvent, OnFinishedReading, OnParsedCustomEvent);
        if (m_CharacterController as MecanimManager != null)
        {
            m_BMLParser.EventCategoryName = GenericEventNames.Mecanim;
        }
    }

    public bool LoadXMLString(string character, string xmlStr)
    {
        return m_BMLParser.LoadXMLString(character, xmlStr);
    }

    void OnParsedBMLTiming(BMLParser.BMLTiming bmlTiming) { }
    void OnParsedVisemeTiming(BMLParser.LipData lipData) { }
    void OnParsedBMLEvent(XmlTextReader reader, string eventType, CutsceneEvent ce)
    {
        if (eventType == "animation" || eventType == "sbm:animation")
        {
            ce.ChangedEventFunction("PlayAnim", 2);
            ce.SetParameters(reader);
        }
        else if (eventType == "speech")
        {
            ce.ChangedEventFunction("PlayAudio", 1);
            ce.SetParameters(reader);
            StartCoroutine(DownloadUtteranceCoroutine(ce, reader["ref"]));
            Debug.Log("reader[\"ref\"]: " + reader["ref"]);
        }
    }

    IEnumerator DownloadUtteranceCoroutine(CutsceneEvent audioEvent, string utteranceName)
    {
        m_DownloadingUtterance = true;
        WWW www = new WWW(string.Format("{0}/{1}.wav", m_AudioUrl, utteranceName));
        yield return www;

        while (!www.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
        while (!www.audioClip.isReadyToPlay)
#else
        while (www.audioClip.loadState == AudioDataLoadState.Unloaded ||
               www.audioClip.loadState == AudioDataLoadState.Loading)
#endif
        {
            yield return new WaitForEndOfFrame();
        }

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(string.Format("Failed to download utterance {0}", utteranceName));
        }

        CutsceneEventParam audioParam = audioEvent.FindParameter("uttID");
        audioParam.SetObjData(www.audioClip);
        audioParam.stringData = www.audioClip.name;
        audioParam.objData.name = utteranceName;
        m_DownloadingUtterance = false;
    }

    void OnFinishedReading(bool succeeded, List<CutsceneEvent> createdEvents)
    {
        Cutscene cs = (Cutscene)Instantiate(m_CutscenePrefab);
        vrSpokeMessage = string.Empty;

        foreach (CutsceneEvent ce in createdEvents)
        {
            ce.SetMetaData(m_CharacterController);
            cs.AddEvent(ce);

            if (ce.FunctionName == "SendVHMsg")
            {
                if (ce.m_Params[0].stringData.IndexOf("vrSpoke") != -1)
                {
                    vrSpokeMessage = ce.m_Params[0].stringData;
                    //cs.RemoveEvent(ce);
                }
            }
        }

        if (!string.IsNullOrEmpty(vrSpokeMessage))
        {
            VHMsgBase vhmsg = VHMsgBase.Get();
            if (vhmsg is VHMsgWebRequest)
            {
                ((VHMsgWebRequest)vhmsg).SetUrlParam("ClientNeedsResponse", "false");
            }
            m_UtteranceNum = vrSpokeMessage.Split(' ')[3];
            StartCoroutine(SendStartMessages(m_UtteranceNum));
        }

        cs.AddOnFinishedCutsceneCallback(OnFinishedCutscene);

        StartCoroutine(PlayCutscene(cs));
    }

    void OnParsedCustomEvent(XmlReader reader)
    {

    }

    IEnumerator PlayCutscene(Cutscene cs)
    {
        while (m_DownloadingUtterance)
        {
            yield return new WaitForEndOfFrame();
        }

        cs.Play();
    }

    void OnFinishedCutscene(Cutscene cs)
    {
        StartCoroutine(SendCompletionMessages());
        Destroy(cs.gameObject);
    }

    IEnumerator SendStartMessages(string uttNum)
    {
        VHMsgBase vhmsg = VHMsgBase.Get();
        vhmsg.SendVHMsg(string.Format("vrAgentBML Brad {0} start", uttNum));
        yield return new WaitForSeconds(0.2f);
        vhmsg.SendVHMsg(string.Format("vrAgentBML Brad {0} end complete", uttNum));
    }

    IEnumerator SendCompletionMessages()
    {
        //VHMsgBase vhmsg = VHMsgBase.Get();
        if (!string.IsNullOrEmpty(vrSpokeMessage))
        {
            //vhmsg.SendVHMsg(vrSpokeMessage);
            //yield return new WaitForSeconds(0.2f);
            //vhmsg.SendVHMsg(string.Format("vrAgentBML Brad {0} end complete", m_UtteranceNum));
        }

        //m_SpeechBox.TypingEnabled = true;
        yield break;
    }
    #endregion
}
