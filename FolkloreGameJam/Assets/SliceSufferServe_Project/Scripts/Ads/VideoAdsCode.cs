using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class VideoAdsCode : MonoBehaviour
{
    [Header("LevelPlay Settings")]
#if UNITY_ANDROID
    [SerializeField] private string appKey = "YOUR_ANDROID_APP_KEY";
#else
    [SerializeField] private string appKey = "YOUR_IOS_APP_KEY";
#endif

    [Header("Ad Unit IDs - USE TEST IDs FIRST")]
    [SerializeField] private string interstitialAdUnitId = "16f0c2e4299"; // TEST ID
    [SerializeField] private string rewardedAdUnitId = "16f0c2e4299";     // TEST ID

    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedAd;
    
    private bool isLoadingInterstitial = false;
    private bool isLoadingRewarded = false;
    private bool sdkInitialized = false;

    private void Start()
    {
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;
        LevelPlay.Init(appKey);
    }

    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        sdkInitialized = true;
        Debug.Log("✅ SDK Initialized");
        
        SetupInterstitial();
        SetupRewarded();
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"❌ SDK Init Failed: {error.ErrorMessage}");
    }

    private void SetupInterstitial()
    {
        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
        // ... setup events
        LoadInterstitialAd();
    }

    private void SetupRewarded()
    {
        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);
        // ... setup events
        LoadRewardedAd();
    }

    public void LoadInterstitialAd()
    {
        if (interstitialAd != null && sdkInitialized && !isLoadingInterstitial)
        {
            isLoadingInterstitial = true;
            interstitialAd.LoadAd();
            Invoke(nameof(ResetInterstitialLoading), 30f);
        }
    }

    public void LoadRewardedAd()
    {
        if (rewardedAd != null && sdkInitialized && !isLoadingRewarded)
        {
            isLoadingRewarded = true;
            rewardedAd.LoadAd();
            Invoke(nameof(ResetRewardedLoading), 30f);
        }
    }

    private void ResetInterstitialLoading() => isLoadingInterstitial = false;
    private void ResetRewardedLoading() => isLoadingRewarded = false;

    // ... rest of your code
}