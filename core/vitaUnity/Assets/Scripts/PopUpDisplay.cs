using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PopUpDisplay : MonoBehaviour
{
    #region Variables
    [SerializeField] MenuManagerPanelPopupDialog m_Popup;
    [SerializeField] MenuManagerPanelPopupDialogInput m_PopupInput;
    Canvas m_Canvas;
    static PopUpDisplay instance;
    #endregion

    #region Properties
    public static PopUpDisplay Instance
    {
        get { return instance; }
    }
    #endregion

    #region Functions
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(instance);
            m_Canvas = transform.parent.GetComponent<Canvas>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
    }

    MenuManagerPanelPopupDialog PopUp()
    {
        if (m_Popup == null)
        {
            m_Popup = FindObjectOfType<MenuManagerPanelPopupDialog>();
        }

        return m_Popup;
    }

    MenuManagerPanelPopupDialogInput PopUpInput()
    {
        if (m_PopupInput == null)
        {
            m_PopupInput = FindObjectOfType<MenuManagerPanelPopupDialogInput>();
        }

        return m_PopupInput;
    }

    void CheckForError(string title, string message)
    {
        if (title.Contains("Error") || title.Contains("error"))
        {
            Debug.LogError(message);
        }
    }

    string NicefyMessage(string message)
    {
        if (message.Contains("GetEntity() - :ERROR: System.ArgumentNullException"))
        {
            message = "Network Connection Lost";
        }

        return message;
    }

    public void Display(string title, string message)                                                 { Display(title, message, "Ok", "Cancel", true, null, null); }
    public void Display(string title, string message, bool singleButton)                              { Display(title, message, "Ok", "Cancel", singleButton, null, null); }
    public void Display(string title, string message, string okButtonName)                            { Display(title, message, okButtonName, "Cancel", false, null, null); }
    public void Display(string title, string message, string okButtonName, string cancelButtonName)   { Display(title, message, okButtonName, cancelButtonName, false, null, null); }
    public void DisplayYesNo(string title, string message, UnityAction okCallback, UnityAction cancelCallback) { Display(title, message, "Yes", "No", false, okCallback, cancelCallback); }
    public void DisplayOkCancel(string title, string message, UnityAction okCallback, UnityAction cancelCallback) { Display(title, message, "Ok", "Cancel", false, okCallback, cancelCallback); }
    public void Display(string title, string message, string okButtonName, string cancelButtonName, bool singleButton,
        UnityAction okCallback, UnityAction cancelCallback)
    {
        CheckForError(title, message);
        message = NicefyMessage(message);
        PopUp().gameObject.SetActive(true);
        PopUp().SetDialog(title, message, okButtonName, cancelButtonName, singleButton, okCallback, cancelCallback);
        PopUp().transform.SetParent(m_Canvas.transform);
        PopUp().transform.SetAsLastSibling();
    }


    public void DisplayOkCancelInput(string title, string message, MenuManagerPanelPopupDialogInput.PopupInputDelegate okCallback, MenuManagerPanelPopupDialogInput.PopupInputDelegate cancelCallback) { DisplayInput(title, message, "Ok", "Cancel", false, okCallback, cancelCallback); }
    public void DisplayInput(string title, string message, string okButtonName, string cancelButtonName, bool singleButton,
        MenuManagerPanelPopupDialogInput.PopupInputDelegate okCallback, MenuManagerPanelPopupDialogInput.PopupInputDelegate cancelCallback)
    {
        CheckForError(title, message);
        message = NicefyMessage(message);
        PopUpInput().gameObject.SetActive(true);
        PopUpInput().SetDialog(title, message, okButtonName, cancelButtonName, singleButton, okCallback, cancelCallback);
        PopUpInput().transform.SetParent(m_Canvas.transform);
        PopUpInput().transform.SetAsLastSibling();
    }


    /*
    void OkCB() { Debug.Log("OkCB"); }
    void CancelCB() { Debug.Log("CancelCB"); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Display("test", "test message", "ok", "cancel", false, OkCB, CancelCB);
        }
    }
    */
    #endregion
}
