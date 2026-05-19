using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayGameOverUI : MonoBehaviour
{
    [Header("Game Over Elements")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private RectTransform receiptImage;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private TextMeshProUGUI gameOverStarsText;
    [SerializeField] private TextMeshProUGUI gameOverNextGoalText;
    [SerializeField] private TextMeshProUGUI gameOverCurrencyEarnedText;
    [SerializeField] private Button leaderboardUI;

    [Header("Currency Reward")]
    [SerializeField] private int scorePointsPerCurrency = 1;

    public bool IsShowing => gameOverPanel != null && gameOverPanel.activeSelf;

    public void Initialize()
    {
        if (gameOverHighScoreText != null && ScoreManager.Instance != null)
        {
            gameOverHighScoreText.text = $"High Score: {ScoreManager.Instance.GetHighScore()}";
        }
    }

    public StageGoalResult ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        int currentScore = ScoreManager.Instance.GetCurrentScore();

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = $"Score: {currentScore}";
        }

        StageGoalResult stageGoalResult = GameManager.Instance.EvaluateAndSaveStageGoal(currentScore);
        UpdateStageGoalUI(stageGoalResult);

        int earnedCurrency = CurrencySystem.AwardCurrencyFromScore(currentScore, scorePointsPerCurrency);
        UpdateCurrencyRewardUI(earnedCurrency);

        if (currentScore >= ScoreManager.Instance.GetHighScore())
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);
        }

        SteamLeaderboardManager.UpdateScore(currentScore);
        return stageGoalResult;
    }

    private void UpdateStageGoalUI(StageGoalResult result)
    {
        if (gameOverStarsText != null)
        {
            gameOverStarsText.text = result == null ? string.Empty : $"Stars: {result.Stars} / 3";
        }

        if (gameOverNextGoalText == null)
        {
            return;
        }

        if (result == null)
        {
            gameOverNextGoalText.text = string.Empty;
            return;
        }

        int? nextStarScore = result.NextStarScore;
        gameOverNextGoalText.text = nextStarScore.HasValue
            ? $"Next star: {nextStarScore.Value}"
            : "All stars earned";
    }

    private void UpdateCurrencyRewardUI(int earnedCurrency)
    {
        if (gameOverCurrencyEarnedText != null)
        {
            gameOverCurrencyEarnedText.text = $"+{earnedCurrency} Currency";
        }
    }
}
