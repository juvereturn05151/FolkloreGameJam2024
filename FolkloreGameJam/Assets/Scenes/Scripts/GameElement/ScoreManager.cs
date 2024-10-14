using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore = 0;
    private int highScore = 0;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;

    private void Awake()
    {
        Debug.Log("Score Manager");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load the saved high score from PlayerPrefs
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);

        // Check if we have a new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);  // Save high score to PlayerPrefs
            PlayerPrefs.Save();  // Ensure the data is saved to disk

            OnHighScoreChanged?.Invoke(highScore);
            Debug.Log("New High Score! Current High Score: " + highScore);
        }
    }

    public void SubtractScore(int points)
    {
        currentScore -= points;
        if (currentScore < 0) currentScore = 0;

        OnScoreChanged?.Invoke(currentScore);
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetHighScore()
    {
        return highScore;
    }

    // Optionally, you can add a method to reset the high score
    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
        OnHighScoreChanged?.Invoke(highScore);
        Debug.Log("High Score has been reset.");
    }
}