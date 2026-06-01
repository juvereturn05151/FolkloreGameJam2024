using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

public class StoreManager : MonoBehaviour, IDetailedStoreListener
{
    public const string DisableAdsProductId = "remove_ads";
    public const string DisableAdsDisplayPrice = "$4.99";

    public StoreRealMoneyPurchaseRequestedEvent OnRealMoneyPurchaseRequested = new StoreRealMoneyPurchaseRequestedEvent();
    public UnityEvent OnAdsDisabledPurchased = new UnityEvent();
    public UnityEvent OnAdsAlreadyDisabled = new UnityEvent();
    public UnityEvent OnPurchaseFailed = new UnityEvent();

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;
    private bool isInitializing;
    private bool buyDisableAdsWhenInitialized;

    public bool AreAdsDisabled => SaveSystem.AreAdsDisabled();
    public bool IsStoreReady => storeController != null && extensionProvider != null;
    public string DisableAdsPrice => GetDisableAdsPrice();

    private void Awake()
    {
        InitializePurchasing();
    }

    public void BuyDisableAds()
    {
        RequestBuyDisableAds();
    }

    public void RequestBuyDisableAds()
    {
        if (SaveSystem.AreAdsDisabled())
        {
            OnAdsAlreadyDisabled?.Invoke();
            return;
        }

        OnRealMoneyPurchaseRequested?.Invoke(DisableAdsProductId, DisableAdsDisplayPrice);

        if (!IsStoreReady)
        {
            buyDisableAdsWhenInitialized = true;
            InitializePurchasing();
            Debug.Log("IAP store is not ready yet. Purchase will start after initialization.");
            return;
        }

        Product product = storeController.products.WithID(DisableAdsProductId);
        if (product == null || !product.availableToPurchase)
        {
            Debug.LogWarning($"IAP product is not available: {DisableAdsProductId}");
            OnPurchaseFailed?.Invoke();
            return;
        }

        storeController.InitiatePurchase(product);
        Debug.Log($"Real money purchase requested: {DisableAdsProductId} ({DisableAdsPrice}).");
    }

    public void CompleteDisableAdsPurchase()
    {
        if (SaveSystem.AreAdsDisabled())
        {
            OnAdsAlreadyDisabled?.Invoke();
            return;
        }

        SaveSystem.SetAdsDisabled(true);
        InterstitialAdManager.OnAdsDisabled();
        OnAdsDisabledPurchased?.Invoke();
        Debug.Log("Disable ads purchased and saved.");
    }

    public void FailPurchase()
    {
        OnPurchaseFailed?.Invoke();
    }

    public void RestoreDisableAdsEntitlement()
    {
        if (!IsStoreReady)
        {
            InitializePurchasing();
            return;
        }

        Product product = storeController.products.WithID(DisableAdsProductId);
        if (product != null && product.hasReceipt)
        {
            CompleteDisableAdsPurchase();
            return;
        }

        Debug.Log("No remove ads entitlement receipt found to restore.");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        isInitializing = false;
        RestoreDisableAdsEntitlement();

        if (buyDisableAdsWhenInitialized)
        {
            buyDisableAdsWhenInitialized = false;
            RequestBuyDisableAds();
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        isInitializing = false;
        buyDisableAdsWhenInitialized = false;
        Debug.LogWarning($"Unity IAP initialization failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        isInitializing = false;
        buyDisableAdsWhenInitialized = false;
        Debug.LogWarning($"Unity IAP initialization failed: {error}. {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        Product product = purchaseEvent.purchasedProduct;
        if (product != null && product.definition.id == DisableAdsProductId)
        {
            CompleteDisableAdsPurchase();
        }

        return PurchaseProcessingResult.Complete;
    }

    void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"Purchase failed: {product?.definition.id}. {failureReason}");
        OnPurchaseFailed?.Invoke();
    }

    void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning($"Purchase failed: {product?.definition.id}. {failureDescription.reason}: {failureDescription.message}");
        OnPurchaseFailed?.Invoke();
    }

    private void InitializePurchasing()
    {
        if (IsStoreReady || isInitializing)
        {
            return;
        }

        isInitializing = true;
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(DisableAdsProductId, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
    }

    private string GetDisableAdsPrice()
    {
        if (!IsStoreReady)
        {
            return DisableAdsDisplayPrice;
        }

        Product product = storeController.products.WithID(DisableAdsProductId);
        if (product == null || product.metadata == null || string.IsNullOrWhiteSpace(product.metadata.localizedPriceString))
        {
            return DisableAdsDisplayPrice;
        }

        return product.metadata.localizedPriceString;
    }
}

[System.Serializable]
public class StoreRealMoneyPurchaseRequestedEvent : UnityEvent<string, string>
{
}
