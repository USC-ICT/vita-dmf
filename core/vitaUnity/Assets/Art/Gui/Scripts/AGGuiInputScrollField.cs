using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This updates RectTransform values based on current state of the text input field so user can view their input and the
/// input caret follows the scroll view.
/// </summary>
public class AGGuiInputScrollField : MonoBehaviour
{
    public InputField m_inputField;
    public Scrollbar m_scrollBar;
    public RectTransform m_textContent;
    internal GameObject m_inputCaret;
    RectTransform m_inputCaretRect;
    Rect m_caretOrigValue;

    void Start()
    {
    }

    void Update()
    {
        //Get the auto-generated caret gameobject
        if (m_inputCaret == null)
        {
            foreach (Transform xform in this.gameObject.GetComponentsInChildren<Transform>())
            {
                if (xform.gameObject.name.Contains("Input Caret"))
                {
                    m_inputCaret = xform.gameObject;
                    m_inputCaret.transform.SetParent(m_textContent.transform);
                    break;
                }
            }
        }
        else
        {
            m_inputCaretRect = m_inputCaret.GetComponent<RectTransform>();
            m_caretOrigValue = m_inputCaretRect.rect;
        }

        //Change text and caret pivots based on if we've reached the bottom of the scroll area
        if (m_scrollBar.size < 0.99)
        {
            m_textContent.pivot = new Vector2(m_textContent.pivot.x, 0);
            if (m_inputCaretRect != null)           m_inputCaretRect.pivot = new Vector2(m_inputCaretRect.pivot.x, 0);
        }
        else
        {
            m_textContent.pivot = new Vector2(m_textContent.pivot.x, 1);
            if (m_inputCaretRect != null)           m_inputCaretRect.pivot = new Vector2(m_inputCaretRect.pivot.x, 1);
        }

        //This sets the pivot and position of the rect to follow the scrolling/expanding text
        if (m_inputCaretRect != null)               m_inputCaretRect.rect.Set(m_caretOrigValue.x, m_caretOrigValue.y + m_textContent.rect.y, m_caretOrigValue.width, m_caretOrigValue.height);
    }

    public delegate void SetTextDelegate(string error);

    public void SetText(string textString, SetTextDelegate callback)
    {
        StartCoroutine(SetTextInternal(textString, callback));
    }

    /// <summary>
    /// This forces the expansion of Unity's text field auto-tructating what it assumes as excess substring (around line 1357 in InputField.cs)
    /// 
    /// Using a callback method since this function will take a number of frames as well as EventSystem's focus to complete. This is helpful for UI screens with
    /// multiple AGGuiInputScrollField components, so one happens after the other, letting all text "render" out.
    /// </summary>
    /// <param name="textString"></param>
    /// <returns></returns>
    IEnumerator SetTextInternal(string textString, SetTextDelegate callback)
    {
        m_inputField.text = textString;

        EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            GameObject originalSelection = eventSystem.currentSelectedGameObject;
            bool inputFieldInteractable = m_inputField.interactable;
            m_inputField.interactable = true;

            float chrsInALine = (float)this.gameObject.GetComponent<RectTransform>().rect.width / (float)m_inputField.textComponent.fontSize;
            float timesF = (float)textString.Length / chrsInALine;
            int timesI = (int)timesF * 2;

            //Add more refresh passes per newline char (\n)
            string[] textStringNewLineChecker = textString.Split('\n');
            timesI += (textStringNewLineChecker.Count() - 1);
            
            for (int i = 0; i < timesI; i++)
            {
                yield return new WaitForEndOfFrame();

                eventSystem.SetSelectedGameObject(m_inputField.gameObject);
                m_inputField.caretPosition = m_inputField.text.Length - 1;
                GameObject textObj = m_inputField.textComponent.gameObject;
                textObj.SetActive(false);
                textObj.SetActive(true);
                m_inputField.caretPosition = 0;
            }

            m_inputField.interactable = inputFieldInteractable; //Reset interactivity to original setting
            eventSystem.SetSelectedGameObject(originalSelection);
        }

        if (callback != null)
            callback("");
    }
}
