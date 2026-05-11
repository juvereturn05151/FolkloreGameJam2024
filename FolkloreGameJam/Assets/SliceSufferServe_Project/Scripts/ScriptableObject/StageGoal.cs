using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StageGoal", menuName = "Scriptable Objects/Stage Goal")]
public class StageGoal : ScriptableObject
{
    [Header("Stage")]
    [SerializeField] private string stageId;
    [SerializeField] private string displayName;

    [Header("Star Thresholds")]
    [SerializeField] private int oneStarScore = 100;
    [SerializeField] private int twoStarScore = 200;
    [SerializeField] private int threeStarScore = 300;

    public string StageId => string.IsNullOrWhiteSpace(stageId) ? name : stageId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public int OneStarScore => oneStarScore;
    public int TwoStarScore => twoStarScore;
    public int ThreeStarScore => threeStarScore;

    public StageGoalResult Evaluate(int score)
    {
        int stars = 0;

        if (score >= oneStarScore)
        {
            stars = 1;
        }

        if (score >= twoStarScore)
        {
            stars = 2;
        }

        if (score >= threeStarScore)
        {
            stars = 3;
        }

        return new StageGoalResult(this, score, stars);
    }

    public StageGoalResult EvaluateAndSaveBest(int score)
    {
        StageGoalResult result = Evaluate(score);

        int previousBestScore = GetBestScore();
        int previousBestStars = GetBestStars();

        if (score > previousBestScore)
        {
            PlayerPrefs.SetInt(GetBestScoreKey(), score);
        }

        if (result.Stars > previousBestStars)
        {
            PlayerPrefs.SetInt(GetBestStarsKey(), result.Stars);
        }

        PlayerPrefs.Save();
        return result;
    }

    public int GetBestScore()
    {
        return PlayerPrefs.GetInt(GetBestScoreKey(), 0);
    }

    public int GetBestStars()
    {
        return PlayerPrefs.GetInt(GetBestStarsKey(), 0);
    }

    private string GetBestScoreKey()
    {
        return $"StageGoal.{StageId}.BestScore";
    }

    private string GetBestStarsKey()
    {
        return $"StageGoal.{StageId}.BestStars";
    }

    private void OnValidate()
    {
        oneStarScore = Mathf.Max(0, oneStarScore);
        twoStarScore = Mathf.Max(oneStarScore, twoStarScore);
        threeStarScore = Mathf.Max(twoStarScore, threeStarScore);
    }
}

[Serializable]
public class StageGoalResult
{
    public StageGoalResult(StageGoal goal, int score, int stars)
    {
        Goal = goal;
        Score = score;
        Stars = stars;
    }

    public StageGoal Goal { get; }
    public int Score { get; }
    public int Stars { get; }
    public bool HasAllStars => Stars >= 3;

    public int? NextStarScore
    {
        get
        {
            if (Goal == null || HasAllStars)
            {
                return null;
            }

            return Stars switch
            {
                0 => Goal.OneStarScore,
                1 => Goal.TwoStarScore,
                2 => Goal.ThreeStarScore,
                _ => null
            };
        }
    }
}
