/* AGLightSettings.cs
 *
 * This gives a simple API to control setting character-specific lights. It is Designed to be used in
 * conjunction with a custom inspector script that enables users to easily toggle between declared light sets.
 *
 * Set up
 * Add this script component to your parent gameobject that holds all your light sets (ideally a prefab root).
 * To change light sets declaration, go into "Debug" mode in Unity's inpsector and modify the "m_sets" variable.
 *
 * Usage (Scripting)
 * 1. Set the selection index "m_selectedSetIndex"; Use "GetAllSetNames()" for a List<string> of declared sets.
 * 2. Call "UpdateState()"
 *
 * Joe Yip
 * 2015-Aug-14
 * yip@ict.usc.edu
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AGLightSettingsVita : MonoBehaviour
{
    #region Variables
    [Serializable]
    public class AGLightSettingsParam
    {
        public string m_setName;
        public GameObject m_setLightsAutoFound;
    }

    public List<AGLightSettingsParam> m_sets = new List<AGLightSettingsParam>();
    public int m_selectedSetIndex = 0;
    #endregion

    #region Private Functions
    /// <summary>
    /// Returns the GO based on the set name
    /// </summary>
    /// <param name="setName">String name of the set</param>
    /// <returns></returns>
    private GameObject GetSetGO(string setName)
    {
        GameObject GO = null;
        foreach (AGLightSettingsParam set in m_sets)
        {
            if (setName == set.m_setName)
            {
                GO = set.m_setLightsAutoFound;
                break;
            }
        }
        return GO;
    }

    /// <summary>
    /// Sets the active state of the given game object.
    /// </summary>
    /// <param name="go">GameObject</param>
    /// <param name="activeState">Boolean</param>
    private void SetActiveState(GameObject go, bool activeState)
    {
        if (go != null)
        {
            go.SetActive(activeState);
        }
    }

    /// <summary>
    /// Deactivates all GOs referenced in light sets.
    /// </summary>
    private void SetAllInactive()
    {
        RefreshGameObjectRefs(); //Make sure all GO refs are found/updated
        foreach (string setName in GetAllSetNames())
        {
            SetActiveState(GetSetGO(setName), false);
        }
    }

    /// <summary>
    /// Checks and returns provided parent for the named child.
    /// </summary>
    /// <param name="m_goName"></param>
    /// <param name="m_goParentName"></param>
    /// <returns></returns>
    private GameObject GetGameObject(string m_goName, string m_goParentName)
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

    /// <summary>
    /// Finds all gameObject references by string name. This will occur if the GO assignment is null,
    /// or if the reference GO name does not match the string name.
    /// </summary>
    private void RefreshGameObjectRefs()
    {
        foreach (AGLightSettingsParam m_set in m_sets)
        {
            //If GO ref is null
            if (m_set.m_setLightsAutoFound == null)
            {
                m_set.m_setLightsAutoFound = GetGameObject(m_set.m_setName, this.gameObject.name);
            }
            //If GO name does not match specified name
            else if (m_set.m_setLightsAutoFound.name != m_set.m_setName)
            {
                m_set.m_setLightsAutoFound = GetGameObject(m_set.m_setName, this.gameObject.name);
            }

            //Final check/warning
            if (m_set.m_setLightsAutoFound == null)
            {
                Debug.LogWarning(string.Format("Light set reference by the name '{0}' could not be found. Problems may arise.", m_set.m_setName));
            }
        }
    }
    #endregion

    void Start()
    {
        UpdateState ();
    }

    /// <summary>
    /// Sets the set by string name
    /// </summary>
    /// <param name="setName"></param>
    public void UseSetNamed(string setName)
    {
        //Set settings
        m_selectedSetIndex = GetAllSetNames().IndexOf(setName);
        UpdateState ();
    }

    /// <summary>
    /// Updates/refreshes the current state based on current settings
    /// </summary>
    public void UpdateState()
    {
        SetAllInactive ();
        List<string> goOrNoGo = GetAllSetNames();
        if (m_selectedSetIndex < goOrNoGo.Count && m_selectedSetIndex >= 0)
            SetActiveState(GetSetGO(goOrNoGo[m_selectedSetIndex]), true);
    }

    /// <summary>
    /// Returns a List<string> of names based on list object
    /// </summary>
    /// <returns>List<></returns>
    public List<string> GetAllSetNames()
    {
        List<string> setNames = new List<string>();
        foreach (AGLightSettingsParam set in m_sets)
        {
            setNames.Add(set.m_setName);
        }
        return setNames;
    }
}
