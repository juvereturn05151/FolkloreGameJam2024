using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayGameOverUI : MonoBehaviour
{
    [Header("Game Over Elements")]
    [SerializeField] 
    private GameObject gameOverPanel;
    [SerializeField] 
    private RectTransform receiptImage;
    [SerializeField] 
    private TextMeshProUGUI gameOverScoreText;
    [SerializeField] 
    private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] 
    private TextMeshProUGUI gameOverStarsText;
    [SerializeField] 
    private TextMeshProUGUI gameOverNextGoalText;
    [SerializeField] 
    private TextMeshProUGUI gameOverCurrencyEarnedText;
    [SerializeField] 
    private Button leaderboardUI;
    [SerializeField]
    private Animator starAnimator;

    [Header("Curtain")]
    [SerializeField] 
    private Animator curtainAnimator;
    [SerializeField] 
    private string curtainCloseParameterName = "Close";
    [SerializeField] 
    private string curtainCloseStateName = "curtain_close";
    [SerializeField] 
    private float curtainCloseFallbackDelay = 1.5f;
    [SerializeField] 
    private GameObject[] objectsToDisableDuringCurtain;

    [Header("Currency Reward")]
    [SerializeField] private int scorePointsPerCurrency = 1;

    private StageGoalResult preparedStageGoalResult;

    public bool IsShowing => gameOverPanel != null && gameOverPanel.activeSelf;

    public void Initialize()
    {
        if (gameOverHighScoreText != null && ScoreManager.Instance != null)
        {
            gameOverHighScoreText.text = $"High Score: {ScoreManager.Instance.GetHighScore()}";
        }
    }

    public StageGoalResult PrepareGameOver()
    {
        int currentScore = ScoreManager.Instance.GetCurrentScore();

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = $"Score: {currentScore}";
        }

        StageGoalResult stageGoalResult = GameManager.Instance.EvaluateAndSaveStageGoal(currentScore);
        preparedStageGoalResult = stageGoalResult;
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

    public IEnumerator CloseCurtainThenShow()
    {
        if (curtainAnimator == null)
        {
            ShowGameOverPanel();
            yield break;
        }

        curtainAnimator.gameObject.SetActive(true);
        curtainAnimator.SetBool(curtainCloseParameterName, true);
        yield return null;

        int closeStateHash = Animator.StringToHash(curtainCloseStateName);
        float elapsedTime = 0f;

        while (elapsedTime < curtainCloseFallbackDelay)
        {
            AnimatorStateInfo stateInfo = curtainAnimator.GetCurrentAnimatorStateInfo(0);
            bool isCloseState = stateInfo.IsName(curtainCloseStateName) || stateInfo.shortNameHash == closeStateHash;
            bool isFinished = isCloseState && !curtainAnimator.IsInTransition(0) && stateInfo.normalizedTime >= 1f;

            if (isFinished)
            {
                break;
            }

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach(GameObject obj in objectsToDisableDuringCurtain)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        foreach (Food food in FindObjectsOfType<Food>())    
        {
            if (food != null)
            {
                Destroy(food.gameObject);
            }
        }

        yield return InterstitialAdManager.ShowGameplayToGameOverAd();

        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        GlobalCurrencyPanel.SetVisible(true);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            TriggerStarAnimation();
        }
    }

    private void TriggerStarAnimation()
    {
        if (starAnimator == null || preparedStageGoalResult == null)
        {
            return;
        }

        switch (Mathf.Clamp(preparedStageGoalResult.Stars, 0, 3))
        {
            case 1:
                starAnimator.SetTrigger("1Star");
                break;
            case 2:
                starAnimator.SetTrigger("2Stars");
                break;
            case 3:
                starAnimator.SetTrigger("3Stars");
                break;
        }
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
