using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using UnityEngine;

public class GooglePlayManager : MonoBehaviour
{
    public const string ClassicLeaderboardId = "CgkIpNabzsEbEAIQAQ";
    public const string FirstBloodAchievementId = "CgkIpNabzsEbEAIQAg";
    public const string ProfitableRestaurantAchievementId = "CgkIpNabzsEbEAIQAw";
    public const string FirstStepAchievementId = "CgkIpNabzsEbEAIQBA";
    public const string ImpressiveButcherAchievementId = "CgkIpNabzsEbEAIQBQ";
    public const string CompletionistButcherAchievementId = "CgkIpNabzsEbEAIQBg";
    public const string PerfectionistButcherAchievementId = "CgkIpNabzsEbEAIQBw";
    public const string ShopaholicAchievementId = "CgkIpNabzsEbEAIQCA";

    public static GooglePlayManager Instance;

    public bool IsAuthenticated { get; private set; }

    private bool isAuthenticating;
    private static bool platformActivated;

    private readonly List<System.Action<bool>> pendingAuthCallbacks = new List<System.Action<bool>>();

    [Header("Debug")]
    [SerializeField]
    private bool showDebugGui = true;

    private string lastAuthStatus = "Not requested";
    private string lastLeaderboardStatus = "Not requested";
    private string lastUiStatus = "Not requested";
    private string lastScoreUploadStatus = "Not requested";
    private string lastAchievementStatus = "Not requested";
    private string lastLocalUserName = "";
    private string lastLocalUserId = "";
    private string lastDebugMessage = "Ready";
    private Vector2 debugScrollPosition;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ActivatePlatform();
    }

    private void Start()
    {
        // Do not automatically authenticate here while debugging.
        // Authentication will be requested only when leaderboard / score / achievement needs it.
        lastDebugMessage = "GooglePlayManager ready.";
        Debug.Log("[GPGS] GooglePlayManager ready.");
    }

    public static void ReportClassicScore(long score)
    {
        GooglePlayManager manager = EnsureInstance();

        manager.Authenticate(success =>
        {
            if (!success)
            {
                manager.lastScoreUploadStatus = "Skipped: authentication failed";
                manager.lastDebugMessage = "Score upload skipped because authentication failed.";
                Debug.LogWarning("[GPGS] Score upload skipped because authentication failed.");
                return;
            }

            PlayGamesPlatform.Instance.ReportScore(score, ClassicLeaderboardId, uploadSuccess =>
            {
                manager.lastScoreUploadStatus = uploadSuccess ? "Upload Success" : "Upload Failed";
                manager.lastDebugMessage = "Score upload result: " + manager.lastScoreUploadStatus;
                Debug.LogError("[GPGS] Classic leaderboard upload success: " + uploadSuccess);
            });
        });
    }

    public static void ShowClassicLeaderboard(System.Action<bool, UIStatus> callback = null)
    {
        GooglePlayManager manager = EnsureInstance();

        manager.Authenticate(success =>
        {
            if (!success)
            {
                manager.lastUiStatus = "NotAuthorized";
                manager.lastDebugMessage = "Leaderboard UI skipped because authentication failed.";
                Debug.LogWarning("[GPGS] Leaderboard UI skipped because authentication failed.");
                callback?.Invoke(false, UIStatus.NotAuthorized);
                return;
            }

            PlayGamesPlatform.Instance.ShowLeaderboardUI(ClassicLeaderboardId, status =>
            {
                manager.lastUiStatus = status.ToString();
                manager.lastDebugMessage = "Leaderboard UI status: " + status;
                Debug.LogError("[GPGS] Classic leaderboard UI status: " + status);
                callback?.Invoke(status == UIStatus.Valid || status == UIStatus.UserClosedUI, status);
            });
        });
    }

