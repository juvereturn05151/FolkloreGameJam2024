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
        
        GameManager.Instance.MaxScore = PlayerPrefs.GetInt("HighScore", 0);
        gameOverHighScoreText.text = $"High Score: {GameManager.Instance.MaxScore}";
    }

    public void OnGameOver()
    {
        gameOverPanel.SetActive(true);
        
        var _currentScore = GameManager.Instance.CurrentScore;
        // var _maxScore = GameManager.Instance.MaxScore;
        gameOverScoreText.text = $"Score: {_currentScore}";
        if (_currentScore >= GameManager.Instance.MaxScore)
        {
            GameManager.Instance.MaxScore = _currentScore;
            PlayerPrefs.SetInt("HighScore", GameManager.Instance.MaxScore);
        }
        gameOverHighScoreText.text = $"High Score: {GameManager.Instance.MaxScore}";
    }
}
