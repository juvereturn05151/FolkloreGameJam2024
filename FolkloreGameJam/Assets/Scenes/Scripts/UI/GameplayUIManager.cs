using System;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance;
    
    [Header("Game Over Elements")]
    [SerializeField] private MMF_Player ghostAngerFeedback;
    public UnityAction OnGhostAnger;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;

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

        // Initialize UI with the current score
        UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
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

    public void OnGameOver()
    {
        gameOverPanel.SetActive(true);
        
        var _currentScore = ScoreManager.Instance.GetCurrentScore();
        // var _maxScore = GameManager.Instance.MaxScore;
        gameOverScoreText.text = $"Score: {_currentScore}";
        if (_currentScore >= ScoreManager.Instance.GetHighScore())
        {
            PlayerPrefs.SetInt("HighScore", _currentScore);
        }
        gameOverHighScoreText.text = $"High Score: {_currentScore}";
    }
}
