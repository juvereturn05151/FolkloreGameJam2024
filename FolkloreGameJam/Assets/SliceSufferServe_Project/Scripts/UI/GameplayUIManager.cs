using UnityEngine;
using UnityEngine.Events;

public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance;
    
    public UnityAction OnGhostAnger;

    [SerializeField] private GameplayGameOverUI gameOverUI;
    [SerializeField] private GameplayHUDUI gameplayHUDUI;
    [SerializeField] private GameplaySuperComboUI superComboUI;
    [SerializeField] private GameplayStageNavigationUI stageNavigationUI;

    private bool gameOverSequenceStarted;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;

        ResolveSplitComponents();
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
        
        gameOverUI?.Initialize();

        gameplayHUDUI?.Bind();
        superComboUI?.BindSuperMeter();
        superComboUI?.BindCombo();

        stageNavigationUI?.SetupButtons();
        stageNavigationUI?.SetNextStageButtonState(false, "Need 1 Star");

        gameplayHUDUI?.Initialize();
    }

    private void OnDestroy()
    {
        gameplayHUDUI?.Unbind();
        superComboUI?.UnbindSuperMeter();
        superComboUI?.UnbindCombo();
        stageNavigationUI?.UnbindButtons();
    }

    public void OnGameOver()
    {
        if (gameOverUI == null || gameOverSequenceStarted || gameOverUI.IsShowing)
        {
            return;
        }

        gameOverSequenceStarted = true;
        StageGoalResult stageGoalResult = gameOverUI.PrepareGameOver();
        stageNavigationUI?.UpdateAfterGameOver(stageGoalResult);
        GameManager.Instance.ApplyGameOver();
        StartCoroutine(gameOverUI.CloseCurtainThenShow());
    }

    public void GoToNextStage()
    {
        stageNavigationUI?.GoToNextStage();
    }

    public void GoToStoryModeSelect()
    {
        stageNavigationUI?.GoToStoryModeSelect();
    }

    public void Restart()
    {
        stageNavigationUI?.Restart();
    }

    public void GoToLeaderboard()
    {
        stageNavigationUI?.GoToLeaderboard();
    }

    private void ResolveSplitComponents()
    {
        if (gameOverUI == null)
        {
            gameOverUI = GetComponent<GameplayGameOverUI>();
        }

        if (gameplayHUDUI == null)
        {
            gameplayHUDUI = GetComponent<GameplayHUDUI>();
        }

        if (superComboUI == null)
        {
            superComboUI = GetComponent<GameplaySuperComboUI>();
        }

        if (stageNavigationUI == null)
        {
            stageNavigationUI = GetComponent<GameplayStageNavigationUI>();
        }
    }
}
