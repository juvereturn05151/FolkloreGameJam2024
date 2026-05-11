using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StageGoal", menuName = "Scriptable Objects/Stage Goal")]
public class StageGoal : ScriptableObject
{
    [Header("Stage")]
    [SerializeField] private string stageId;
    [SerializeField] private string displayName;
    [SerializeField] private int levelNumber = 1;

    [Header("Star Thresholds")]
    [SerializeField] private int oneStarScore = 100;
    [SerializeField] private int twoStarScore = 200;
    [SerializeField] private int threeStarScore = 300;

    public string StageId => string.IsNullOrWhiteSpace(stageId) ? name : stageId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public int LevelNumber => Mathf.Max(0, levelNumber);
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
        SaveSystem.SaveLevelProgress(StageId, LevelNumber, Mathf.Max(score, previousBestScore), Mathf.Max(result.Stars, previousBestStars));
        return result;
    }

    public int GetBestScore()
    {
        LevelProgressSaveData progress = SaveSystem.GetLevelProgress(StageId);
        int savedBestScore = progress == null ? 0 : progress.bestScore;
        return Mathf.Max(savedBestScore, PlayerPrefs.GetInt(GetBestScoreKey(), 0));
    }

    public int GetBestStars()
    {
        LevelProgressSaveData progress = SaveSystem.GetLevelProgress(StageId);
        int savedBestStars = progress == null ? 0 : progress.bestStars;
        return Mathf.Max(savedBestStars, PlayerPrefs.GetInt(GetBestStarsKey(), 0));
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
        levelNumber = Mathf.Max(0, levelNumber);
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
