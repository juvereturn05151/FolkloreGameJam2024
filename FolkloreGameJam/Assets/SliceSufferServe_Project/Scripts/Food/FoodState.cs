public enum FoodState
{
    Normal,
    MediumRotten,
    SuperRotten,
    Disappear
}

public static class FoodStateUtility
{
    public static FoodState Next(FoodState state)
    {
        return state switch
        {
            FoodState.Normal => FoodState.MediumRotten,
            FoodState.MediumRotten => FoodState.SuperRotten,
            FoodState.SuperRotten => FoodState.Disappear,
            FoodState.Disappear => FoodState.Disappear,
            _ => FoodState.Disappear
        };
    }

    public static bool IsTerminal(FoodState state)
    {
        return state == FoodState.Disappear;
    }
}