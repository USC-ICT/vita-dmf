using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerPanelTeacherProfile : MonoBehaviour, MenuManager.IMenuManagerInterface
{
    #region Variables
    MenuManager m_menuManager;
    EntityDBVitaProfile m_user;

    GameObject m_headerName;
    GameObject m_firstName;
    GameObject m_lastName;
    GameObject m_userName;
    GameObject m_password;
    GameObject m_dateCreated;

    GameObject m_saveButton;

    GameObject m_favResponsesAcknowledgementContent;
    GameObject m_favResponsesAnswerContent;
    GameObject m_favResponsesEngagementContent;
    GameObject m_favResponsesElaborationContent;
    GameObject m_favResponsesDistractionContent;
    GameObject m_favResponsesOpeningContent;

    GameObject m_favResponsesAcknowledgementLoading;
    GameObject m_favResponsesAnswerLoading;
    GameObject m_favResponsesEngagementLoading;
    GameObject m_favResponsesElaborationLoading;
    GameObject m_favResponsesDistractionLoading;
    GameObject m_favResponsesOpeningLoading;

    GameObject m_popupFavorites;
    GameObject m_popupFavoritesContent;
    Text m_popupFavoritesTitle;
    Button m_popupCloseBtn;
    Button m_popupSaveBtn;

    GameObject m_responseToggleWidget;

    //Need a reference to one character's responses set to edit favorites
    public GameObject m_characterUtterancesPrefab;
    GameObject m_characterUtterancesPrefabInstance = null;
    List<GameObject> m_popupChrUttPrefabList = new List<GameObject>(); //Used for quickly accessing which items have been selected

    //These are to keep track of adding/removing favorite functions, so we can wait until after add/remove are done before refreshing
    int m_favOpsStarted = 0;
    int m_favOpsFinished = 0;
    #endregion

    void Start()
    {
        m_menuManager = GameObject.FindObjectOfType<MenuManager>();
        GameObject menu = m_menuManager.GetMenuGameObject(MenuManager.Menu.TeacherProfile);
        GameObject resources = VHUtils.FindChildRecursive(menu, "Resources");

        m_headerName = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "HeaderRow Grp"), "GuiTextPrefab");

        m_firstName = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_FirstName");
        m_lastName = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_LastName");
        m_userName = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_UserName");
        m_password = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_Password");
        m_dateCreated = VHUtils.FindChildRecursive(menu, "GiuInputPrefab_DateCreated");

        m_saveButton = VHUtils.FindChildRecursive(menu, "GuiButtonPrefab_Save");

        m_favResponsesAcknowledgementContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Acknowledgements"), "Content");
        m_favResponsesAnswerContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Answers"), "Content");
        m_favResponsesEngagementContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Engagement"), "Content");
        m_favResponsesElaborationContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Elaborations"), "Content");
        m_favResponsesDistractionContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Distractions"), "Content");
        m_favResponsesOpeningContent = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Opening"), "Content");

        m_favResponsesAcknowledgementLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Acknowledgements"), "IconLoading");
        m_favResponsesAnswerLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Answers"), "IconLoading");
        m_favResponsesEngagementLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Engagement"), "IconLoading");
        m_favResponsesElaborationLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Elaborations"), "IconLoading");
        m_favResponsesDistractionLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Distractions"), "IconLoading");
        m_favResponsesOpeningLoading = VHUtils.FindChildRecursive(VHUtils.FindChildRecursive(menu, "GuiTeacherFavoritesWidgetPrefab_Opening"), "IconLoading");

        m_popupFavorites = VHUtils.FindChildRecursive(resources, "PanelTeacherProfileFavoritesPopupPrefab");
        m_popupFavoritesContent = VHUtils.FindChildRecursive(m_popupFavorites, "Content");
        m_popupFavoritesTitle = VHUtils.FindChildRecursive(m_popupFavorites, "GuiTextPrefab_PopupTitle").GetComponent<Text>();
        m_popupCloseBtn = VHUtils.FindChildRecursive(m_popupFavorites, "GuiButtonPrefab_Done").GetComponent<Button>();
        m_popupSaveBtn = VHUtils.FindChildRecursive(m_popupFavorites, "GuiButtonPrefab_Save").GetComponent<Button>();

        //m_responseBtnWidget = VHUtils.FindChildRecursive(resources, "GuiButtonInterviewResponsePrefab");
        m_responseToggleWidget = VHUtils.FindChildRecursive(resources, "GuiToggleInterviewResponsePrefab");
    }

    #region Public Functions
    public void OnMenuEnter()
    {
        string loginName = VitaGlobals.m_loggedInProfile.name;
        string loginOrganization = VitaGlobals.m_loggedInProfile.organization;
        m_headerName.GetComponent<Text>().text = loginName + "\n" + loginOrganization;

        VitaGlobalsUI.m_unsavedChanges = false;

        m_characterUtterancesPrefabInstance = Instantiate(m_characterUtterancesPrefab);
        m_favResponsesAcknowledgementLoading.SetActive(false);
        m_favResponsesAnswerLoading.SetActive(false);
        m_favResponsesDistractionLoading.SetActive(false);
        m_favResponsesElaborationLoading.SetActive(false);
        m_favResponsesEngagementLoading.SetActive(false);
        m_favResponsesOpeningLoading.SetActive(false);

        RefreshTeacherInfo();
    }

    public void OnMenuExit()
    {
        VitaGlobalsUI.m_unsavedChanges = false;

        Destroy(m_characterUtterancesPrefabInstance);
        m_characterUtterancesPrefabInstance = null;
    }

    public void OnValueChangedName(string value)
    {
        //Debug.LogFormat("MenuManagerPanelTeacherProfile.OnValueChangedName() - {0}", value);
        m_user.name = m_firstName.GetComponent<InputField>().text;
        m_saveButton.GetComponent<Button>().interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }

    public void OnValueChangedUsername(string value)
    {
        //Debug.LogFormat("MenuManagerPanelTeacherProfile.OnValueChangedUsername() - {0}", value);
        m_user.username = m_userName.GetComponent<InputField>().text;
        m_saveButton.GetComponent<Button>().interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }

    public void OnValueChangedPassword(string value)
    {
        //Debug.LogFormat("MenuManagerPanelTeacherProfile.OnValueChangedPassword() - {0}", value);
        m_user.password = m_password.GetComponent<InputField>().text;
        m_saveButton.GetComponent<Button>().interactable = AnyChangesMade();
        VitaGlobalsUI.m_unsavedChanges = AnyChangesMade();
    }
    #endregion

    #region UI Hooks
    public void BtnBack()
    {
    }

    public void BtnHome()
    {
    }

    public void BtnSaveProfile()
    {
        StartCoroutine(BtnSaveChangesInternal());
    }

    IEnumerator BtnSaveChangesInternal()
    {
        EntityDBVitaProfile original = VitaGlobals.m_loggedInProfile;

        // look if the username of the teacher has changed
        if (original.username != m_user.username)
        {
            bool waitingOnCallback = true;

            // check for duplicates.  error out because these are not allowed.
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.GetAllUsers((users, error) =>
            {
                waitingOnCallback = false;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("GetAllClasses() error: {0}", error));
                    return;
                }

                if (users.Exists(item => item.username == m_user.username))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("BtnSaveChangesInternal() - '{0}' - duplicate teacher name is in the list.  Teacher names need to be unique.", m_user.username));
                    return;
                }
            });

            while (waitingOnCallback)
                yield return new WaitForEndOfFrame();
        }


        //yield return StartCoroutine(BtnSaveChangesTeacherUsername());
        yield return StartCoroutine(BtnSaveChangesTeacherPassword());
        yield return StartCoroutine(BtnSaveChangesTeacherInfo());


        VitaGlobals.m_loggedInProfile = m_user;

        RefreshTeacherInfo();
    }

    /// <summary>
    /// Fills the popup window with a list of responses and highlight the ones that have been chosen as favorites.
    /// </summary>
    /// <param name="m_category"></param>
    public void BtnPopupEditFavorites(string m_category)
    {
        //Debug.LogFormat("BtnPopupEditFavorites({0})", m_category);

        //RefreshTeacherInfo(); //Note: for efficiency, we can consider managing the favorites list locally, upon saving, so we don't have to "Get" it every time

        //Set up popup
        m_popupCloseBtn.onClick.RemoveAllListeners();
        m_popupSaveBtn.onClick.RemoveAllListeners();
        m_popupCloseBtn.onClick.AddListener(() => { BtnPopupClose(m_category); });
        m_popupSaveBtn.onClick.AddListener(() => { BtnPopupSave(m_category); });
        VHUtils.DeleteChildren(m_popupFavoritesContent.transform);
        m_popupFavoritesTitle.text = m_category + " Favorites";
        m_popupChrUttPrefabList.Clear();
        m_popupFavorites.SetActive(true);

        //Get "this" category's utterances to compare against DB stored favorites
        AGUtteranceDataFilter filterCmp = m_characterUtterancesPrefabInstance.GetComponent<AGUtteranceDataFilter>();
        bool[] scenarios = new bool[VitaGlobals.m_vitaMoods.Length];
        for (int i = 0; i < scenarios.Length; i++)
            scenarios[i] = true;  //Use all dispositions

        VitaGlobals.VitaResponseTypes m_categoryEnum = (VitaGlobals.VitaResponseTypes)Array.IndexOf(VitaGlobals.m_vitaResponseTypes, m_category);
        List<AGUtteranceData> utteranceList = filterCmp.GetSounds(0, scenarios, filterCmp.GetResponseTypeBoolArrayByEnum(m_categoryEnum));

        //Create string list for comparing favorite names
        List<string> favoriteStringNames = new List<string>();
        foreach (var fav in m_user.favorites)
        {
            try
            {
                favoriteStringNames.Add(DBUser.FavoriteData.ToFavorite(fav).FavoriteId);
            }
            catch (Exception e)
            {
                //This put as a safeguard, for wrong data due to development phase
                Debug.LogWarningFormat("Favorite data is in the wrong format, ignoring: {0}\n\n<color=red>{1}</color>", fav.ToString(), e.ToString());
                continue;
            }
        }

        //Populate and highlight chosen favorites
        foreach (AGUtteranceData utterance in utteranceList)
        {
            GameObject widget = AddWidgetToList(m_responseToggleWidget, m_popupFavoritesContent, utterance.name, "TextMain", utterance.GetComponent<AudioSpeechFile>().UtteranceText);
            m_popupChrUttPrefabList.Add(widget);
            GameObject thisWidget = widget;
            widget.GetComponent<Toggle>().onValueChanged.AddListener(delegate { LimitThreeFavorites(thisWidget.GetComponent<Toggle>()); });

            if (favoriteStringNames.Contains(GetUtteranceBaseName(utterance.name)))
            {
                widget.GetComponent<Toggle>().isOn = true;
            }
        }
    }

    /// <summary>
    /// Closes the popup without saving
    /// </summary>
    /// <param name="m_categoryName"></param>
    public void BtnPopupClose(string m_categoryName)
    {
        //Debug.LogFormat("BtnPopupClose({0})", m_categoryName);

        m_popupFavorites.SetActive(false);

        VHUtils.DeleteChildren(m_popupFavoritesContent.transform);
        m_popupChrUttPrefabList.Clear();
    }

    /// <summary>
    /// Closes the popup and saves the chosen 3 favorite responses
    /// </summary>
    /// <param name="m_categoryName"></param>
    public void BtnPopupSave(string m_categoryName)
    {
        Debug.LogFormat("BtnPopupSave({0})", m_categoryName);

        m_popupFavorites.SetActive(false);
        m_favOpsStarted = m_popupChrUttPrefabList.Count;
        m_favOpsFinished = 0;

        foreach (GameObject go in m_popupChrUttPrefabList)
        {
            string baseName = GetUtteranceBaseName(go.name);
            if (go.GetComponent<Toggle>().isOn == true)
            {
                AddFavorite(baseName);
            }
            else
            {
                RemoveFavorite(baseName);
            }
        }

        ////Clear this menu
        //VHUtils.DeleteChildren(m_popupFavoritesContent.transform);
        //m_popupChrUttPrefabList.Clear();

        //Update favorites list
        StartCoroutine(UpdateFavoriteContentAfterChanges(m_categoryName));

    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Gets a list of favorite responses based on the given responseType
    /// </summary>
    /// <param name="m_responseType">VitaGlobals.VitaResponseTypes</param>
    /// <returns></returns>
    List<AGUtteranceData> GetFavoriteResponses(VitaGlobals.VitaResponseTypes m_responseType)
    {
        List<AGUtteranceData> m_returnUttList = new List<AGUtteranceData>();
        AGUtteranceDataFilter filterCmp = m_characterUtterancesPrefabInstance.GetComponent<AGUtteranceDataFilter>();
        bool[] scenarios = new bool[VitaGlobals.m_vitaMoods.Length];
        for (int i = 0; i < scenarios.Length; i++)
            scenarios[i] = true;  //Use all scenarios

        List<AGUtteranceData> utteranceList = filterCmp.GetSounds(0, scenarios, filterCmp.GetResponseTypeBoolArrayByEnum(m_responseType));

        //Create string list for comparing favorite names
        List<string> favoriteStringNames = new List<string>();
        foreach (var fav in m_user.favorites)
        {
            //Debug.LogWarning(fav);
            try
            {
                favoriteStringNames.Add(DBUser.FavoriteData.ToFavorite(fav).FavoriteId);
            }
            catch (Exception e)
            {
                //This put as a safeguard, for wrong data due to development phase
                Debug.LogWarningFormat("Favorite data is in the wrong format, ignoring: {0}\n\n<color=red>{1}</color>", fav.ToString(), e.ToString());
                //Debug.LogError(e);
                continue;
            }
        }

        foreach (AGUtteranceData utterance in utteranceList)
        {
            if (favoriteStringNames.Contains(GetUtteranceBaseName(utterance.name)))
            {
                m_returnUttList.Add(utterance);
                //Debug.LogWarning("Returning fav: " + utterance.name);
            }
        }

        return m_returnUttList;
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

    void AddFavorite(string baseName)
    {
        List<string> favoritesToAdd = m_user.favorites.FindAll(item => item.Contains(baseName));

        if (favoritesToAdd == null || favoritesToAdd.Count == 0)
        {
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            {
                DBUser.FavoriteData fav = DBUser.FavoriteData.NewFavorite(baseName);
                dbUser.AddFavorite(m_user.username, fav, (profile, error) =>
                {
                    if (m_favOpsStarted == 0)
                    {
                        return;
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            PopUpDisplay.Instance.Display("Error", string.Format("AddFavoriteQuestionToProfile() error: {0}", error));
                            m_favOpsFinished += 1;
                            return;
                        }

                        m_user.favorites.Add(DBUser.FavoriteData.ToJSONString(fav));
                        VitaGlobals.m_loggedInProfile = m_user;
                        m_favOpsFinished += 1;
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("{0}", e.ToString());
                        m_favOpsFinished += 1;
                    }
                });
            }
        }
        else
        {
            m_favOpsFinished += 1;
        }
    }

    void RemoveFavorite(string baseName)
    {
        List<string> favoritesToRemove = m_user.favorites.FindAll(item => item.Contains(baseName));

        if (favoritesToRemove != null && favoritesToRemove.Count > 0)
        {
            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            int internalOperationsCounter = 0; //This counter used to deal with multiple entries in favorites, which causes a premature "operationFinish"

            for (int i = favoritesToRemove.Count - 1; i >= 0; i--)
            {
                int inx = i;
                dbUser.RemoveFavorite(m_user.username, DBUser.FavoriteData.ToFavorite(favoritesToRemove[inx]), (profile, error) =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            PopUpDisplay.Instance.Display("Error", string.Format("RemoveFavoriteQuestionFromProfile() error: {0}", error));
                            internalOperationsCounter += 1;
                            return;
                        }

                        m_user.favorites.Remove(favoritesToRemove[inx]);
                        VitaGlobals.m_loggedInProfile = m_user;
                        internalOperationsCounter += 1;
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("{0}", e.ToString());
                        internalOperationsCounter += 1;
                    }

                    if (internalOperationsCounter == favoritesToRemove.Count)
                    {
                        m_favOpsFinished += 1;
                        internalOperationsCounter = 0;
                    }
                });
            }
        }
        else
        {
            m_favOpsFinished += 1;
        }
    }

    IEnumerator UpdateFavoriteContentAfterChanges(string m_categoryName)
    {
        //Rather than calling "RefreshTeacherInfo()" to update the entire page, this section will update only the targeted area, which speeds things up a lot
        VitaGlobals.VitaResponseTypes m_categoryEnum = (VitaGlobals.VitaResponseTypes)Array.IndexOf(VitaGlobals.m_vitaResponseTypes, m_categoryName);
        GameObject targetResponseContent = null;
        GameObject targetResponseLoading = null;
        if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Acknowledgement) { targetResponseContent = m_favResponsesAcknowledgementContent; targetResponseLoading = m_favResponsesAcknowledgementLoading; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Answer) { targetResponseContent = m_favResponsesAnswerContent; targetResponseLoading = m_favResponsesAnswerLoading; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Distraction) { targetResponseContent = m_favResponsesDistractionContent; targetResponseLoading = m_favResponsesDistractionLoading; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Elaboration) { targetResponseContent = m_favResponsesElaborationContent; targetResponseLoading = m_favResponsesElaborationLoading; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Engagement) { targetResponseContent = m_favResponsesEngagementContent; targetResponseLoading = m_favResponsesEngagementLoading; }
        else if (m_categoryEnum == VitaGlobals.VitaResponseTypes.Opening) { targetResponseContent = m_favResponsesOpeningContent; targetResponseLoading = m_favResponsesOpeningLoading; }

        targetResponseLoading.SetActive(true);
        while (m_favOpsStarted != m_favOpsFinished)
        {
            //Debug.Log("Waiting for favorites changes to save before updating: " + m_categoryName);
            yield return new WaitForEndOfFrame();
        }
        targetResponseLoading.SetActive(false);

        //Debug.Log("Now Updating: " + m_categoryName);
        m_user = new EntityDBVitaProfile(VitaGlobals.m_loggedInProfile);

        foreach (var f in m_user.favorites)
        {
            Debug.Log("<color=green>"+f+"</color>");
        }

        VHUtils.DeleteChildren(targetResponseContent.transform);
        //Debug.Log("Now Updating: " + targetResponseContent.name);
        foreach (AGUtteranceData utterance in GetFavoriteResponses(m_categoryEnum))
        {
            Debug.Log("Adding fav utt: " + utterance.name);
            GameObject widget = AddWidgetToList(m_responseToggleWidget, targetResponseContent, utterance.name, "TextMain", utterance.GetComponent<AudioSpeechFile>().UtteranceText);
            widget.GetComponent<Toggle>().isOn = true;
            widget.GetComponent<Toggle>().enabled = false;
        }

        //Clear favorites popup menu
        VHUtils.DeleteChildren(m_popupFavoritesContent.transform);
        m_popupChrUttPrefabList.Clear();
        m_favOpsStarted = 0;
        m_favOpsFinished = 0;
    }

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
        m_widgetDisplayString = m_widgetDisplayString.Replace("\n", "");

        //Debug.LogFormat("AddWidgetToList({0})", m_widgetDisplayString);

        GameObject widgetGo = Instantiate(m_widget);
        GameObject widgetText = VHUtils.FindChildRecursive(widgetGo, m_textObjName);
        widgetGo.name = m_widgetName;
        widgetGo.SetActive(true);
        widgetGo.transform.SetParent(m_listContent.transform, false);
        widgetText.GetComponent<Text>().text = m_widgetDisplayString;

        return widgetGo;
    }

    void LimitThreeFavorites(Toggle triggeredToggle)
    {
        int m_numSelectedFavs = 0;

        //Tally number of selected
        foreach (GameObject widget in m_popupChrUttPrefabList)
        {
            if (widget.GetComponent<Toggle>().isOn)
            {
                m_numSelectedFavs += 1;
            }
        }

        //Un-toggle if already three selected
        if (m_numSelectedFavs > 3)
        {
            triggeredToggle.isOn = false;
        }
    }

    void RefreshTeacherInfo()
    {
        EntityDBVitaProfile original = VitaGlobals.m_loggedInProfile;

        if (original == null)
        {
            Debug.LogWarning("RefreshTeacherInfo() failed; 'original' var is null");
            Debug.LogWarning("Using dummy favorites until profiles are properly hooked");

            m_user = new EntityDBVitaProfile("bob-smith", "12345", "dmf", "Bob Smith", (int)DBUser.AccountType.TEACHER);
            m_user.favorites = new List<string>() {"Alex_Neutral_Acknowledgement_2001", "Alex_Neutral_Acknowledgement_2003", "Alex_Neutral_Answers_2002", "Alex_Neutral_Answers_2004", "Alex_Neutral_Distractions_2001", "Alex_Neutral_Distractions_2002", "Alex_Neutral_Elaborations_2002", "Alex_Neutral_Elaborations_2001", "Alex_Neutral_Engagement_2003", "Alex_Neutral_Opening_2001"};
        }
        else
        {
            m_user = new EntityDBVitaProfile(original);
        }

        //foreach (var f in m_user.favorites)
        //{
        //    Debug.Log("<color=green>" + f + "</color>");
        //}

        m_firstName.GetComponent<InputField>().text = m_user.name;
        m_lastName.GetComponent<InputField>().text = "";
        m_userName.GetComponent<InputField>().text = m_user.username;
        m_password.GetComponent<InputField>().text = m_user.password;
        m_dateCreated.GetComponent<InputField>().text = VitaGlobals.TicksToString(m_user.createdate);

        m_saveButton.GetComponent<Button>().interactable = false;
        VitaGlobalsUI.m_unsavedChanges = false;

        //Populate favorite responses
        VHUtils.DeleteChildren(m_favResponsesAcknowledgementContent.transform);
        VHUtils.DeleteChildren(m_favResponsesAnswerContent.transform);
        VHUtils.DeleteChildren(m_favResponsesEngagementContent.transform);
        VHUtils.DeleteChildren(m_favResponsesElaborationContent.transform);
        VHUtils.DeleteChildren(m_favResponsesDistractionContent.transform);
        VHUtils.DeleteChildren(m_favResponsesOpeningContent.transform);

        foreach (AGUtteranceData utterance in GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Acknowledgement))
        {
            GameObject widget = AddWidgetToList(m_responseToggleWidget, m_favResponsesAcknowledgementContent, utterance.name, "TextMain", utterance.GetComponent<AudioSpeechFile>().UtteranceText);
            widget.GetComponent<Toggle>().isOn = true;
            widget.GetComponent<Toggle>().enabled = false;
        }

        foreach (AGUtteranceData utterance in GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Answer))
        {
            GameObject widget = AddWidgetToList(m_responseToggleWidget, m_favResponsesAnswerContent, utterance.name, "TextMain", utterance.GetComponent<AudioSpeechFile>().UtteranceText);
            widget.GetComponent<Toggle>().isOn = true;
            widget.GetComponent<Toggle>().enabled = false;
        }

        foreach (AGUtteranceData utterance in GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Engagement))
        {
            GameObject widget = AddWidgetToList(m_responseToggleWidget, m_favResponsesEngagementContent, utterance.name, "TextMain", utterance.GetComponent<AudioSpeechFile>().UtteranceText);
            widget.GetComponent<Toggle>().isOn = true;
            widget.GetComponent<Toggle>().enabled = false;
        }

        foreach (AGUtteranceData utterance in GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Elaboration))
        {
            GameObject widget = AddWidgetToList(m_responseToggleWidget, m_favResponsesElaborationContent, utterance.name, "TextMain", utterance.GetComponent<AudioSpeechFile>().UtteranceText);
            widget.GetComponent<Toggle>().isOn = true;
            widget.GetComponent<Toggle>().enabled = false;
        }

        foreach (AGUtteranceData utterance in GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Distraction))
        {
            GameObject widget = AddWidgetToList(m_responseToggleWidget, m_favResponsesDistractionContent, utterance.name, "TextMain", utterance.GetComponent<AudioSpeechFile>().UtteranceText);
            widget.GetComponent<Toggle>().isOn = true;
            widget.GetComponent<Toggle>().enabled = false;
        }

        foreach (AGUtteranceData utterance in GetFavoriteResponses(VitaGlobals.VitaResponseTypes.Opening))
        {
            GameObject widget = AddWidgetToList(m_responseToggleWidget, m_favResponsesOpeningContent, utterance.name, "TextMain", utterance.GetComponent<AudioSpeechFile>().UtteranceText);
            widget.GetComponent<Toggle>().isOn = true;
            widget.GetComponent<Toggle>().enabled = false;
        }
    }

    bool TeacherEquals(EntityDBVitaProfile originalTeacher, EntityDBVitaProfile newTeacher)
    {
        return /*originalTeacher.username == newTeacher.username &&*/    // ignore name since we special case it
               /*originalTeacher.password == newTeacher.password &&*/
               originalTeacher.name == newTeacher.name;
    }

    bool AnyChangesMade()
    {
        EntityDBVitaProfile original = VitaGlobals.m_loggedInProfile;

        if (original.username != m_user.username)
        {
            //Debug.LogWarningFormat("AnyChangesMade() - username - {0} - {1}", original.username, m_user.username);
            return true;
        }

        if (original.password != m_user.password)
        {
            //Debug.LogWarningFormat("AnyChangesMade() - password - {0} - {1}", original.password, m_user.password);
            return true;
        }

        if (!TeacherEquals(original, m_user))
        {
            //Debug.LogWarningFormat("AnyChangesMade() - teacherequal - {0} - {1}", original.username, m_user.username);
            return true;
        }

        return false;
    }
    #endregion

    IEnumerator BtnSaveChangesTeacherUsername()
    {
        EntityDBVitaProfile original = VitaGlobals.m_loggedInProfile;

        // look if the username of the teacher has changed
        if (original.username != m_user.username)
        {
            bool waitingOnCallback = true;

            Debug.LogWarningFormat("BtnSaveChanges() - changing teacher name - {0}", m_user.username);

            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.UpdateUserName(original.username, m_user.username, error =>
            {
                waitingOnCallback = false;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateUserName() error: {0}", error));
                    return;
                }
            });

            while (waitingOnCallback)
                yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator BtnSaveChangesTeacherPassword()
    {
        EntityDBVitaProfile original = VitaGlobals.m_loggedInProfile;

        // look if the password of the teacher has changed
        if (original.password != m_user.password)
        {
            bool waitingOnCallback = true;

            Debug.LogWarningFormat("BtnSaveChanges() - changing teacher password - {0}", m_user.password);

            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.UpdateUserPassword(original.username, m_user.password, error =>
            {
                waitingOnCallback = false;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateUserPassword() error: {0}", error));
                    return;
                }
            });

            while (waitingOnCallback)
                yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator BtnSaveChangesTeacherInfo()
    {
        EntityDBVitaProfile original = VitaGlobals.m_loggedInProfile;

        // look for changes made in teacher profile
        if (!TeacherEquals(original, m_user))
        {
            bool waitingOnCallback = true;

            Debug.LogWarningFormat("BtnSaveChanges() - updating teacher with changes - {0}", m_user.username);

            DBUser dbUser = GameObject.FindObjectOfType<DBUser>();
            dbUser.UpdateUser(m_user.username, m_user.organization, m_user.name, (DBUser.AccountType)m_user.type, m_user.lastlogin, error =>
            {
                waitingOnCallback = false;

                if (!string.IsNullOrEmpty(error))
                {
                    PopUpDisplay.Instance.Display("Error", string.Format("UpdateUser() error: {0}", error));
                    return;
                }
            });

            while (waitingOnCallback)
                yield return new WaitForEndOfFrame();
        }
    }
}
