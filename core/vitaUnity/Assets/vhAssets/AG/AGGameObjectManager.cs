using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple GameObject manager to activate/deactivate/toggle GOs.
/// </summary>
public class AGGameObjectManager : MonoBehaviour
{
    [Serializable]
    public class AGGameObjectSet
    {
        public string m_gameObjectSet;
        public string m_goName;
        public string m_goParentName;
        public GameObject m_go;
        public bool m_active = false;
    }

    //[HideInInspector]
    public int m_currentToggledGoSet = -1;

    public List<AGGameObjectSet> m_goSets = new List<AGGameObjectSet>();

    /// <summary>
    /// Deactivates all GOs in all sets
    /// </summary>
    public void DeactivateAll()
    {
        foreach (AGGameObjectSet m_goSet in m_goSets)
        {
            SetActive(m_goSet.m_gameObjectSet, false);
        }
    }

    /// <summary>
    /// Sets active state of a single GO
    /// </summary>
    /// <param name="m_goName"></param>
    public void SetActive(string m_goNameSet, bool m_activeState)
    {
        GetAllGameObjects();

        if (!VerifyExist(m_goNameSet))
        {
            Debug.LogWarning("GameObject set'" + m_goNameSet + "' not found, nothing happened.");
            return;
        }

        foreach (AGGameObjectSet m_goSet in m_goSets)
        {
            if (m_goSet.m_gameObjectSet == m_goNameSet)
            {
                if (m_goSet.m_go == null)
                {
                    m_goSet.m_go = GetGameObject(m_goSet.m_goName, m_goSet.m_goParentName);
                }
                else if (m_goSet.m_go.name != m_goSet.m_goName)
                {
                    m_goSet.m_go = GetGameObject(m_goSet.m_goName, m_goSet.m_goParentName);
                }
                m_goSet.m_go.SetActive(m_activeState);
                m_goSet.m_active = m_activeState;
                m_currentToggledGoSet = -1;
                break;
            }
        }
    }

    /// <summary>
    /// Enables a single GO and deactivates the rest
    /// </summary>
    /// <param name="m_goName"></param>
    public void Toggle(string m_goNameSet)
    {
        if (!VerifyExist(m_goNameSet))
        {
            Debug.LogWarning("GameObject '" + m_goNameSet + "' not found, nothing happened.");
            return;
        }
        DeactivateAll();
        SetActive(m_goNameSet, true);
        m_currentToggledGoSet = GetAllSetNames().IndexOf(m_goNameSet);
    }

    /// <summary>
    /// Checks if the string goName exists
    /// </summary>
    /// <param name="m_goName"></param>
    /// <returns></returns>
    public bool VerifyExist(string m_goName)
    {
        bool found = false;
        foreach (string m_goSetName in GetAllSetNames())
        {
            if (m_goSetName == m_goName)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public List<string> GetAllSetNames()
    {
        List<string> m_setNames = new List<string>();

        foreach (AGGameObjectSet m_goSet in m_goSets)
        {
            m_setNames.Add(m_goSet.m_gameObjectSet);
        }
        return m_setNames;
    }

    public void GetAllGameObjects()
    {
        foreach (AGGameObjectSet m_goSet in m_goSets)
        {
            m_goSet.m_go = GetGameObject(m_goSet.m_goName, m_goSet.m_goParentName);
        }
    }

    /// <summary>
    /// Checks and returns provided parent for the named child.
    /// </summary>
    /// <param name="m_goName"></param>
    /// <param name="m_goParentName"></param>
    /// <returns></returns>
    GameObject GetGameObject(string m_goName, string m_goParentName)
    {
        GameObject m_go = null;
        GameObject m_parentGo = GameObject.Find(m_goParentName);

        //Verify parent object
        if (m_parentGo == null)
        {
            Debug.LogWarning(String.Format("Parent object '{0}' could not be found (won't find child '{1}').", m_goParentName, m_goName));
            return null;
        }

        //Find child object
        foreach (Transform obj in this.gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (obj.name == m_goName)
            {
                m_go = obj.gameObject;
                break;
            }
        }
        if (m_go == null)
        {
            Debug.LogWarning(String.Format("GameObject '{0}' could not be found under '{1}'", m_goName, m_goParentName));
        }

        return m_go;
    }
}
