using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VitaGlobalsUI : MonoBehaviour
{
    public static string m_asteriskString = " * ";

    public static Color m_uiNormalColor =           new Color(1f, 1f, 1f, 162f/255f);
    public static Color m_uiHighlightColor =        new Color(1f, 1f, 1f, 180f / 255f);
    public static Color m_uiHighlightYellowColor =  new Color(249f/255f, 202f/255f, 22f/255f, 180f/255f);

    public static bool m_unsavedChanges = false;   // are there unsaved changes in the current menu, and we need to prompt the user?
    public static string m_unsavedChangesTitle = "Warning";
    public static string m_unsavedChangesMessage = "There are unsaved changes on this screen.  Are you sure you want to change to a different menu?";

    /// <summary>
    /// Function to instantiate widgets into a list's content object.
    /// </summary>
    /// <param name="m_widget"></param>
    /// <param name="m_listContent"></param>
    /// <param name="m_textObjName"></param>
    /// <param name="m_widgetDisplayString"></param>
    /// <returns></returns>
    /// //Need to go through and replace this in other scripts (Joe)
    public static GameObject AddWidgetToList(GameObject m_widget, GameObject m_listContent, string m_widgetName, string m_textObjName, string m_widgetDisplayString)
    {
        m_widgetDisplayString = m_widgetDisplayString.Replace("\n", "");
        //Debug.LogFormat("AddWidgetToList({0})", m_widgetDisplayString);
        GameObject widgetGo = Instantiate(m_widget);
        widgetGo.name = m_widgetName;
        widgetGo.transform.SetParent(m_listContent.transform, false);
        widgetGo.SetActive(true);
        if (m_textObjName != null)
        {
            GameObject widgetText = VHUtils.FindChildRecursive(widgetGo, m_textObjName);
            widgetText.GetComponent<Text>().text = m_widgetDisplayString;
        }
        return widgetGo;
    }

    //Replaces *InList() (Joe)
    public static T GetEntryInList<T>(string entryName, string comparedProperty, List<T> listOfEntries)
    {
        T returnEntry = default(T);
        foreach (var item in listOfEntries)
        {
            if ((string)item.GetType().GetProperty(comparedProperty).GetValue(item, null) == entryName)
            {
                returnEntry = item;
                break;
            }
        }
        return returnEntry;
    }

    public static string PruneAsterisk(string stringValue)
    {
        if (stringValue.LastIndexOf(m_asteriskString) >= 0)
        {
            stringValue = stringValue.Substring(0, stringValue.Length - m_asteriskString.Length);
        }

        return stringValue;
    }

    public static void SetTextWithAsteriskIfChanges<T>(T textObj, bool changes)
    {
        string textObjString = (string)textObj.GetType().GetProperty("text").GetValue(textObj, null);
        textObj.GetType().GetProperty("text").SetValue(textObj, SetTextWithAsteriskIfChangesInternal(textObjString, changes), null);
    }
    public static void SetTextWithAsteriskIfChanges<T>(Dropdown dropdownObj, int dropdownValue, bool changes) //Specifically for Dropdowns
    {
        string dropdownObjString = dropdownObj.options[dropdownValue].text;
        dropdownObj.options[dropdownValue].text = SetTextWithAsteriskIfChangesInternal(dropdownObjString, changes);
    }
    static string SetTextWithAsteriskIfChangesInternal(string stringValue, bool changes)
    {
        string returnString = stringValue;
        if (changes)
        {
            if (returnString.LastIndexOf(m_asteriskString) < 0)
            {
                returnString = returnString + m_asteriskString;
            }
        }
        else
        {
            if (returnString.LastIndexOf(m_asteriskString) >= 0)
            {
                returnString = returnString.Substring(0, returnString.Length - m_asteriskString.Length);
            }
        }
        return returnString;
    }

    /// <summary>
    /// This sorts a "session" list as compared to the "DB" aka "original" list after a new entry is added, so anything new (aka not on DB) will stay at the bottom of the "session" list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cl"></param>
    public static void SortListAfterAddingNew<T>(List<T> sessionList, List<T> dbList, string comparedProperty)
    {
        //Create list of "new" classes
        List<T> tempList = new List<T>();
        foreach (var c in sessionList)
        {
            if (GetEntryInList<T>((string)c.GetType().GetProperty(comparedProperty).GetValue(c, null), comparedProperty, dbList) == null)
            {
                tempList.Add(c);
            }
        }

        //Remove all "new" from "current" session's list
        foreach (var c in tempList)
        {
            sessionList.Remove(c);
        }

        //Add them back, so they will be sorted at the end, whilst the rest would mirror m_classesOriginal's order (with new ones hanging at the end)
        foreach (var c in tempList)
        {
            sessionList.Add(c);
        }
    }
}
