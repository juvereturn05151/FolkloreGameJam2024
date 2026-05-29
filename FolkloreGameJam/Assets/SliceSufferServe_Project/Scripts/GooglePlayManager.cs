using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using UnityEngine;

public class GooglePlayManager : MonoBehaviour
{
    public const string ClassicLeaderboardId = "CggIgI6H5hsQAhAI";

    public static GooglePlayManager Instance;
    public bool IsAuthenticated { get; private set; }

    private bool isAuthenticating;
    private bool activeAuthIsManual;
    private readonly List<System.Action<bool>> pendingAuthCallbacks = new List<System.Action<bool>>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ActivatePlatform();
        Authenticate(false, null);
    }

    public static void ReportClassicScore(long score)
    {
        GooglePlayManager manager = EnsureInstance();
        manager.Authenticate(false, success =>
        {
            if (!success)
            {
                Debug.LogWarning("Google Play Games score upload skipped because authentication failed.");
                return;
            }

            PlayGamesPlatform.Instance.ReportScore(score, ClassicLeaderboardId, uploadSuccess =>
            {
                Debug.Log("Google Play Games classic leaderboard upload success: " + uploadSuccess);
            });
        });
    }

    public static void ShowClassicLeaderboard()
    {
        GooglePlayManager manager = EnsureInstance();
        manager.Authenticate(true, success =>
        {
            if (!success)
            {
                Debug.LogWarning("Google Play Games leaderboard skipped because authentication failed.");
                return;
            }

            PlayGamesPlatform.Instance.ShowLeaderboardUI(ClassicLeaderboardId);
        });
    }

    private static GooglePlayManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject managerObject = new GameObject(nameof(GooglePlayManager));
        return managerObject.AddComponent<GooglePlayManager>();
    }

    private void Authenticate(bool manualSignIn, System.Action<bool> callback)
    {
        ActivatePlatform();

        if (IsAuthenticated)
        {
            callback?.Invoke(true);
            return;
        }

        if (isAuthenticating)
        {
            if (callback != null)
            {
                if (manualSignIn && !activeAuthIsManual)
                {
                    pendingAuthCallbacks.Add(success =>
                    {
                        if (success)
                        {
                            callback(true);
                            return;
                        }

                        Authenticate(true, callback);
                    });
                }
                else
                {
                    pendingAuthCallbacks.Add(callback);
                }
            }

            return;
        }

        isAuthenticating = true;
        activeAuthIsManual = manualSignIn;
        if (callback != null)
        {
            pendingAuthCallbacks.Add(callback);
        }

        System.Action<SignInStatus> onAuthenticated = status =>
        {
            isAuthenticating = false;
            activeAuthIsManual = false;
            IsAuthenticated = status == SignInStatus.Success;
            Debug.Log("Google Play Games status: " + status);

            System.Action<bool>[] callbacks = pendingAuthCallbacks.ToArray();
            pendingAuthCallbacks.Clear();

            for (int i = 0; i < callbacks.Length; i++)
            {
                callbacks[i]?.Invoke(IsAuthenticated);
            }
        };

        if (manualSignIn)
        {
            PlayGamesPlatform.Instance.ManuallyAuthenticate(onAuthenticated);
        }
        else
        {
            PlayGamesPlatform.Instance.Authenticate(onAuthenticated);
        }
    }

    private static void ActivatePlatform()
    {
        PlayGamesPlatform.Activate();
    }
}
