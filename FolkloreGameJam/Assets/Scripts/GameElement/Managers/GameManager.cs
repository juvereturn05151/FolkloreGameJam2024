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

    public enum GameState
    {
        StartGame,
        EndGame,
        Stop
    }

    private GameState state;

    public GameState State
    {
        get => state;
        set => state = value;
    }

    [SerializeField] private bool isGameOver;
    public bool IsGameOver => isGameOver;
    [SerializeField] private HumanGenerator humanGen1;
    [SerializeField] private HumanGenerator humanGen2;
    [SerializeField] private CustomerGenerator customerGen;

    public bool IsTutorial;

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
        // increaseScoreFeedback.PlayFeedbacks();
        if (GameUtility.FeedbackManagerExists()) 
        {
            FeedbackManager.Instance.increaseScoreFeedback.PlayFeedbacks();
        }

        ScoreManager.Instance.AddScore(_value);
    }

    public void DecreaseScore(int _value)
    {
        // decreaseScoreFeedback.PlayFeedbacks();
        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.5f, 0.25f);
            FeedbackManager.Instance.decreaseScoreFeedback.PlayFeedbacks();
        }
        ScoreManager.Instance.AddScore(-_value);
    }

    #endregion

    public void PlayAgain()
    {
        if (GameUtility.SoundManagerExists()) 
        {
            SoundManager.instance.PlayGameplayBGM();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ApplyGameOver() 
    {
        isGameOver = true;

        if (humanGen1 != null) 
        {
            humanGen1.gameObject.SetActive(false);
        }

        if (humanGen2 != null) 
        {
            humanGen2.gameObject.SetActive(false);
        }

        if (customerGen != null) 
        {
            customerGen.gameObject.SetActive(false);
        }
    }
}
