using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class DebugConsole : DebugLogger
{
    #region Constants
    public const char ParameterStart = ' ';
    public delegate void ConsoleCallback(string commandEntered, DebugConsole console);
    const float MinInputHeight = 24;
    #endregion

    #region Variables
    static DebugConsole _DebugConsole;

    public float m_PercentageOfScreenTall = 0.4f;
    //public float m_InputPercentageTall = 0.05f;
    public int m_NumVisibleLines = 12;
    public bool m_DrawConsole = false;

    Dictionary<string, ConsoleCallback> m_CommandFunctionMap = new Dictionary<string, ConsoleCallback>();

    // TextArea for command string
    string m_CommandString = string.Empty;

    // true draws the console to the screen
    bool m_bConsoleLoggingEnabled = true;

    List<string> m_PreviousCommands = new List<string>();
    List<string> m_BrokenUpConsoleText = new List<string>(); // used for storage when breaking up text for the console that is too long
    int m_PreviousCommandIndex = 0;
    Image m_MainPanel;
    RectTransform m_LoggedTextContent;
    InputField m_InputField;
    ScrollRect m_ScrollRect;
    float m_PreferredFontSize;

    #if UNITY_EDITOR
    float m_TempPercentageOfScreenTall;
    int m_TempNumVisibleLines;
    #endif
    #endregion

    #region Properties
    public string CommandString
    {
        get { return m_CommandString; }
        set { m_InputField.text = m_CommandString = value; }
    }

    public float PercentageOfScreenTall
    {
        get { return m_PercentageOfScreenTall; }
        set
        {
             m_PercentageOfScreenTall = value;
            #if UNITY_EDITOR
            m_TempPercentageOfScreenTall = value;
            #endif
            m_MainPanel.rectTransform.offsetMax = new Vector2(0, -(Screen.height - (Screen.height * m_PercentageOfScreenTall)));
        }
    }

    public float InputFieldHeight
    {
        get { RectTransform fieldRect = m_InputField.GetComponent<RectTransform>(); return fieldRect.sizeDelta.y; }
        set
        {
            value = Mathf.Max(MinInputHeight, value);
            RectTransform fieldRect = m_InputField.GetComponent<RectTransform>();
            fieldRect.sizeDelta = new Vector2(0, value/*m_InputPercentageTall*/);
            uGuiUtils.StretchToParent(m_ScrollRect.GetComponent<RectTransform>(), 0, value, 0, 0);
            //m_InputField.textComponent.fontSize = (int)(fieldRect.rect.height * 0.5f);
        }
    }

    public int NumVisibleLines
    {
        get { return m_NumVisibleLines; }
        set
        {
            m_NumVisibleLines = value;
            #if UNITY_EDITOR
            m_TempNumVisibleLines = value;
            #endif
            m_PreferredFontSize = Mathf.Round(m_ScrollRect.GetComponent<RectTransform>().rect.height / (m_NumVisibleLines));
            ResizeTextLog();
        }
    }

    float CalculateInputFieldHeight { get { return m_PreferredFontSize * 1.2f; } }

    public bool DrawConsole
    {
        get { return m_DrawConsole; }
    }

    #endregion

    public static DebugConsole Get()
    {
        if (_DebugConsole == null)
        {
            _DebugConsole = Object.FindObjectOfType(typeof(DebugConsole)) as DebugConsole;
        }

        return _DebugConsole;
    }

    public void Awake()
    {
        transform.position = Vector3.zero;
        Canvas canvas = uGuiUtils.CreateCanvas("DebugConsoleCanvas", this.gameObject, m_CanvasSortingOrder);

        // add ugui elements to the canvas
        // main panel
        m_MainPanel = uGuiUtils.CreateImage("DebugConsoleUI", canvas.transform);
        m_MainPanel.color = new Color(0, 0, 0, 100f / 255f);
        uGuiUtils.StretchToParent(m_MainPanel.rectTransform, 0, 0, 0, 0);
        m_MainPanel.rectTransform.pivot = new Vector2(0.5f, 0);

        PercentageOfScreenTall = m_PercentageOfScreenTall;

        // create scroll rect
        m_ScrollRect = uGuiUtils.CreateScrollRect("Scroll View", m_MainPanel.transform, true);
        m_ScrollRect.scrollSensitivity = 10;
        uGuiUtils.StretchToParent(m_ScrollRect.GetComponent<RectTransform>(), 0, Screen.height /** m_InputPercentageTall*/, 0, 0);
        m_ScrollRect.horizontal = false;
        m_ScrollRect.GetComponent<Image>().color = new Color(0, 0, 0, 100f / 255f);
        GameObject scrollContentGO = VHUtils.FindChildRecursive(m_ScrollRect.gameObject, "Content");
        m_LoggedTextContent = scrollContentGO.GetComponent<RectTransform>();
        VerticalLayoutGroup verticalLayout = m_LoggedTextContent.gameObject.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(5, 5, 5, 5);
        verticalLayout.childForceExpandHeight = false;
        m_LoggedTextContent.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // create input field
        m_InputField = uGuiUtils.CreateInputField("ConsoleInput", m_MainPanel.transform, (string s) => InvokeCommand(s));
        m_InputField.caretWidth = 3;
        RectTransform fieldRect = m_InputField.GetComponent<RectTransform>();
        m_InputField.placeholder.GetComponent<Text>().resizeTextForBestFit = true;
        m_InputField.textComponent.resizeTextForBestFit = true;
        fieldRect.pivot = new Vector2(0, 0);
        uGuiUtils.SetAnchors(fieldRect, 0, 0, 1, 0);
        fieldRect.position = new Vector3(0, 0, 0);

        NumVisibleLines = m_NumVisibleLines;
        InputFieldHeight = CalculateInputFieldHeight;

        m_MainPanel.gameObject.SetActive(m_DrawConsole);

        // allow unity output logs to go to the console
        LogCallbackHandler handler = GameObject.FindObjectOfType<LogCallbackHandler>();
        if (handler)
            handler.AddCallback(HandleUnityLog);
        else
            Debug.LogWarning("DebugConsole: LogCallbackHandler component not found in the scene.  Add one if you wish to display Unity Log() messages");

        CreateUIText(m_LoggedTextContent.transform);

        #if UNITY_EDITOR
        m_TempPercentageOfScreenTall = m_PercentageOfScreenTall;
        m_TempNumVisibleLines = m_NumVisibleLines;
        #endif
        //StartCoroutine(Test());

        AddText(" ");
    }

    IEnumerator Test()
    {
        yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();
        for (int i = 0; i < m_LoggedText.Length; i++)
        {
            m_LoggedText[i].gameObject.SetActive(true);
        }
        ResizeTextLog();
        //Canvas.ForceUpdateCanvases();
        for (int i = 0; i < m_LoggedText.Length; i++)
        {
            m_LoggedText[i].gameObject.SetActive(false);
        }

        AddText(" ");
    }


    public void AddCommandCallback(string commandString, ConsoleCallback cb)
    {
        commandString.ToLower();
        if (0 == string.Compare(commandString, "help") || 0 == string.Compare(commandString, "?")
            || 0 == string.Compare(commandString, "clear") || 0 == string.Compare(commandString, "cls")
            || 0 == string.Compare(commandString, "q") || 0 == string.Compare(commandString, "quit")
            || 0 == string.Compare(commandString, "exit") || 0 == string.Compare(commandString, "enable_console_logging")
        )
        {
            // reserved keywords
            commandString += " is a reserved keyword by the console.";
            //AddTextToLog(commandString);
            return;
        }

        if (m_CommandFunctionMap.ContainsKey(commandString))
        {
            m_CommandFunctionMap[commandString] = cb;
        }
        else
        {
            m_CommandFunctionMap.Add(commandString, cb);
        }
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ToggleConsole();
        }

        if (m_DrawConsole)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (m_PreviousCommandIndex > 0)
                {
                    CommandString = m_PreviousCommands[--m_PreviousCommandIndex];
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (m_PreviousCommandIndex < m_PreviousCommands.Count - 1)
                {
                    CommandString = m_PreviousCommands[++m_PreviousCommandIndex];
                }
            }
            else if (Input.GetKeyDown(KeyCode.PageUp))
            {
                m_ScrollRect.verticalScrollbar.value += 0.1f;
            }
            else if (Input.GetKeyDown(KeyCode.PageDown))
            {
                m_ScrollRect.verticalScrollbar.value -= 0.1f;
            }
            else if (Input.GetKeyDown(KeyCode.End))
            {
                m_ScrollRect.verticalScrollbar.value = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Home))
            {
                m_ScrollRect.verticalScrollbar.value = 1;
            }
        }

        #if UNITY_EDITOR
        if (m_TempPercentageOfScreenTall != m_PercentageOfScreenTall)
        {
            PercentageOfScreenTall = m_PercentageOfScreenTall;
        }

        if (m_TempNumVisibleLines != m_NumVisibleLines)
        {
            NumVisibleLines = m_NumVisibleLines;
        }

        #endif
    }

    public void ToggleConsole()
    {
        m_DrawConsole = !m_DrawConsole;
        CommandString = string.Empty;
        m_MainPanel.gameObject.SetActive(m_DrawConsole);
        SelectInputField();

        if (m_DrawConsole)
        {
            m_ScrollRect.verticalScrollbar.value = 0;
        }
    }

    void SelectInputField()
    {
        m_InputField.ActivateInputField();
        m_InputField.Select();
    }

    void InvokeCommand(string command)
    {
        if (command == null)
        {
            return;
        }
        else
        {
            command = command.Trim();
            command = command.Replace("\n", "");
            //VHGUI.TextArea(m_CommandStringPosition, m_CommandString, Color.yellow);
        }

        SelectInputField();

        if (command == string.Empty)
        {
            return;
        }

        // format the string to get rid of extra ending spaces and multiple spaces in a row
        int spaceIndex = 0;
        do
        {
            spaceIndex = command.IndexOf(ParameterStart, spaceIndex);
            if (spaceIndex != -1)
            {
                ++spaceIndex;
                while (spaceIndex < command.Length && command[spaceIndex] == ParameterStart)
                {
                    command = command.Remove(spaceIndex, 1);
                }
            }
        }
        while (spaceIndex != -1);

        if (command[0] == ParameterStart)
        {
            command = command.Remove(0, 1);
        }

        //bool commandExists = false;
        string commandStringWithoutParameters = command;
        string consoleLogString = commandStringWithoutParameters; // used for logging messages to console

        int uiIndex = commandStringWithoutParameters.IndexOf(ParameterStart);
        if (uiIndex != -1)
        {
            // we don't want to check parameters, we just want the command,
            commandStringWithoutParameters = commandStringWithoutParameters.Remove(uiIndex, command.Length - uiIndex);
            commandStringWithoutParameters = commandStringWithoutParameters.ToLower();
        }

        if (m_CommandFunctionMap.ContainsKey(commandStringWithoutParameters))
        {
            // this command string exists, call its function
            m_CommandFunctionMap[commandStringWithoutParameters](command, this);
        }
        else if (0 == string.Compare(command, "clear") || 0 == string.Compare(command, "cls"))
        {
            consoleLogString = string.Empty;
            ClearConsoleLog(true);
        }
        else if (0 == string.Compare(command, "help") || 0 == string.Compare(command, "?"))
        {
            // show all the commands available to them
            consoleLogString = "Commands Available: ";
            foreach (KeyValuePair<string, ConsoleCallback> kvp in m_CommandFunctionMap)
            {
                consoleLogString += "   " + kvp.Key;
            }
        }
        else if (0 == string.Compare(command, "q") || 0 == string.Compare(command, "quit")
            || 0 == string.Compare(command, "exit"))
        {
            VHUtils.ApplicationQuit();
        }
        else if (0 == string.Compare(commandStringWithoutParameters, "enable_console_logging"))
        {
            if (!ParseBool(command, ref m_bConsoleLoggingEnabled))
            {
                AddText(command + " requires parameter '0' or '1'");
            }
        }
        else
        {
            // command doesn't exist
            consoleLogString = commandStringWithoutParameters + " command doesn't exist. Type ? or help for a list of commands.";
        }

        // display what you wrote in the log
        AddText(consoleLogString);

        // add it so you can find it again with the arrow keys
        if (!m_PreviousCommands.Contains(command))
        {
            //for (int i = 0; i < command.Length; i++)
            //{
            //    if (command[i] == '\n')
            //    {
            //        command = command.Remove(i, 1);
            //    }
            //}
            m_PreviousCommands.Add(command);
        }

        m_PreviousCommandIndex = m_PreviousCommands.Count;

        CommandString = string.Empty;

        StartCoroutine(ForceScrollerToBottom());
    }

    IEnumerator ForceScrollerToBottom()
    {
        // this is a hack to get the vertical scroller value to show the bottom line
        // unity forces me to wait for a few frames
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        m_ScrollRect.verticalScrollbar.value = 0;
    }

    void ClearConsoleLog(bool clearCache)
    {
        if (clearCache)
        {
            m_CachedText.Clear();
        }

        for (int i = 0; i < m_LoggedTextContent.transform.childCount; i++)
        {
            m_LoggedTextContent.transform.GetChild(i).gameObject.SetActive(false);
            //m_LoggedTextContent.transform.GetChild(i).GetComponent<Text>().text = string.Empty;
        }
    }

    public void AddText(string text)
    {
        AddText(text, Color.white);
    }

    override public void AddText(string text, Color c, LogType logType)
    {
        AddText(text, c);
    }

    public void AddText(string text, Color color)
    {
        if (text == null || text == string.Empty || !m_bConsoleLoggingEnabled)
        {
            return;
        }

        m_CachedText.Add(new TextLine(text, color));

        m_BrokenUpConsoleText = BreakUpTextLine(text);

        for (int i = 0; i < m_BrokenUpConsoleText.Count; i++)
        {
            string textCopy = m_BrokenUpConsoleText[i].Substring(0, Mathf.Min(m_BrokenUpConsoleText[i].Length, MaxTextLength));

            //InputField inputField = m_LoggedText[m_TextCacheIndex];
            Text loggedText = m_LoggedText[m_TextCacheIndex];

            loggedText.color = color;
            loggedText.text = textCopy;
            loggedText.gameObject.SetActive(true);

            m_TextCacheIndex += 1;
            m_TextCacheIndex %= m_MaxLogCapacity;

            loggedText.transform.SetAsLastSibling();
        }

        if (m_ScrollRect.verticalScrollbar.value == 0)
        {
            StartCoroutine(ForceScrollerToBottom());
        }
    }

    protected override void RebuildDisplay()
    {
        NumVisibleLines = m_NumVisibleLines;
        ClearConsoleLog(false);

        //int w = Screen.width;

        List<TextLine> temp = new List<TextLine>(m_CachedText);
        m_CachedText.Clear();

        foreach (TextLine ct in temp)
        {
            AddText(ct.text.Trim(), ct.color);
        }

        //ResizeTextLog();
    }


    void ResizeText(Text loggedText)
    {
        //m_HolderText.fontSize = (int)((float)m_PreferredFontSize * m_FontScaler);
        m_HolderText.GetComponent<LayoutElement>().preferredHeight = m_PreferredFontSize;

        if (loggedText != null)
        {
            //loggedText.GetComponent<LayoutElement>().preferredHeight = m_PreferredFontSize;
            //loggedText.fontSize = (int)((float)m_PreferredFontSize * m_FontScaler);
            loggedText.GetComponent<LayoutElement>().preferredHeight = m_PreferredFontSize;
        }
    }

    void ResizeTextLog()
    {
        InputFieldHeight = CalculateInputFieldHeight;
        for (int i = 0; i < m_LoggedTextContent.transform.childCount; i++)
        {
            Transform child = m_LoggedTextContent.transform.GetChild(i);
            ResizeText(child.GetComponent<Text>());
        }
    }

    // seperates 1 long line into multiple shorter lines in order to fit on the screen
    List<string> BreakUpTextLine(string text)
    {
        m_BrokenUpConsoleText.Clear();

        // break up the lines by searching for newlings
        //Debug.Log("Screen.width: " + Screen.width);
        text = uGuiUtils.InsertBreaks(text, m_HolderText.font, m_HolderText.fontSize, (int)((float)Screen.width * 0.95f));
        string[] newLineSeperatedLines = text.Split('\n');
        m_BrokenUpConsoleText.AddRange(newLineSeperatedLines);
        return m_BrokenUpConsoleText;
    }

    void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        switch (type)
        {
        case LogType.Error:
            AddText(logString, Color.red);
            break;

        case LogType.Warning:
            AddText(logString, Color.yellow);
            break;

        default:
            AddText(logString);
            break;
        }
    }

    #region String Parsing Functions
    // these are helper functions to get parameters out of the command string that was entered
    // i.e. in the string enable_lighting 1, 1 is the parameter
    // these functions return true if they succeeded in finding the respective parameter
    public bool ParseBool(string commandEntered, ref bool out_value)
    {
        bool success = false;
        string subString = "";

        success = ParseSingleParameter(commandEntered, ref subString, 0);
        if (success)
        {
            success = bool.TryParse(subString, out out_value);
            if (!success)
            {
                // try one more time, see if they put a number instead of the words true or false
                int holder = 0;
                success = int.TryParse(subString, out holder);
                if (success)
                {
                    out_value = holder == 0 ? false : true;
                }
            }
        }

        return success;
    }

    public bool ParseInt(string commandEntered, ref int out_value)
    {
        bool success = false;
        string subString = "";

        success = ParseSingleParameter(commandEntered, ref subString, 0);
        if (success)
        {
            success = int.TryParse(subString, out out_value);
        }

        return success;
    }

    public bool ParseFloat(string commandEntered, ref float out_value)
    {
        bool success = false;
        string subString = "";

        success = ParseSingleParameter(commandEntered, ref subString, 0);
        if (success)
        {
            success = float.TryParse(subString, out out_value);
        }

        return success;
    }

    public bool ParseVector2(string commandEntered, ref Vector2 out_value)
    {
        bool success = false;
        string subString = "";
        int uiSearchStartIndex = 0, uiParamStartIndex = 0, uiParamEndIndex = 0;
        const int NumTimesToLoop = 2;

        for (int i = 0; i < NumTimesToLoop; i++)
        {
            success = ParseSingleParameter(commandEntered, ref subString, ref uiParamStartIndex,
                ref uiParamEndIndex, uiSearchStartIndex + uiParamEndIndex);
            if (success)
            {
                float val = 0;
                success = float.TryParse(subString, out val);
                out_value[i] = val;
            }

            if (!success)
            {
                break;
            }
        }

        return success;
    }

    public bool ParseVector3(string commandEntered, ref Vector3 out_value)
    {
        bool success = false;
        string subString = "";
        int uiSearchStartIndex = 0, uiParamStartIndex = 0, uiParamEndIndex = 0;
        const int NumTimesToLoop = 3;

        for (int i = 0; i < NumTimesToLoop; i++)
        {
            success = ParseSingleParameter(commandEntered, ref subString, ref uiParamStartIndex,
                ref uiParamEndIndex, uiSearchStartIndex + uiParamEndIndex);
            if (success)
            {
                float val = 0;
                success = float.TryParse(subString, out val);
                out_value[i] = val;
            }

            if (!success)
            {
                break;
            }
        }

        return success;
    }

    public bool ParseVHMSG(string commandEntered, ref string out_opName, ref string out_arg)
    {
        bool success = false;
        //string subString = "";
        int uiStartIndex = 0, uiEndIndex = 0, uiSearchStartIndex = 0;

        // start the the beginning of the string and look for the vhmsg opcode
        success = ParseSingleParameter(commandEntered, ref out_opName, ref uiStartIndex, ref uiEndIndex, uiSearchStartIndex);
        if (success)
        {
            // now try to find the argument if it has one by using the remainder of the string
            // this second check doesn't have to succeed because not all vhmsg's have arguments, some just use opcodes
            if (uiEndIndex < commandEntered.Length - 1 && uiEndIndex != -1)
            {
                out_arg = commandEntered.Substring(uiEndIndex + 1, commandEntered.Length - uiEndIndex - 1);
            }
        }

        return success;
    }

    // these are helper functions for the rest of the Parsing functions.
    public bool ParseSingleParameter(string commandEntered, ref string out_value, int uiSearchStartIndex)
    {
        int uiStartIndex = 0, uiEndIndex = 0;
        return ParseSingleParameter(commandEntered, ref out_value, ref uiStartIndex, ref uiEndIndex, uiSearchStartIndex);
    }

    public bool ParseSingleParameter(string commandEntered, ref string out_value,
        ref int out_uiParamStartIndex, ref int out_uiParamEndIndex, int uiSearchStartIndex)
    {
        bool success = false;

        // find where the parameter begins using the start delimiter
        out_uiParamStartIndex = commandEntered.IndexOf(ParameterStart, uiSearchStartIndex);
        if (out_uiParamStartIndex != -1 && out_uiParamStartIndex != commandEntered.Length - 1)
        {
            // now find where it ends
            out_uiParamEndIndex = commandEntered.IndexOf(ParameterStart, out_uiParamStartIndex + 1);
            if (out_uiParamEndIndex != -1)
            {
                out_value = commandEntered.Substring(out_uiParamStartIndex + 1, out_uiParamEndIndex - out_uiParamStartIndex - 1);
            }
            else
            {
                // there aren't anymore parameters, you've reached the end of the string
                out_value = commandEntered.Substring(out_uiParamStartIndex + 1/*, commandEntered.Length*/);
            }

            success = true;
        }

        return success;
    }
    #endregion
}
