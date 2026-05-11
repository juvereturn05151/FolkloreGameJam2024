using UnityEngine;

public static class CurrencySystem
{
    public static int CalculateCurrencyFromScore(int score, int scorePointsPerCurrency)
    {
        if (score <= 0)
        {
            return 0;
        }

        scorePointsPerCurrency = Mathf.Max(1, scorePointsPerCurrency);
        return Mathf.FloorToInt((float)score / scorePointsPerCurrency);
    }

    public static int AwardCurrencyFromScore(int score, int scorePointsPerCurrency)
    {
        int earnedCurrency = CalculateCurrencyFromScore(score, scorePointsPerCurrency);
        SaveSystem.AddCurrency(earnedCurrency);
        return earnedCurrency;
    }
}
