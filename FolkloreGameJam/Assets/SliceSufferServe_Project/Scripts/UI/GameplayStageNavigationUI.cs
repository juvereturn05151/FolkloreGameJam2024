using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayStageNavigationUI : MonoBehaviour
{
    [Header("Stage Navigation")]
    [SerializeField] private StageLevelDatabase levelDatabase;
    [SerializeField] private string storyModeSelectSceneName = "StoryModeSelect";
    [SerializeField] private RectTransform gameOverActionsRoot;
    [SerializeField] private Button nextStageButton;
    [SerializeField] private TextMeshProUGUI nextStageButtonText;
    [SerializeField] private Button storyModeSelectButton;

    public void SetupButtons()
    {
        if (gameOverActionsRoot != null)
        {
            gameOverActionsRoot.gameObject.SetActive(false);
        }

        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(false);
        }

        if (nextStageButton != null && nextStageButtonText == null)
        {
            nextStageButtonText = nextStageButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.gameObject.SetActive(false);
        }

        if (nextStageButton != null)
        {
            nextStageButton.onClick.RemoveListener(GoToNextStage);
            nextStageButton.onClick.AddListener(GoToNextStage);
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.onClick.RemoveListener(GoToStoryModeSelect);
            storyModeSelectButton.onClick.AddListener(GoToStoryModeSelect);
        }
    }

    public void UnbindButtons()
    {
        if (nextStageButton != null)
        {
            nextStageButton.onClick.RemoveListener(GoToNextStage);
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.onClick.RemoveListener(GoToStoryModeSelect);
        }
    }

    public void UpdateAfterGameOver(StageGoalResult result)
    {
        if (gameOverActionsRoot != null)
        {
            gameOverActionsRoot.gameObject.SetActive(true);
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.gameObject.SetActive(true);
        }

        bool hasOneStar = result != null && result.Stars >= 1;
        bool hasNextStage = GetNextStage() != null;
        bool canGoNext = hasOneStar && hasNextStage;
        string label = canGoNext ? "Next Stage" : hasOneStar ? "Last Stage" : "Need 1 Star";

        SetNextStageButtonState(canGoNext, label);
    }

    public void SetNextStageButtonState(bool canGoNext, string label)
    {
        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(true);
            nextStageButton.interactable = canGoNext;
        }

        if (nextStageButtonText != null)
        {
            nextStageButtonText.text = label;
        }
    }

    public void GoToNextStage()
    {
        StageLevelConfig nextStage = GetNextStage();
        if (nextStage == null)
        {
            return;
        }

        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlayGameplayBGM();
        }

        StageSelection.SelectLevel(nextStage);
        SceneManager.LoadScene(StageSelection.GetEntrySceneName(nextStage));
    }

    public void GoToStoryModeSelect()
    {
        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlayMenuBGM();
        }

        StageSelection.Clear();
        SceneManager.LoadScene(storyModeSelectSceneName);
    }

    private StageLevelConfig GetNextStage()
    {
        StageLevelConfig selectedLevel = StageSelection.SelectedLevel;
        if (selectedLevel == null || levelDatabase == null)
        {
            return null;
        }

        return levelDatabase.GetNextLevel(selectedLevel);
    }

    public void Restart()
    {
        GameManager.Instance.PlayAgain();
    }

    public void GoToLeaderboard()
    {
        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlayMenuBGM();
        }

        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadLeaderboard);
    }

    private void LoadLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }
}
