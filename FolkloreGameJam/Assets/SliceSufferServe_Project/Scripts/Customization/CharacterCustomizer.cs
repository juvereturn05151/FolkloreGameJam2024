using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizer : MonoBehaviour
{
    private const string HeadsResourcePath = "CharacterParts/Heads";
    private const string NecksResourcePath = "CharacterParts/Necks";
    private const string BodiesResourcePath = "CharacterParts/Bodies";
    private const string LegsResourcePath = "CharacterParts/Legs";

    [Header("Preview Sprite Renderers")]
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private SpriteRenderer neckRenderer;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer legsRenderer;

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
    [SerializeField] private Transform manualControlsRoot;
    [SerializeField] private Text statusText;

    private CharacterCustomizationData selectedData;

    private void Awake()
    {
        ResolvePreviewReferences();
        LoadSprites();
        selectedData = CharacterCustomizationData.LoadFromPlayerPrefs();
        ClampSelectedIndices();
        ApplyPreview();
        BuildDefaultManualControls();
        UpdateStatusText();
    }

    public void NextHead()
    {
        selectedData.headIndex = NextIndex(selectedData.headIndex, headOptions);
        Debug.Log($"Manual customization selected head index: {selectedData.headIndex}");
        ApplyHead();
        UpdateStatusText();
    }

    public void PreviousHead()
    {
        selectedData.headIndex = PreviousIndex(selectedData.headIndex, headOptions);
        Debug.Log($"Manual customization selected head index: {selectedData.headIndex}");
        ApplyHead();
        UpdateStatusText();
    }

    public void NextNeck()
    {
        selectedData.neckIndex = NextIndex(selectedData.neckIndex, neckOptions);
        Debug.Log($"Manual customization selected neck index: {selectedData.neckIndex}");
        ApplyNeck();
        UpdateStatusText();
    }

    public void PreviousNeck()
    {
        selectedData.neckIndex = PreviousIndex(selectedData.neckIndex, neckOptions);
        Debug.Log($"Manual customization selected neck index: {selectedData.neckIndex}");
        ApplyNeck();
        UpdateStatusText();
    }

    public void NextBody()
    {
        selectedData.bodyIndex = NextIndex(selectedData.bodyIndex, bodyOptions);
        Debug.Log($"Manual customization selected body index: {selectedData.bodyIndex}");
        ApplyBody();
        UpdateStatusText();
    }

    public void PreviousBody()
    {
        selectedData.bodyIndex = PreviousIndex(selectedData.bodyIndex, bodyOptions);
        Debug.Log($"Manual customization selected body index: {selectedData.bodyIndex}");
        ApplyBody();
        UpdateStatusText();
    }

    public void NextLegs()
    {
        selectedData.legsIndex = NextIndex(selectedData.legsIndex, legsOptions);
        Debug.Log($"Manual customization selected legs index: {selectedData.legsIndex}");
        ApplyLegs();
        UpdateStatusText();
    }

    public void PreviousLegs()
    {
        selectedData.legsIndex = PreviousIndex(selectedData.legsIndex, legsOptions);
        Debug.Log($"Manual customization selected legs index: {selectedData.legsIndex}");
        ApplyLegs();
        UpdateStatusText();
    }

    public void SaveCustomization()
    {
        ClampSelectedIndices();
        selectedData.SaveToPlayerPrefs();
        UpdateStatusText("Saved manual customization.");
    }

    private void ResolvePreviewReferences()
    {
        headImage ??= FindImageByName("HeadPreviewImage");
        neckImage ??= FindImageByName("NeckPreviewImage");
        bodyImage ??= FindImageByName("StomachPreviewImage");
        legsImage ??= FindImageByName("LegPreviewImage");
        statusText ??= FindTextByName("SelectedHumanLabel");
    }

    private void LoadSprites()
    {
        if (headOptions == null || headOptions.Length == 0)
        {
            headOptions = Resources.LoadAll<Sprite>(HeadsResourcePath);
        }

        if (neckOptions == null || neckOptions.Length == 0)
        {
            neckOptions = Resources.LoadAll<Sprite>(NecksResourcePath);
        }

        if (bodyOptions == null || bodyOptions.Length == 0)
        {
            bodyOptions = Resources.LoadAll<Sprite>(BodiesResourcePath);
        }

        if (legsOptions == null || legsOptions.Length == 0)
        {
            legsOptions = Resources.LoadAll<Sprite>(LegsResourcePath);
        }

        Debug.Log($"Manual customization loaded sprites: heads={headOptions.Length}, necks={neckOptions.Length}, bodies={bodyOptions.Length}, legs={legsOptions.Length}");
    }

    private void BuildDefaultManualControls()
    {
        if (!createDefaultManualControls || manualControlsRoot != null || GameObject.Find("ManualCustomizationControls") != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Manual customization UI could not be created because no Canvas was found.");
            return;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject panelObject = new GameObject("ManualCustomizationControls", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvas.transform, false);

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(1f, 0.5f);
        panel.anchorMax = new Vector2(1f, 0.5f);
        panel.pivot = new Vector2(1f, 0.5f);
        panel.anchoredPosition = new Vector2(-32f, 0f);
        panel.sizeDelta = new Vector2(320f, 420f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.08f, 0.07f, 0.06f, 0.86f);

        manualControlsRoot = panelObject.transform;
        CreateLabel(panel, "Manual Character", font, new Vector2(0f, 168f), 24);
        CreatePartRow(panel, "Head", PreviousHead, NextHead, font, 95f);
        CreatePartRow(panel, "Neck", PreviousNeck, NextNeck, font, 35f);
        CreatePartRow(panel, "Body", PreviousBody, NextBody, font, -25f);
        CreatePartRow(panel, "Legs", PreviousLegs, NextLegs, font, -85f);
        CreateButton(panel, "Save", SaveCustomization, font, new Vector2(0f, -160f), new Vector2(180f, 44f), new Color(0.74f, 0.22f, 0.16f, 1f));
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

    private static void CreateButton(RectTransform parent, string text, UnityEngine.Events.UnityAction action, Font font, Vector2 anchoredPosition, Vector2 size, Color color)
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
        SetSprite(headRenderer, headImage, GetSprite(headOptions, selectedData.headIndex));
    }

    private void ApplyNeck()
    {
        SetSprite(neckRenderer, neckImage, GetSprite(neckOptions, selectedData.neckIndex));
    }

    private void ApplyBody()
    {
        SetSprite(bodyRenderer, bodyImage, GetSprite(bodyOptions, selectedData.bodyIndex));
    }

    private void ApplyLegs()
    {
        SetSprite(legsRenderer, legsImage, GetSprite(legsOptions, selectedData.legsIndex));
    }

    private void UpdateStatusText(string prefix = null)
    {
        if (statusText == null)
        {
            return;
        }

        string status = $"Head {DisplayIndex(selectedData.headIndex, headOptions)} | Neck {DisplayIndex(selectedData.neckIndex, neckOptions)} | Body {DisplayIndex(selectedData.bodyIndex, bodyOptions)} | Legs {DisplayIndex(selectedData.legsIndex, legsOptions)}";
        statusText.text = string.IsNullOrWhiteSpace(prefix) ? status : $"{prefix} {status}";
    }

    private static int NextIndex(int currentIndex, Sprite[] options)
    {
        return options == null || options.Length == 0 ? 0 : (currentIndex + 1) % options.Length;
    }

    private static int PreviousIndex(int currentIndex, Sprite[] options)
    {
        if (options == null || options.Length == 0)
        {
            return 0;
        }

        return (currentIndex - 1 + options.Length) % options.Length;
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

    private static void SetSprite(SpriteRenderer spriteRenderer, Image image, Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }

        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
        }
    }
}
