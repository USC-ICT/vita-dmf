using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

public class UnitySequencerIO
{
    #region Constants
    const string Machinima = "machinima";
    #endregion

    #region Variables
    CutsceneEditor m_Timeline;
    BMLParser m_BMLParser;
    CutsceneEvent m_UtteranceAudioEvent;
    #endregion

    #region Properties
    CutsceneEditor Timeline
    {
        get { return m_Timeline; }
    }

    public bool UseMecanimEvents
    {
        set
        {
            m_BMLParser.EventCategoryName = value ? GenericEventNames.Mecanim : GenericEventNames.SmartBody;
        }
    }
    #endregion

    #region Functions
    public UnitySequencerIO(CutsceneEditor sequencer)
    {
        m_Timeline = sequencer;
        m_BMLParser = new BMLParser(OnAddBmlTiming, OnAddVisemeTiming, ParsedBMLEvent, OnFinishedReading, OnParsedCustomEvent);
        UseMecanimEvents = true;
    }

    public void OnAddBmlTiming(BMLParser.BMLTiming bmlTiming)
    {
        bool prevVal = Timeline.CreateBMLEvents;
        Timeline.CreateBMLEvents = true;
        Timeline.AddBmlTiming(bmlTiming.id, bmlTiming.time, bmlTiming.text);

        // reset
        Timeline.CreateBMLEvents = prevVal;
    }

    public void OnAddVisemeTiming(BMLParser.LipData lipData)
    {
        bool prevVal = Timeline.CreateBMLEvents;
        Timeline.CreateBMLEvents = true;
        Timeline.AddVisemeTiming(lipData.viseme, lipData.startTime, lipData.endTime);

        // reset
        Timeline.CreateBMLEvents = prevVal;
    }

    public bool LoadXMLString(string character, string xmlStr)
    {
        return m_BMLParser.LoadXMLString(character, xmlStr);
    }

    public bool LoadFile(string filePathAndName)
    {
        return m_BMLParser.LoadFile(filePathAndName);
    }

    void OnFinishedReading(bool succeeded, List<CutsceneEvent> createdEvents)
    {
        // get the name of the character that these events are associated with
        foreach (CutsceneEvent ce in createdEvents)
        {
            CutsceneEventParam characterParam = ce.FindParameter("character");
            if (characterParam != null && !string.IsNullOrEmpty(characterParam.stringData))
            {
                m_UtteranceAudioEvent.FindParameter("character").stringData = characterParam.stringData;
                break;
            }
        }

        // The sendvhmsg events from the parser have the correct timings, so copy them over
        // for the timeline event to use
        foreach (CutsceneEvent ce in createdEvents)
        {
            if (ce.FunctionName == "SendVHMsg")
            {
                CutsceneEvent timeLineEvent = Timeline.FindEventByID(ce.UniqueId);
                timeLineEvent.StartTime = ce.StartTime;
            }
        }
    }

    void OnParsedCustomEvent(XmlTextReader reader)
    {
        if (reader.Name == Machinima)
        {
            float zoom = 0;
            if (float.TryParse(reader["zoom"], out zoom))
            {
                Timeline.Zoom = zoom;
            }

            int numTrackGroups = 0;
            if (int.TryParse(reader["numGroups"], out numTrackGroups))
            {
                for (int i = Timeline.GroupManager.NumGroups; i < numTrackGroups; i++)
                {
                    Timeline.AddTrackGroup();
                }
            }
        }
    }

    public void ParsedBMLEvent(XmlTextReader reader, string type, CutsceneEvent ce)
    {
        if (ce != null && ce.FunctionName.Contains("SendVHMsg"))
        {
            string message = ce.FindParameter("message").stringData;

            if (message.Contains("vrAgentSpeech partial") || message.Contains("vrSpoke"))
            {
                // we don't want these messages
                return;
            }
        }
        else if (ce != null && ce.FunctionName.Contains("PlayViseme"))
        {
            if (reader["messageType"] == "visemeStop")
            {
                 // we don't want these messages
                return;
            }
        }

        const float minPosition = TimelineWindow.TrackStartingY + TimelineWindow.TrackHeight * 2; // the first 2 tracks are reserved
        float eventYPos = minPosition; // the first 2 tracks are reserved
        if (!string.IsNullOrEmpty(reader["ypos"]))
        {
            eventYPos = float.Parse(reader["ypos"]);
        }

        if (eventYPos < minPosition)
        {
            eventYPos = minPosition;
        }

        Vector2 eventPos = new Vector2(Timeline.GetPositionFromTime(Timeline.StartTime, Timeline.EndTime, ce.StartTime, Timeline.m_TrackScrollArea), eventYPos);
        CutsceneEvent newEvent = Timeline.CreateEventAtPosition(eventPos) as CutsceneEvent;

        ce.CloneData(newEvent);
        newEvent.m_UniqueId = ce.UniqueId;
        newEvent.StartTime = ce.StartTime;

        Timeline.ChangedCutsceneEventType(ce.EventType, newEvent);
        Timeline.ChangedEventFunction(newEvent, ce.FunctionName, ce.FunctionOverloadIndex);
        Timeline.CalculateTimelineLength();
        newEvent.GuiPosition.width = Timeline.GetWidthFromTime(newEvent.EndTime, newEvent.GuiPosition.x);
        newEvent.SetParameters(reader);

        // try to setup the reference to the character on the event using xml data
        if (newEvent.EventType == GenericEventNames.SmartBody || newEvent.EventType == GenericEventNames.Mecanim)
        {
            CutsceneEventParam characterParam = newEvent.FindParameter("character");
            characterParam.SetObjData(ce.FindParameter("character").objData);
            characterParam.stringData = ce.FindParameter("character").stringData;
        }

        // setup the length
        float length = ce.Length;
        CutsceneEventParam lengthParam = newEvent.GetLengthParameter();
        if (lengthParam != null)
        {
            lengthParam.SetLength(length);
            newEvent.SetEventLengthFromParameter(lengthParam.Name);
        }

        if (type == "speech")
        {
            m_UtteranceAudioEvent = newEvent;
        }
    }

