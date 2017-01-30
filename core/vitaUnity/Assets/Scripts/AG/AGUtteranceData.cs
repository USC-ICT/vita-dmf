using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AGUtteranceData : MonoBehaviour
{
    public int m_character = -1;
    public bool[] m_scenario = new bool[VitaGlobals.m_vitaScenarios.Length];
    public int m_disposition = -1;
    public bool[] m_responseType = new bool[VitaGlobals.m_vitaResponseTypes.Length];
}