#if UNITY_ANDROID
    public static void LoadClassicLeaderboardScores(int rowCount, System.Action<bool, LeaderboardScoreData> callback)
    {
        GooglePlayManager manager = EnsureInstance();

        manager.Authenticate(success =>
        {
            if (!success)
            {
                manager.lastLeaderboardStatus = "Skipped: authentication failed";
                manager.lastDebugMessage = "Leaderboard scores skipped because authentication failed.";
                Debug.LogWarning("[GPGS] Leaderboard scores skipped because authentication failed.");
                callback?.Invoke(false, null);
                return;
            }

            PlayGamesPlatform.Instance.LoadScores(
                ClassicLeaderboardId,
                LeaderboardStart.PlayerCentered,
                rowCount,
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                data =>
                {
                    if (data == null)
                    {
                        manager.lastLeaderboardStatus = "No data";
                        manager.lastDebugMessage = "Leaderboard scores returned no data.";
                        Debug.LogWarning("[GPGS] Classic leaderboard scores returned no data.");
                        callback?.Invoke(false, null);
                        return;
                    }

                    manager.lastLeaderboardStatus =
                        data.Status +
                        " / Valid: " + data.Valid +
                        " / Scores: " + (data.Scores == null ? 0 : data.Scores.Length) +
                        " / PlayerScore: " + (data.PlayerScore == null ? "null" : data.PlayerScore.value.ToString());

                    manager.lastDebugMessage = "Leaderboard scores status: " + manager.lastLeaderboardStatus;

                    Debug.LogError("[GPGS] Classic leaderboard scores status: " + data.Status);
                    Debug.LogError("[GPGS] Classic leaderboard scores valid: " + data.Valid);
                    Debug.LogError("[GPGS] Classic leaderboard scores count: " + (data.Scores == null ? 0 : data.Scores.Length));
                    Debug.LogError("[GPGS] Classic leaderboard player score: " + (data.PlayerScore == null ? "null" : data.PlayerScore.value.ToString()));

                    callback?.Invoke(data.Valid, data);
                });
        });
    }