    public void CreateXml(string filePathAndName, Cutscene cutscene)
    {
        StreamWriter outfile = null;

        for (int i = 0; i < cutscene.CutsceneEvents.Count; i++)
        {
            if (cutscene.CutsceneEvents[i].FunctionName == "Marker")
            {
                continue;
            }

            for (int j = i + 1; j < cutscene.CutsceneEvents.Count; j++)
            {
                if (cutscene.CutsceneEvents[i].Name == cutscene.CutsceneEvents[j].Name)
                {
                    EditorUtility.DisplayDialog("Error", string.Format("You can't have 2 events with the same name \'{0}\'. XML Not Saved!", cutscene.CutsceneEvents[i].Name), "Ok");
                    return;
                }
            }
        }

        try
        {
            outfile = new StreamWriter(string.Format("{0}", filePathAndName));
            outfile.WriteLine(@"<?xml version=""1.0""?>");
            outfile.WriteLine(@"<act>");
            outfile.WriteLine(@"  <bml xmlns:sbm=""http://sourceforge.net/apps/mediawiki/smartbody/index.php?title=SmartBody_BML"" xmlns:mm=""https://confluence.ict.usc.edu/display/VHTK/Home"">");
            //outfile.WriteLine(@"  <bml xmlns:mm=""https://vhtoolkit.ict.usc.edu/"">");
            outfile.WriteLine(string.Format(@"  <speech id=""visSeq_3"" ref=""{0}"" type=""application/ssml+xml"" />", Path.GetFileNameWithoutExtension(filePathAndName)));

            // write out mm cutscene meta data
            outfile.WriteLine(string.Format(@"  <{0} numGroups=""{1}"" numEvents=""{2}"" zoom=""{3}"" />", Machinima, cutscene.GroupManager.NumGroups, cutscene.NumEvents, Timeline.Zoom));

            //for (int i = 0; i < cutscene.GroupManager.NumGroups

            // sort the events by chronological order
            List<CutsceneEvent> timeSortedEvents = new List<CutsceneEvent>();
            timeSortedEvents.AddRange(cutscene.CutsceneEvents);
            timeSortedEvents.Sort(delegate(CutsceneEvent a, CutsceneEvent b)
            {
                return a.StartTime > b.StartTime ? 1 : -1;
            });

            // save out the events
            foreach (CutsceneEvent ce in timeSortedEvents)
            {
                string xmlString = ce.GetXMLString();
                if (!string.IsNullOrEmpty(xmlString))
                {
                    outfile.WriteLine(string.Format("    {0}", xmlString));
                }
            }

            outfile.WriteLine(@"  </bml>");
            outfile.WriteLine(@"</act>");
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("CreateXml failed: {0}", e.Message));
            EditorUtility.DisplayDialog("Error", string.Format("An error occured when saving \'{0}\'. XML was not properly saved!", Path.GetFileNameWithoutExtension(filePathAndName)), "Ok");
            outfile.WriteLine(@"  </bml>");
            outfile.WriteLine(@"</act>");
        }
        finally
        {
            if (outfile != null)
            {
                outfile.Close();
            }
        }
    }

    public void ListenToNVBG(bool listen)
    {
        VHMsgBase vhmsg = VHMsgBase.Get();
        if (vhmsg == null)
        {
            Debug.LogError(string.Format("Machinima Maker can't listen to NVBG because there is no VHMsgManager in the scene"));
            return;
        }

        if (listen)
        {
            vhmsg.SubscribeMessage("vrSpeak");
            vhmsg.RemoveMessageEventHandler(VHMsg_MessageEventHandler);
            vhmsg.AddMessageEventHandler(VHMsg_MessageEventHandler);
        }
        else
        {
            vhmsg.RemoveMessageEventHandler(VHMsg_MessageEventHandler);
        }
    }

    void VHMsg_MessageEventHandler(object sender, VHMsgBase.Message message)
    {
        Debug.Log("msg received: " + message.s);
        string[] splitargs = message.s.Split(" ".ToCharArray());
        if (splitargs[0] == "vrSpeak")
        {
            if (splitargs.Length > 4)
            {
                if (splitargs[3].Contains("idle"))
                {
                    // we don't want random idle fidgets
                    // i.e. vrSpeak Brad all idle-1193041418-823
                    return;
                }

                Cutscene selectedCutscene = Timeline.GetSelectedCutscene();
                if (selectedCutscene == null)
                {
                    EditorUtility.DisplayDialog("Error", "You need to create a cutscene so that NVBG can be listened to", "Ok");
                    return;
                }

                if (EditorUtility.DisplayDialog("Warning", string.Format("Do you want to overwrite cutscene {0} with the message from NVBG. You will lose all the current events?", selectedCutscene.CutsceneName), "Yes", "No"))
                {
                    //Timeline.AddCutscene();
                    Timeline.RemoveEvents(selectedCutscene.CutsceneEvents);
                    string character = splitargs[1];
                    string xml = String.Join(" ", splitargs, 4, splitargs.Length - 4);
                    m_BMLParser.LoadXMLString(character, xml);
                }
            }
        }
    }
    #endregion
}
