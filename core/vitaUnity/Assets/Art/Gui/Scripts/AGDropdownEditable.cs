using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AGDropdownEditable : MonoBehaviour
{
    public Dropdown m_dropdown;
    public Text m_dropdownText;
    public InputField m_inputField;

    void Update()
    {
        UpdateInputField();
    }

    public void OnInputFieldEndEdit()
    {
        //Debug.Log(m_inputField.text);
        if (m_dropdown.options.Count > 0)
            m_dropdown.options[m_dropdown.value].text = m_inputField.text;
        UpdateInputField();
        //Debug.Log(m_dropdown.options[m_dropdown.value].text);
    }

    public void OnDropdownValueChange()
    {
        UpdateInputField();
    }

    void UpdateInputField()
    {

        if (m_dropdown.options.Count > 0)
            m_inputField.text = m_dropdown.options[m_dropdown.value].text;
        else
            m_inputField.text = "";
    }
}
