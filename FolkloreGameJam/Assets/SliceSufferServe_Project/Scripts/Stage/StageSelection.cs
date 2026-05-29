public static class StageSelection
{
    private const string DefaultGameplaySceneName = "GameplayScene";
    private const string FirstTutorialSceneName = "FirstTutorial";
    private const string SecondTutorialSceneName = "SecondTutorial";

    public static StageLevelConfig SelectedLevel { get; private set; }
    public static bool IsClassicMode => IsClassicLevel(SelectedLevel);

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

    private static bool IsClassicLevel(StageLevelConfig levelConfig)
    {
        if (levelConfig == null)
        {
            return false;
        }

        return IsClassicText(levelConfig.LevelId)
            || IsClassicText(levelConfig.DisplayName)
            || IsClassicText(levelConfig.name);
    }

    private static bool IsClassicText(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.ToLowerInvariant().Contains("classic");
    }
}
