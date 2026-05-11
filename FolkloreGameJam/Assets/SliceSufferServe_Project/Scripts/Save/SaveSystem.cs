using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "game-save.json";
    private static GameSaveData data;
    public static event Action<int> OnCurrencyChanged;

    public static GameSaveData Data
    {
        get
        {
            if (data == null)
            {
                Load();
            }

            return data;
        }
    }

    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static void Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            data = new GameSaveData();
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            data = JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to load save data. A new save will be used. {exception.Message}");
            data = new GameSaveData();
        }

        if (data == null)
        {
            data = new GameSaveData();
        }

        if (data.levelProgress == null)
        {
            data.levelProgress = new System.Collections.Generic.List<LevelProgressSaveData>();
        }
    }

    public static void Save()
    {
        if (data == null)
        {
            data = new GameSaveData();
        }

        try
        {
            string directory = Path.GetDirectoryName(SaveFilePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to save game data. {exception.Message}");
        }
    }

    public static bool AreAdsDisabled()
    {
        return Data.adsDisabled;
    }

    public static void SetAdsDisabled(bool disabled)
    {
        Data.adsDisabled = disabled;
        Save();
    }

    public static int GetCurrencyBalance()
    {
        return Data.currencyBalance;
    }

    public static void AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Data.currencyBalance += amount;
        Data.totalCurrencyEarned += amount;
        Save();
        OnCurrencyChanged?.Invoke(Data.currencyBalance);
    }

    public static bool SpendCurrency(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Data.currencyBalance < amount)
        {
            return false;
        }

        Data.currencyBalance -= amount;
        Save();
        OnCurrencyChanged?.Invoke(Data.currencyBalance);
        return true;
    }

    public static LevelProgressSaveData GetLevelProgress(string levelId)
    {
        return Data.GetLevelProgress(levelId);
    }

    public static LevelProgressSaveData SaveLevelProgress(string levelId, int levelNumber, int score, int stars)
    {
        if (string.IsNullOrWhiteSpace(levelId))
        {
            Debug.LogWarning("Cannot save level progress without a level id.");
            return null;
        }

        stars = Mathf.Clamp(stars, 0, 3);
        levelNumber = Mathf.Max(0, levelNumber);

        LevelProgressSaveData progress = Data.GetOrCreateLevelProgress(levelId);
        progress.levelNumber = Mathf.Max(progress.levelNumber, levelNumber);
        progress.bestScore = Mathf.Max(progress.bestScore, score);
        progress.bestStars = Mathf.Max(progress.bestStars, stars);

        if (stars > 0 && (levelNumber > Data.highestCompletedLevelNumber || string.IsNullOrEmpty(Data.highestCompletedLevelId)))
        {
            Data.highestCompletedLevelNumber = levelNumber;
            Data.highestCompletedLevelId = levelId;
        }

        Save();
        return progress;
    }

    public static void Clear()
    {
        data = new GameSaveData();

        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
        }
    }
}
