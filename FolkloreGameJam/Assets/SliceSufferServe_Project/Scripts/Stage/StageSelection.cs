public static class StageSelection
{
    public static StageLevelConfig SelectedLevel { get; private set; }

    public static void SelectLevel(StageLevelConfig levelConfig)
    {
        SelectedLevel = levelConfig;
    }

    public static void Clear()
    {
        SelectedLevel = null;
    }
}
