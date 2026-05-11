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

    [Header("Store")]
    [SerializeField] private StoreManager storeManager;
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

    private string selectedTabId;
    private readonly List<TabButtonBinding> tabButtonBindings = new List<TabButtonBinding>();

    private void Awake()
    {
        if (storeManager == null)
        {
            storeManager = FindFirstObjectByType<StoreManager>();
        }
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
        if (storeManager != null && storeManager.AreAdsDisabled)
        {
            Refresh();
            return;
        }

        if (purchasePromptText != null)
        {
            purchasePromptText.text = $"Remove all ads for {StoreManager.DisableAdsDisplayPrice}?";
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
        SetStatus($"Purchase requested: {StoreManager.DisableAdsDisplayPrice}");
    }

    public void CancelPurchasePrompt()
    {
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
        RefreshItemsPlaceholderUI();
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
            purchaseConfirmButton.onClick.AddListener(ConfirmDisableAdsPurchase);
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
            purchaseConfirmButton.onClick.RemoveListener(ConfirmDisableAdsPurchase);
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
            disableAdsPriceText.text = adsDisabled ? "Owned" : StoreManager.DisableAdsDisplayPrice;
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
            itemsPlaceholderText.text = "Items coming soon.";
        }
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

    public void GoBack()
    {
        if (string.IsNullOrWhiteSpace(backSceneName))
        {
            return;
        }

        SceneManager.LoadScene(backSceneName);
    }
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
    [SerializeField] private string tabId;
    [SerializeField] private Button tabButton;
    [SerializeField] private GameObject panel;

    public string TabId => tabId;
    public Button TabButton => tabButton;
    public GameObject Panel => panel;
}
