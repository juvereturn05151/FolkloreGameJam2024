using UnityEngine;
using UnityEngine.Events;

public class StoreManager : MonoBehaviour
{
    public const string DisableAdsProductId = "disable_ads";
    public const string DisableAdsDisplayPrice = "$4.99";

    public StoreRealMoneyPurchaseRequestedEvent OnRealMoneyPurchaseRequested = new StoreRealMoneyPurchaseRequestedEvent();
    public UnityEvent OnAdsDisabledPurchased = new UnityEvent();
    public UnityEvent OnAdsAlreadyDisabled = new UnityEvent();
    public UnityEvent OnPurchaseFailed = new UnityEvent();

    public bool AreAdsDisabled => SaveSystem.AreAdsDisabled();

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
        Debug.Log($"Real money purchase requested: {DisableAdsProductId} ({DisableAdsDisplayPrice}).");
    }

    public void CompleteDisableAdsPurchase()
    {
        if (SaveSystem.AreAdsDisabled())
        {
            OnAdsAlreadyDisabled?.Invoke();
            return;
        }

        SaveSystem.SetAdsDisabled(true);
        OnAdsDisabledPurchased?.Invoke();
        Debug.Log("Disable ads purchased and saved.");
    }

    public void FailPurchase()
    {
        OnPurchaseFailed?.Invoke();
    }
}

[System.Serializable]
public class StoreRealMoneyPurchaseRequestedEvent : UnityEvent<string, string>
{
}
