using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

static public class uGuiUtils
{
    #region Constants
    static readonly Color DefaultTextColor = new Color(0.196f, 0.196f, 0.196f, 1);
    //static readonly Color DefaultPanelColor = new Color(1, 1, 1, 100f / 255f);
    #endregion

    #region Functions
    static public Canvas CreateCanvas(string canvasName, int sortingOrder)
    {
        return CreateCanvas(canvasName, null, sortingOrder, RenderMode.ScreenSpaceOverlay);
    }

    static public Canvas CreateCanvas(string canvasName, GameObject parent, int sortingOrder)
    {
        return CreateCanvas(canvasName, parent, sortingOrder, RenderMode.ScreenSpaceOverlay);
    }

    static public Canvas CreateCanvas(string canvasName, int sortingOrder, RenderMode renderMode)
    {
        return CreateCanvas(canvasName, null, sortingOrder, RenderMode.ScreenSpaceOverlay);
    }

    static public Canvas CreateCanvas(string canvasName, GameObject parent, int sortingOrder, RenderMode renderMode)
    {
        GameObject canvasGO = new GameObject("Canvas", new System.Type[] { typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster) } );
        canvasGO.name = canvasName;
        if (parent != null)
            canvasGO.transform.SetParent(parent.transform);

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        // need an event system too if there isn't one
        if (GameObject.FindObjectOfType<EventSystem>() == null)
        {
            EventSystem eventSystem = GameObject.Instantiate(Resources.Load<EventSystem>("vhAssetsEventSystem"));
            eventSystem.name = eventSystem.name.Replace("(Clone)", "");
        }

