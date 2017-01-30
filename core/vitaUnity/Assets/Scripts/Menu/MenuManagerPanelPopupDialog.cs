using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Provides a framework for easily working with/populating the error dialog
/// </summary>
public class MenuManagerPanelPopupDialog : MonoBehaviour
{
    [SerializeField] Text m_title;
    [SerializeField] Text m_message;
    [SerializeField] Button m_confirmBtn;
    [SerializeField] Button m_okBtn;
    [SerializeField] Button m_cancelBtn;
    [SerializeField] Button m_closeBtn;
    [SerializeField] Text m_confirmTxt;
    [SerializeField] Text m_okTxt;
    [SerializeField] Text m_cancelTxt;
    [SerializeField] GameObject m_oneBtnGrp;
    [SerializeField] GameObject m_twoBtnGrp;


    void Awake()
    {
        m_title = VHUtils.FindChildRecursive(this.gameObject, "GuiTextPrefab_PopupTitle").GetComponent<Text>();
        m_message = VHUtils.FindChildRecursive(this.gameObject, "GuiTextPrefab_PopupText").GetComponent<Text>();

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

    void Update()
    {
    }

    public void SetDialog(string title, string message)                                                 { SetDialog(title, message, "Ok", "Cancel", false, null, null); }
    public void SetDialog(string title, string message, bool singleButton)                              { SetDialog(title, message, "Ok", "Cancel", singleButton, null, null); }
    public void SetDialog(string title, string message, string okButtonName)                            { SetDialog(title, message, okButtonName, "Cancel", false, null, null); }
    public void SetDialog(string title, string message, string okButtonName, string cancelButtonName)   { SetDialog(title, message, okButtonName, cancelButtonName, false, null, null); }
    public void SetDialog(string title, string message, string okButtonName, string cancelButtonName, bool singleButton,
        UnityAction okCallback, UnityAction cancelCallback)
    {
        m_oneBtnGrp.SetActive(singleButton);
        m_twoBtnGrp.SetActive(!singleButton);

        m_title.text = title;
        m_message.text = message;
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

    void SetupButtonCallback(Button b, UnityAction cb)
    {
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(HideMenu);
        if (cb != null)
        {
            b.onClick.AddListener(cb);
        }
    }

    void HideMenu()
    {
        gameObject.SetActive(false);
    }
}
