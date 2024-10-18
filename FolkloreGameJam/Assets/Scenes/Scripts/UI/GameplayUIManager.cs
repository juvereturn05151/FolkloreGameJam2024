using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance;
    
    [Header("Game Over Elements")]
    [SerializeField] private MMF_Player ghostAngerFeedback;
    public UnityAction OnGhostAnger;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private RectTransform receiptImage;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    
    [Header("Gameplay UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image heartImage;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private void Start()
    {
        OnGhostAnger += () =>
        {
            ghostAngerFeedback.PlayFeedbacks();
            Camera.main.DOShakePosition(0.5f, 2f);
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
        }

        if (HPManager.Instance != null) 
        {
            HPManager.Instance.OnHealthChanged += UpdateHP;
        }

        // Initialize UI with the current score
        UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
        
        heartImage.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo);
        heartImage.rectTransform.DOShakePosition(0.5f, 5f).SetLoops(-1, LoopType.Yoyo);
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
        }

        if (HPManager.Instance != null)
        {
            HPManager.Instance.OnHealthChanged -= UpdateHP;
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
        timeText.text = "Time: " + formattedTime;
    }

    private void UpdateHP(int hp)
    {
        hpText.text = "HP: " + hp;
    }

    public void OnGameOver()
    {
        gameOverPanel.SetActive(true);
        receiptImage.DOScale(new Vector3(120f, 120f), 0.5f).SetEase(Ease.InQuart);
        
        var _currentScore = ScoreManager.Instance.GetCurrentScore();
        // var _maxScore = GameManager.Instance.MaxScore;
        gameOverScoreText.text = $"Score: {_currentScore}";
        if (_currentScore >= ScoreManager.Instance.GetHighScore())
        {
            PlayerPrefs.SetInt("HighScore", _currentScore);
        }
        // gameOverHighScoreText.text = $"High Score: {_currentScore}";
        gameOverHighScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);

        GameManager.Instance.ApplyGameOver();
    }

    public void Restart() 
    {
        GameManager.Instance.PlayAgain();
    }
}
