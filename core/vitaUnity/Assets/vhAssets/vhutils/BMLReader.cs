﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used as a helper class to obtain word timing info from bml files using the BMLParser class
/// </summary>
public class BMLReader
{
    #region Constants
    public class UtteranceTiming
    {
        public List<BMLParser.BMLTiming> m_Timings = new List<BMLParser.BMLTiming>();
        public List<BMLParser.LipData> m_LipData = new List<BMLParser.LipData>();
        public List<BMLParser.CurveData> m_CurveData = new List<BMLParser.CurveData>();

        public void Clear()
        {
            m_Timings.Clear();
            m_LipData.Clear();
            m_CurveData.Clear();
        }

        /// <summary>
        /// Iterates through m_CurveData and returns the lowest start time
        /// </summary>
        /// <returns>The earliest curve time.</returns>
        public float GetEarliestCurveTime()
        {
            float min = float.MaxValue;
            for (int i = 0; i < m_CurveData.Count; i++)
            {
                float curveStartTime = m_CurveData[i].GetTime(0);
                if (min > curveStartTime)
                {
                    min = curveStartTime;
                }
            }
            return min;
        }
    }
    #endregion

    #region Variables
    BMLParser m_Parser;
    UtteranceTiming m_UtteranceTiming = new UtteranceTiming();
    #endregion

    #region Functions
    public BMLReader()
    {
        m_Parser = new BMLParser(OnParsedBMLTiming, OnParsedVisemeTiming, OnParsedCurveData);
    }

    public UtteranceTiming ReadBmlFile(string bmlFilePath)
    {
        m_UtteranceTiming.Clear();
        m_Parser.LoadFile(bmlFilePath);
        return m_UtteranceTiming;
    }

    public UtteranceTiming ReadBml(string bmlText)
    {
        m_UtteranceTiming.Clear();
        m_Parser.LoadBMLString(bmlText, false);
        return m_UtteranceTiming;
    }

    void OnParsedBMLTiming(BMLParser.BMLTiming bmlTiming)
    {
        m_UtteranceTiming.m_Timings.Add(bmlTiming);
    }

    void OnParsedVisemeTiming(BMLParser.LipData lipData)
    {
        m_UtteranceTiming.m_LipData.Add(lipData);
    }

    void OnParsedCurveData(BMLParser.CurveData curveData)
    {
        m_UtteranceTiming.m_CurveData.Add(curveData);
    }
    #endregion
}
