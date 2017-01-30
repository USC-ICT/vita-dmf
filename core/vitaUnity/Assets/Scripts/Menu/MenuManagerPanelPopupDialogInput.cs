using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Provides a framework for easily working with/populating an input dialog
/// </summary>
public class MenuManagerPanelPopupDialogInput : MonoBehaviour
{
    [SerializeField] Text m_title;
    [SerializeField] InputField m_inputField;
    [SerializeField] Button m_confirmBtn;
    [SerializeField] Button m_okBtn;
    [SerializeField] Button m_cancelBtn;
    [SerializeField] Button m_closeBtn;
    [SerializeField] Text m_confirmTxt;
    [SerializeField] Text m_okTxt;
    [SerializeField] Text m_cancelTxt;
    [SerializeField] GameObject m_oneBtnGrp;
    [SerializeField] GameObject m_twoBtnGrp;
    EventSystem m_eventSystem;

    void Awake()
    {
        m_title = VHUtils.FindChildRecursive(this.gameObject, "GuiTextPrefab_PopupTitle").GetComponent<Text>();
        m_inputField = VHUtils.FindChildRecursive(this.gameObject, "GuiInputPrefab_PopupInput").GetComponent<InputField>();

        m_confirmBtn = VHUtils.FindChildRecursive(this.gameObject, "GuiButtonPrefab_Confirm").GetComponent<Button>();
        m_okBtn = VHUtils.FindChildRecursive(this.gameObject, "GuiButtonPrefab_Ok").GetComponent<Button>();
        m_cancelBtn = VHUtils.FindChildRecursive(this.gameObject, "GuiButtonPrefab_Cancel").GetComponent<Button>();
        m_closeBtn = VHUtils.FindChildRecursive(this.gameObject, "GuiButtonPrefab_Close").GetComponent<Button>();

        m_confirmTxt = VHUtils.FindChildRecursive(m_confirmBtn.gameObject, "TextMain_NoStatus").GetComponent<Text>();
        m_okTxt = VHUtils.FindChildRecursive(m_okBtn.gameObject, "TextMain_NoStatus").GetComponent<Text>();
        m_cancelTxt = VHUtils.FindChildRecursive(m_cancelBtn.gameObject, "TextMain_NoStatus").GetComponent<Text>();

        m_oneBtnGrp = VHUtils.FindChildRecursive(this.gameObject, "ConfirmGrp");
        m_twoBtnGrp = VHUtils.FindChildRecursive(this.gameObject, "OptionGrp");
    }

    //Auto select input field
    void OnEnable()
    {
        if (m_eventSystem == null) m_eventSystem = GameObject.FindObjectOfType<EventSystem>();
        if (m_eventSystem == null) return;
        StartCoroutine(OnEnableCoroutine());
    }

    IEnumerator OnEnableCoroutine()
    {
        yield return new WaitForEndOfFrame();
        m_eventSystem.SetSelectedGameObject(m_inputField.gameObject);
    }

    void Update()
    {
        //Hitting enter auto-confirms the popup dialog
        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (m_oneBtnGrp.activeInHierarchy)  m_confirmBtn.onClick.Invoke();
            else                                m_okBtn.onClick.Invoke();
        }
    }

    public void SetDialog(string title, string message)                                                 { SetDialog(title, message, "Ok", "Cancel", false, null, null); }
    public void SetDialog(string title, string message, bool singleButton)                              { SetDialog(title, message, "Ok", "Cancel", singleButton, null, null); }
    public void SetDialog(string title, string message, string okButtonName)                            { SetDialog(title, message, okButtonName, "Cancel", false, null, null); }
    public void SetDialog(string title, string message, string okButtonName, string cancelButtonName)   { SetDialog(title, message, okButtonName, cancelButtonName, false, null, null); }
    public void SetDialog(string title, string message, string okButtonName, string cancelButtonName, bool singleButton,
        PopupInputDelegate okCallback, PopupInputDelegate cancelCallback)
    {
        m_oneBtnGrp.SetActive(singleButton);
        m_twoBtnGrp.SetActive(!singleButton);

        m_title.text = title;
        m_inputField.text = "";
        m_inputField.placeholder.GetComponent<Text>().text = message;
        m_okTxt.text = okButtonName;
        m_cancelTxt.text = cancelButtonName;
        m_confirmTxt.text = okButtonName;

        if (singleButton)
        {
            SetupButtonCallback(m_confirmBtn, okCallback);
            SetupButtonCallback(m_closeBtn, okCallback);
        }
        else
        {
            SetupButtonCallback(m_okBtn, okCallback);
            SetupButtonCallback(m_cancelBtn, cancelCallback);
            SetupButtonCallback(m_closeBtn, cancelCallback);
        }
    }

    public delegate void PopupInputDelegate(string input);

    void SetupButtonCallback(Button b, PopupInputDelegate cb)
    {
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(HideMenu);
        if (cb != null)
        {
            b.onClick.AddListener(delegate { cb(m_inputField.text); });
        }
    }

    void HideMenu()
    {
        gameObject.SetActive(false);
    }
}