#endif

    public static void UnlockAchievement(string achievementId, System.Action<bool> callback = null)
    {
        if (string.IsNullOrWhiteSpace(achievementId))
        {
            callback?.Invoke(false);
            return;
        }

        GooglePlayManager manager = EnsureInstance();

        manager.Authenticate(success =>
        {
            if (!success)
            {
                manager.lastAchievementStatus = "Skipped: authentication failed";
                manager.lastDebugMessage = "Achievement unlock skipped because authentication failed.";
                Debug.LogWarning("[GPGS] Achievement unlock skipped because authentication failed.");
                callback?.Invoke(false);
                return;
            }

            PlayGamesPlatform.Instance.UnlockAchievement(achievementId, unlockSuccess =>
            {
                manager.lastAchievementStatus = achievementId + " / " + (unlockSuccess ? "Success" : "Failed");
                manager.lastDebugMessage = "Achievement unlock result: " + manager.lastAchievementStatus;

                Debug.LogError("[GPGS] Achievement unlock " + achievementId + ": " + unlockSuccess);
                callback?.Invoke(unlockSuccess);
            });
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

    private void Authenticate(System.Action<bool> callback)
    {
        ActivatePlatform();

        lastDebugMessage = "Authenticate requested.";

        Debug.LogError("[GPGS] Authenticate requested.");
        Debug.LogError("[GPGS] Cached IsAuthenticated: " + IsAuthenticated);
        Debug.LogError("[GPGS] localUser.authenticated: " + PlayGamesPlatform.Instance.localUser.authenticated);

        if (IsAuthenticated || PlayGamesPlatform.Instance.localUser.authenticated)
        {
            IsAuthenticated = true;
            lastAuthStatus = "Already authenticated";
            UpdateLocalUserInfo();
            callback?.Invoke(true);
            return;
        }

        if (isAuthenticating)
        {
            lastDebugMessage = "Already authenticating. Callback queued.";

            if (callback != null)
            {
                pendingAuthCallbacks.Add(callback);
            }

            return;
        }

        isAuthenticating = true;
        lastAuthStatus = "Authenticating...";
        lastDebugMessage = "Authentication started.";

        if (callback != null)
        {
            pendingAuthCallbacks.Add(callback);
        }

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            isAuthenticating = false;
            IsAuthenticated = status == SignInStatus.Success;
            lastAuthStatus = status.ToString();

            UpdateLocalUserInfo();

            lastDebugMessage =
                "Authenticate finished. Status: " +
                status +
                " / Authenticated: " +
                IsAuthenticated;

            Debug.LogError("[GPGS] Authenticate status: " + status);
            Debug.LogError("[GPGS] localUser.authenticated after auth: " + PlayGamesPlatform.Instance.localUser.authenticated);
            Debug.LogError("[GPGS] localUser.userName: " + lastLocalUserName);
            Debug.LogError("[GPGS] localUser.id: " + lastLocalUserId);

#if UNITY_ANDROID
            if (IsAuthenticated)
            {
                AndroidAchievementSystem.SyncEligibleAchievements();
            }
#endif

            System.Action<bool>[] callbacks = pendingAuthCallbacks.ToArray();
            pendingAuthCallbacks.Clear();

            for (int i = 0; i < callbacks.Length; i++)
            {
                callbacks[i]?.Invoke(IsAuthenticated);
            }
        });
    }

    private void UpdateLocalUserInfo()
    {
        try
        {
            lastLocalUserName = PlayGamesPlatform.Instance.localUser.userName;
            lastLocalUserId = PlayGamesPlatform.Instance.localUser.id;
        }
        catch (System.Exception exception)
        {
            lastLocalUserName = "";
            lastLocalUserId = "";
            lastDebugMessage = "Local user read error: " + exception.Message;
        }
    }

    private static void ActivatePlatform()
    {
        if (platformActivated)
        {
            return;
        }

        PlayGamesPlatform.Activate();
        platformActivated = true;

        Debug.LogError("[GPGS] PlayGamesPlatform activated.");
    }

    private void OnGUI()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if (!showDebugGui)
        {
            return;
        }

        int width = Mathf.Min(Screen.width - 20, 900);
        int height = Mathf.Min(Screen.height - 20, 780);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 24;
        labelStyle.wordWrap = true;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 30;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.wordWrap = true;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 26;

        GUILayout.BeginArea(new Rect(10, 10, width, height), GUI.skin.box);

        debugScrollPosition = GUILayout.BeginScrollView(debugScrollPosition);

        GUILayout.Label("GOOGLE PLAY GAMES DEBUG", titleStyle);
        GUILayout.Space(10);

        GUILayout.Label("Package: " + Application.identifier, labelStyle);
        GUILayout.Label("Platform Activated: " + platformActivated, labelStyle);
        GUILayout.Label("Is Authenticating: " + isAuthenticating, labelStyle);
        GUILayout.Label("Cached IsAuthenticated: " + IsAuthenticated, labelStyle);

        bool localAuthenticated = false;
        string localUserName = "";
        string localUserId = "";

        try
        {
            localAuthenticated = PlayGamesPlatform.Instance.localUser.authenticated;
            localUserName = PlayGamesPlatform.Instance.localUser.userName;
            localUserId = PlayGamesPlatform.Instance.localUser.id;
        }
        catch (System.Exception exception)
        {
            GUILayout.Label("Local User Read Error: " + exception.Message, labelStyle);
        }

        GUILayout.Label("localUser.authenticated: " + localAuthenticated, labelStyle);
        GUILayout.Label("localUser.userName: " + localUserName, labelStyle);
        GUILayout.Label("localUser.id: " + localUserId, labelStyle);

        GUILayout.Space(10);

        GUILayout.Label("Last Auth Status: " + lastAuthStatus, labelStyle);
        GUILayout.Label("Last Leaderboard Scores Status: " + lastLeaderboardStatus, labelStyle);
        GUILayout.Label("Last Leaderboard UI Status: " + lastUiStatus, labelStyle);
        GUILayout.Label("Last Score Upload Status: " + lastScoreUploadStatus, labelStyle);
        GUILayout.Label("Last Achievement Status: " + lastAchievementStatus, labelStyle);
        GUILayout.Label("Last Debug Message: " + lastDebugMessage, labelStyle);

        GUILayout.Space(16);

        if (GUILayout.Button("Manual Authenticate", buttonStyle, GUILayout.Height(70)))
        {
            ActivatePlatform();

            lastAuthStatus = "Manual authenticating...";
            lastDebugMessage = "Manual authentication started.";
            isAuthenticating = true;

            PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
            {
                isAuthenticating = false;
                IsAuthenticated = status == SignInStatus.Success;
                lastAuthStatus = status.ToString();

                UpdateLocalUserInfo();

                lastDebugMessage =
                    "Manual authenticate finished. Status: " +
                    status +
                    " / Authenticated: " +
                    IsAuthenticated;

                Debug.LogError("[GPGS_DEBUG_GUI] Manual Authenticate status: " + status);
                Debug.LogError("[GPGS_DEBUG_GUI] localUser.authenticated: " + PlayGamesPlatform.Instance.localUser.authenticated);
                Debug.LogError("[GPGS_DEBUG_GUI] localUser.userName: " + lastLocalUserName);
                Debug.LogError("[GPGS_DEBUG_GUI] localUser.id: " + lastLocalUserId);
            });
        }

        if (GUILayout.Button("Show Leaderboard UI", buttonStyle, GUILayout.Height(70)))
        {
            ShowClassicLeaderboard((success, status) =>
            {
                lastUiStatus = status.ToString();
                lastDebugMessage = "Show Leaderboard UI success: " + success + " / Status: " + status;
                Debug.LogError("[GPGS_DEBUG_GUI] Show leaderboard UI success: " + success + ", status: " + status);
            });
        }

