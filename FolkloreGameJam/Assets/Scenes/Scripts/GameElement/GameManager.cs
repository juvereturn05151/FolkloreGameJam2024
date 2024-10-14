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
        ScoreManager.Instance.AddScore(_value);
    }

    public void DecreaseScore(int _value)
    {
        ScoreManager.Instance.AddScore(_value);
    }

    #endregion

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
