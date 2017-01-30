using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System;
using System.Collections.Generic;

public class TtsReader : MonoBehaviour
{
    #region Constants
    public class TtsData
    {
        public List<WordTiming> m_WordTimings = new List<WordTiming>();
        public List<MarkData> m_Marks = new List<MarkData>();
    }

    public class WordTiming
    {
        public float start;
        public float end;
        public List<VisemeData> m_VisemesUsed = new List<VisemeData>();
        public List<MarkData> m_Marks = new List<MarkData>();

        public WordTiming(float _start, float _end)
        {
            start = _start;
            end = _end;
        }

        public float Duration { get { return end - start; } }
    }

    public class VisemeData
    {
        public float start;
        public float articulation;
        public string type = "";

        public VisemeData(float _start, float _articulation, string _type)
        {
            start = _start;
            articulation = _articulation;
            type = _type;
        }
    }

    public class MarkData
    {
        public string name = "";
        public float time;

        public MarkData(string _name, float _time)
        {
            name = _name;
            time = _time;
        }
    }
    #endregion

    #region Variables
    //List<TtsTiming> m_Timings = new List<TtsTiming>();
    #endregion

    #region Functions
    public TtsData ReadTtsXml(string xmlStr, out string audioFilePath)
    {
        //m_Character = character;
        //bool succeeded = true;
        StringReader xml = null;
        XmlTextReader reader = null;
        TtsData ttsData = null;
        audioFilePath = "";

        try
        {
            xml = new StringReader(xmlStr);
            reader = new XmlTextReader(xml);
            ttsData = ParseTts(reader, out audioFilePath);
        }
        catch (Exception e)
        {
            //succeeded = false;
            Debug.LogError(string.Format("Failed when loading. Error: {0} {1}. couldn't load string {2}", e.Message, e.InnerException, xmlStr));
        }
        finally
        {
            if (xml != null)
            {
                xml.Close();
            }

            if (reader != null)
            {
                reader.Close();
            }
        }

        return ttsData;
    }

    TtsData ParseTts(XmlTextReader reader, out string audioFilePath)
    {
        TtsData ttsData = new TtsData();
        List<WordTiming> timings = new List<WordTiming>();
        audioFilePath = "";

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                if (reader.Name == "soundFile")
                {
                    audioFilePath = reader["name"];
                }
                if (reader.Name == "word")
                {
                    WordTiming wordTiming = CreateWordTimingData(reader["start"], reader["end"]);
                    timings.Add(wordTiming);
                }
                if (reader.Name == "mark")
                {
                    MarkData markData = new MarkData(reader["name"], float.Parse(reader["time"]));
                    ttsData.m_Marks.Add(markData);
                }
                if (reader.Name == "viseme")
                {
                    VisemeData visemeData = CreateVisemeData(reader["start"], reader["articulation"], reader["type"]);
                    if (visemeData != null)
                    {
                        if (timings.Count > 0)
                        {
                            timings[timings.Count - 1].m_VisemesUsed.Add(visemeData);
                        }
                        else
                        {

                        }
                    }
                }
                break;
            }
        }

        ttsData.m_WordTimings = timings;

        return ttsData;
    }

    WordTiming CreateWordTimingData(string start, string end)
    {
        float startTime;
        if (!float.TryParse(start, out startTime))
        {
            Debug.LogError("Failed to parse start time");
            return null;
        }

        float endTime;
        if (!float.TryParse(end, out endTime))
        {
            Debug.LogError("Failed to parse endTime");
            return null;
        }

        return new WordTiming(startTime, endTime);
    }

    VisemeData CreateVisemeData(string start, string articulation, string type)
    {
        float startTime;
        if (!float.TryParse(start, out startTime))
        {
            Debug.LogError("Failed to parse start time");
            return null;
        }

        float articulationAmount;
        if (!float.TryParse(articulation, out articulationAmount))
        {
            Debug.LogError("Failed to parse articulation");
            return null;
        }

        return new VisemeData(startTime, articulationAmount, type);
    }

    /// <summary>
    /// The key of the dictionary will be T0, T1, Tx .....
    /// </summary>
    /// <returns>The marked words.</returns>
    /// <param name="wordTimings">Word timings.</param>
    public Dictionary<string, WordTiming> GetMarkedWords(List<WordTiming> wordTimings)
    {
        Dictionary<string, WordTiming> markedWords = new Dictionary<string, WordTiming>();
        for (int i = 0; i < wordTimings.Count; i++)
        {
            markedWords.Add("T" + i.ToString(), wordTimings[i]);
        }
        return markedWords;
    }
    #endregion
}
