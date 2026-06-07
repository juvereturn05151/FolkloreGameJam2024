public static class StageUnlockSystem
{
    public const string FirstTutorialId = "tutorial_first";
    public const string SecondTutorialId = "tutorial_second";
    public const string ThirdTutorialId = "tutorial_third";

    public static bool IsLevelUnlocked(StageLevelDatabase levelDatabase, int levelIndex)
    {
        if (levelDatabase == null || levelIndex < 0 || levelIndex >= levelDatabase.Count)
        {
            return false;
        }

        if (levelIndex == 0)
        {
            return SaveSystem.IsTutorialCompleted(FirstTutorialId)
                || IsLevelCompleted(levelDatabase.GetLevel(levelIndex));
        }

        if (levelIndex == 3)
        {
            return SaveSystem.IsTutorialCompleted(SecondTutorialId)
                || IsLevelCompleted(levelDatabase.GetLevel(levelIndex));
        }

        if (levelIndex == 7)
        {
            return SaveSystem.IsTutorialCompleted(ThirdTutorialId)
                || IsLevelCompleted(levelDatabase.GetLevel(levelIndex));
        }

        return IsLevelCompleted(levelDatabase.GetLevel(levelIndex - 1));
    }

    public static bool IsTutorialUnlocked(StageLevelDatabase levelDatabase, int targetLevelIndex)
    {
        switch (targetLevelIndex)
        {
            case 0:
                return true;
            case 3:
                return levelDatabase != null
                    && levelDatabase.Count > 2
                    && IsLevelCompleted(levelDatabase.GetLevel(2));
            case 7:
                return levelDatabase != null
                    && levelDatabase.Count > 6
                    && IsLevelCompleted(levelDatabase.GetLevel(6));
            default:
                return false;
        }
    }

    public static bool IsLevelCompleted(StageLevelConfig levelConfig)
    {
        if (levelConfig == null)
        {
            return false;
        }

        if (levelConfig.StageGoal != null && levelConfig.StageGoal.GetBestStars() > 0)
        {
            return true;
        }

        return SaveSystem.HasCompletedLevel(levelConfig.LevelId);
    }

    public static string GetTutorialIdForTargetLevelIndex(int targetLevelIndex)
    {
        switch (targetLevelIndex)
        {
            case 0:
                return FirstTutorialId;
            case 3:
                return SecondTutorialId;
            case 7:
                return ThirdTutorialId;
            default:
                return string.Empty;
        }
    }
}
