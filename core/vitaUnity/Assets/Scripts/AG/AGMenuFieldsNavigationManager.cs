using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class AGMenuFieldsNavigationManager : MonoBehaviour
{
    public enum AGUITypes
    {
        InputField, Button, Dropdown
    }

    [Serializable]
    public class AGMenuField
    {
        public string MenuName;
        public GameObject MenuField;
        public AGUITypes MenuType;
        internal bool isCurrentField;
    }

    public List<AGMenuField> m_menuFields = new List<AGMenuField>();
    EventSystem m_eventSystem;

    void Start()
    {
        m_eventSystem = GameObject.FindObjectOfType<EventSystem>();
        SetSelectedField(GetNextField());

        //Add listeners to all managed menu fields for update states
        foreach (AGMenuField menuField in m_menuFields)
        {
            AGMenuField menuFieldCopy = menuField;
            if (menuField.MenuType == AGUITypes.InputField)
            {
                menuField.MenuField.GetComponent<InputField>().onEndEdit.AddListener(delegate { SetSelectedField(menuFieldCopy); });
            }
            else if (menuField.MenuType == AGUITypes.Button)
            {
                menuField.MenuField.GetComponent<Button>().onClick.AddListener(delegate { SetSelectedField(menuFieldCopy); });
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            while (true)
            {
                AGMenuField last = GetLastField();
                if (last.MenuField.activeInHierarchy == false)
                {
                    last.isCurrentField = true;
                }
                else
                {
                    SetSelectedField(last);
                    break;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            while (true)
            {
                AGMenuField next = GetNextField();
                if (next.MenuField.activeInHierarchy == false)
                {
                    next.isCurrentField = true;
                }
                else
                {
                    SetSelectedField(next);
                    break;
                }
            }
        }

        if (m_eventSystem.currentSelectedGameObject != null)
        {
            foreach (AGMenuField menuField in m_menuFields)
            {
                menuField.isCurrentField = false;
                if (m_eventSystem.currentSelectedGameObject == menuField.MenuField)
                {
                    menuField.isCurrentField = true;
                }
            }
        }
    }

    AGMenuField GetFieldClass(GameObject field)
    {
        AGMenuField returnField = m_menuFields[0];
        foreach (AGMenuField menuField in m_menuFields)
        {
            if (menuField.MenuField == field)
            {
                returnField = menuField;
            }
        }
        return returnField;
    }

    /// <summary>
    /// Gets the next UI field
    /// </summary>
    /// <returns></returns>
    AGMenuField GetNextField()
    {
        if (m_menuFields.Count == 0)
        {
            return null;
        }

        //Set it to the first one by default
        AGMenuField returnField = m_menuFields[0];
        for (int i = 0; i < m_menuFields.Count; i++)
        {
            if (m_menuFields[i].isCurrentField)
            {
                //If within threshold, return the next. If not, the "default" set above will work.
                if (i + 1 <= m_menuFields.Count - 1)
                {
                    returnField = m_menuFields[i + 1];
                }
            }
        }
        return returnField;
    }

    /// <summary>
    /// Gets the last UI field
    /// </summary>
    /// <returns></returns>
    AGMenuField GetLastField()
    {
        if (m_menuFields.Count == 0)
        {
            return null;
        }

        //Set it to the first one by default
        AGMenuField returnField = m_menuFields[m_menuFields.Count - 1];
        for (int i = m_menuFields.Count-1; i >= 0; i += -1)
        {
            if (m_menuFields[i].isCurrentField)
            {
                //If within threshold, return the next. If not, the "default" set above will work.
                if (i - 1 >= 0)
                {
                    returnField = m_menuFields[i - 1];
                }
            }
        }
        return returnField;
    }

    void SetSelectedField(AGMenuField field)
    {
        if (m_eventSystem.alreadySelecting)
            return;

        if (!this.gameObject.activeInHierarchy)
            return;

        if (field == null || !field.MenuField.activeInHierarchy)
            return;

        // if canvas component is disabled (we're loading), we can't select UI objects.  See https://app.asana.com/0/73327800800553/208420648895821
        if (!GameObject.Find("Canvas").GetComponent<Canvas>().enabled)
            return;

        StartCoroutine(SetSelectedFieldCoroutine(field));
    }

    IEnumerator SetSelectedFieldCoroutine(AGMenuField field)
    {
        m_eventSystem.SetSelectedGameObject(field.MenuField);
        yield return new WaitForEndOfFrame();

        //Update list of selected
        foreach (AGMenuField menuField in m_menuFields)
        {
            menuField.isCurrentField = false;
        }
        field.isCurrentField = true;
    }
}
