using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AGUtteranceDataFilter: MonoBehaviour
{
    public class AGUtteranceDictionary
    {
        public string utteranceName;
        public string utteranceText;
    }

    AGUtteranceData[] m_soundCmps;

    //void Start()
    //{
    //    //Example
    //    GetSounds(1, -1, GetScenarioBoolArrayByEnum(VitaGlobals.VitaScenarios.Hostile), GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Primary));
    //}

    //Public overloaded method
    public List<AGUtteranceData> GetSounds()                                                                        { return GetSoundsInternal(-1, -1, new bool[] { }, new bool[] { }); }
    public List<AGUtteranceData> GetSounds(int character)                                                           { return GetSoundsInternal(character, -1, new bool[] { }, new bool[] { }); }
    public List<AGUtteranceData> GetSounds(int character, int disposition)                                          { return GetSoundsInternal(character, disposition, new bool[] { }, new bool[] { }); }
    public List<AGUtteranceData> GetSounds(int character, bool[] scenario, bool[] responseType)                     { return GetSoundsInternal(character, -1, scenario, responseType); }
    public List<AGUtteranceData> GetSounds(int character, int disposition, bool[] scenario, bool[] responseType)    { return GetSoundsInternal(character, disposition, scenario, responseType); }

    /// <summary>
    /// All values are based on VitaGlobals indicies
    ///
    /// If a filter is not specified or null, all of it will return (ie. if no response type is specified, all response types will be returned)
    /// </summary>
    /// <param name="characterIndex"></param>
    /// <returns></returns>
    List<AGUtteranceData> GetSoundsInternal(int characterIndex, int disposition, bool[] scenario, bool[] responseType)
    {
        bool m_verboseDebug = false;
        //Debug.LogFormat("GetSounds({0}, {1}, {2}, {3})", characterIndex, disposition, scenario, responseType);

        if (m_verboseDebug)
        {
            string dispositionString = "--";
            if (disposition != -1) dispositionString = VitaGlobals.m_vitaMoods[disposition];
            string debugFunctionMessage = "GetSounds(" + VitaGlobals.m_vitaCharacterInfo[characterIndex].prefab + ", " + dispositionString + ", [";
            for (int i_scenario = 0; i_scenario < scenario.Length; i_scenario++)
            {
                if (scenario[i_scenario] == true)
                {
                    debugFunctionMessage += VitaGlobals.m_vitaScenarios[i_scenario] + ", ";
                }
            }

            debugFunctionMessage += "], [";

            for (int i_responseType = 0; i_responseType < responseType.Length; i_responseType++)
            {
                if (responseType[i_responseType] == true)
                {
                    debugFunctionMessage += VitaGlobals.m_vitaResponseTypes[i_responseType] + ", ";
                }
            }

            debugFunctionMessage += "])";

            Debug.LogFormat(debugFunctionMessage);
        }

        List<AGUtteranceData> m_filteredSounds = new List<AGUtteranceData>();
        GetInternalSoundCmpList();

        //Internal vars for mutability into "tailored" values later
        int m_characterIndex = characterIndex;
        int m_disposition = disposition;
        bool[] m_scenario = scenario;
        bool[] m_responseType = responseType;

        //Debug.LogFormat("Found {0} sound objects", m_soundCmps.Length);
        int i = 1;
        foreach (AGUtteranceData m_soundCmp in m_soundCmps)
        {
            //Debug.LogFormat("Checking sound object #{0}", i);
            //If the pass in disposition and responseType bool[]s do not have the correct size, we ignore that; used like a null value
            //Setting these "null" values to whatever the class we are checking against because we want a match on these ignored values
            if (characterIndex == -1)                                           m_characterIndex = m_soundCmp.m_character;
            if (disposition == -1)                                              m_disposition = m_soundCmp.m_disposition;
            if (m_scenario.Length != VitaGlobals.m_vitaScenarios.Length)        m_scenario = m_soundCmp.m_scenario;
            if (responseType.Length != VitaGlobals.m_vitaResponseTypes.Length)  m_responseType = m_soundCmp.m_responseType;

            //Debug.LogFormat("{4}\nSetting chr: {0}\nSetting scenario: {1}\nSetting disposition length: {2}\nSetting response type length: {3}\n",
            //    m_soundCmp.m_character, m_soundCmp.m_scenario, m_soundCmp.m_disposition, m_soundCmp.m_responseType.Length, m_soundCmp.gameObject.name);
            //Debug.LogFormat("{0}; Scenario: {1}, ResponseType: {2}", m_soundCmp.gameObject.name, GetBoolArrayCompareResults(m_scenario, m_soundCmp.m_scenario), GetBoolArrayCompareResults(m_responseType, m_soundCmp.m_responseType));

            if (m_characterIndex == m_soundCmp.m_character &&
                m_disposition == m_soundCmp.m_disposition &&
                GetBoolArrayCompareResults(m_scenario, m_soundCmp.m_scenario) &&
                GetBoolArrayCompareResults(m_responseType, m_soundCmp.m_responseType)
                )
            {
                m_filteredSounds.Add(m_soundCmp);
            }
            i += 1;
        }

        m_filteredSounds.Sort((a, b) => a.name.CompareTo(b.name));

        if (m_verboseDebug)
        {
            //Create return message
            string returnMessage = "Returning " + m_filteredSounds.Count.ToString() + " sounds (out of " + m_soundCmps.Length.ToString() + ").\n\n";
            foreach (var u in m_filteredSounds)
            {
                returnMessage += u.gameObject.name + "\n";
            }
            returnMessage += "\nRejected:\n";
            foreach (var u in m_soundCmps)
            {
                if (m_filteredSounds.Contains(u) == false)
                {
                    returnMessage += u.gameObject.name + "\n";
                }
            }

            Debug.LogFormat(returnMessage);
            //Debug.LogFormat("Returning {0} sounds (out of {1}).", m_filteredSounds.Count, m_soundCmps.Length);
        }

        return m_filteredSounds;
    }

    /// <summary>
    /// Returns a list of strings from the specified utterance list.
    /// </summary>
    /// <param name="utterances"></param>
    /// <returns></returns>
    public List<string> GetSoundStrings(List<AGUtteranceData> utterances)
    {
        List<string> m_soundStringList = new List<string>();
        foreach (AGUtteranceData u in utterances)
        {
            m_soundStringList.Add(u.name);
        }
        return m_soundStringList;
    }

    /// <summary>
    /// Helper function to generate a bool[] with "true" values indicated by the specified enum value
    /// </summary>
    /// <param name="enumValue">VitaGlobals.VitaMoods</param>
    /// <returns>
    /// bool[] { false, false, true };
    /// </returns>
    [Obsolete("Use GetScenarioBoolArrayByEnum() instead")]
    public virtual bool[] GetMoodBoolArrayByEnum(VitaGlobals.VitaMoods enumValue)
    {
        bool[] m_newBoolArray = new bool[VitaGlobals.m_vitaMoods.Length];
        m_newBoolArray[(int)enumValue] = true;
        return m_newBoolArray;
    }

    /// <summary>
    /// Helper function to generate a bool[] with "true" values indicated by the specified enum value
    /// </summary>
    /// <param name="enumValue">VitaGlobals.VitaScenarios</param>
    /// <returns>
    /// bool[] { false, false, true };
    /// </returns>
    public virtual bool[] GetScenarioBoolArrayByEnum(VitaGlobals.VitaScenarios enumValue)
    {
        bool[] m_newBoolArray = new bool[VitaGlobals.m_vitaScenarios.Length];
        m_newBoolArray[(int)enumValue] = true;
        return m_newBoolArray;
    }

    /// <summary>
    /// Helper function to generate a bool[] with "true" values indicated by the specified enum value
    /// </summary>
    /// <param name="enumValue">VitaGlobals.VitaResponseTypes</param>
    /// <returns>
    /// bool[] { false, false, true };
    /// </returns>
    public virtual bool[] GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes enumValue)
    {
        bool[] m_newBoolArray = new bool[VitaGlobals.m_vitaResponseTypes.Length];
        m_newBoolArray[(int)enumValue] = true;
        return m_newBoolArray;
    }

    public List<AGUtteranceDictionary> GetUtteranceDictionary()
    {
        List<AGUtteranceDictionary> m_utteranceDictionary = new List<AGUtteranceDictionary>();
        GetInternalSoundCmpList();

        foreach (AGUtteranceData sound in m_soundCmps)
        {
            AudioSpeechFile m_audioCmp = sound.gameObject.GetComponent<AudioSpeechFile>();
            m_utteranceDictionary.Add(new AGUtteranceDictionary() { utteranceName = sound.name, utteranceText = m_audioCmp.UtteranceText });
        }

        return m_utteranceDictionary;
    }

    void GetInternalSoundCmpList()
    {
        if (m_soundCmps == null)
        {
            m_soundCmps = GameObject.FindObjectsOfType<AGUtteranceData>();
        }
    }

    /// <summary>
    /// This function only checks for any true -> true matches.
    ///
    /// boolA               boolB
    /// {                   {
    ///     false,              true,
    ///     false,              false,
    ///     true,               false,
    ///     false,              false,
    ///     true     ----->     true
    /// }                   }
    /// </summary>
    /// <param name="boolA"></param>
    /// <param name="boolB"></param>
    /// <returns></returns>
    bool MatchBoolArrayPositivePositions(bool[] boolA, bool[] boolB)
    {
        bool returnValue = false;
        for (int i = 0; i < boolA.Length; i++ )
        {
            if (boolA[i] == true && boolB[i] == true)
            {
                returnValue = true;
                break;
            }
        }
        return returnValue;
    }

    bool IsArrayAllFalse(bool[] boolA)
    {
        bool returnValue = true;
        foreach (bool boolValue in boolA)
        {
            if (boolValue != boolA[0])
            {
                returnValue = false;
                break;
            }
        }
        return returnValue;
    }

    bool GetBoolArrayCompareResults(bool[] boolA, bool[] boolB)
    {
        bool returnValue = false;
        if (IsArrayAllFalse(boolA))
            returnValue = true;
        else
        {
            returnValue = MatchBoolArrayPositivePositions(boolA, boolB);
        }
        return returnValue;
    }
}
