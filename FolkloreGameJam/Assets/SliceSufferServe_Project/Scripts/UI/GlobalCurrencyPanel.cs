using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalCurrencyPanel : MonoBehaviour
{
    private const string ResourcePath = "GlobalCurrencyPanel";

    private static GlobalCurrencyPanel instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI currencyBalanceText;

    [Header("Layout")]
    [SerializeField] private Vector2 anchoredPosition = new Vector2(-230f, -95f);
    [SerializeField] private Vector2 panelSize = new Vector2(300f, 72f);
    [SerializeField] private Color panelColor = new Color(0.2f, 0.16f, 0.13f, 0.95f);
    [SerializeField] private Color textColor = new Color(1f, 0.9f, 0.35f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GlobalCurrencyPanel prefab = Resources.Load<GlobalCurrencyPanel>(ResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"Global currency panel prefab missing at Resources/{ResourcePath}.");
            return;
        }

        Instantiate(prefab);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureBuilt();
        Refresh();
    }

    private void OnEnable()
    {
        SaveSystem.OnCurrencyChanged += UpdateCurrencyBalance;
        Refresh();
    }

    private void OnDisable()
    {
        SaveSystem.OnCurrencyChanged -= UpdateCurrencyBalance;
    }

    private void EnsureBuilt()
    {
        if (currencyBalanceText != null)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panelObject = new GameObject("CurrencyPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(transform, false);

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = Vector2.one;
        panel.anchorMax = Vector2.one;
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = anchoredPosition;
        panel.sizeDelta = panelSize;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = panelColor;

        GameObject textObject = new GameObject("CurrencyBalanceText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panel, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 8f);
        textRect.offsetMax = new Vector2(-14f, -8f);

        currencyBalanceText = textObject.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            currencyBalanceText.font = TMP_Settings.defaultFontAsset;
        }

        currencyBalanceText.alignment = TextAlignmentOptions.Center;
        currencyBalanceText.fontSize = 42f;
        currencyBalanceText.color = textColor;
        currencyBalanceText.text = "0";
    }

    private void Refresh()
    {
        UpdateCurrencyBalance(SaveSystem.GetCurrencyBalance());
    }

    private void UpdateCurrencyBalance(int currencyBalance)
    {
        if (currencyBalanceText != null)
        {
            currencyBalanceText.text = currencyBalance.ToString();
        }
    }
}
