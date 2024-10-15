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
    
    [SerializeField] private bool isGameOver;
    public bool IsGameOver => isGameOver;

    [SerializeField] private MMF_Player increaseScoreFeedback;
    [SerializeField] private MMF_Player decreaseScoreFeedback;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private void Start()
    {

    }

    #region -Score Fucntions-

    public void IncreaseScore(int _value)
    {
        increaseScoreFeedback.PlayFeedbacks();
        ScoreManager.Instance.AddScore(_value);
    }

    public void DecreaseScore(int _value)
    {
        decreaseScoreFeedback.PlayFeedbacks();
        ScoreManager.Instance.AddScore(-_value);
    }

    #endregion

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
