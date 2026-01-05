using UnityEngine;
using Unity.Services.Core;
using Unity.Services.LevelPlay;
using System.Threading.Tasks;

public class LevelPlayManager : MonoBehaviour
{
    [Header("LevelPlay Settings")]
    [SerializeField] private string appKey;

    [Header("Ad Unit IDs")]
    [SerializeField] private string rewardedAdUnitId;
    [SerializeField] private string interstitialAdUnitId;
    [SerializeField] private string bannerAdUnitId;

    public static LevelPlayManager Instance { get; private set; }

    private LevelPlayRewardedAd rewardedAd;
    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayBannerAd bannerAd;

    private bool appOpenShown = false;

    private async void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize Unity Services first
        await UnityServices.InitializeAsync();

        // LevelPlay initialization
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;
        LevelPlay.Init(appKey);
    }

    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay initialized on device!");

        SetupRewarded();
        //SetupInterstitial();
        //SetupBanner();

        Invoke(nameof(ShowAppOpen), 1f);
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay init failed: {error.ErrorMessage}");
    }

    #region Rewarded
    private void SetupRewarded()
    {
        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);

        rewardedAd.OnAdLoaded += adInfo => Debug.Log("Rewarded loaded");
        rewardedAd.OnAdLoadFailed += error => Debug.LogError("Rewarded failed: " + error.ErrorMessage);
        rewardedAd.OnAdDisplayed += adInfo => Debug.Log("Rewarded displayed");
        rewardedAd.OnAdClosed += adInfo => rewardedAd.LoadAd();
        rewardedAd.OnAdRewarded += (adInfo, reward) =>
        {
            Debug.Log("User should be rewarded!");
            HandleUserEarnedReward();
        };

        rewardedAd.LoadAd();
    }

    public void ShowRewarded()
    {
        if (rewardedAd != null && rewardedAd.IsAdReady())
            rewardedAd.ShowAd();
        else
            rewardedAd?.LoadAd();
    }

    public void HandleUserEarnedReward()
    {
        print("REWARD GIVEN TO PLAYER - 50 AMMO ADDED");
     //   AmmoText.ammoAmount += 50; // Same as in AdManager - 50 ammo added
    }
    #endregion

    #region Interstitial
    private void SetupInterstitial()
    {
        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

        interstitialAd.OnAdLoaded += adInfo => Debug.Log("Interstitial loaded");
        interstitialAd.OnAdLoadFailed += error => Debug.LogError("Interstitial failed: " + error.ErrorMessage);
        interstitialAd.OnAdDisplayed += adInfo => Debug.Log("Interstitial displayed");
        interstitialAd.OnAdClosed += adInfo => interstitialAd.LoadAd();

        interstitialAd.LoadAd();
    }

    public void ShowInterstitial()
    {
        if (interstitialAd != null && interstitialAd.IsAdReady())
            interstitialAd.ShowAd();
        else
            interstitialAd?.LoadAd();
    }

    private void ShowAppOpen()
    {
        if (!appOpenShown && interstitialAd != null && interstitialAd.IsAdReady())
        {
            appOpenShown = true;
            interstitialAd.ShowAd();
        }
    }
    #endregion

    #region Banner
    private void SetupBanner()
    {
        bannerAd = new LevelPlayBannerAd(bannerAdUnitId);

        bannerAd.OnAdLoaded += adInfo => Debug.Log("Banner loaded");
        bannerAd.OnAdLoadFailed += error => Debug.LogError("Banner failed: " + error.ErrorMessage);

        // Banner position is set via dashboard
       bannerAd.LoadAd();
    }

    public void HideBanner()
    {
        //bannerAd?.HideAd();
    }
    #endregion

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;
    }
}