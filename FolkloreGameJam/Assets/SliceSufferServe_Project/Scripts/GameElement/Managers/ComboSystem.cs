using System;

public static class ComboSystem
{
    public static event Action<int> OnComboChanged;

    public static int CurrentCombo { get; private set; }

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
}
