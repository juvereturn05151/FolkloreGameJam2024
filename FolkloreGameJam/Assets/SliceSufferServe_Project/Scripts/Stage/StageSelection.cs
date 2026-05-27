public static class StageSelection
{
    private const string DefaultGameplaySceneName = "GameplayScene";
    private const string FirstTutorialSceneName = "FirstTutorial";
    private const string SecondTutorialSceneName = "SecondTutorial";

    public static StageLevelConfig SelectedLevel { get; private set; }

    public static void SelectLevel(StageLevelConfig levelConfig)
    {
        SelectedLevel = levelConfig;
    }

    public static void Clear()
    {
        SelectedLevel = null;
    }

    public static string GetEntrySceneName(StageLevelConfig levelConfig)
    {
        return GetTutorialSceneName(levelConfig);
    }

    public static string GetGameplaySceneName(StageLevelConfig levelConfig)
    {
        return levelConfig == null ? DefaultGameplaySceneName : levelConfig.GameplaySceneName;
    }

    public static string GetTutorialSceneName(StageLevelConfig levelConfig)
    {
        if (levelConfig == null)
        {
            return DefaultGameplaySceneName;
        }

        switch (levelConfig.LevelNumber)
        {
            case 1:
                return FirstTutorialSceneName;
            case 4:
                return SecondTutorialSceneName;
            default:
                return levelConfig.GameplaySceneName;
        }
    }

    public static string GetSelectedGameplaySceneName()
    {
        return GetGameplaySceneName(SelectedLevel);
    }
}
