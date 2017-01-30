using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

public class MenuManagerPanelInterviewScreen : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
        List<AGUtteranceData> PrimaryContent;

        List<AGUtteranceData> BuyTimeContent;
        List<AGUtteranceData> BuyTimeContentAlreadyPlayed = new List<AGUtteranceData>();
        Dictionary<string, List<GameObject>>    FavoriteList;

        //These used to store all utterances in that response type
        List<AGUtteranceData> OpeningsContent;
        List<AGUtteranceData> EngagementsContent;
        List<AGUtteranceData> AcknowledgementsContent;
        List<AGUtteranceData> ElaborationsContent;
        List<AGUtteranceData> DistractionsContent;
        List<AGUtteranceData> AnswersContent;

        //These used to store the favorite 3 utterances
        List<AGUtteranceData> OpeningFavContent;
        List<AGUtteranceData> EngagementFavContent;
        List<AGUtteranceData> AcknowledgementsFavContent;
        List<AGUtteranceData> ElaborationsFavContent;
        List<AGUtteranceData> DistractionsFavContent;
        List<AGUtteranceData> AnswersFavContent;

        GameObject   CompleteScenarioObj;

        VitaGlobals.VitaResponseTypes CurrentlyPinnedResponseType;
        GameObject   PinnedScrollGrp;
        GameObject   PrimaryScrollGrp;
        Text         PinnedTitleText;
        GameObject   PinnedContentObj;
        GameObject   OpeningsContentObj;
        GameObject   EngagementsContentObj;
        GameObject   AcknowledgementsContentObj;
        GameObject   ElaborationsContentObj;
        GameObject   DistractionsContentObj;
        GameObject   AnswersContentObj;
        GameObject   NextUtteranceContentObj;
        GameObject   NextUttBtn;
        GameObject   NextUttFinishedObj;
        GameObject   m_favResponsesAcknowledgementLoading;
        GameObject   m_favResponsesAnswerLoading;
        GameObject   m_favResponsesEngagementLoading;
        GameObject   m_favResponsesElaborationLoading;
        GameObject   m_favResponsesDistractionLoading;
        GameObject   m_favResponsesOpeningLoading;

        MenuManager  m_menuManager;
        GameObject   m_headerName;
        GameObject   PrimaryContentObj;
        GameObject   ResponseListItem;

        //Favorites Popup
        GameObject   m_popupArResponses;
        GameObject   m_popupArResponsesContent;
        Text         m_popupArResponsesTitle;
        List<GameObject> m_popupChrUttPrefabList = new List<GameObject>(); //Used for quickly accessing which items have been selected

        //Scoring Note Popup
        GameObject              m_popupScoring;
        AGGuiToggleScoreGraph   m_mias_firstImpression;
        AGGuiToggleScoreGraph   m_mias_interviewResponses;
        AGGuiToggleScoreGraph   m_mias_selfPromoting;
        AGGuiToggleScoreGraph   m_mias_activeListening;
        AGGuiToggleScoreGraph   m_mias_closing;
        InputField              m_field_scoringNote;

        //GameObject   PopupDialogObj;
        //GameObject   PopupDialogObjCopy;
        //MenuManagerPanelPopupDialog PopupDialogCmp;

        GameObject   EventLogContentObj;
        RectTransform EventLogContentRectTransform;
        Scrollbar    EventLogVertScrollBar;
        GameObject   DialogTeacherPrefab;
        GameObject   DialogStudentPrefab;
        GameObject   DialogDataPrefab;
        //GameObject   DialogAlarmPrefab;

        GameObject   MainObj;
        Main         MainCmp;
        AGInterviewButtonStateManager InterviewBtnStateMng;

        AGUtteranceDataFilter m_chrUttCmp;
    #endregion

    #region Menu Initialization
    void Start()
    {
        MainObj = GameObject.Find("Main");

        //Resources
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu            = m_menuManager.GetMenuGameObject(MenuManager.Menu.InterviewScreen);
        GameObject resources       = VHUtils.FindChildRecursive(menu,                                             "Resources");
        m_headerName               = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"),"GuiTextPrefab (2)");
        ResponseListItem           = VHUtils.FindChildRecursive(resources,                                        "GuiButtonInterviewResponsePrefab");
        DialogTeacherPrefab        = VHUtils.FindChildRecursive(resources,                                        "GuiDialogTeacherPrefab");
        DialogStudentPrefab        = VHUtils.FindChildRecursive(resources,                                        "GuiDialogStudentPrefab");
        DialogDataPrefab           = VHUtils.FindChildRecursive(resources,                                        "GuiDialogDataPrefab");
        //DialogAlarmPrefab          = VHUtils.FindChildRecursive(resources,                                        "GuiDialogAlarmPrefab");

        CompleteScenarioObj        = VHUtils.FindChildRecursive(menu,                                             "GuiButtonPrefab_CompleteScenario");

        //Responses, Popup, Chat objects
        CurrentlyPinnedResponseType= VitaGlobals.VitaResponseTypes.Primary;
        PinnedScrollGrp            = VHUtils.FindChildRecursive(menu,                                                                   "ScrollView_Pinned");
        PrimaryScrollGrp           = VHUtils.FindChildRecursive(menu,                                                                   "ScrollView_Primary");
        PinnedTitleText            = VHUtils.FindChildRecursive(menu,                                                                   "GuiTextPrefab_PinnedTitle").GetComponent<Text>();
        PinnedContentObj           = VHUtils.FindChildRecursive(PinnedScrollGrp,                                                        "Content");
        PrimaryContentObj          = VHUtils.FindChildRecursive(PrimaryScrollGrp,                                                       "Content");
        OpeningsContentObj         = VHUtils.FindChildRecursive(GameObject.Find("GuiTeacherFavoritesWidgetPrefab_Openings"),            "Content");
        EngagementsContentObj      = VHUtils.FindChildRecursive(GameObject.Find("GuiTeacherFavoritesWidgetPrefab_Engagements"),         "Content");
        AcknowledgementsContentObj = VHUtils.FindChildRecursive(GameObject.Find("GuiTeacherFavoritesWidgetPrefab_Acknowledgements"),    "Content");
        ElaborationsContentObj     = VHUtils.FindChildRecursive(GameObject.Find("GuiTeacherFavoritesWidgetPrefab_Elaborations"),        "Content");
        DistractionsContentObj     = VHUtils.FindChildRecursive(GameObject.Find("GuiTeacherFavoritesWidgetPrefab_Distractions"),        "Content");
        AnswersContentObj          = VHUtils.FindChildRecursive(GameObject.Find("GuiTeacherFavoritesWidgetPrefab_Answers"),             "Content");
        NextUtteranceContentObj    = VHUtils.FindChildRecursive(GameObject.Find("GuiTeacherFavoritesWidgetPrefab_NextUtterance"),       "Content");
        NextUttBtn                 = VHUtils.FindChildRecursive(NextUtteranceContentObj,                                                "GuiButtonInterviewResponsePrefab_NextUtt");
        NextUttFinishedObj         = VHUtils.FindChildRecursive(NextUtteranceContentObj,                                                "GuiTextPrefab_NextUttFinished");
        
        GameObject EventLogGo      = VHUtils.FindChildRecursive(menu,                                                                   "ScrollView_EventLog");
        EventLogContentObj         = VHUtils.FindChildRecursive(EventLogGo,                                                             "Content");
        EventLogContentRectTransform =                                                                                                  EventLogContentObj.GetComponent<RectTransform>();
        EventLogVertScrollBar      = VHUtils.FindChildRecursive(EventLogGo,                                                             "Scrollbar Vertical").GetComponent<Scrollbar>();
        m_favResponsesAcknowledgementLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu,                              "GuiTeacherFavoritesWidgetPrefab_Acknowledgements"), "IconLoading");
        m_favResponsesAnswerLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu,                                       "GuiTeacherFavoritesWidgetPrefab_Answers"), "IconLoading");
        m_favResponsesEngagementLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu,                                   "GuiTeacherFavoritesWidgetPrefab_Engagements"), "IconLoading");
        m_favResponsesElaborationLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu,                                  "GuiTeacherFavoritesWidgetPrefab_Elaborations"), "IconLoading");
        m_favResponsesDistractionLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu,                                  "GuiTeacherFavoritesWidgetPrefab_Distractions"), "IconLoading");
        m_favResponsesOpeningLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu,                                      "GuiTeacherFavoritesWidgetPrefab_Openings"), "IconLoading");

        //Adaptive Responses Popup
        m_popupArResponses           = VHUtils.FindChildRecursive(resources, "PanelInterviewAdaptiveResponsesPopupPrefab");
        m_popupArResponsesContent    = VHUtils.FindChildRecursive(m_popupArResponses, "Content");
        m_popupArResponsesTitle      = VHUtils.FindChildRecursive(m_popupArResponses, "GuiTextPrefab_PopupTitle").GetComponent<Text>();

        //Scoring Note
        m_popupScoring             = VHUtils.FindChildRecursive(resources, "PanelPopupScoringNotePrefab");
        m_mias_firstImpression     = VHUtils.FindChildRecursive(m_popupScoring, "GuiToggleScoreCirclesPrefab_FirstImpressions").GetComponent<AGGuiToggleScoreGraph>();
        m_mias_interviewResponses  = VHUtils.FindChildRecursive(m_popupScoring, "GuiToggleScoreCirclesPrefab_InterviewResponses").GetComponent<AGGuiToggleScoreGraph>();
        m_mias_selfPromoting       = VHUtils.FindChildRecursive(m_popupScoring, "GuiToggleScoreCirclesPrefab_SelfPromoting").GetComponent<AGGuiToggleScoreGraph>();
        m_mias_activeListening     = VHUtils.FindChildRecursive(m_popupScoring, "GuiToggleScoreCirclesPrefab_ActiveListening").GetComponent<AGGuiToggleScoreGraph>();
        m_mias_closing             = VHUtils.FindChildRecursive(m_popupScoring, "GuiToggleScoreCirclesPrefab_Closing").GetComponent<AGGuiToggleScoreGraph>();
        m_field_scoringNote        = VHUtils.FindChildRecursive(m_popupScoring, "GuiInputScrollFieldPrefab_ScoringNote").GetComponent<InputField>();
    }

    void Update()
    {
        //This section to keep event log scroll list at the bottom, showing the newest entries
        if (EventLogContentRectTransform != null && EventLogVertScrollBar != null)
        {
            if (EventLogVertScrollBar.size < .99)
            {
                EventLogContentRectTransform.pivot = new Vector2(EventLogContentRectTransform.pivot.x, 0);
            }
            else
            {
                EventLogContentRectTransform.pivot = new Vector2(EventLogContentRectTransform.pivot.x, 1);
            }
        }
    }

    public void OnMenuEnter()
    {
        string loginName = "Demo User";
        string loginOrganization = "Demo Organization";
        if (VitaGlobals.m_loggedInProfile != null)
        {
            loginName = VitaGlobals.m_loggedInProfile.name;
            loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        }
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        if (VitaGlobals.m_interviewType == VitaGlobals.InterviewType.Demo)
            CompleteScenarioObj.SetActive(false);

        //Clear menus
        VHUtils.DeleteChildren(PinnedContentObj.transform);
        VHUtils.DeleteChildren(PrimaryContentObj.transform);
        VHUtils.DeleteChildren(OpeningsContentObj.transform);
        VHUtils.DeleteChildren(EngagementsContentObj.transform);
        VHUtils.DeleteChildren(AcknowledgementsContentObj.transform);
        VHUtils.DeleteChildren(ElaborationsContentObj.transform);
        VHUtils.DeleteChildren(DistractionsContentObj.transform);
        VHUtils.DeleteChildren(AnswersContentObj.transform);
        VHUtils.DeleteChildren(EventLogContentObj.transform);
        NextUttBtn.SetActive(true);
        NextUttFinishedObj.SetActive(false);
        foreach (AGGuiToggleScoreGraph score in new AGGuiToggleScoreGraph[] { m_mias_firstImpression, m_mias_interviewResponses, m_mias_selfPromoting, m_mias_activeListening, m_mias_closing })
        {
            score.SetScore(-1);
        }
        UpdatePinnedTitle();

        //Wait for scene ready
        StartCoroutine(WaitForCutSceneReady());
    }

    public void OnMenuExit()
    {
    }

    IEnumerator WaitForCutSceneReady()
    {
        if (!VHUtils.SceneManagerActiveSceneName().StartsWith("Env"))
            yield break;

        while (GameObject.FindObjectOfType<Main>().m_vitaCurrentCharacter == -1)
        {
            yield return new WaitForEndOfFrame();
        }

        MainCmp = MainObj.GetComponent<Main>();
        MenuManager menuManager = GameObject.FindObjectOfType<MenuManager>();
        while (menuManager.m_cutscenes.Length == 0)
        {
            yield return new WaitForEndOfFrame();
        }

        while (GameObject.FindObjectOfType<AGUtteranceDataFilter>() == null)
        {
            yield return new WaitForEndOfFrame();
        }

        m_chrUttCmp = GameObject.FindObjectOfType<AGUtteranceDataFilter>();
        m_favResponsesAcknowledgementLoading.SetActive(false);
        m_favResponsesAnswerLoading.SetActive(false);
        m_favResponsesDistractionLoading.SetActive(false);
        m_favResponsesElaborationLoading.SetActive(false);
        m_favResponsesEngagementLoading.SetActive(false);
        m_favResponsesOpeningLoading.SetActive(false);
        InterviewBtnStateMng = GameObject.FindObjectOfType<AGInterviewButtonStateManager>();

        //Utterances by response types
        bool[] m_scenarioBoolArray = new bool[VitaGlobals.m_vitaScenarios.Length];
        for (int i = 0; i < m_scenarioBoolArray.Length; i++ )
        {
            m_scenarioBoolArray[i] = true;
        }

        PrimaryContent =            m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_chrUttCmp.GetScenarioBoolArrayByEnum((VitaGlobals.VitaScenarios)MainCmp.m_vitaCurrentMood),     m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Primary));
        AcknowledgementsContent =   m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_scenarioBoolArray,                                                                              m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Acknowledgement));
        AnswersContent =            m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_scenarioBoolArray,                                                                              m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Answer));
        BuyTimeContent =            m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_scenarioBoolArray,                                                                              m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.BuyTime));
        DistractionsContent =       m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_scenarioBoolArray,                                                                              m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Distraction));
        ElaborationsContent =       m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_scenarioBoolArray,                                                                              m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Elaboration));
        EngagementsContent =        m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_scenarioBoolArray,                                                                              m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Engagement));
        OpeningsContent =           m_chrUttCmp.GetSounds(MainCmp.m_vitaCurrentCharacter, m_scenarioBoolArray,                                                                              m_chrUttCmp.GetResponseTypeBoolArrayByEnum(VitaGlobals.VitaResponseTypes.Opening));

        //Just the favorite utterances
        AcknowledgementsFavContent = GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Acknowledgement);
        AnswersFavContent = GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Answer);
        DistractionsFavContent = GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Distraction);
        ElaborationsFavContent = GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Elaboration);
        EngagementFavContent = GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Engagement);
        OpeningFavContent = GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Opening);

        SetPrimaryUtt(PrimaryContentObj, PrimaryContent);
        SetCategotyUtt(OpeningsContentObj, OpeningFavContent);
        SetCategotyUtt(EngagementsContentObj, EngagementFavContent);
        SetCategotyUtt(AcknowledgementsContentObj, AcknowledgementsFavContent);
        SetCategotyUtt(ElaborationsContentObj, ElaborationsFavContent);
        SetCategotyUtt(DistractionsContentObj, DistractionsFavContent);
        SetCategotyUtt(AnswersContentObj, AnswersFavContent);

        BuyTimeContentAlreadyPlayed = new List<AGUtteranceData>();

        SetNextUtt();
    }
    #endregion

    #region UI Populating Functions
    /// <summary>
    /// Function to instantiate widgets into a list's content object.
    /// </summary>
    /// <param name="m_widget"></param>
    /// <param name="m_listContent"></param>
    /// <param name="m_textObjName"></param>
    /// <param name="m_widgetDisplayString"></param>
    /// <returns></returns>
    GameObject AddWidgetToList(GameObject m_widget, GameObject m_listContent, string m_widgetName, string m_textObjName, string m_widgetDisplayString)
    {
        //Debug.LogFormat("AddWidgetToList({0})", m_widgetDisplayString);

        GameObject widgetGo = Instantiate(m_widget);
        GameObject widgetText = VHUtils.FindChildRecursive(widgetGo, m_textObjName);
        widgetGo.name = m_widgetName;
        widgetGo.SetActive(true);
        widgetGo.transform.SetParent(m_listContent.transform, false);
        //widgetGo.GetComponent<Button>().onClick.AddListener(delegate {m_delegateFunction});
        widgetText.GetComponent<Text>().text = m_widgetDisplayString;
        return widgetGo;
    }

    /// <summary>
    /// Sets utterances for non-primary categories
    /// <param name="content"></param>
    /// <param name="utts"></param>
    public void SetCategotyUtt(GameObject content, List<AGUtteranceData> utts)                          { SetCategotyUtt(content, utts, utts.Count); }
    public void SetCategotyUtt(GameObject content, List<AGUtteranceData> utts, int numberOfEntries)
    {
        //This toggles each scroll group on/off
        if (content == PinnedContentObj)
        {
            VHUtils.DeleteChildren(content.transform);
            PinnedScrollGrp.SetActive(true);
            PrimaryScrollGrp.SetActive(false);
        }
        
        for (int i = 0; i < utts.Count && i < numberOfEntries; i++)
        {
            AGUtteranceData utt = utts[i];
            string uttTextString = utt.gameObject.GetComponent<AudioSpeechFile>().UtteranceText.Replace("\n", "");
            GameObject uttGo = AddWidgetToList(ResponseListItem, content, utt.name, "TextMain", uttTextString);
            uttGo.GetComponent<Button>().onClick.AddListener(delegate { SetButtonPressed(uttGo); PlayUtt(utt.name, uttTextString); });
            
            //Set scenario indicator on the response button
            GameObject scenarioIndicator = VHUtils.FindChildRecursive(uttGo, "ScenarioIndicator");
            scenarioIndicator.SetActive(utt.m_scenario[MainCmp.m_vitaCurrentMood]);
        }
    }

    /// <summary>
    /// Specifically sets primary response utterances to the hidden column. This will also keep track of if the primary response already has a GO and recorded
    /// in the response state manager, so no duplication occurs.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="utts"></param>
    public void SetPrimaryUtt(GameObject content, List<AGUtteranceData> utts)
    {
        //This toggles each scroll group pon/off
        PinnedScrollGrp.SetActive(false);
        PrimaryScrollGrp.SetActive(true);
        SelectNextUtt(); //Specifically for highlighting next primary response

        foreach (AGUtteranceData utt in utts)
        {
            string uttName = utt.name;
            string uttTextString = utt.gameObject.GetComponent<AudioSpeechFile>().UtteranceText.Replace("\n", "");
            if (InterviewBtnStateMng.GetButton(utt.name) == null)
            {
                GameObject uttGo = AddWidgetToList(ResponseListItem, content, uttName, "TextMain", uttTextString);
                InterviewBtnStateMng.m_interviewButtonStates.Add(new AGInterviewButtonStateManager.AGInterviewButtonState { ButtonGO = uttGo, ButtonName = uttName, UtteranceDataCmp = utt, ButtonPressed = false });
                uttGo.GetComponent<Button>().onClick.AddListener(delegate { SetButtonPressed(uttGo); PlayUtt(uttName, uttTextString); });

                //Set scenario indicator on the response button; all to false, since it is unnesccessary to indicate otherwise
                GameObject scenarioIndicator = VHUtils.FindChildRecursive(uttGo, "ScenarioIndicator");
                scenarioIndicator.SetActive(false);
            }
        }
    }

    void SetButtonPressed(GameObject button)
    {
        //Mark it on the button state manager
        InterviewBtnStateMng.SetButtonPressed(button, true);

        ColorBlock modifiedColorBlock = button.GetComponent<Button>().colors;
        modifiedColorBlock.normalColor = modifiedColorBlock.disabledColor;
        button.GetComponent<Button>().colors = modifiedColorBlock;
    }
    #endregion

    #region Favorites Popup
    /// <summary>
    /// Fills the popup window with a list of responses and highlight the ones that have been chosen as favorites.
    /// </summary>
    /// <param name="m_category"></param>
    void PopupArResponses(string m_category)
    {
        Debug.LogFormat("PopupArResponses({0})", m_category);

        //Set up popup
        VHUtils.DeleteChildren(m_popupArResponsesContent.transform);
        m_popupArResponsesTitle.text = m_category + " Responses";
        m_popupChrUttPrefabList.Clear();
        m_popupArResponses.SetActive(true);

        VitaGlobals.VitaResponseTypes m_categoryEnum = (VitaGlobals.VitaResponseTypes)Array.IndexOf(VitaGlobals.m_vitaResponseTypes, m_category);
        List<AGUtteranceData> utteranceList = new List<AGUtteranceData>();
        if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Acknowledgement)    { utteranceList = AcknowledgementsContent; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Answer)        { utteranceList = AnswersContent; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Distraction)   { utteranceList = DistractionsContent; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Elaboration)   { utteranceList = ElaborationsContent; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Engagement)    { utteranceList = EngagementsContent; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Opening)       { utteranceList = OpeningsContent; }

        //Populate with adaptive responses
        foreach (AGUtteranceData utterance in utteranceList)
        {
            string uttName = utterance.name;
            string uttText = utterance.GetComponent<AudioSpeechFile>().UtteranceText;
            GameObject widget = AddWidgetToList(ResponseListItem, m_popupArResponsesContent, uttName, "TextMain", uttText);
            m_popupChrUttPrefabList.Add(widget);
            widget.GetComponent<Button>().onClick.AddListener(delegate { PlayUtt(uttName, uttText); BtnPopupClose(); });

            //Set scenario indicator on the response button
            GameObject scenarioIndicator = VHUtils.FindChildRecursive(widget, "ScenarioIndicator");
            scenarioIndicator.SetActive(utterance.m_scenario[MainCmp.m_vitaCurrentMood]);
        }
    }

    void UpdatePinnedTitle()
    {
        PinnedTitleText.text = String.Format("Pinned '{0}'", VitaGlobals.m_vitaResponseTypes[(int)CurrentlyPinnedResponseType]);
    }

    /// <summary>
    /// Gets a list of favorite responses based on the given responseType
    /// </summary>
    /// <param name="m_responseType">VitaGlobals.VitaResponseTypes</param>
    /// <returns></returns>
    List<AGUtteranceData> GetFavoriteResponses(VitaGlobals.VitaResponseTypes m_responseType)
    {
        List<AGUtteranceData> returnUtterances = new List<AGUtteranceData>();
        List<AGUtteranceData> categoryUtts = new List<AGUtteranceData>();
        if (m_responseType == VitaGlobals.VitaResponseTypes.Acknowledgement)    { categoryUtts = AcknowledgementsContent; }
        else if (m_responseType == VitaGlobals.VitaResponseTypes.Answer)        { categoryUtts = AnswersContent; }
        else if (m_responseType == VitaGlobals.VitaResponseTypes.Distraction)   { categoryUtts = DistractionsContent; }
        else if (m_responseType == VitaGlobals.VitaResponseTypes.Elaboration)   { categoryUtts = ElaborationsContent; }
        else if (m_responseType == VitaGlobals.VitaResponseTypes.Engagement)    { categoryUtts = EngagementsContent; }
        else if (m_responseType == VitaGlobals.VitaResponseTypes.Opening)       { categoryUtts = OpeningsContent; }

        List<string> defaultFavorites = new List<string>()
        {
            // https://app.asana.com/0/73327800800553/211683773499716
            "_Softtouch_Opening_2001",
            "_Softtouch_Opening_2002",
            "_Neutral_Opening_2001",
            "_Softtouch_Elaborations_2001",
            "_Softtouch_Elaborations_2002",
            "_Neutral_Elaborations_2001",
            "_Neutral_Answers_2002",
            "_Neutral_Answers_2001",
            "_Neutral_Answers_2005",
            "_Neutral_Acknowledgement_2006",
            "_Neutral_Acknowledgement_2005",
            "_Neutral_Acknowledgement_2002",
            "_Neutral_Engagement_2003",
            "_Neutral_Engagement_2002",
            "_Softtouch_Engagement_2002",
            "_Neutral_Distractions_2001",
            "_Neutral_Distractions_2002",
        };

        List<string> favorites;

        if (VitaGlobals.m_loggedInProfile == null)
        {
            favorites = defaultFavorites;
        }
        else
        {
            favorites = VitaGlobals.m_loggedInProfile.favorites;
        }

        foreach (string favUtt in favorites)
        {
            foreach (AGUtteranceData categoryUtt in categoryUtts)
            {
                //If utterance base name in favorites, add it
                if (favUtt.Contains(GetUtteranceBaseName(categoryUtt.name)))
                {
                    //Don't add if already in there
                    if (!returnUtterances.Contains(categoryUtt))
                    {
                        returnUtterances.Add(categoryUtt);
                    }
                    break;
                }
            }
        }

        //string msg = "Returning: ";
        //foreach (AGUtteranceData utt in returnUtterances) msg += GetUtteranceBaseName(utt.name) + ", ";
        //Debug.Log("<color=green>" + msg + "</color>");
        return returnUtterances;
    }

    /// <summary>
    /// Function to prune off "ChrName_" part of "ChrName_Opening_2001" utterance names
    /// </summary>
    /// <param name="utteranceName"></param>
    /// <returns></returns>
    string GetUtteranceBaseName(string utteranceName)
    {
        return utteranceName.Substring(utteranceName.IndexOf('_') + 1).Replace("\n", "");
    }

    string GetUtteranceBaseNameNoDisposition(string utteranceName)
    {
        string baseName = GetUtteranceBaseName(utteranceName);
        return baseName.Substring(baseName.IndexOf('_') + 1).Replace("\n", "");
    }

    /// <summary>
    /// Closes the popup without saving
    /// </summary>
    /// <param name="m_categoryName"></param>
    public void BtnPopupClose()
    {
        //Debug.LogFormat("BtnPopupClose({0})", m_categoryName);

        m_popupArResponses.SetActive(false);

        VHUtils.DeleteChildren(m_popupArResponsesContent.transform);
        m_popupChrUttPrefabList.Clear();
    }
    #endregion

    #region Logging
    /// <summary>
    /// Log event in DB and create entry in UI's event log
    /// </summary>
    /// <param name="utt"></param>
    /// <param name="uttText"></param>
    public void LogEvent_Teacher(string utt, string uttText)
    {
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewPlayUtterance(utt));
        AddWidgetToList(DialogTeacherPrefab, EventLogContentObj, "DialogTeacherPrefab_" + utt, "Text", uttText);
    }

    public void LogEvent_Student(string uttText)
    {
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewStudentResponse(uttText));
        AddWidgetToList(DialogStudentPrefab, EventLogContentObj, "DialogStudentPrefab", "Text", uttText);
    }

    public void LogEvent_Scoring(List<int> miasScore, string scoringNote)
    {
        VitaGlobals.m_localSessionInfo.eventData.Add(DBSession.EventData.NewScoreNote(miasScore, scoringNote));

        string s0 = miasScore[0] == -1 ? "-" : miasScore[0].ToString();
        string s1 = miasScore[1] == -1 ? "-" : miasScore[1].ToString();
        string s2 = miasScore[2] == -1 ? "-" : miasScore[2].ToString();
        string s3 = miasScore[3] == -1 ? "-" : miasScore[3].ToString();
        string s4 = miasScore[4] == -1 ? "-" : miasScore[4].ToString();
        string note = string.Format("F[ {0} ]    I[ {1} ]    S[ {2} ]    A[ {3} ]    C[ {4} ]\n{5}", s0, s1, s2, s3, s4, scoringNote);
        AddWidgetToList(DialogDataPrefab, EventLogContentObj, "DialogDataPrefab_ScoreNote", "Text", note);
    }
    #endregion

    #region Playback
    void GetStudentResponse()
    {
        Debug.LogWarning("Generating dummy student responses");
        string[] m_studentResponseStrings = new string[] { "Student response",
            //"I'd like to learn how to fly",
            //"My brother likes to play basketball with me",
            //"In the past I've had to deal with workplace disagreements and this is how I ended up resolving them",
            //"I'd like to share my experiences with you, cuz Ima rock-star!"
        };
        string m_studentResponseString = m_studentResponseStrings[UnityEngine.Random.Range(0, m_studentResponseStrings.Length)];
        LogEvent_Student(m_studentResponseString);
    }

    /// <summary>
    /// Plays an utterance and sets up the next Primary response in small window
    /// </summary>
    /// <param name="utt"></param>
    /// <param name="uttText"></param>
    public void PlayUtt(string utt, string uttText)
    {
        Debug.LogFormat("PlayUtt({0}, {1})", utt, uttText);
        NetworkRelay.SendNetworkMessage("playcutscene "+utt);
        LogEvent_Teacher(utt, uttText);

        //Setup next Primary response utterance in the small window
        SetNextUtt();

        //This is a hack for now to get student reponse/user input, but may probably turn into a "wait for user" type of function
        //GetStudentResponse();
    }

    /// <summary>
    /// Sets up next response. This defaults to the lowest numbered primary response that hasn't yet been used.
    /// </summary>
    void SetNextUtt()
    {
        string NextUtt = "";
        string uttText = "";

        if (InterviewBtnStateMng == null)
        {
            Debug.LogWarning("No interview button state manager found.");
            return;
        }

        //Find and highlight "next reponse" button
        AGInterviewButtonStateManager.AGInterviewButtonState NextUttButton = InterviewBtnStateMng.GetLowestNumUnusedPrimaryResponseButton();
        if (NextUttButton != null)
        {
            GameObject.FindObjectOfType<EventSystem>().SetSelectedGameObject(NextUttButton.ButtonGO);
            NextUtt = NextUttButton.UtteranceDataCmp.name;
            uttText = NextUttButton.UtteranceDataCmp.GetComponent<AudioSpeechFile>().UtteranceText;

            //Updated "Next" window
            NextUttBtn.SetActive(true);
            NextUttFinishedObj.SetActive(false);
            GameObject t = VHUtils.FindChild(NextUttBtn, "TextMain");
            t.GetComponent<Text>().text = uttText;
            Button btn = NextUttBtn.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            //Set up interactivity between "Next Utterance" window and advancing the highlighting in the "pinned" primary questions window
            GameObject targetPrimaryWidget = null;
            foreach (AGInterviewButtonStateManager.AGInterviewButtonState button in InterviewBtnStateMng.m_interviewButtonStates)
            {
                if (button.ButtonName == NextUtt)
                {
                    targetPrimaryWidget = button.ButtonGO;
                }
            }
            if (targetPrimaryWidget == null)
            {
                Debug.LogWarning("Next primary utterance button widget not found.");
            }
            else
            {
                btn.onClick.AddListener(delegate { SetButtonPressed(targetPrimaryWidget); PlayUtt(NextUtt, uttText); });
            }
        }
        else
        {
            NextUttBtn.SetActive(false);
            NextUttFinishedObj.SetActive(true);
        }
    }

    /// <summary>
    /// Used to make sure the next primary utterance is selected upon the primaryUtteranceScrollView gets re-enabled.
    /// </summary>
    void SelectNextUtt()
    {
        if (InterviewBtnStateMng != null)
        {
            //Find and highlight "next reponse" button
            AGInterviewButtonStateManager.AGInterviewButtonState NextUttButton = InterviewBtnStateMng.GetLowestNumUnusedPrimaryResponseButton();
            if (NextUttButton != null)
            {
                NextUttBtn.SetActive(true);
                NextUttFinishedObj.SetActive(false);
                GameObject.FindObjectOfType<EventSystem>().SetSelectedGameObject(NextUttButton.ButtonGO);
            }
            else
            {
                NextUttBtn.SetActive(false);
                NextUttFinishedObj.SetActive(true);
            }
        }
    }
    #endregion

    #region UI Hooks
    public void BtnBuyMeTime()
    {
        AGUtteranceData randomBuyTimeUtt;
        while (true)
        {
            randomBuyTimeUtt = BuyTimeContent[UnityEngine.Random.Range(0, BuyTimeContent.Count)];
            if (BuyTimeContentAlreadyPlayed.Count == BuyTimeContent.Count)
                BuyTimeContentAlreadyPlayed.Clear();

            if (BuyTimeContentAlreadyPlayed.Contains(randomBuyTimeUtt))
                continue;

            BuyTimeContentAlreadyPlayed.Add(randomBuyTimeUtt);
            break;
        }

        Debug.LogFormat("BtnBuyMeTime() - Playing: {0} - CountTotal: {1} - CountPlayed: {2}", randomBuyTimeUtt.name, BuyTimeContent.Count, BuyTimeContentAlreadyPlayed.Count);

        string randomByTimeUttText = randomBuyTimeUtt.GetComponent<AudioSpeechFile>().UtteranceText;
        PlayUtt(randomBuyTimeUtt.name, randomByTimeUttText);
    }

    public void BtnPopup_ScoringNote()
    {
        //Clear menu
        m_field_scoringNote.text = "";
        foreach (AGGuiToggleScoreGraph score in new AGGuiToggleScoreGraph[] { m_mias_firstImpression, m_mias_interviewResponses, m_mias_selfPromoting, m_mias_activeListening, m_mias_closing })
        {
            score.SetScore(-1);
        }
        m_popupScoring.SetActive(true);
    }

    public void BtnCompleteScenario()
    {
        VitaGlobals.m_setReturnMenu = true;
        VitaGlobals.m_returnMenu = MenuManager.Menu.InterviewCompletion;

        NetworkRelay.SendNetworkMessage("endinterview");
    }

    public void BtnPopupClose_ScoringNote()
    {
        m_popupScoring.SetActive(false);
    }

    public void BtnExitScenario()
    {
        if (VitaGlobals.m_interviewType == VitaGlobals.InterviewType.Demo)
        {
            BtnExitScenarioCallback();
        }
        else
        {
            PopUpDisplay.Instance.DisplayOkCancel("Warning", "The interview session hasn't been completed. Any progress will not be saved. Continue to exit?",
                BtnExitScenarioCallback, null);
        }
    }

    void BtnExitScenarioCallback()
    {
        VitaGlobals.m_setReturnMenu = true;
        if (VitaGlobals.m_interviewType == VitaGlobals.InterviewType.Demo)
            VitaGlobals.m_returnMenu = MenuManager.Menu.DemoConfigure;
        else
            VitaGlobals.m_returnMenu = MenuManager.Menu.TeacherConfigure;

        NetworkRelay.SendNetworkMessage("endinterview");


        //PopupDialogObjCopy.SetActive(false);
        //Destroy(PopupDialogObjCopy);
        //PopupDialogCmp = null;
    }

    public void BtnIntervieweeQuestion()
    {
        Debug.LogWarning("Waiting for popup window (interviewee question)");
        //GameObject.FindObjectOfType<MenuManager>().ChangeMenu(MenuManager.Menu);
    }

    public void BtnHome()
    {
        ;
    }

    public void BtnBack()
    {
        ;
    }

    public void BtnPinPrimaryQuestions()
    {
        CurrentlyPinnedResponseType = VitaGlobals.VitaResponseTypes.Primary;
        UpdatePinnedTitle();
        SetPrimaryUtt(PrimaryContentObj, PrimaryContent);
    }

    public void BtnPinOpening()
    {
        CurrentlyPinnedResponseType = VitaGlobals.VitaResponseTypes.Opening;
        UpdatePinnedTitle();
        SetCategotyUtt(PinnedContentObj, OpeningsContent);
    }

    public void BtnPinEngagement()
    {
        CurrentlyPinnedResponseType = VitaGlobals.VitaResponseTypes.Engagement;
        UpdatePinnedTitle();
        SetCategotyUtt(PinnedContentObj, EngagementsContent);
    }

    public void BtnPinAcknowledgements()
    {
        CurrentlyPinnedResponseType = VitaGlobals.VitaResponseTypes.Acknowledgement;
        UpdatePinnedTitle();
        SetCategotyUtt(PinnedContentObj, AcknowledgementsContent);
    }

    public void BtnPinElaborations()
    {
        CurrentlyPinnedResponseType = VitaGlobals.VitaResponseTypes.Elaboration;
        UpdatePinnedTitle();
        SetCategotyUtt(PinnedContentObj, ElaborationsContent);
    }

    public void BtnPinDistractions()
    {
        CurrentlyPinnedResponseType = VitaGlobals.VitaResponseTypes.Distraction;
        UpdatePinnedTitle();
        SetCategotyUtt(PinnedContentObj, DistractionsContent);
    }

    public void BtnPinAnswers()
    {
        CurrentlyPinnedResponseType = VitaGlobals.VitaResponseTypes.Answer;
        UpdatePinnedTitle();
        SetCategotyUtt(PinnedContentObj, AnswersContent);
    }

    public void BtnFavOpening()
    {
        PopupArResponses("Opening");
    }

    public void BtnFavEngagement()
    {
        PopupArResponses("Engagement");
    }

    public void BtnFavAcknowledgements()
    {
        PopupArResponses("Acknowledgement");
    }

    public void BtnFavElaborations()
    {
        PopupArResponses("Elaboration");
    }

    public void BtnFavDistractions()
    {
        PopupArResponses("Distraction");
    }

    public void BtnFavAnswers()
    {
        PopupArResponses("Answer");
    }

    public void BtnPopupSave_ScoringNote()
    {
        List<int> m_miasScore = new List<int>();
        foreach (AGGuiToggleScoreGraph score in new AGGuiToggleScoreGraph[] { m_mias_firstImpression, m_mias_interviewResponses, m_mias_selfPromoting, m_mias_activeListening, m_mias_closing })
            m_miasScore.Add(score.GetScore());
        string m_scoringNoteString = m_field_scoringNote.text;
        LogEvent_Scoring(m_miasScore, m_scoringNoteString);
        BtnPopupClose_ScoringNote();
    }
    #endregion
}
