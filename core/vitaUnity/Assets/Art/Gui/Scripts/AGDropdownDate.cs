using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class AGDropdownDate : MonoBehaviour
{
    public Dropdown m_dropdownMonth;
    public Dropdown m_dropdownDay;
    public Dropdown m_dropdownYear;
    int m_startYear = 2016;

    void Start()
    {
        //Create year options
        List<Dropdown.OptionData> dropdownData = new List<Dropdown.OptionData>();
        for (int i = 0; i < 20; i++)
        {
            dropdownData.Add(new Dropdown.OptionData { text = (m_startYear + i).ToString(), image = null });
        }
        m_dropdownYear.ClearOptions();
        m_dropdownYear.options = dropdownData;
    }

    /// <summary>
    /// ie. 10/1/2016
    /// </summary>
    /// <param name="mmddyyyy"></param>
    public void SetDate(string mmddyyyy)
    {
        string[] dateData = mmddyyyy.Split('/');
        if (dateData.Length != 3)
        {
            Debug.LogError(string.Format("Specified date ({0}) is not in the correct format. Expecting MM/DD/YYYY", mmddyyyy));
        }

        foreach (Dropdown d in new List<Dropdown>() { m_dropdownMonth, m_dropdownDay, m_dropdownYear }) { SetListenersState(d, false); }
        m_dropdownMonth.value = Convert.ToInt16(dateData[0]) - 1;
        m_dropdownDay.value = Convert.ToInt16(dateData[1]) - 1;
        m_dropdownYear.value = Convert.ToInt16(dateData[2]) - m_startYear;
        foreach (Dropdown d in new List<Dropdown>() { m_dropdownMonth, m_dropdownDay, m_dropdownYear }) { SetListenersState(d, true); }
    }

    /// <summary>
    /// ie. 10/1/2016
    /// </summary>
    /// <returns></returns>
    public string GetDate()
    {
        return (m_dropdownMonth.value + 1).ToString().PadLeft(2, '0') + "/" + (m_dropdownDay.value + 1).ToString().PadLeft(2, '0') + "/" + (m_dropdownYear.value + m_startYear).ToString();
    }

    void SetListenersState(Dropdown dropdown, bool enable)
    {
        for (int i = 0; i < dropdown.onValueChanged.GetPersistentEventCount(); i++ )
        {
            dropdown.onValueChanged.SetPersistentListenerState(i, enable ? UnityEngine.Events.UnityEventCallState.EditorAndRuntime : UnityEngine.Events.UnityEventCallState.Off);
        }
    }

    public void OnValueChangeMonthYear()
    {
        int day = m_dropdownDay.value + 1; //Store current value
        int month = (m_dropdownMonth.value + 1);
        int year = (m_dropdownYear.value + m_startYear);
        int daysInMonth = DateTime.DaysInMonth(year, month);

        //Re-fill dropdown with correct number of days in month that year
        m_dropdownDay.ClearOptions();
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
        for (int i = 1; i <= daysInMonth; i++)
        {
            list.Add(new Dropdown.OptionData { text = i.ToString(), image = null });
        }
        m_dropdownDay.AddOptions(list);

        //Re-select day dropdown, after checks
        if (day > daysInMonth) day = daysInMonth;
        m_dropdownDay.value = day - 1;
    }
}
