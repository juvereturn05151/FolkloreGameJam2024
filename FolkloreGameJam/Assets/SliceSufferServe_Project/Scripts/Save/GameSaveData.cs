using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int saveVersion = 1;
    public bool adsDisabled;
    public int currencyBalance;
    public int totalCurrencyEarned;
    public string highestCompletedLevelId = string.Empty;
    public int highestCompletedLevelNumber;
    public List<LevelProgressSaveData> levelProgress = new List<LevelProgressSaveData>();

    public LevelProgressSaveData GetOrCreateLevelProgress(string levelId)
    {
        LevelProgressSaveData progress = GetLevelProgress(levelId);

        if (progress != null)
        {
            return progress;
        }

        progress = new LevelProgressSaveData(levelId);
        levelProgress.Add(progress);
        return progress;
    }

    public LevelProgressSaveData GetLevelProgress(string levelId)
    {
        if (string.IsNullOrWhiteSpace(levelId))
        {
            return null;
        }

        for (int i = 0; i < levelProgress.Count; i++)
        {
            if (levelProgress[i] != null && levelProgress[i].levelId == levelId)
            {
                return levelProgress[i];
            }
        }

        return null;
    }
}

[Serializable]
public class LevelProgressSaveData
{
    public string levelId;
    public int levelNumber;
    public int bestScore;
    [Range(0, 3)] public int bestStars;

    public LevelProgressSaveData()
    {
    }

    public LevelProgressSaveData(string levelId)
    {
        this.levelId = levelId;
    }
}
