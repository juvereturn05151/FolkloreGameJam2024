using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoreUIManager : MonoBehaviour
{
    private const string DisableAdsTabId = "disable_ads";
    private const string ItemsTabId = "items";
    private const string HumanTabId = "human";
    private const int HumanPartPrice = 1000;
    private const int SpecialHumanPartPrice = 6000;
    private const int WeaponCursorPrice = 5000;

    [Header("Store")]
    [SerializeField] private StoreManager storeManager;
    [SerializeField] private string defaultTabId = DisableAdsTabId;
    [SerializeField] private string backSceneName = "GameModeSelect";
    [SerializeField] private List<StoreTabView> tabs = new List<StoreTabView>();

    [Header("Shared UI")]
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI currencyBalanceText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TMP_FontAsset priceFontAsset;

    [Header("Disable Ads UI")]
    [SerializeField] private TextMeshProUGUI disableAdsPriceText;
    [SerializeField] private TextMeshProUGUI disableAdsPurchaseButtonText;
    [SerializeField] private Button disableAdsPurchaseButton;

    [Header("Purchase Prompt")]
    [SerializeField] private GameObject purchasePromptPanel;
    [SerializeField] private TextMeshProUGUI purchasePromptText;
    [SerializeField] private Button purchaseConfirmButton;
    [SerializeField] private Button purchaseCancelButton;

    [Header("Items Placeholder UI")]
    [SerializeField] private TextMeshProUGUI itemsPlaceholderText;

    private string selectedTabId;
    private readonly List<TabButtonBinding> tabButtonBindings = new List<TabButtonBinding>();
    private readonly List<HumanStoreItemView> humanItemViews = new List<HumanStoreItemView>();
    private readonly List<WeaponStoreItemView> weaponItemViews = new List<WeaponStoreItemView>();
    private CursorCustomizationCatalog cursorCatalog;
    private GameObject humanPanel;
    private HumanStoreItem pendingHumanPurchase;
    private WeaponStoreItem pendingWeaponPurchase;
    private bool humanStoreUiBuilt;
    private bool weaponStoreUiBuilt;

    private void Awake()
    {
        if (storeManager == null)
        {
            storeManager = FindFirstObjectByType<StoreManager>();
        }

        cursorCatalog = CursorCustomizationCatalog.LoadDefault();
        ApplyPriceFont(disableAdsPriceText);
    }

    private void OnEnable()
    {
        SaveSystem.OnCurrencyChanged += UpdateCurrencyBalance;

        if (storeManager != null)
        {
            storeManager.OnAdsDisabledPurchased.AddListener(Refresh);
            storeManager.OnAdsAlreadyDisabled.AddListener(Refresh);
            storeManager.OnPurchaseFailed.AddListener(ShowPurchaseFailed);
        }

        EnsureHumanStoreUI();
        WireButtons();
        SelectTab(string.IsNullOrWhiteSpace(selectedTabId) ? defaultTabId : selectedTabId);
        Refresh();
    }

    private void OnDisable()
    {
        SaveSystem.OnCurrencyChanged -= UpdateCurrencyBalance;

        if (storeManager != null)
        {
            storeManager.OnAdsDisabledPurchased.RemoveListener(Refresh);
            storeManager.OnAdsAlreadyDisabled.RemoveListener(Refresh);
            storeManager.OnPurchaseFailed.RemoveListener(ShowPurchaseFailed);
        }

        UnwireButtons();
    }

    public void SelectDisableAdsTab()
    {
        SelectTab(DisableAdsTabId);
    }

    public void SelectItemsTab()
    {
        SelectTab(ItemsTabId);
    }

    public void SelectHumanTab()
    {
        SelectTab(HumanTabId);
    }

    public void SelectTab(string tabId)
    {
        selectedTabId = tabId;

        for (int i = 0; i < tabs.Count; i++)
        {
            StoreTabView tab = tabs[i];

            if (tab == null)
            {
                continue;
            }

            bool isSelected = tab.TabId == selectedTabId;

            if (tab.Panel != null)
            {
                tab.Panel.SetActive(isSelected);
            }

            if (tab.TabButton != null)
            {
                tab.TabButton.interactable = !isSelected;
            }
        }

        Refresh();
    }

    public void OpenDisableAdsPurchasePrompt()
    {
        pendingHumanPurchase = null;
        pendingWeaponPurchase = null;

        if (storeManager != null && storeManager.AreAdsDisabled)
        {
            Refresh();
            return;
        }

        if (purchasePromptText != null)
        {
            purchasePromptText.text = $"Remove all ads for {GetDisableAdsPrice()}?";
        }

        if (purchasePromptPanel != null)
        {
            purchasePromptPanel.SetActive(true);
            return;
        }

        ConfirmDisableAdsPurchase();
    }

    public void ConfirmDisableAdsPurchase()
    {
        if (purchasePromptPanel != null)
        {
            purchasePromptPanel.SetActive(false);
        }

        if (storeManager == null)
        {
            SetStatus("Store is unavailable.");
            return;
        }

        storeManager.RequestBuyDisableAds();
        SetStatus($"Purchase requested: {GetDisableAdsPrice()}");
    }

    public void CancelPurchasePrompt()
    {
        pendingHumanPurchase = null;
        pendingWeaponPurchase = null;

        if (purchasePromptPanel != null)
        {
            purchasePromptPanel.SetActive(false);
        }

        SetStatus(string.Empty);
    }

    public void Refresh()
    {
        UpdateCurrencyBalance(SaveSystem.GetCurrencyBalance());
        RefreshDisableAdsUI();
        EnsureWeaponStoreUI();
        RefreshHumanItemsUI();
        RefreshWeaponItemsUI();
    }

    private void WireButtons()
    {
        tabButtonBindings.Clear();

        for (int i = 0; i < tabs.Count; i++)
        {
            StoreTabView tab = tabs[i];

            if (tab?.TabButton == null)
            {
                continue;
            }

            string tabId = tab.TabId;
            UnityAction action = () => SelectTab(tabId);
            tab.TabButton.onClick.AddListener(action);
            tabButtonBindings.Add(new TabButtonBinding(tab.TabButton, action));
        }

        if (disableAdsPurchaseButton != null)
        {
            disableAdsPurchaseButton.onClick.AddListener(OpenDisableAdsPurchasePrompt);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }

        if (purchaseConfirmButton != null)
        {
            purchaseConfirmButton.onClick.AddListener(ConfirmCurrentPurchase);
        }

        if (purchaseCancelButton != null)
        {
            purchaseCancelButton.onClick.AddListener(CancelPurchasePrompt);
        }
    }

    private void UnwireButtons()
    {
        for (int i = 0; i < tabButtonBindings.Count; i++)
        {
            TabButtonBinding binding = tabButtonBindings[i];

            if (binding.Button != null)
            {
                binding.Button.onClick.RemoveListener(binding.Action);
            }
        }

        tabButtonBindings.Clear();

        if (disableAdsPurchaseButton != null)
        {
            disableAdsPurchaseButton.onClick.RemoveListener(OpenDisableAdsPurchasePrompt);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
        }

        if (purchaseConfirmButton != null)
        {
            purchaseConfirmButton.onClick.RemoveListener(ConfirmCurrentPurchase);
        }

        if (purchaseCancelButton != null)
        {
            purchaseCancelButton.onClick.RemoveListener(CancelPurchasePrompt);
        }
    }

    private void RefreshDisableAdsUI()
    {
        bool adsDisabled = storeManager != null && storeManager.AreAdsDisabled;

        if (disableAdsPriceText != null)
        {
            disableAdsPriceText.text = adsDisabled ? "Owned" : GetDisableAdsPrice();
        }

        if (disableAdsPurchaseButtonText != null)
        {
            disableAdsPurchaseButtonText.text = adsDisabled ? "Owned" : "Buy";
        }

        if (disableAdsPurchaseButton != null)
        {
            disableAdsPurchaseButton.interactable = !adsDisabled;
        }

        if (adsDisabled)
        {
            SetStatus("Ads disabled.");
        }
    }

    private void RefreshItemsPlaceholderUI()
    {
        if (itemsPlaceholderText != null)
        {
            itemsPlaceholderText.text = string.Empty;
            itemsPlaceholderText.gameObject.SetActive(false);
        }
    }

    private void EnsureHumanStoreUI()
    {
        if (humanStoreUiBuilt)
        {
            return;
        }

        StoreTabView itemsTab = FindTab(ItemsTabId);
        StoreTabView disableAdsTab = FindTab(DisableAdsTabId);
        Button templateButton = itemsTab?.TabButton ?? disableAdsTab?.TabButton;
        GameObject templatePanel = itemsTab?.Panel ?? disableAdsTab?.Panel;

        if (templateButton == null || templatePanel == null)
        {
            return;
        }

        Button humanTabButton = CreateHumanTabButton(templateButton);
        humanPanel = CreateHumanPanel(templatePanel);
        tabs.Add(new StoreTabView(HumanTabId, humanTabButton, humanPanel));
        RepositionTabButtons();
        BuildHumanItems();
        humanStoreUiBuilt = true;
    }

    private void EnsureWeaponStoreUI()
    {
        if (weaponStoreUiBuilt)
        {
            return;
        }

        StoreTabView weaponsTab = FindTab(ItemsTabId);
        if (weaponsTab?.Panel == null)
        {
            return;
        }

        RefreshItemsPlaceholderUI();
        BuildWeaponItems(weaponsTab.Panel.transform);
        weaponStoreUiBuilt = true;
    }

    private Button CreateHumanTabButton(Button templateButton)
    {
        Button existing = FindButtonByName("HumanTabButton");
        if (existing != null)
        {
            return existing;
        }

        Button button = Instantiate(templateButton, templateButton.transform.parent);
        button.name = "HumanTabButton";
        SetText(button.GetComponentInChildren<TextMeshProUGUI>(true), "Human");
        return button;
    }

    private GameObject CreateHumanPanel(GameObject templatePanel)
    {
        GameObject existing = FindTransformByName("HumanPanel")?.gameObject;
        if (existing != null)
        {
            return existing;
        }

        GameObject panel = new GameObject("HumanPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(templatePanel.transform.parent, false);

        RectTransform templateRect = templatePanel.GetComponent<RectTransform>();
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = templateRect.anchorMin;
        rectTransform.anchorMax = templateRect.anchorMax;
        rectTransform.pivot = templateRect.pivot;
        rectTransform.anchoredPosition = templateRect.anchoredPosition;
        rectTransform.sizeDelta = templateRect.sizeDelta;
        rectTransform.offsetMin = templateRect.offsetMin;
        rectTransform.offsetMax = templateRect.offsetMax;

        Image image = panel.GetComponent<Image>();
        Image templateImage = templatePanel.GetComponent<Image>();
        image.color = templateImage != null ? templateImage.color : new Color(0.13f, 0.18f, 0.15f, 0.96f);
        panel.SetActive(false);
        return panel;
    }

    private void BuildHumanItems()
    {
        if (humanPanel == null || humanItemViews.Count > 0)
        {
            return;
        }

        RectTransform content = CreateHumanItemsContent(humanPanel.transform);
        string[] personaNames = { "Indian", "Chinese", "Jewish", "Hipster", "American Blond" };

        AddHumanSection(content, "Heads", HumanType.NormalHuman, BodyPartType.Head, HumanPartPrice, personaNames);
        AddHumanSection(content, "Necks", HumanType.NormalHuman, BodyPartType.Neck, HumanPartPrice, personaNames);
        AddHumanSection(content, "Stomachs", HumanType.NormalHuman, BodyPartType.Stomach, HumanPartPrice, personaNames);
        AddHumanSection(content, "Legs", HumanType.NormalHuman, BodyPartType.Leg, HumanPartPrice, personaNames);
        AddHumanSection(content, "Rock Thrower Heads", HumanType.RockThrowerHuman, BodyPartType.Head, SpecialHumanPartPrice);
        AddHumanSection(content, "Rock Thrower Necks", HumanType.RockThrowerHuman, BodyPartType.Neck, SpecialHumanPartPrice);
        AddHumanSection(content, "Rock Thrower Stomachs", HumanType.RockThrowerHuman, BodyPartType.Stomach, SpecialHumanPartPrice);
        AddHumanSection(content, "Rock Thrower Legs", HumanType.RockThrowerHuman, BodyPartType.Leg, SpecialHumanPartPrice);
        AddHumanSection(content, "Obese Heads", HumanType.ObeseHuman, BodyPartType.Head, SpecialHumanPartPrice);
        AddHumanSection(content, "Obese Necks", HumanType.ObeseHuman, BodyPartType.Neck, SpecialHumanPartPrice);
        AddHumanSection(content, "Obese Stomachs", HumanType.ObeseHuman, BodyPartType.Stomach, SpecialHumanPartPrice);
        AddHumanSection(content, "Obese Legs", HumanType.ObeseHuman, BodyPartType.Leg, SpecialHumanPartPrice);
    }

    private RectTransform CreateHumanItemsContent(Transform parent)
    {
        return CreateStoreItemsContent(parent, "HumanItems");
    }

    private RectTransform CreateStoreItemsContent(Transform parent, string namePrefix)
    {
        GameObject scrollObject = new GameObject($"{namePrefix}ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(60f, 36f);
        scrollRectTransform.offsetMax = new Vector2(-60f, -36f);

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);

        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);

        Mask mask = viewportObject.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = new GameObject($"{namePrefix}Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);

        RectTransform rectTransform = contentObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = rectTransform;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        return rectTransform;
    }

    private void BuildWeaponItems(Transform parent)
    {
        if (parent == null || weaponItemViews.Count > 0)
        {
            return;
        }

        cursorCatalog ??= CursorCustomizationCatalog.LoadDefault();
        if (cursorCatalog == null)
        {
            return;
        }

        RectTransform content = CreateStoreItemsContent(parent, "WeaponItems");
        TextMeshProUGUI titleText = CreateText(content, "Weapons", 32, TextAlignmentOptions.Left, new Color(1f, 0.92f, 0.78f, 1f));
        titleText.name = "WeaponsTitle";
        AddLayoutElement(titleText.gameObject, 44f);

        foreach (CursorCustomizationOption option in cursorCatalog.Options)
        {
            if (option == null || option.Id == CursorCustomizationSelection.DefaultCursorId)
            {
                continue;
            }

            WeaponStoreItem item = new WeaponStoreItem(option);
            weaponItemViews.Add(CreateWeaponItemView(content, item));
        }
    }

    private void AddHumanSection(RectTransform content, string title, HumanType humanType, BodyPartType part, int price, string[] personaNames = null)
    {
        TextMeshProUGUI titleText = CreateText(content, title, 32, TextAlignmentOptions.Left, new Color(1f, 0.92f, 0.78f, 1f));
        titleText.name = $"{title}Title";
        AddLayoutElement(titleText.gameObject, 44f);

        Sprite[] sprites = CharacterCustomizer.LoadHumanPartSprites(humanType, part);
        int freePartCount = CharacterCustomizer.GetFreeHumanPartCount(humanType, part);
        for (int i = freePartCount; i < sprites.Length; i++)
        {
            int unlockableIndex = i - freePartCount;
            string personaName = personaNames != null && unlockableIndex >= 0 && unlockableIndex < personaNames.Length ? personaNames[unlockableIndex] : $"Unlockable {unlockableIndex + 1}";
            string displayName = humanType == HumanType.NormalHuman
                ? $"{personaName} {GetPartDisplayName(part)}"
                : $"{CharacterCustomizer.GetHumanTypeDisplayName(humanType)} {GetPartDisplayName(part)} {unlockableIndex + 1}";
            HumanStoreItem item = new HumanStoreItem(humanType, part, i, price, displayName, sprites[i]);
            humanItemViews.Add(CreateHumanItemView(content, item));
        }
    }

    private HumanStoreItemView CreateHumanItemView(RectTransform parent, HumanStoreItem item)
    {
        GameObject row = new GameObject($"{item.DisplayName}StoreItem", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);

        Image background = row.GetComponent<Image>();
        background.color = new Color(0.08f, 0.07f, 0.06f, 0.78f);

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 12, 12);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        AddLayoutElement(row, 86f);

        Image preview = CreateHumanItemPreview(row.transform, item.Sprite);
        TextMeshProUGUI label = CreateText(row.transform, item.DisplayName, 26, TextAlignmentOptions.Left, new Color(0.95f, 0.9f, 0.82f, 1f));
        LayoutElement labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;
        labelLayout.preferredHeight = 62f;

        TextMeshProUGUI price = CreateText(row.transform, item.Price.ToString("N0"), 24, TextAlignmentOptions.Center, new Color(1f, 0.82f, 0.36f, 1f));
        ApplyPriceFont(price);
        AddLayoutElement(price.gameObject, 130f, 62f);

        Button buyButton = CreateHumanBuyButton(row.transform);
        TextMeshProUGUI buyText = buyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        HumanStoreItem capturedItem = item;
        buyButton.onClick.AddListener(() => OpenHumanPurchasePrompt(capturedItem));

        return new HumanStoreItemView(item, buyButton, buyText, price, preview);
    }

    private WeaponStoreItemView CreateWeaponItemView(RectTransform parent, WeaponStoreItem item)
    {
        GameObject row = new GameObject($"{item.DisplayName}WeaponStoreItem", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);

        Image background = row.GetComponent<Image>();
        background.color = new Color(0.08f, 0.07f, 0.06f, 0.78f);

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 12, 12);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        AddLayoutElement(row, 86f);

        Image preview = CreateTexturePreview(row.transform, item.CursorTexture);
        TextMeshProUGUI label = CreateText(row.transform, item.DisplayName, 26, TextAlignmentOptions.Left, new Color(0.95f, 0.9f, 0.82f, 1f));
        LayoutElement labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;
        labelLayout.preferredHeight = 62f;

        TextMeshProUGUI price = CreateText(row.transform, WeaponCursorPrice.ToString("N0"), 24, TextAlignmentOptions.Center, new Color(1f, 0.82f, 0.36f, 1f));
        ApplyPriceFont(price);
        AddLayoutElement(price.gameObject, 130f, 62f);

        Button buyButton = CreateHumanBuyButton(row.transform);
        TextMeshProUGUI buyText = buyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        WeaponStoreItem capturedItem = item;
        buyButton.onClick.AddListener(() => OpenWeaponPurchasePrompt(capturedItem));

        return new WeaponStoreItemView(item, buyButton, buyText, price, preview);
    }

    private Image CreateHumanItemPreview(Transform parent, Sprite sprite)
    {
        GameObject previewObject = new GameObject("Preview", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        previewObject.transform.SetParent(parent, false);

        LayoutElement layout = previewObject.GetComponent<LayoutElement>();
        layout.preferredWidth = 64f;
        layout.preferredHeight = 62f;

        Image image = previewObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = Color.white;
        return image;
    }

    private Image CreateTexturePreview(Transform parent, Texture2D texture)
    {
        Sprite sprite = null;
        if (texture != null)
        {
            sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        return CreateHumanItemPreview(parent, sprite);
    }

    private Button CreateHumanBuyButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredWidth = 150f;
        layout.preferredHeight = 62f;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.65f, 0.22f, 0.16f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = CreateText(buttonObject.transform, "Buy", 26, TextAlignmentOptions.Center, new Color(1f, 0.94f, 0.82f, 1f));
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }

    private TextMeshProUGUI CreateText(Transform parent, string value, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = statusText != null ? statusText.font : null;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private void OpenHumanPurchasePrompt(HumanStoreItem item)
    {
        if (item == null)
        {
            return;
        }

        if (CharacterCustomizer.IsHumanPartUnlocked(item.HumanType, item.Part, item.OptionIndex))
        {
            SetStatus($"{item.DisplayName} already owned.");
            RefreshHumanItemsUI();
            return;
        }

        pendingHumanPurchase = item;

        if (purchasePromptText != null)
        {
            purchasePromptText.text = $"Buy {item.DisplayName} for {item.Price:N0}?";
        }

        if (purchasePromptPanel != null)
        {
            purchasePromptPanel.SetActive(true);
            return;
        }

        ConfirmHumanPurchase();
    }

    private void OpenWeaponPurchasePrompt(WeaponStoreItem item)
    {
        if (item == null)
        {
            return;
        }

        if (CharacterCustomizer.IsWeaponCursorUnlocked(item.CursorId))
        {
            SetStatus($"{item.DisplayName} already owned.");
            RefreshWeaponItemsUI();
            return;
        }

        pendingHumanPurchase = null;
        pendingWeaponPurchase = item;

        if (purchasePromptText != null)
        {
            purchasePromptText.text = $"Buy {item.DisplayName} for {WeaponCursorPrice:N0}?";
        }

        if (purchasePromptPanel != null)
        {
            purchasePromptPanel.SetActive(true);
            return;
        }

        ConfirmWeaponPurchase();
    }

    private void ConfirmCurrentPurchase()
    {
        if (pendingHumanPurchase != null)
        {
            ConfirmHumanPurchase();
            return;
        }

        if (pendingWeaponPurchase != null)
        {
            ConfirmWeaponPurchase();
            return;
        }

        ConfirmDisableAdsPurchase();
    }

    private void ConfirmHumanPurchase()
    {
        if (purchasePromptPanel != null)
        {
            purchasePromptPanel.SetActive(false);
        }

        HumanStoreItem item = pendingHumanPurchase;
        pendingHumanPurchase = null;

        if (item == null)
        {
            return;
        }

        if (CharacterCustomizer.IsHumanPartUnlocked(item.HumanType, item.Part, item.OptionIndex))
        {
            SetStatus($"{item.DisplayName} already owned.");
            RefreshHumanItemsUI();
            return;
        }

        if (!SaveSystem.SpendCurrency(item.Price))
        {
            SetStatus($"Not enough currency. Need {item.Price:N0}.");
            Refresh();
            return;
        }

        CharacterCustomizer.SetHumanPartUnlocked(item.HumanType, item.Part, item.OptionIndex, true);
        SetStatus($"Unlocked {item.DisplayName}.");
        Refresh();
    }

    private void ConfirmWeaponPurchase()
    {
        if (purchasePromptPanel != null)
        {
            purchasePromptPanel.SetActive(false);
        }

        WeaponStoreItem item = pendingWeaponPurchase;
        pendingWeaponPurchase = null;

        if (item == null)
        {
            return;
        }

        if (CharacterCustomizer.IsWeaponCursorUnlocked(item.CursorId))
        {
            SetStatus($"{item.DisplayName} already owned.");
            RefreshWeaponItemsUI();
            return;
        }

        if (!SaveSystem.SpendCurrency(WeaponCursorPrice))
        {
            SetStatus($"Not enough currency. Need {WeaponCursorPrice:N0}.");
            Refresh();
            return;
        }

        CharacterCustomizer.SetWeaponCursorUnlocked(item.CursorId, true);
        SetStatus($"Unlocked {item.DisplayName}.");
        Refresh();
    }

    private void RefreshHumanItemsUI()
    {
        for (int i = 0; i < humanItemViews.Count; i++)
        {
            HumanStoreItemView view = humanItemViews[i];
            if (view == null || view.Item == null)
            {
                continue;
            }

            bool owned = CharacterCustomizer.IsHumanPartUnlocked(view.Item.HumanType, view.Item.Part, view.Item.OptionIndex);
            if (view.BuyButton != null)
            {
                view.BuyButton.interactable = !owned;
            }

            if (view.BuyButtonText != null)
            {
                view.BuyButtonText.text = owned ? "Owned" : "Buy";
            }

            if (view.PriceText != null)
            {
                view.PriceText.text = owned ? "Owned" : view.Item.Price.ToString("N0");
            }
        }
    }

    private void RefreshWeaponItemsUI()
    {
        for (int i = 0; i < weaponItemViews.Count; i++)
        {
            WeaponStoreItemView view = weaponItemViews[i];
            if (view == null || view.Item == null)
            {
                continue;
            }

            bool owned = CharacterCustomizer.IsWeaponCursorUnlocked(view.Item.CursorId);
            if (view.BuyButton != null)
            {
                view.BuyButton.interactable = !owned;
            }

            if (view.BuyButtonText != null)
            {
                view.BuyButtonText.text = owned ? "Owned" : "Buy";
            }

            if (view.PriceText != null)
            {
                view.PriceText.text = owned ? "Owned" : WeaponCursorPrice.ToString("N0");
            }
        }
    }

    private StoreTabView FindTab(string tabId)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i] != null && tabs[i].TabId == tabId)
            {
                return tabs[i];
            }
        }

        return null;
    }

    private void RepositionTabButtons()
    {
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i]?.TabButton != null)
            {
                buttons.Add(tabs[i].TabButton);
            }
        }

        float spacing = 310f;
        float startX = -spacing * (buttons.Count - 1) * 0.5f;
        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform rectTransform = buttons[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, rectTransform.anchorMin.y);
                rectTransform.anchorMax = new Vector2(0.5f, rectTransform.anchorMax.y);
                rectTransform.anchoredPosition = new Vector2(startX + spacing * i, rectTransform.anchoredPosition.y);
            }
        }
    }

    private static void AddLayoutElement(GameObject gameObject, float preferredHeight)
    {
        AddLayoutElement(gameObject, -1f, preferredHeight);
    }

    private static void AddLayoutElement(GameObject gameObject, float preferredWidth, float preferredHeight)
    {
        LayoutElement layout = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        if (preferredWidth >= 0f)
        {
            layout.preferredWidth = preferredWidth;
        }

        layout.preferredHeight = preferredHeight;
    }

    private static string GetPartDisplayName(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head:
                return "Head";
            case BodyPartType.Neck:
                return "Neck";
            case BodyPartType.Stomach:
                return "Stomach";
            case BodyPartType.Leg:
                return "Leg";
            default:
                return "Part";
        }
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void ApplyPriceFont(TextMeshProUGUI text)
    {
        if (text != null && priceFontAsset != null)
        {
            text.font = priceFontAsset;
        }
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

    private static Transform FindTransformByName(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && transforms[i].name == objectName)
            {
                return transforms[i];
            }
        }

        return null;
    }

    private void UpdateCurrencyBalance(int currencyBalance)
    {
        if (currencyBalanceText != null)
        {
            currencyBalanceText.text = currencyBalance.ToString();
        }
    }

    private void ShowPurchaseFailed()
    {
        SetStatus("Purchase failed.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private string GetDisableAdsPrice()
    {
        return storeManager != null ? storeManager.DisableAdsPrice : StoreManager.DisableAdsDisplayPrice;
    }

    public void GoBack()
    {
        if (string.IsNullOrWhiteSpace(backSceneName))
        {
            return;
        }

        SceneManager.LoadScene(backSceneName);
    }
}

public class HumanStoreItem
{
    public HumanStoreItem(HumanType humanType, BodyPartType part, int optionIndex, int price, string displayName, Sprite sprite)
    {
        HumanType = humanType;
        Part = part;
        OptionIndex = optionIndex;
        Price = price;
        DisplayName = displayName;
        Sprite = sprite;
    }

    public HumanType HumanType { get; }
    public BodyPartType Part { get; }
    public int OptionIndex { get; }
    public int Price { get; }
    public string DisplayName { get; }
    public Sprite Sprite { get; }
}

public class HumanStoreItemView
{
    public HumanStoreItemView(HumanStoreItem item, Button buyButton, TextMeshProUGUI buyButtonText, TextMeshProUGUI priceText, Image previewImage)
    {
        Item = item;
        BuyButton = buyButton;
        BuyButtonText = buyButtonText;
        PriceText = priceText;
        PreviewImage = previewImage;
    }

    public HumanStoreItem Item { get; }
    public Button BuyButton { get; }
    public TextMeshProUGUI BuyButtonText { get; }
    public TextMeshProUGUI PriceText { get; }
    public Image PreviewImage { get; }
}

public class WeaponStoreItem
{
    public WeaponStoreItem(CursorCustomizationOption option)
    {
        CursorId = option.Id;
        DisplayName = option.DisplayName;
        CursorTexture = option.cursorTexture;
    }

    public string CursorId { get; }
    public string DisplayName { get; }
    public Texture2D CursorTexture { get; }
}

public class WeaponStoreItemView
{
    public WeaponStoreItemView(WeaponStoreItem item, Button buyButton, TextMeshProUGUI buyButtonText, TextMeshProUGUI priceText, Image previewImage)
    {
        Item = item;
        BuyButton = buyButton;
        BuyButtonText = buyButtonText;
        PriceText = priceText;
        PreviewImage = previewImage;
    }

    public WeaponStoreItem Item { get; }
    public Button BuyButton { get; }
    public TextMeshProUGUI BuyButtonText { get; }
    public TextMeshProUGUI PriceText { get; }
    public Image PreviewImage { get; }
}

public class TabButtonBinding
{
    public TabButtonBinding(Button button, UnityAction action)
    {
        Button = button;
        Action = action;
    }

    public Button Button { get; }
    public UnityAction Action { get; }
}

[Serializable]
public class StoreTabView
{
    public StoreTabView()
    {
    }

    public StoreTabView(string tabId, Button tabButton, GameObject panel)
    {
        this.tabId = tabId;
        this.tabButton = tabButton;
        this.panel = panel;
    }

    [SerializeField] private string tabId;
    [SerializeField] private Button tabButton;
    [SerializeField] private GameObject panel;

    public string TabId => tabId;
    public Button TabButton => tabButton;
    public GameObject Panel => panel;
}
