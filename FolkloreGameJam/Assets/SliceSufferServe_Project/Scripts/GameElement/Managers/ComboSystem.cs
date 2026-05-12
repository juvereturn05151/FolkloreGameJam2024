using System;

public static class ComboSystem
{
    public static event Action<int> OnComboChanged;

    public static int CurrentCombo { get; private set; }
    public static int CurrentScoreMultiplier => GetScoreMultiplier(CurrentCombo);

    public static void AddCombo(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentCombo += amount;
        OnComboChanged?.Invoke(CurrentCombo);
    }

    public static void ResetCombo()
    {
        if (CurrentCombo == 0)
        {
            OnComboChanged?.Invoke(CurrentCombo);
            return;
        }

        CurrentCombo = 0;
        OnComboChanged?.Invoke(CurrentCombo);
    }

    public static int GetScoreMultiplier(int combo)
    {
        if (combo >= 10)
        {
            return 4;
        }

        if (combo >= 6)
        {
            return 3;
        }

        if (combo >= 3)
        {
            return 2;
        }

        return 1;
    }

    public static int ApplyScoreMultiplier(int baseScore)
    {
        return baseScore * CurrentScoreMultiplier;
    }

    public static bool IsSpecialEffectActive => CurrentCombo >= 10;
}