        return canvas;
    }

    static public Image CreateImage(string goName, Transform parent)
    {
        return CreateImage(goName, parent, null);
    }

    static public Image CreateImage(string goName, Transform parent, Sprite sprite)
    {
        Image image = GameObject.Instantiate(Resources.Load<Image>("vhAssetsImage"));
        image.transform.SetParent(parent);
        image.name = goName;
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        return image;
    }

    static public Text CreateText(string goName, Transform parent, string text)
    {
        return CreateText(goName, parent, text, DefaultTextColor);
    }

    static public Text CreateText(string goName, Transform parent, string text, Color textColor)
    {
        Text t = GameObject.Instantiate<Text>((Resources.Load<Text>("vhAssetsText")));
        t.name = goName;
        //t.fontSize = 14;
        t.transform.SetParent(parent);
        t.color = textColor;
        t.text = text;
        return t;
    }

    static public Text CreateLayoutText(string goName, Transform parent, string text, Color textColor, float screenPctWidth, float screenPctHeight)
    {
        Text t = CreateText(goName, parent, text, textColor);
        LayoutElement layout = t.gameObject.AddComponent<LayoutElement>();
        SetPreferredLayout(layout, screenPctWidth, screenPctHeight);
        return t;
    }

    static public void SetPreferredLayout(LayoutElement layout, float screenPctWidth, float screenPctHeight)
    {
        layout.preferredWidth = screenPctWidth * Screen.width;
        layout.preferredHeight = screenPctHeight * Screen.height;
    }

    static public InputField CreateInputField(string goName, Transform parent, UnityEngine.Events.UnityAction<string> onEndEditCallback)
    {
        InputField field = GameObject.Instantiate(Resources.Load<InputField>("vhAssetsInputField"));
        field.name = goName;
        field.transform.SetParent(parent);
        if (onEndEditCallback != null)
        {
            field.onEndEdit.AddListener(onEndEditCallback);
        }
        return field;
    }

    static public Button CreateButton(string goName, Transform parent, UnityEngine.Events.UnityAction onClickCallback, string text)
    {
        Button button = GameObject.Instantiate(Resources.Load<Button>("vhAssetsButton"));
        button.name = goName;
        button.transform.SetParent(parent);
        button.GetComponentInChildren<Text>().text = text;
        if (onClickCallback != null)
        {
            button.onClick.AddListener(onClickCallback);
        }
        return button;
    }

    /// <summary>
    /// Creates a button with a Layout Element component.
    /// </summary>
    /// <returns>The layout button.</returns>
    /// <param name="goName">Go name.</param>
    /// <param name="parent">Parent.</param>
    /// <param name="onClickCallback">On click callback.</param>
    /// <param name="text">Text.</param>
    /// <param name="screenPctWidth">Used with Screen.width to calculate preferred Width</param>
    /// <param name="screenPctHeight">Used with Screen.width to calculate preferred Height</param>
    static public Button CreateLayoutButton(string goName, Transform parent, UnityEngine.Events.UnityAction onClickCallback, string text, float screenPctWidth, float screenPctHeight)
    {
        Button button = CreateButton(goName, parent, onClickCallback, text);
        LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = screenPctWidth * Screen.width;
        layout.preferredHeight = screenPctHeight * Screen.height;
        return button;
    }

    static public Toggle CreateToggle(string goName, Transform parent, UnityEngine.Events.UnityAction<bool> onValueChangedCallback, string text, bool isChecked)
    {
        Toggle toggle = GameObject.Instantiate(Resources.Load<Toggle>("vhAssetsToggle"));
        toggle.name = goName;
        toggle.transform.SetParent(parent);
        toggle.isOn = isChecked;
        toggle.GetComponentInChildren<Text>().text = text;
        if (onValueChangedCallback != null)
        {
            toggle.onValueChanged.AddListener(onValueChangedCallback);
        }
        return toggle;
    }

    static public Toggle CreateLayoutToggle(string goName, Transform parent, UnityEngine.Events.UnityAction<bool> onValueChangedCallback,
        string text, bool isChecked, float screenPctWidth, float screenPctHeight)
    {
        Toggle toggle = CreateToggle(goName, parent, onValueChangedCallback, text, isChecked);
        LayoutElement layout = toggle.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = screenPctWidth * Screen.width;
        layout.preferredHeight = screenPctHeight * Screen.height;
        return toggle;
    }

    static public Dropdown CreateDropdown(string goName, Transform parent, UnityEngine.Events.UnityAction<int> onValueChangedCallback, List<string> options)
    {
        Dropdown dropdown = GameObject.Instantiate(Resources.Load<Dropdown>("vhAssetsDropdown"));
        dropdown.name = goName;
        dropdown.transform.SetParent(parent);
        if (onValueChangedCallback != null)
        {
            dropdown.onValueChanged.AddListener(onValueChangedCallback);
        }

        if (options != null)
        {
            dropdown.AddOptions(options);
        }
        return dropdown;
    }

    static public Scrollbar CreateScrollbar(string goName, Transform parent, UnityEngine.Events.UnityAction<float> onValueChangedCallback, float val)
    {
        Scrollbar scrollbar = GameObject.Instantiate(Resources.Load<Scrollbar>("vhAssetsScrollbar"));
        scrollbar.name = goName;
        scrollbar.transform.SetParent(parent);
        scrollbar.value = val;
        if (onValueChangedCallback != null)
        {
            scrollbar.onValueChanged.AddListener(onValueChangedCallback);
        }
        return scrollbar;
    }

    static public Slider CreateSlider(string goName, Transform parent, UnityEngine.Events.UnityAction<float> onValueChangedCallback, float val)
    {
        Slider slider = GameObject.Instantiate(Resources.Load<Slider>("vhAssetsSlider"));
        slider.name = goName;
        slider.transform.SetParent(parent);
        slider.value = val;
        if (onValueChangedCallback != null)
        {
            slider.onValueChanged.AddListener(onValueChangedCallback);
        }
        return slider;
    }

    static public RawImage CreateRawImage(string goName, Transform parent, Texture texture)
    {
        RawImage rawImage = GameObject.Instantiate(Resources.Load<RawImage>("vhAssetsRawImage"));
        rawImage.name = goName;
        rawImage.transform.SetParent(parent);
        rawImage.texture = texture;
        return rawImage;
    }

    static public ScrollRect CreateScrollRect(string goName, Transform parent, bool useContentSizeFitter)
    {
        ScrollRect scrollRect = GameObject.Instantiate(Resources.Load<ScrollRect>("vhAssetsScrollView"));
        scrollRect.name = goName;
        scrollRect.transform.SetParent(parent);
        if (useContentSizeFitter)
        {
            GameObject contentGO = VHUtils.FindChildRecursive(scrollRect.gameObject, "Content");
            contentGO.AddComponent<ContentSizeFitter>();
        }
        return scrollRect;
        /*Image image = CreateImage(goName, parent);
        GameObject go = image.gameObject;
        ScrollRect scrollRect = go.AddComponent<ScrollRect>();

        GameObject viewportGO = new GameObject("View Port", new System.Type[] { typeof(Mask), typeof(Image) });
        Image viewportImage = viewportGO.GetComponent<Image>();//CreateImage("View Port", scrollRect.transform);
        viewportGO.transform.SetParent(scrollRect.transform);
        viewportGO.GetComponent<Mask>().showMaskGraphic = false;
        //viewportGO.GetComponent<Mask>().enabled = false;
        viewportGO.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        //viewportImage.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        StretchToParent(viewportImage.GetComponent<RectTransform>());

        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportImage.transform);
        if (useContentSizeFitter)
        {
            contentGO.AddComponent<ContentSizeFitter>();
        }

        scrollRect.content = contentGO.GetComponent<RectTransform>();
        scrollRect.content.pivot = new Vector2(0, 1);
        scrollRect.viewport = viewportImage.GetComponent<RectTransform>();

        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        StretchToParent(contentRect);
        SetAnchors(contentRect, 0, 1, 1, 1);

        scrollRect.horizontalScrollbar = CreateScrollbar("Scrollbar Horizontal", scrollRect.transform, Scrollbar.Direction.LeftToRight);
        scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.horizontalScrollbarSpacing = -3;

        RectTransform horizontalRect = scrollRect.horizontalScrollbar.GetComponent<RectTransform>();
        horizontalRect.pivot = Vector2.zero;
        SetAnchors(horizontalRect, 0, 0, 1, 0);
        horizontalRect.sizeDelta = new Vector2(17, 20);

        scrollRect.verticalScrollbar = CreateScrollbar("Scrollbar Vertical", scrollRect.transform, Scrollbar.Direction.BottomToTop);
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = -3;

        RectTransform verticalRect = scrollRect.verticalScrollbar.GetComponent<RectTransform>();
        verticalRect.pivot = Vector2.one;
        SetAnchors(verticalRect, 1, 0, 1, 1);
        verticalRect.sizeDelta = new Vector2(20, 17);

        return scrollRect;*/
    }

    public static Scrollbar CreateScrollbar(string goName, Transform parent, Scrollbar.Direction direction)
    {
        Scrollbar scrollbar = GameObject.Instantiate(Resources.Load<Scrollbar>("vhAssetsScrollbar"));
        scrollbar.transform.SetParent(parent);
        return scrollbar;

        /*Image image = CreateImage(goName, parent);
        Scrollbar scrollbar = image.gameObject.AddComponent<Scrollbar>();
        image.color = DefaultPanelColor;

        scrollbar.direction = direction;
        GameObject slidingArea = new GameObject("Sliding Area", new System.Type[] { typeof(RectTransform) } );
        slidingArea.transform.SetParent(scrollbar.transform);
        StretchToParent(slidingArea.GetComponent<RectTransform>());
        slidingArea.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, -20);

        //Object o = Resources.Load("UI/Skin/UISprite");

        Image handle = CreateImage("Handle", slidingArea.transform);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        //StretchToParent(handleRect);
        handleRect.sizeDelta = new Vector2(20, 20);

        scrollbar.handleRect = handleRect;
        return scrollbar;*/
    }

    public static T CreateLayoutGroup<T>(string goName, Transform parent) where T : LayoutGroup
    {
        return CreateLayoutGroup<T>(goName, parent, new RectOffset(), TextAnchor.UpperLeft);
    }

    public static T CreateLayoutGroup<T>(string goName, Transform parent, RectOffset padding, TextAnchor childAlignment) where T : LayoutGroup
    {
        GameObject go = new GameObject(goName, new System.Type[] { typeof(RectTransform), typeof(T) } );
        go.transform.SetParent(parent);
        T group = go.GetComponent<T>();
        group.padding = padding;
        group.childAlignment = childAlignment;
        return group;
    }

    static public void StretchToParent(RectTransform rectTransform)
    {
        StretchToParent(rectTransform, 0, 0, 0, 0);
    }

    static public void StretchToParent(RectTransform rectTransform, float leftInset, float bottomInset, float rightInset, float topInset)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        SetInset(rectTransform, leftInset, bottomInset, rightInset, topInset);
    }

    public static void SetAnchors(RectTransform rectTransform, float minX, float minY, float maxX, float maxY)
    {
        rectTransform.anchorMin = new Vector2(minX, minY);
        rectTransform.anchorMax = new Vector2(maxX, maxY);
    }

    public static void SetInset(RectTransform rectTransform, float leftInset, float bottomInset, float rightInset, float topInset)
    {
        rectTransform.offsetMin = new Vector2(leftInset, bottomInset);
        rectTransform.offsetMax = new Vector2(rightInset, topInset);
    }

    static public void FadeAlpha(this Text text, float fadeTime, float startAlpha, float targetAlpha)
    {
        text.StartCoroutine(FadeAlphaInternal(text, fadeTime, startAlpha, targetAlpha));
    }

    static IEnumerator FadeAlphaInternal(Text text, float fadeTime, float startAlpha, float targetAlpha)
    {
        float t = 0;
        Color holder = text.color;

        while (t < fadeTime)
        {
            holder.a = Mathf.SmoothStep(startAlpha, targetAlpha, t / fadeTime);
            text.color = holder;
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            //Debug.Log(holder.a);
        }
    }

    static public void FadeAlphaInOut(this Text text, float totalFadeTime, float startRampTime, float endRampTime)
    {
        // fades text in for startRampTime, fades text out for endRampTime, over a total of totalFadeTime
        if (text.gameObject.activeSelf)
        {
            text.StartCoroutine(FadeAlphaInOutInternal(text, totalFadeTime, startRampTime, endRampTime));
        }
    }

    static IEnumerator FadeAlphaInOutInternal(Text text, float totalFadeTime, float startRampTime, float endRampTime)
    {
        const float startAlpha = 0;
        const float midAlpha = 1;
        const float endAlpha = 0;

        float startTime = Time.time;
        float startEndTime = (startTime + totalFadeTime) - endRampTime;  // time we need to start fading out
        float curTime = Time.time;
        Color color = text.color;

        while (curTime < startTime + totalFadeTime)
        {
            if (curTime < startTime + startRampTime)  // fade in
                color.a = Mathf.Lerp(startAlpha, midAlpha, (curTime - startTime) / startRampTime);
            else if (curTime > startEndTime)  // fade out
                color.a = Mathf.Lerp(midAlpha, endAlpha, (curTime - startEndTime) / endRampTime);
            else
                color.a = midAlpha;

            text.color = color;

            //Debug.Log(string.Format("{0} {1} {2} {3} {4} {5}", curTime, startTime, startTime + fadeTime, startTime + startRampTime, startEndTime, color.a));

            yield return new WaitForEndOfFrame();
            curTime = Time.time;
        }
    }

    public static int CalculateTextWidth(this Text text)
    {
        return CalculateTextWidth(text.text, text.font, text.fontSize);
    }

    public static int CalculateTextWidth(string text, Font font, int fontSize)
    {
        int totalLength = 0;
        CharacterInfo characterInfo = new CharacterInfo();

        char[] arr = text.ToCharArray();

        foreach (char c in arr)
        {
            font.GetCharacterInfo(c, out characterInfo, fontSize);

            totalLength += characterInfo.advance;
        }

        return totalLength;
    }

    /// <summary>
    /// Inserts the breaks based on the number of pixels specified in the breakInterval
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="breakInterval">Number of pixels before a newline is inserted</param>
    public static void InsertBreaks(this Text text, int breakIntervalPixels)
    {
        text.text = InsertBreaks(text.text, text.font, text.fontSize, breakIntervalPixels);
    }

    public static string InsertBreaks(string text, Font font, int fontSize, int breakIntervalPixels)
    {
        int length = 0;
        Font myFont = font;
        CharacterInfo characterInfo = new CharacterInfo();
        text = text.Replace("\r\n", "\n");

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\n')
            {
                length = 0;
                continue;
            }


            myFont.GetCharacterInfo(c, out characterInfo, fontSize);
            length += characterInfo.advance;

            if (length > breakIntervalPixels)
            {
                length = 0;
                text = text.Insert(i, "\n");
                i++;
            }
        }
        return text;
    }

    public static int CalculateNumTextLines(this Text text)
    {
        int msgWidth = text.CalculateTextWidth();
        return Mathf.Max(1, Mathf.CeilToInt((float)msgWidth / (float)Screen.width));
    }
    #endregion
}
