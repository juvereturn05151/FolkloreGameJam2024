using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterCustomizer : MonoBehaviour
{
    private const string HeadsResourcePath = "Characters/Human/Normal/Heads";
    private const string NecksResourcePath = "Characters/Human/Normal/Necks";
    private const string StomachsResourcePath = "Characters/Human/Normal/Stomach";
    private const string LegsResourcePath = "Characters/Human/Normal/Legs";

    [Header("Preview UI Images")]
    [SerializeField] private Image headImage;
    [SerializeField] private Image neckImage;
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image legsImage;

    [Header("Optional Sprite Overrides")]
    [SerializeField] private Sprite[] headOptions;
    [SerializeField] private Sprite[] neckOptions;
    [SerializeField] private Sprite[] bodyOptions;
    [SerializeField] private Sprite[] legsOptions;

    [Header("Manual UI")]
    [SerializeField] private bool createDefaultManualControls = true;
    [SerializeField] private bool allPartsUnlockedForTesting = true;
    [SerializeField] private Transform manualControlsRoot;
    [SerializeField] private Text statusText;

    private CharacterCustomizationData selectedData;

    private void Awake()
    {
        LoadSprites();
        selectedData = CharacterCustomizationData.LoadFromPlayerPrefs();
        ClampSelectedIndices();
        BuildDefaultManualControls();
        ResolvePreviewReferences();
        BindManualUiButtons();
        ApplyPreview();
        UpdateStatusText();
    }

    public void NextHead()
    {
        selectedData.headIndex = NextUnlockedIndex(BodyPartType.Head, selectedData.headIndex, headOptions);
        Debug.Log($"Manual customization selected head index: {selectedData.headIndex}");
        ApplyHead();
        UpdateStatusText();
    }

    public void PreviousHead()
    {
        selectedData.headIndex = PreviousUnlockedIndex(BodyPartType.Head, selectedData.headIndex, headOptions);
        Debug.Log($"Manual customization selected head index: {selectedData.headIndex}");
        ApplyHead();
        UpdateStatusText();
    }

    public void NextNeck()
    {
        selectedData.neckIndex = NextUnlockedIndex(BodyPartType.Neck, selectedData.neckIndex, neckOptions);
        Debug.Log($"Manual customization selected neck index: {selectedData.neckIndex}");
        ApplyNeck();
        UpdateStatusText();
    }

    public void PreviousNeck()
    {
        selectedData.neckIndex = PreviousUnlockedIndex(BodyPartType.Neck, selectedData.neckIndex, neckOptions);
        Debug.Log($"Manual customization selected neck index: {selectedData.neckIndex}");
        ApplyNeck();
        UpdateStatusText();
    }

    public void NextStomach()
    {
        selectedData.bodyIndex = NextUnlockedIndex(BodyPartType.Stomach, selectedData.bodyIndex, bodyOptions);
        Debug.Log($"Manual customization selected stomach index: {selectedData.bodyIndex}");
        ApplyBody();
        UpdateStatusText();
    }

    public void NextBody()
    {
        NextStomach();
    }

    public void PreviousStomach()
    {
        selectedData.bodyIndex = PreviousUnlockedIndex(BodyPartType.Stomach, selectedData.bodyIndex, bodyOptions);
        Debug.Log($"Manual customization selected stomach index: {selectedData.bodyIndex}");
        ApplyBody();
        UpdateStatusText();
    }

    public void PreviousBody()
    {
        PreviousStomach();
    }

    public void NextLegs()
    {
        selectedData.legsIndex = NextUnlockedIndex(BodyPartType.Leg, selectedData.legsIndex, legsOptions);
        Debug.Log($"Manual customization selected legs index: {selectedData.legsIndex}");
        ApplyLegs();
        UpdateStatusText();
    }

    public void PreviousLegs()
    {
        selectedData.legsIndex = PreviousUnlockedIndex(BodyPartType.Leg, selectedData.legsIndex, legsOptions);
        Debug.Log($"Manual customization selected legs index: {selectedData.legsIndex}");
        ApplyLegs();
        UpdateStatusText();
    }

    public void SelectDefaultCharacter()
    {
        selectedData.headIndex = 0;
        selectedData.neckIndex = 0;
        selectedData.bodyIndex = 0;
        selectedData.legsIndex = 0;
        selectedData.SaveToPlayerPrefs();
        ApplyPreview();
        UpdateStatusText("Using default normal human.");
        Debug.Log("Manual customization selected default normal human.");
    }

    public void SaveCustomization()
    {
        ClampSelectedIndices();
        selectedData.SaveToPlayerPrefs();
        UpdateStatusText("Saved manual customization.");
    }

    public void BackToGameModeSelect()
    {
        SceneManager.LoadScene("GameModeSelect");
    }

    public static void SetNormalHumanPartUnlocked(BodyPartType part, int index, bool unlocked)
    {
        if (index <= 0)
        {
            return;
        }

        PlayerPrefs.SetInt(GetUnlockKey(part, index), unlocked ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Manual customization unlock updated: part={part}, index={index}, unlocked={unlocked}");
    }

    private void ResolvePreviewReferences()
    {
        headImage ??= FindImageByName("HeadPreviewImage");
        neckImage ??= FindImageByName("NeckPreviewImage");
        bodyImage ??= FindImageByName("StomachPreviewImage");
        legsImage ??= FindImageByName("LegPreviewImage");
        statusText ??= FindTextByName("SelectedHumanLabel");
    }

    private void BindManualUiButtons()
    {
        BindButton("HeadPreviousButton", PreviousHead);
        BindButton("HeadNextButton", NextHead);
        BindButton("NeckPreviousButton", PreviousNeck);
        BindButton("NeckNextButton", NextNeck);
        BindButton("StomachPreviousButton", PreviousStomach);
        BindButton("StomachNextButton", NextStomach);
        BindButton("LegPreviousButton", PreviousLegs);
        BindButton("LegNextButton", NextLegs);
        BindButton("DefaultButton", SelectDefaultCharacter);
        BindButton("SaveButton", SaveCustomization);
        BindButton("BackButton", BackToGameModeSelect);
    }

    private void LoadSprites()
    {
        Sprite[] resourceHeads = Resources.LoadAll<Sprite>(HeadsResourcePath);
        Sprite[] resourceNecks = Resources.LoadAll<Sprite>(NecksResourcePath);
        Sprite[] resourceStomachs = Resources.LoadAll<Sprite>(StomachsResourcePath);
        Sprite[] resourceLegs = Resources.LoadAll<Sprite>(LegsResourcePath);

        if (resourceHeads.Length > 0)
        {
            headOptions = resourceHeads;
        }
        else if (headOptions == null)
        {
            headOptions = System.Array.Empty<Sprite>();
        }

        if (resourceNecks.Length > 0)
        {
            neckOptions = resourceNecks;
        }
        else if (neckOptions == null)
        {
            neckOptions = System.Array.Empty<Sprite>();
        }

        if (resourceStomachs.Length > 0)
        {
            bodyOptions = resourceStomachs;
        }
        else if (bodyOptions == null)
        {
            bodyOptions = System.Array.Empty<Sprite>();
        }

        if (resourceLegs.Length > 0)
        {
            legsOptions = resourceLegs;
        }
        else if (legsOptions == null)
        {
            legsOptions = System.Array.Empty<Sprite>();
        }

        SortSpritesByDefaultFirst(headOptions);
        SortSpritesByDefaultFirst(neckOptions);
        SortSpritesByDefaultFirst(bodyOptions);
        SortSpritesByDefaultFirst(legsOptions);

        Debug.Log($"Manual customization loaded normal human sprites: heads={headOptions.Length}, necks={neckOptions.Length}, stomachs={bodyOptions.Length}, legs={legsOptions.Length}");
    }

    private void BuildDefaultManualControls()
    {
        if (!createDefaultManualControls || manualControlsRoot != null || GameObject.Find("ManualCharacterCustomizationUI") != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            canvas = CreateManualCanvas();
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject rootObject = new GameObject("ManualCharacterCustomizationUI", typeof(RectTransform));
        rootObject.transform.SetParent(canvas.transform, false);

        RectTransform root = rootObject.GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        CreateBackground(root);
        CreatePreview(root, font);

        GameObject panelObject = new GameObject("ManualCustomizationControls", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(root, false);
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(1f, 0.5f);
        panel.anchorMax = new Vector2(1f, 0.5f);
        panel.pivot = new Vector2(1f, 0.5f);
        panel.anchoredPosition = new Vector2(-44f, 0f);
        panel.sizeDelta = new Vector2(360f, 500f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.08f, 0.07f, 0.06f, 0.86f);

        manualControlsRoot = panelObject.transform;
        CreateLabel(panel, "Normal Human", font, new Vector2(0f, 212f), 24);
        CreatePartRow(panel, "Head", PreviousHead, NextHead, font, 136f);
        CreatePartRow(panel, "Neck", PreviousNeck, NextNeck, font, 62f);
        CreatePartRow(panel, "Stomach", PreviousStomach, NextStomach, font, -12f);
        CreatePartRow(panel, "Leg", PreviousLegs, NextLegs, font, -86f);
        CreateButton(panel, "Default", SelectDefaultCharacter, font, new Vector2(-92f, -196f), new Vector2(146f, 48f), new Color(0.24f, 0.22f, 0.18f, 1f));
        CreateButton(panel, "Save", SaveCustomization, font, new Vector2(92f, -196f), new Vector2(146f, 48f), new Color(0.74f, 0.22f, 0.16f, 1f));
    }

    private static Canvas CreateManualCanvas()
    {
        GameObject canvasObject = new GameObject("ManualCharacterCustomizationCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static void CreateBackground(RectTransform root)
    {
        GameObject backgroundObject = new GameObject("ManualBackground", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(root, false);

        RectTransform background = backgroundObject.GetComponent<RectTransform>();
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;

        Image image = backgroundObject.GetComponent<Image>();
        image.color = new Color(0.05f, 0.045f, 0.04f, 1f);
    }

    private void CreatePreview(RectTransform root, Font font)
    {
        CreateLabel(root, "Preview", font, new Vector2(-420f, 360f), 28);

        GameObject previewObject = new GameObject("ManualPreview", typeof(RectTransform), typeof(Image));
        previewObject.transform.SetParent(root, false);

        RectTransform preview = previewObject.GetComponent<RectTransform>();
        preview.anchorMin = new Vector2(0.5f, 0.5f);
        preview.anchorMax = new Vector2(0.5f, 0.5f);
        preview.pivot = new Vector2(0.5f, 0.5f);
        preview.anchoredPosition = new Vector2(-420f, -20f);
        preview.sizeDelta = new Vector2(520f, 760f);

        Image previewImage = previewObject.GetComponent<Image>();
        previewImage.color = new Color(0.11f, 0.10f, 0.09f, 0.95f);

        Vector2 previewSize = new Vector2(420f, 746f);
        legsImage ??= CreatePreviewImage(preview, "LegPreviewImage", Vector2.zero, previewSize);
        bodyImage ??= CreatePreviewImage(preview, "StomachPreviewImage", Vector2.zero, previewSize);
        neckImage ??= CreatePreviewImage(preview, "NeckPreviewImage", Vector2.zero, previewSize);
        headImage ??= CreatePreviewImage(preview, "HeadPreviewImage", Vector2.zero, previewSize);

        statusText ??= CreateStatusText(root, font);
    }

    private static Image CreatePreviewImage(RectTransform parent, string objectName, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateStatusText(RectTransform root, Font font)
    {
        GameObject statusObject = new GameObject("ManualCustomizationStatusText", typeof(RectTransform), typeof(Text));
        statusObject.transform.SetParent(root, false);

        RectTransform rectTransform = statusObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(-420f, 34f);
        rectTransform.sizeDelta = new Vector2(760f, 42f);

        Text text = statusObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.95f, 0.91f, 0.82f, 1f);
        return text;
    }

    private void CreatePartRow(RectTransform parent, string label, UnityEngine.Events.UnityAction previousAction, UnityEngine.Events.UnityAction nextAction, Font font, float y)
    {
        CreateLabel(parent, label, font, new Vector2(0f, y + 18f), 18);
        CreateButton(parent, "<", previousAction, font, new Vector2(-86f, y - 12f), new Vector2(64f, 36f), new Color(0.24f, 0.22f, 0.18f, 1f));
        CreateButton(parent, ">", nextAction, font, new Vector2(86f, y - 12f), new Vector2(64f, 36f), new Color(0.24f, 0.22f, 0.18f, 1f));
    }

    private static void CreateLabel(RectTransform parent, string text, Font font, Vector2 anchoredPosition, int fontSize)
    {
        GameObject labelObject = new GameObject(text + "Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(280f, 32f);

        Text labelText = labelObject.GetComponent<Text>();
        labelText.text = text;
        labelText.font = font;
        labelText.fontSize = fontSize;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = new Color(0.95f, 0.91f, 0.82f, 1f);
    }

    private static Button CreateButton(RectTransform parent, string text, UnityEngine.Events.UnityAction action, Font font, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject buttonObject = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = Vector2.zero;
        textTransform.offsetMax = Vector2.zero;

        Text buttonText = textObject.GetComponent<Text>();
        buttonText.text = text;
        buttonText.font = font;
        buttonText.fontSize = 20;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        return button;
    }

    private void ClampSelectedIndices()
    {
        selectedData.headIndex = ClampIndex(selectedData.headIndex, headOptions);
        selectedData.neckIndex = ClampIndex(selectedData.neckIndex, neckOptions);
        selectedData.bodyIndex = ClampIndex(selectedData.bodyIndex, bodyOptions);
        selectedData.legsIndex = ClampIndex(selectedData.legsIndex, legsOptions);
    }

    private void ApplyPreview()
    {
        ApplyHead();
        ApplyNeck();
        ApplyBody();
        ApplyLegs();
    }

    private void ApplyHead()
    {
        SetSprite(headImage, GetSprite(headOptions, selectedData.headIndex));
    }

    private void ApplyNeck()
    {
        SetSprite(neckImage, GetSprite(neckOptions, selectedData.neckIndex));
    }

    private void ApplyBody()
    {
        SetSprite( bodyImage, GetSprite(bodyOptions, selectedData.bodyIndex));
    }

    private void ApplyLegs()
    {
        SetSprite( legsImage, GetSprite(legsOptions, selectedData.legsIndex));
    }

    private void UpdateStatusText(string prefix = null)
    {
        if (statusText == null)
        {
            return;
        }

        string status = $"Head {DisplayIndex(selectedData.headIndex, headOptions)} | Neck {DisplayIndex(selectedData.neckIndex, neckOptions)} | Stomach {DisplayIndex(selectedData.bodyIndex, bodyOptions)} | Leg {DisplayIndex(selectedData.legsIndex, legsOptions)}";
        statusText.text = string.IsNullOrWhiteSpace(prefix) ? status : $"{prefix} {status}";
    }

    private int NextUnlockedIndex(BodyPartType part, int currentIndex, Sprite[] options)
    {
        if (options == null || options.Length == 0)
        {
            return 0;
        }

        for (int step = 1; step <= options.Length; step++)
        {
            int candidate = (currentIndex + step) % options.Length;
            if (IsPartUnlocked(part, candidate))
            {
                return candidate;
            }
        }

        return 0;
    }

    private int PreviousUnlockedIndex(BodyPartType part, int currentIndex, Sprite[] options)
    {
        if (options == null || options.Length == 0)
        {
            return 0;
        }

        for (int step = 1; step <= options.Length; step++)
        {
            int candidate = (currentIndex - step + options.Length) % options.Length;
            if (IsPartUnlocked(part, candidate))
            {
                return candidate;
            }
        }

        return 0;
    }

    private bool IsPartUnlocked(BodyPartType part, int index)
    {
        if (index == 0 || allPartsUnlockedForTesting)
        {
            return true;
        }

        return PlayerPrefs.GetInt(GetUnlockKey(part, index), 0) == 1;
    }

    private static string GetUnlockKey(BodyPartType part, int index)
    {
        return $"UnlockedNormalHuman{part}{index}";
    }

    private static int ClampIndex(int index, Sprite[] options)
    {
        return options == null || options.Length == 0 ? 0 : Mathf.Clamp(index, 0, options.Length - 1);
    }

    private static Sprite GetSprite(Sprite[] options, int index)
    {
        if (options == null || options.Length == 0)
        {
            return null;
        }

        return options[Mathf.Clamp(index, 0, options.Length - 1)];
    }

    private static string DisplayIndex(int index, Sprite[] options)
    {
        int count = options == null ? 0 : options.Length;
        return count == 0 ? "0/0" : $"{Mathf.Clamp(index, 0, count - 1) + 1}/{count}";
    }

    private static void SortSpritesByDefaultFirst(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length <= 1)
        {
            return;
        }

        System.Array.Sort(sprites, CompareSpritesByDefaultFirst);
    }

    private static int CompareSpritesByDefaultFirst(Sprite left, Sprite right)
    {
        if (left == right)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        bool leftDefault = IsDefaultSpriteName(left.name);
        bool rightDefault = IsDefaultSpriteName(right.name);

        if (leftDefault != rightDefault)
        {
            return leftDefault ? -1 : 1;
        }

        return string.CompareOrdinal(left.name, right.name);
    }

    private static bool IsDefaultSpriteName(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
        {
            return false;
        }

        return !System.Text.RegularExpressions.Regex.IsMatch(spriteName, @"\d+$");
    }

    private static Image FindImageByName(string objectName)
    {
        Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == objectName)
            {
                return images[i];
            }
        }

        return null;
    }

    private static Text FindTextByName(string objectName)
    {
        Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == objectName)
            {
                return texts[i];
            }
        }

        return null;
    }

    private static Button FindButtonByName(string objectName)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].name == objectName)
            {
                return buttons[i];
            }
        }

        return null;
    }

    private static void BindButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        Button button = FindButtonByName(objectName);
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static void SetSprite(Image image, Sprite sprite)
    {
        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
        }
    }
}