#if UNITY_ANDROID
        if (GUILayout.Button("Load Leaderboard Scores", buttonStyle, GUILayout.Height(70)))
        {
            LoadClassicLeaderboardScores(10, (success, data) =>
            {
                if (data == null)
                {
                    lastLeaderboardStatus = "No data. Success: " + success;
                }
                else
                {
                    lastLeaderboardStatus =
                        data.Status +
                        " / Valid: " + data.Valid +
                        " / Scores: " + (data.Scores == null ? 0 : data.Scores.Length);
                }

                lastDebugMessage = "Load Leaderboard Scores result: " + lastLeaderboardStatus;
                Debug.LogError("[GPGS_DEBUG_GUI] Load scores success: " + success + ", status: " + lastLeaderboardStatus);
            });
        }
#endif

        if (GUILayout.Button("Report Test Score 123", buttonStyle, GUILayout.Height(70)))
        {
            ReportClassicScore(123);
            lastScoreUploadStatus = "Requested test score upload: 123";
            lastDebugMessage = "Requested test score upload.";
        }

        if (GUILayout.Button("Clear Cached Auth State", buttonStyle, GUILayout.Height(70)))
        {
            IsAuthenticated = false;
            isAuthenticating = false;
            pendingAuthCallbacks.Clear();
            lastAuthStatus = "Cleared cached auth state";
            lastDebugMessage = "Cleared cached auth state.";
            Debug.LogError("[GPGS_DEBUG_GUI] Cleared cached auth state.");
        }

        if (GUILayout.Button("Hide Debug GUI", buttonStyle, GUILayout.Height(70)))
        {
            showDebugGui = false;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
#endif
    }
}

public static class AndroidAchievementSystem
{
    private const int ProfitableRestaurantServesRequired = 100;
    private const int StoryLevelCount = 13;

    private const string FirstBloodEarnedKey = "AndroidAchievement.FirstBlood.Earned";
    private const string OrganServesKey = "AndroidAchievement.ProfitableRestaurant.OrganServes";
    private const string FirstStepEarnedKey = "AndroidAchievement.FirstStep.Earned";
    private const string ImpressiveButcherEarnedKey = "AndroidAchievement.ImpressiveButcher.Earned";
    private const string CompletionistButcherEarnedKey = "AndroidAchievement.CompletionistButcher.Earned";
    private const string PerfectionistButcherEarnedKey = "AndroidAchievement.PerfectionistButcher.Earned";
    private const string ShopaholicEarnedKey = "AndroidAchievement.Shopaholic.Earned";
    private const string UploadedPrefix = "AndroidAchievement.Uploaded.";

    public static void ReportFirstBlood()
    {
        SetEarned(FirstBloodEarnedKey);
        SyncEligibleAchievements();
    }

    public static void ReportOrganServed(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        int serves = PlayerPrefs.GetInt(OrganServesKey, 0);
        PlayerPrefs.SetInt(OrganServesKey, Mathf.Max(serves + amount, serves));
        PlayerPrefs.Save();

        SyncEligibleAchievements();
    }

    public static void ReportStageResult(StageGoalResult result)
    {
        if (result == null || result.Goal == null || StageSelection.IsClassicMode)
        {
            return;
        }

        if (result.Stars > 0 && result.Goal.LevelNumber == 1)
        {
            SetEarned(FirstStepEarnedKey);
        }

        if (result.Stars >= 3)
        {
            SetEarned(ImpressiveButcherEarnedKey);
        }

        if (HasCompletedAllStoryLevels())
        {
            SetEarned(CompletionistButcherEarnedKey);
        }

        if (HasPerfectedAllStoryLevels())
        {
            SetEarned(PerfectionistButcherEarnedKey);
        }

        SyncEligibleAchievements();
    }

    public static void ReportStoreInventoryChanged()
    {
        if (HasBoughtAllCosmeticStoreItems())
        {
            SetEarned(ShopaholicEarnedKey);
        }

        SyncEligibleAchievements();
    }

