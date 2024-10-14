using System;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    #region -UI Elements-

    [Header("Score Elements")] 
    [SerializeField] private int currentScore;
    public int CurrentScore => currentScore;
    [SerializeField] private int maxScore;
    public int MaxScore { get => maxScore; set => maxScore = value; }
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Player Health")]
    [SerializeField] private int maxHealth = 4;
    [SerializeField] private int currentHealth;
    [SerializeField] private TextMeshProUGUI healthText;

    #endregion
    
    [SerializeField] private bool isGameOver;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private void Start()
    {
        scoreText.text = $"Score: {currentScore}";
        currentHealth = maxHealth;
        healthText.text = $"Health: {currentHealth}";
    }

    #region -Score Fucntions-

    public void IncreaseScore(int _value)
    {
        currentScore += _value;
        scoreText.text = $"Score: {currentScore}";
    }

    public void DecreaseScore(int _value)
    {
        currentScore -= _value;
        scoreText.text = $"Score: {currentScore}";
    }

    #endregion
    
    #region -Health Fucntions-

    public void IncreaseHealth(int _value)
    {
        currentHealth += _value;
        healthText.text = $"Health: {currentHealth}";
    }

    public void DecreaseHealth(int _value)
    {
        currentHealth -= _value;
        healthText.text = $"Health: {currentHealth}";

        if (currentHealth >= 0)
        {
            GameplayUIManager.Instance.OnGhostAnger?.Invoke();
            
            if (currentHealth <= 0)
            {
                isGameOver = true;
                GameplayUIManager.Instance.OnGameOver();
            }
        }
    }
    
    #endregion

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
