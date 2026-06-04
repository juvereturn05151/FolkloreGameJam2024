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
    private const int SpecialHumanPartPrice = 2000;
    private const int WeaponCursorPrice = 3000;
    private const float HumanItemRowHeight = 176f;
    private const float HumanItemPreviewWidth = 190f;
    private const float HumanItemPreviewHeight = 148f;
    private const float HumanItemPreviewScale = 2.2f;
    private const float WeaponItemRowWidth = 1030f;
    private const float WeaponItemRowHeight = 198f;
    private const float WeaponItemPreviewWidth = 176f;
    private const float WeaponItemPreviewHeight = 154f;
    private const float WeaponItemPreviewScale = 1f;
    private const float WeaponItemInfoWidth = 450f;
    private const float WeaponItemPriceWidth = 145f;
    private const float WeaponItemControlHeight = 78f;
    private const float WeaponItemBuyButtonWidth = 160f;

    [Header("Store")]
    [SerializeField] private StoreManager storeManager;
    [SerializeField] private CharacterCustomizationManager customizationManager;
    [SerializeField] private string defaultTabId = DisableAdsTabId;
    [SerializeField] private string backSceneName = "GameModeSelect";
    [SerializeField] private List<StoreTabView> tabs = new List<StoreTabView>();

    [Header("Shared UI")]
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI currencyBalanceText;
    [SerializeField] private TextMeshProUGUI statusText;

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

    [Header("Generated UI Prefabs")]
    [SerializeField] private StoreItemsContentView storeItemsContentPrefab;
    [SerializeField] private TextMeshProUGUI sectionTitlePrefab;
    [SerializeField] private StoreItemRowView humanItemPrefab;
    [SerializeField] private StoreItemRowView weaponItemPrefab;

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

        ResolveCustomizationManager();
        cursorCatalog = CursorCustomizationCatalog.LoadDefault();
    }

    private void OnEnable()
    {
        SaveSystem.OnCurrencyChanged += UpdateCurrencyBalance;

        if (storeManager != null)
        {
            storeManager.OnAdsDisabledPurchased.AddListener(Refresh);
            storeManager.OnAdsAlreadyDisabled.AddListener(Refresh);
            storeManager.OnPurchaseFailed.AddListener(ShowPurchaseFailed);
            storeManager.RestoreDisableAdsEntitlement();
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
        AndroidAchievementSystem.ReportStoreInventoryChanged();
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

        StoreTabView humanTab = FindTab(HumanTabId);
        if (humanTab?.Panel == null)
        {
            return;
        }

        humanPanel = humanTab.Panel;
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

    private void BuildHumanItems()
    {
        if (humanPanel == null || humanItemViews.Count > 0)
        {
            return;
        }

        RectTransform content = CreateHumanItemsContent(humanPanel.transform);
        string[] personaNames = { "Person1", "Person2", "Person3", "Person4", "Person5" };

        AddHumanSection(content, "Heads", HumanType.NormalHuman, BodyPartType.Head, HumanPartPrice, personaNames);
        AddHumanSection(content, "Necks", HumanType.NormalHuman, BodyPartType.Neck, HumanPartPrice, personaNames);
        AddHumanSection(content, "Stomachs", HumanType.NormalHuman, BodyPartType.Stomach, HumanPartPrice, personaNames);
        AddHumanSection(content, "Legs", HumanType.NormalHuman, BodyPartType.Leg, HumanPartPrice, personaNames);
        AddHumanSection(content, "Rock Thrower Heads", HumanType.RockThrowerHuman, BodyPartType.Head, SpecialHumanPartPrice);
        AddHumanSection(content, "Rock Thrower Necks", HumanType.RockThrowerHuman, BodyPartType.Neck, SpecialHumanPartPrice);
        AddHumanSection(content, "Rock Thrower Stomachs", HumanType.RockThrowerHuman, BodyPartType.Stomach, SpecialHumanPartPrice);
        AddHumanSection(content, "Rock Thrower Legs", HumanType.RockThrowerHuman, BodyPartType.Leg, SpecialHumanPartPrice);
        AddHumanSection(content, "Big Heads", HumanType.BigHuman, BodyPartType.Head, SpecialHumanPartPrice);
        AddHumanSection(content, "Big Necks", HumanType.BigHuman, BodyPartType.Neck, SpecialHumanPartPrice);
        AddHumanSection(content, "Big Stomachs", HumanType.BigHuman, BodyPartType.Stomach, SpecialHumanPartPrice);
        AddHumanSection(content, "Big Legs", HumanType.BigHuman, BodyPartType.Leg, SpecialHumanPartPrice);
        AddHumanSection(content, "Knight Heads", HumanType.KnightHuman, BodyPartType.Head, SpecialHumanPartPrice);
        AddHumanSection(content, "Knight Necks", HumanType.KnightHuman, BodyPartType.Neck, SpecialHumanPartPrice);
        AddHumanSection(content, "Knight Stomachs", HumanType.KnightHuman, BodyPartType.Stomach, SpecialHumanPartPrice);
        AddHumanSection(content, "Knight Legs", HumanType.KnightHuman, BodyPartType.Leg, SpecialHumanPartPrice);
        AddHumanSection(content, "Robot Heads", HumanType.RobotHuman, BodyPartType.Head, SpecialHumanPartPrice);
        AddHumanSection(content, "Robot Necks", HumanType.RobotHuman, BodyPartType.Neck, SpecialHumanPartPrice);
        AddHumanSection(content, "Robot Stomachs", HumanType.RobotHuman, BodyPartType.Stomach, SpecialHumanPartPrice);
        AddHumanSection(content, "Robot Legs", HumanType.RobotHuman, BodyPartType.Leg, SpecialHumanPartPrice);
    }

    private RectTransform CreateHumanItemsContent(Transform parent)
    {
        return CreateStoreItemsContent(parent, "HumanItems");
    }

    private RectTransform CreateStoreItemsContent(Transform parent, string namePrefix)
    {
        if (storeItemsContentPrefab != null)
        {
            StoreItemsContentView view = Instantiate(storeItemsContentPrefab, parent);
            view.name = $"{namePrefix}ScrollView";
            view.ResolveReferences();

            RectTransform prefabRectTransform = view.GetComponent<RectTransform>();
            if (prefabRectTransform != null)
            {
                prefabRectTransform.anchorMin = Vector2.zero;
                prefabRectTransform.anchorMax = Vector2.one;
                prefabRectTransform.offsetMin = new Vector2(60f, 36f);
                prefabRectTransform.offsetMax = new Vector2(-60f, -36f);
            }

            RectTransform content = view.Content;
            if (content != null)
            {
                content.name = $"{namePrefix}Content";
                return content;
            }
        }

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
        layout.childControlHeight = true;
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
        VerticalLayoutGroup weaponLayout = content.GetComponent<VerticalLayoutGroup>();
        if (weaponLayout != null)
        {
            weaponLayout.childForceExpandWidth = false;
        }

        CreateSectionTitle(content, "Weapons", "WeaponsTitle", WeaponItemRowWidth);

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
        CreateSectionTitle(content, title, $"{title}Title");

        Sprite[] sprites = CharacterCustomizer.LoadHumanPartSprites(humanType, part);
        int freePartCount = CharacterCustomizer.GetFreeHumanPartCount(humanType, part);
        if (sprites.Length == 0)
        {
            Sprite defaultSprite = GetDefaultHumanPartSprite(humanType, part);
            if (defaultSprite != null)
            {
                sprites = new[] { defaultSprite };
                freePartCount = 0;
            }
        }

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

    private TextMeshProUGUI CreateSectionTitle(RectTransform parent, string value, string objectName, float preferredWidth = -1f)
    {
        TextMeshProUGUI titleText;
        if (sectionTitlePrefab != null)
        {
            titleText = Instantiate(sectionTitlePrefab, parent);
            titleText.name = objectName;
            titleText.text = value;
        }
        else
        {
            titleText = CreateText(parent, value, 32, TextAlignmentOptions.Left, new Color(1f, 0.92f, 0.78f, 1f));
            titleText.name = objectName;
        }

        AddLayoutElement(titleText.gameObject, preferredWidth, 44f);
        return titleText;
    }

    private HumanStoreItemView CreateHumanItemView(RectTransform parent, HumanStoreItem item)
    {
        if (humanItemPrefab != null)
        {
            StoreItemRowView rowView = Instantiate(humanItemPrefab, parent);
            rowView.name = $"{item.DisplayName}StoreItem";
            rowView.ResolveReferences();

            SetText(rowView.TitleText, item.DisplayName);
            SetText(rowView.DescriptionText, string.Empty);
            SetText(rowView.PriceText, item.Price.ToString("N0"));
            SetText(rowView.BuyButtonText, "Buy");

            if (rowView.DescriptionText != null)
            {
                rowView.DescriptionText.gameObject.SetActive(false);
            }

            if (rowView.PreviewImage != null)
            {
                rowView.PreviewImage.sprite = item.Sprite;
                rowView.PreviewImage.preserveAspect = true;
            }

            if (rowView.BuyButton != null)
            {
                HumanStoreItem prefabCapturedItem = item;
                rowView.BuyButton.onClick.AddListener(() => OpenHumanPurchasePrompt(prefabCapturedItem));
            }

            return new HumanStoreItemView(item, rowView.BuyButton, rowView.BuyButtonText, rowView.PriceText, rowView.PreviewImage);
        }

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

        AddLayoutElement(row, HumanItemRowHeight);

        Image preview = CreateHumanItemPreview(row.transform, item.Sprite);
        TextMeshProUGUI label = CreateText(row.transform, item.DisplayName, 26, TextAlignmentOptions.Left, new Color(0.95f, 0.9f, 0.82f, 1f));
        LayoutElement labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;
        labelLayout.preferredHeight = 148f;

        TextMeshProUGUI price = CreateText(row.transform, item.Price.ToString("N0"), 24, TextAlignmentOptions.Center, new Color(1f, 0.82f, 0.36f, 1f));
        AddLayoutElement(price.gameObject, 130f, 148f);

        Button buyButton = CreateHumanBuyButton(row.transform);
        TextMeshProUGUI buyText = buyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        HumanStoreItem capturedItem = item;
        buyButton.onClick.AddListener(() => OpenHumanPurchasePrompt(capturedItem));

        return new HumanStoreItemView(item, buyButton, buyText, price, preview);
    }

    private WeaponStoreItemView CreateWeaponItemView(RectTransform parent, WeaponStoreItem item)
    {
        if (weaponItemPrefab != null)
        {
            StoreItemRowView rowView = Instantiate(weaponItemPrefab, parent);
            rowView.name = $"{item.DisplayName}WeaponStoreItem";
            rowView.ResolveReferences();

            SetText(rowView.TitleText, item.DisplayName.ToUpperInvariant());
            SetText(rowView.DescriptionText, GetWeaponDescription(item.CursorId));
            SetText(rowView.PriceText, WeaponCursorPrice.ToString("N0"));
            SetText(rowView.BuyButtonText, "Buy");

            if (rowView.PreviewImage != null)
            {
                rowView.PreviewImage.sprite = CreateSprite(item.CursorTexture);
                rowView.PreviewImage.preserveAspect = true;
            }

            if (rowView.BuyButton != null)
            {
                WeaponStoreItem prefabCapturedItem = item;
                rowView.BuyButton.onClick.AddListener(() => OpenWeaponPurchasePrompt(prefabCapturedItem));
            }

            return new WeaponStoreItemView(item, rowView.BuyButton, rowView.BuyButtonText, rowView.PriceText, rowView.PreviewImage);
        }

        GameObject row = new GameObject($"{item.DisplayName}WeaponStoreItem", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);

        Image background = row.GetComponent<Image>();
        background.color = new Color(0.62f, 0.52f, 0.39f, 0.95f);

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(32, 32, 22, 22);
        layout.spacing = 34f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        AddLayoutElement(row, WeaponItemRowWidth, WeaponItemRowHeight);

        Image preview = CreateTexturePreview(row.transform, item.CursorTexture);
        CreateWeaponInfoBlock(row.transform, item);

        TextMeshProUGUI price = CreateText(row.transform, WeaponCursorPrice.ToString("N0"), 44, TextAlignmentOptions.Center, new Color(1f, 0.82f, 0.36f, 1f));
        AddLayoutElement(price.gameObject, WeaponItemPriceWidth, WeaponItemControlHeight);

        Button buyButton = CreateBuyButton(row.transform, WeaponItemBuyButtonWidth, WeaponItemControlHeight, 41);
        TextMeshProUGUI buyText = buyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        WeaponStoreItem capturedItem = item;
        buyButton.onClick.AddListener(() => OpenWeaponPurchasePrompt(capturedItem));

        return new WeaponStoreItemView(item, buyButton, buyText, price, preview);
    }

    private Image CreateHumanItemPreview(Transform parent, Sprite sprite)
    {
        return CreateItemPreview(parent, sprite, HumanItemPreviewWidth, HumanItemPreviewHeight, HumanItemPreviewScale);
    }

    private void CreateWeaponInfoBlock(Transform parent, WeaponStoreItem item)
    {
        GameObject textBlock = new GameObject("Info", typeof(RectTransform), typeof(LayoutElement));
        textBlock.transform.SetParent(parent, false);

        RectTransform textBlockRect = textBlock.GetComponent<RectTransform>();
        textBlockRect.sizeDelta = new Vector2(WeaponItemInfoWidth, WeaponItemPreviewHeight);

        LayoutElement layout = textBlock.GetComponent<LayoutElement>();
        layout.preferredWidth = WeaponItemInfoWidth;
        layout.preferredHeight = WeaponItemPreviewHeight;

        TextMeshProUGUI title = CreateText(textBlock.transform, item.DisplayName.ToUpperInvariant(), 46, TextAlignmentOptions.Left, new Color(0.12f, 0.08f, 0.05f, 1f));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(0f, 58f);
        title.enableWordWrapping = false;
        title.overflowMode = TextOverflowModes.Overflow;

        TextMeshProUGUI description = CreateText(textBlock.transform, GetWeaponDescription(item.CursorId), 30, TextAlignmentOptions.TopLeft, new Color(0.15f, 0.11f, 0.08f, 1f));
        RectTransform descriptionRect = description.GetComponent<RectTransform>();
        descriptionRect.anchorMin = new Vector2(0f, 0f);
        descriptionRect.anchorMax = new Vector2(1f, 1f);
        descriptionRect.pivot = new Vector2(0f, 1f);
        descriptionRect.offsetMin = new Vector2(0f, 0f);
        descriptionRect.offsetMax = new Vector2(0f, -58f);
        description.enableWordWrapping = true;
        description.overflowMode = TextOverflowModes.Overflow;
    }

    private Image CreateItemPreview(Transform parent, Sprite sprite, float preferredWidth, float preferredHeight, float previewScale)
    {
        GameObject previewContainer = new GameObject("Preview", typeof(RectTransform), typeof(LayoutElement), typeof(RectMask2D));
        previewContainer.transform.SetParent(parent, false);

        LayoutElement layout = previewContainer.GetComponent<LayoutElement>();
        layout.preferredWidth = preferredWidth;
        layout.preferredHeight = preferredHeight;

        GameObject previewObject = new GameObject("PreviewImage", typeof(RectTransform), typeof(Image));
        previewObject.transform.SetParent(previewContainer.transform, false);

        RectTransform imageRect = previewObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        imageRect.localScale = Vector3.one * previewScale;

        Image image = previewObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = Color.white;
        return image;
    }

    private Image CreateTexturePreview(Transform parent, Texture2D texture)
    {
        return CreateItemPreview(parent, CreateSprite(texture), WeaponItemPreviewWidth, WeaponItemPreviewHeight, WeaponItemPreviewScale);
    }

    private Button CreateHumanBuyButton(Transform parent)
    {
        return CreateBuyButton(parent, 150f, 62f, 26);
    }

    private Button CreateBuyButton(Transform parent, float preferredWidth, float preferredHeight, int fontSize)
    {
        GameObject buttonObject = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredWidth = preferredWidth;
        layout.preferredHeight = preferredHeight;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.65f, 0.22f, 0.16f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = CreateText(buttonObject.transform, "Buy", fontSize, TextAlignmentOptions.Center, new Color(1f, 0.94f, 0.82f, 1f));
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
        AndroidAchievementSystem.ReportStoreInventoryChanged();
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
        AndroidAchievementSystem.ReportStoreInventoryChanged();
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

    private Sprite GetDefaultHumanPartSprite(HumanType humanType, BodyPartType part)
    {
        ResolveCustomizationManager();
        return customizationManager != null ? customizationManager.GetDefaultSprite(humanType, part) : null;
    }

    private void ResolveCustomizationManager()
    {
        if (customizationManager != null)
        {
            return;
        }

        customizationManager = CharacterCustomizationManager.Instance ?? FindAnyObjectByType<CharacterCustomizationManager>();
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

    private static string GetWeaponDescription(string cursorId)
    {
        switch (cursorId)
        {
            case "claw":
                return "Sharp and deadly. A classic weapon for close encounters.";
            case "chainsaw":
                return "Loud, heavy, and built for messy work.";
            case "rainbow_knife":
                return "A bright blade with a strange appetite.";
            case "flame_blade":
                return "Hot steel for fast, brutal cuts.";
            case "golden_blade":
                return "Ancient metal polished for legendary service.";
            default:
                return "A custom weapon cursor for the kitchen.";
        }
    }

    private static Sprite CreateSprite(Texture2D texture)
    {
        return texture != null
            ? Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f)
            : null;
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
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
