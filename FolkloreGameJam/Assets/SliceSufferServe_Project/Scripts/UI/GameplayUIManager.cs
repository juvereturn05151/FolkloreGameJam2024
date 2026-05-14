using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance;
    
    [Header("Game Over Elements")]
    // [SerializeField] private MMF_Player ghostAngerFeedback;
    public UnityAction OnGhostAnger;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private RectTransform receiptImage;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private TextMeshProUGUI gameOverStarsText;
    [SerializeField] private TextMeshProUGUI gameOverNextGoalText;
    [SerializeField] private TextMeshProUGUI gameOverCurrencyEarnedText;
    [SerializeField] private Button leaderboardUI;

    [Header("Stage Navigation")]
    [SerializeField] private StageLevelDatabase levelDatabase;
    [SerializeField] private string storyModeSelectSceneName = "StoryModeSelect";
    [SerializeField] private RectTransform gameOverActionsRoot;
    [SerializeField] private Button nextStageButton;
    [SerializeField] private TextMeshProUGUI nextStageButtonText;
    [SerializeField] private Button storyModeSelectButton;

    [Header("Currency Reward")]
    [SerializeField] private int scorePointsPerCurrency = 1;
    
    [Header("Gameplay UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image heartImage;
    [SerializeField] private Image clockTimerImage;
    [SerializeField] private Image clockHand;

    [Header("Super Meter UI")]
    [SerializeField] private Slider superMeterSlider;
    [SerializeField] private TextMeshProUGUI superMeterText;
    [SerializeField] private Image superMeterGraphic;
    [SerializeField] private Color superChargingColor = new Color(0.94f, 0.18f, 0.14f, 0.95f);
    [SerializeField] private Color superReadyColor = new Color(1f, 0.75f, 0.12f, 1f);
    [SerializeField] private Color superActiveColor = new Color(0.1f, 0.85f, 1f, 1f);

    [Header("Combo UI")]
    [SerializeField] private GameObject comboRoot;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private GameObject comboSpecialEffectRoot;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private void Start()
    {
        OnGhostAnger += () =>
        {
            if (GameUtility.FeedbackManagerExists()) 
            {
                FeedbackManager.Instance.DamageFeedback.PlayFeedbacks();
                FeedbackManager.Instance.ShakeCameraFeedback(0.5f, 2f);
            }
            // Camera.main.DOShakePosition(0.5f, 2f);
        };
        
        gameOverHighScoreText.text = $"High Score: {ScoreManager.Instance.GetHighScore()}";

        // Subscribe to ScoreManager's score changed event
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        }

        // Subscribe to TimeManager's time changed event
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged += UpdateTimeUI;
            TimeManager.Instance.OnClockChanged += UpdateClockUI;
        }

        if (HPManager.Instance != null)
        {
            HPManager.Instance.OnHealthChanged += UpdateHP;
        }

        ResolveSuperMeterUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSuperMeterChanged += UpdateSuperMeterUI;
            GameManager.Instance.OnSuperActiveTimeChanged += UpdateSuperActiveUI;
            GameManager.Instance.OnSuperActivated += HandleSuperActivated;
            GameManager.Instance.OnSuperEnded += HandleSuperEnded;
            UpdateSuperMeterUI(GameManager.Instance.CurrentSuperMeter, GameManager.Instance.SuperMeterThreshold);
        }

        ComboSystem.OnComboChanged += UpdateComboUI;
        ComboSystem.ResetCombo();

        SetupStageNavigationButtons();
        SetNextStageButtonState(false, "Need 1 Star");

        // Initialize UI with the current score
        UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
        
        heartImage.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= UpdateTimeUI;
            TimeManager.Instance.OnClockChanged -= UpdateClockUI;
        }

        if (HPManager.Instance != null)
        {
            HPManager.Instance.OnHealthChanged -= UpdateHP;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSuperMeterChanged -= UpdateSuperMeterUI;
            GameManager.Instance.OnSuperActiveTimeChanged -= UpdateSuperActiveUI;
            GameManager.Instance.OnSuperActivated -= HandleSuperActivated;
            GameManager.Instance.OnSuperEnded -= HandleSuperEnded;
        }

        ComboSystem.OnComboChanged -= UpdateComboUI;

        if (nextStageButton != null)
        {
            nextStageButton.onClick.RemoveListener(GoToNextStage);
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.onClick.RemoveListener(GoToStoryModeSelect);
        }
    }

    // Callback to update the score UI
    private void UpdateScoreUI(int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }

    // Callback to update the time UI
    private void UpdateTimeUI(string formattedTime)
    {
        //timeText.text = "Time: " + formattedTime;
    }

    private void UpdateClockUI(float currentTime, float maxTime) 
    {
        //clockTimerImage.fillAmount = (currentTime - 18.0f) / (maxTime - 18.0f);
        float normalizedTime = (currentTime - 18.0f) / (maxTime - 18.0f);

        clockTimerImage.fillAmount = normalizedTime;

        // Calculate the angle (0 - 360 degrees) for the clock hand rotation
        float angle = normalizedTime * 360.0f;

        // Rotate the clock hand by the calculated angle
        clockHand.transform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    private void UpdateHP(int hp)
    {
        hpText.text = "HP: " + hp;
    }

    private void ResolveSuperMeterUI()
    {
        Transform content = transform.Find("SuperMeter/Content");
        if (content == null)
        {
            return;
        }

        if (superMeterSlider == null)
        {
            superMeterSlider = content.GetComponent<Slider>();
        }

        if (superMeterGraphic == null)
        {
            superMeterGraphic = content.GetComponent<Image>();
        }

        if (superMeterText == null)
        {
            superMeterText = content.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void UpdateSuperMeterUI(float currentValue, float threshold)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsSuperScoreMultiplierActive)
        {
            return;
        }

        float normalizedValue = threshold <= 0f ? 0f : Mathf.Clamp01(currentValue / threshold);

        if (superMeterSlider != null)
        {
            superMeterSlider.minValue = 0f;
            superMeterSlider.maxValue = Mathf.Max(1f, threshold);
            superMeterSlider.value = Mathf.Clamp(currentValue, superMeterSlider.minValue, superMeterSlider.maxValue);
        }

        SetSuperMeterColor(normalizedValue >= 1f ? superReadyColor : superChargingColor);

        if (superMeterText != null)
        {
            superMeterText.text = normalizedValue >= 1f ? "SUPER READY" : $"SUPER {Mathf.RoundToInt(normalizedValue * 100f)}%";
        }
    }

    private void UpdateSuperActiveUI(float remainingTime, float duration)
    {
        if (superMeterSlider != null)
        {
            superMeterSlider.minValue = 0f;
            superMeterSlider.maxValue = Mathf.Max(0.01f, duration);
            superMeterSlider.value = Mathf.Clamp(remainingTime, superMeterSlider.minValue, superMeterSlider.maxValue);
        }

        SetSuperMeterColor(superActiveColor);

        if (superMeterText != null)
        {
            superMeterText.text = $"SUPER x2 {Mathf.CeilToInt(remainingTime)}s";
        }
    }

    private void HandleSuperActivated()
    {
        Transform target = superMeterText != null ? superMeterText.transform : superMeterSlider != null ? superMeterSlider.transform : null;
        if (target != null)
        {
            target.DOPunchScale(Vector3.one * 0.15f, 0.2f, 4, 0.5f);
        }
    }

    private void HandleSuperEnded()
    {
        if (GameManager.Instance != null)
        {
            UpdateSuperMeterUI(GameManager.Instance.CurrentSuperMeter, GameManager.Instance.SuperMeterThreshold);
        }
    }

    private void SetSuperMeterColor(Color color)
    {
        if (superMeterGraphic != null)
        {
            superMeterGraphic.color = color;
        }
    }

    private void UpdateComboUI(int combo)
    {
        if (comboRoot != null)
        {
            comboRoot.SetActive(combo > 0);
        }

        if (comboText != null)
        {
            comboText.text = $"Combo {combo}  Score x{ComboSystem.GetScoreMultiplier(combo)}";
        }

        if (comboSpecialEffectRoot != null)
        {
            // TODO: Replace this placeholder object with the final combo 10+ special effect.
            comboSpecialEffectRoot.SetActive(ComboSystem.IsSpecialEffectActive);
        }
    }

    public void OnGameOver()
    {
        if (gameOverPanel.activeSelf) 
        {
            return;
        }

        gameOverPanel.SetActive(true);
        // receiptImage.DOScale(new Vector3(120f, 120f), 0.5f).SetEase(Ease.InQuart);
        
        var _currentScore = ScoreManager.Instance.GetCurrentScore();
        // var _maxScore = GameManager.Instance.MaxScore;
        gameOverScoreText.text = $"Score: {_currentScore}";
        StageGoalResult stageGoalResult = GameManager.Instance.EvaluateAndSaveStageGoal(_currentScore);
        UpdateStageGoalUI(stageGoalResult);
        UpdateStageNavigationUI(stageGoalResult);
        int earnedCurrency = CurrencySystem.AwardCurrencyFromScore(_currentScore, scorePointsPerCurrency);
        UpdateCurrencyRewardUI(earnedCurrency);

        if (_currentScore >= ScoreManager.Instance.GetHighScore())
        {
            PlayerPrefs.SetInt("HighScore", _currentScore);

        }
        // gameOverHighScoreText.text = $"High Score: {_currentScore}";
        gameOverHighScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);
        SteamLeaderboardManager.UpdateScore(_currentScore);
        GameManager.Instance.ApplyGameOver();
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
        if (gameOverCurrencyEarnedText == null)
        {
            return;
        }

        gameOverCurrencyEarnedText.text = $"+{earnedCurrency} Currency";
    }

    private void SetupStageNavigationButtons()
    {
        if (gameOverActionsRoot != null)
        {
            gameOverActionsRoot.gameObject.SetActive(false);
        }

        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(false);
        }

        if (nextStageButton != null && nextStageButtonText == null)
        {
            nextStageButtonText = nextStageButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.gameObject.SetActive(false);
        }

        if (nextStageButton != null)
        {
            nextStageButton.onClick.RemoveListener(GoToNextStage);
            nextStageButton.onClick.AddListener(GoToNextStage);
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.onClick.RemoveListener(GoToStoryModeSelect);
            storyModeSelectButton.onClick.AddListener(GoToStoryModeSelect);
        }
    }

    private void UpdateStageNavigationUI(StageGoalResult result)
    {
        if (gameOverActionsRoot != null)
        {
            gameOverActionsRoot.gameObject.SetActive(true);
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.gameObject.SetActive(true);
        }

        bool hasOneStar = result != null && result.Stars >= 1;
        bool hasNextStage = GetNextStage() != null;
        bool canGoNext = hasOneStar && hasNextStage;
        string label = canGoNext ? "Next Stage" : hasOneStar ? "Last Stage" : "Need 1 Star";

        SetNextStageButtonState(canGoNext, label);
    }

    private void SetNextStageButtonState(bool canGoNext, string label)
    {
        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(true);
            nextStageButton.interactable = canGoNext;
        }

        if (nextStageButtonText != null)
        {
            nextStageButtonText.text = label;
        }
    }

    public void GoToNextStage()
    {
        StageLevelConfig nextStage = GetNextStage();
        if (nextStage == null)
        {
            return;
        }

        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlayGameplayBGM();
        }

        StageSelection.SelectLevel(nextStage);
        SceneManager.LoadScene(nextStage.GameplaySceneName);
    }

    public void GoToStoryModeSelect()
    {
        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlayMenuBGM();
        }

        StageSelection.Clear();
        SceneManager.LoadScene(storyModeSelectSceneName);
    }

    private StageLevelConfig GetNextStage()
    {
        StageLevelConfig selectedLevel = StageSelection.SelectedLevel;
        if (selectedLevel == null || levelDatabase == null)
        {
            return null;
        }

        return levelDatabase.GetNextLevel(selectedLevel);
    }

    public void Restart()
    {
        GameManager.Instance.PlayAgain();
    }

    public void GoToLeaderboard() 
    {
        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlayMenuBGM();
        }
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadLeaderboard);
    }

    private void LoadLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }
}