    public static void SyncEligibleAchievements()
    {
        TryUnlockIfEarned(FirstBloodEarnedKey, GooglePlayManager.FirstBloodAchievementId);

        if (PlayerPrefs.GetInt(OrganServesKey, 0) >= ProfitableRestaurantServesRequired)
        {
            TryUnlock(GooglePlayManager.ProfitableRestaurantAchievementId);
        }

        TryUnlockIfEarned(FirstStepEarnedKey, GooglePlayManager.FirstStepAchievementId);
        TryUnlockIfEarned(ImpressiveButcherEarnedKey, GooglePlayManager.ImpressiveButcherAchievementId);
        TryUnlockIfEarned(CompletionistButcherEarnedKey, GooglePlayManager.CompletionistButcherAchievementId);
        TryUnlockIfEarned(PerfectionistButcherEarnedKey, GooglePlayManager.PerfectionistButcherAchievementId);
        TryUnlockIfEarned(ShopaholicEarnedKey, GooglePlayManager.ShopaholicAchievementId);
    }

    private static void TryUnlockIfEarned(string earnedKey, string achievementId)
    {
        if (PlayerPrefs.GetInt(earnedKey, 0) == 1)
        {
            TryUnlock(achievementId);
        }
    }

    private static void TryUnlock(string achievementId)
    {
#if UNITY_ANDROID
        string uploadedKey = UploadedPrefix + achievementId;

        if (PlayerPrefs.GetInt(uploadedKey, 0) == 1)
        {
            return;
        }

        GooglePlayManager.UnlockAchievement(achievementId, success =>
        {
            if (!success)
            {
                return;
            }

            PlayerPrefs.SetInt(uploadedKey, 1);
            PlayerPrefs.Save();
        });
#endif
    }

    private static void SetEarned(string key)
    {
        if (PlayerPrefs.GetInt(key, 0) == 1)
        {
            return;
        }

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    private static bool HasCompletedAllStoryLevels()
    {
        return HasStoryLevelProgress(1);
    }

    private static bool HasPerfectedAllStoryLevels()
    {
        return HasStoryLevelProgress(3);
    }

    private static bool HasStoryLevelProgress(int requiredStars)
    {
        for (int levelNumber = 1; levelNumber <= StoryLevelCount; levelNumber++)
        {
            if (!HasLevelWithStars(levelNumber, requiredStars))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasLevelWithStars(int levelNumber, int requiredStars)
    {
        GameSaveData data = SaveSystem.Data;

        if (data?.levelProgress == null)
        {
            return false;
        }

        for (int i = 0; i < data.levelProgress.Count; i++)
        {
            LevelProgressSaveData progress = data.levelProgress[i];

            if (progress != null &&
                progress.levelNumber == levelNumber &&
                progress.bestStars >= requiredStars)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasBoughtAllCosmeticStoreItems()
    {
        bool hasPurchasableItem = false;

        if (!HasUnlockedAllHumanStoreItems(ref hasPurchasableItem))
        {
            return false;
        }

        if (!HasUnlockedAllWeaponStoreItems(ref hasPurchasableItem))
        {
            return false;
        }

        return hasPurchasableItem;
    }

    private static bool HasUnlockedAllHumanStoreItems(ref bool hasPurchasableItem)
    {
        HumanType[] humanTypes =
        {
            HumanType.NormalHuman,
            HumanType.RockThrowerHuman,
            HumanType.ObeseHuman
        };

        BodyPartType[] bodyParts =
        {
            BodyPartType.Head,
            BodyPartType.Neck,
            BodyPartType.Stomach,
            BodyPartType.Leg
        };

        for (int humanIndex = 0; humanIndex < humanTypes.Length; humanIndex++)
        {
            for (int partIndex = 0; partIndex < bodyParts.Length; partIndex++)
            {
                HumanType humanType = humanTypes[humanIndex];
                BodyPartType part = bodyParts[partIndex];

                Sprite[] sprites = CharacterCustomizer.LoadHumanPartSprites(humanType, part);
                int freeCount = CharacterCustomizer.GetFreeHumanPartCount(humanType, part);

                for (int optionIndex = freeCount; optionIndex < sprites.Length; optionIndex++)
                {
                    hasPurchasableItem = true;

                    if (!CharacterCustomizer.IsHumanPartUnlocked(humanType, part, optionIndex))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool HasUnlockedAllWeaponStoreItems(ref bool hasPurchasableItem)
    {
        CursorCustomizationCatalog cursorCatalog = CursorCustomizationCatalog.LoadDefault();

        if (cursorCatalog == null)
        {
            return true;
        }

        CursorCustomizationOption[] options = cursorCatalog.Options;

        for (int i = 0; i < options.Length; i++)
        {
            CursorCustomizationOption option = options[i];

            if (option == null || option.Id == CursorCustomizationSelection.DefaultCursorId)
            {
                continue;
            }

            hasPurchasableItem = true;

            if (!CharacterCustomizer.IsWeaponCursorUnlocked(option.Id))
            {
                return false;
            }
        }

        return true;
    }


}