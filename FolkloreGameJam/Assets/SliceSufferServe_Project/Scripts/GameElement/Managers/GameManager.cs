using System;
using System.Collections;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event Action<float, float> OnSuperMeterChanged;
    public event Action<float, float> OnSuperActiveTimeChanged;
    public event Action OnSuperActivated;
    public event Action OnSuperEnded;

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

    [Header("Stage Goal")]
    [SerializeField] private StageGoal stageGoal;

    [Header("Super Meter")]
    [SerializeField] private float superMeterThreshold = 100f;
    [SerializeField] private float superMeterGainPerTrash = 25f;
    [SerializeField] private float shakeActivationThreshold = 2.5f;
    [SerializeField] private float shakeActivationCooldown = 0.75f;
    [SerializeField] private float superScoreMultiplier = 2f;
    [SerializeField] private float superScoreMultiplierDuration = 10f;
    [SerializeField] private UnityEvent superActivated;

    public bool IsTutorial;
    public StageGoal CurrentStageGoal => StageSelection.SelectedLevel != null && StageSelection.SelectedLevel.StageGoal != null
        ? StageSelection.SelectedLevel.StageGoal
        : stageGoal;
    public StageGoalResult LastStageGoalResult { get; private set; }
    public float CurrentSuperMeter => currentSuperMeter;
    public float SuperMeterThreshold => Mathf.Max(1f, superMeterThreshold);
    public bool IsSuperMeterFull => currentSuperMeter >= SuperMeterThreshold;
    public bool IsSuperScoreMultiplierActive => superScoreMultiplierCoroutine != null;
    public float SuperScoreMultiplierDuration => Mathf.Max(0f, superScoreMultiplierDuration);

    private float currentSuperMeter;
    private Vector3 previousAcceleration;
    private float lastShakeActivationTime = -999f;
    private bool hasAccelerationSample;
    private float activeScoreMultiplier = 1f;
    private Coroutine superScoreMultiplierCoroutine;
    private float superScoreMultiplierRemainingTime;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private void Start()
    {
        NotifySuperMeterChanged();
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryActivateSuper();
        }

        UpdateShakeActivation();
    }

    #region -Score Fucntions-

    public void IncreaseScore(int _value)
    {
        // increaseScoreFeedback.PlayFeedbacks();
        if (GameUtility.FeedbackManagerExists()) 
        {
            FeedbackManager.Instance.IncreaseScoreFeedback.PlayFeedbacks();
        }

        int scoreValue = Mathf.RoundToInt(_value * activeScoreMultiplier);
        ScoreManager.Instance.AddScore(scoreValue);
    }

    public void DecreaseScore(int _value)
    {
        // decreaseScoreFeedback.PlayFeedbacks();
        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.5f, 0.25f);
            FeedbackManager.Instance.DecreaseScoreFeedback.PlayFeedbacks();
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

    public StageGoalResult EvaluateAndSaveStageGoal(int score)
    {
        StageGoal activeStageGoal = CurrentStageGoal;

        if (activeStageGoal == null)
        {
            LastStageGoalResult = null;
            return null;
        }

        LastStageGoalResult = activeStageGoal.EvaluateAndSaveBest(score);
        return LastStageGoalResult;
    }

    public void AddSuperMeterFromTrash()
    {
        AddSuperMeter(superMeterGainPerTrash);
    }

    public void AddSuperMeter(float amount)
    {
        if (isGameOver || amount <= 0f)
        {
            return;
        }

        currentSuperMeter = Mathf.Clamp(currentSuperMeter + amount, 0f, SuperMeterThreshold);
        NotifySuperMeterChanged();
    }

    public bool TryActivateSuper()
    {
        if (isGameOver || !IsSuperMeterFull)
        {
            return false;
        }

        currentSuperMeter = 0f;
        NotifySuperMeterChanged();

        superActivated?.Invoke();
        OnSuperActivated?.Invoke();
        StartSuperScoreMultiplier();

        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.35f, 0.75f);
        }

        return true;
    }

    private void UpdateShakeActivation()
    {
        Vector3 acceleration = Input.acceleration;
        if (!hasAccelerationSample)
        {
            previousAcceleration = acceleration;
            hasAccelerationSample = true;
            return;
        }

        float shakeAmount = (acceleration - previousAcceleration).magnitude;
        previousAcceleration = acceleration;

        if (shakeAmount < shakeActivationThreshold)
        {
            return;
        }

        if (Time.unscaledTime - lastShakeActivationTime < shakeActivationCooldown)
        {
            return;
        }

        if (TryActivateSuper())
        {
            lastShakeActivationTime = Time.unscaledTime;
        }
    }

    private void NotifySuperMeterChanged()
    {
        OnSuperMeterChanged?.Invoke(currentSuperMeter, SuperMeterThreshold);
    }

    private void StartSuperScoreMultiplier()
    {
        if (superScoreMultiplierCoroutine != null)
        {
            StopCoroutine(superScoreMultiplierCoroutine);
        }

        superScoreMultiplierCoroutine = StartCoroutine(SuperScoreMultiplierCoroutine());
    }

    private IEnumerator SuperScoreMultiplierCoroutine()
    {
        activeScoreMultiplier = Mathf.Max(1f, superScoreMultiplier);
        float duration = SuperScoreMultiplierDuration;
        superScoreMultiplierRemainingTime = duration;
        OnSuperActiveTimeChanged?.Invoke(superScoreMultiplierRemainingTime, duration);

        while (superScoreMultiplierRemainingTime > 0f)
        {
            superScoreMultiplierRemainingTime = Mathf.Max(0f, superScoreMultiplierRemainingTime - Time.deltaTime);
            OnSuperActiveTimeChanged?.Invoke(superScoreMultiplierRemainingTime, duration);
            yield return null;
        }

        activeScoreMultiplier = 1f;
        superScoreMultiplierCoroutine = null;
        OnSuperEnded?.Invoke();
        NotifySuperMeterChanged();
    }
}
