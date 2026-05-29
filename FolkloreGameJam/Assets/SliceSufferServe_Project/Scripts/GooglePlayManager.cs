using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using UnityEngine;

public class GooglePlayManager : MonoBehaviour
{
    public const string ClassicLeaderboardId = "CggIgI6H5hsQAhAI";
    public const string FirstBloodAchievementId = "CggIgI6H5hsQAhAA";
    public const string ProfitableRestaurantAchievementId = "CggIgI6H5hsQAhAB";
    public const string FirstStepAchievementId = "CggIgI6H5hsQAhAC";
    public const string ImpressiveButcherAchievementId = "CggIgI6H5hsQAhAD";
    public const string CompletionistButcherAchievementId = "CggIgI6H5hsQAhAE";
    public const string PerfectionistButcherAchievementId = "CggIgI6H5hsQAhAF";
    public const string ShopaholicAchievementId = "CggIgI6H5hsQAhAG";

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

    public static void UnlockAchievement(string achievementId, System.Action<bool> callback = null)
    {
        if (string.IsNullOrWhiteSpace(achievementId))
        {
            callback?.Invoke(false);
            return;
        }

        GooglePlayManager manager = EnsureInstance();
        manager.Authenticate(false, success =>
        {
            if (!success)
            {
                Debug.LogWarning("Google Play Games achievement unlock skipped because authentication failed.");
                callback?.Invoke(false);
                return;
            }

            PlayGamesPlatform.Instance.UnlockAchievement(achievementId, unlockSuccess =>
            {
                Debug.Log($"Google Play Games achievement unlock {achievementId}: {unlockSuccess}");
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
            if (progress != null && progress.levelNumber == levelNumber && progress.bestStars >= requiredStars)
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
