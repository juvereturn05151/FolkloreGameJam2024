using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

public sealed class InterstitialAdManager : MonoBehaviour
{
    private const string AndroidInterstitialAdUnitId = "ca-app-pub-2619746767391379/4403859026";
    private const float ShowTimeoutSeconds = 15f;

    private static InterstitialAdManager instance;

    private InterstitialAd interstitialAd;
    private bool isInitialized;
    private bool isLoading;
    private bool isShowing;
    private Action pendingShowComplete;

    public static InterstitialAdManager Instance
    {
        get
        {
            EnsureInstance();
            return instance;
        }
    }

    private static string InterstitialAdUnitId
    {
        get
        {
#if UNITY_ANDROID
            return AndroidInterstitialAdUnitId;
#else
            return string.Empty;
#endif
        }
    }

    private bool CanShowInterstitial => interstitialAd != null && interstitialAd.CanShowAd();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject(nameof(InterstitialAdManager));
        instance = managerObject.AddComponent<InterstitialAdManager>();
        DontDestroyOnLoad(managerObject);
    }

    public static IEnumerator ShowGameplayToGameOverAd()
    {
        bool isComplete = false;
        Instance.ShowAd(() => isComplete = true);

        float elapsedTime = 0f;
        while (!isComplete && elapsedTime < ShowTimeoutSeconds)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!isComplete)
        {
            Instance.CompleteAdShow();
        }
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
        InitializeAds();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            DestroyInterstitial();
            instance = null;
        }
    }

    private void InitializeAds()
    {
        if (isInitialized || string.IsNullOrEmpty(InterstitialAdUnitId))
        {
            return;
        }

        isInitialized = true;
#pragma warning disable CS0618
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
#pragma warning restore CS0618
        MobileAds.Initialize(_ => LoadInterstitial());
    }

    private void LoadInterstitial()
    {
        if (isLoading || string.IsNullOrEmpty(InterstitialAdUnitId))
        {
            return;
        }

        isLoading = true;
        DestroyInterstitial();

        AdRequest request = new AdRequest();
        InterstitialAd.Load(InterstitialAdUnitId, request, (ad, error) =>
        {
            isLoading = false;

            if (error != null || ad == null)
            {
                Debug.LogWarning($"Interstitial failed to load: {error}");
                return;
            }

            interstitialAd = ad;
            RegisterAdEvents(interstitialAd);
        });
    }

    private void RegisterAdEvents(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += CompleteAdShow;
        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogWarning($"Interstitial failed to show: {error}");
            CompleteAdShow();
        };
    }

    private void ShowAd(Action onComplete)
    {
        InitializeAds();

        if (isShowing || !CanShowInterstitial)
        {
            LoadInterstitial();
            onComplete?.Invoke();
            return;
        }

        pendingShowComplete = onComplete;
        isShowing = true;

        try
        {
            interstitialAd.Show();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Interstitial show threw an exception: {exception}");
            CompleteAdShow();
        }
    }

    private void CompleteAdShow()
    {
        Action onComplete = pendingShowComplete;
        pendingShowComplete = null;
        isShowing = false;

        DestroyInterstitial();
        LoadInterstitial();

        onComplete?.Invoke();
    }

    private void DestroyInterstitial()
    {
        if (interstitialAd == null)
        {
            return;
        }

        interstitialAd.Destroy();
        interstitialAd = null;
    }
}
