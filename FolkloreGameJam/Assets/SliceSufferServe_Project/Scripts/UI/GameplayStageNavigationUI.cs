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

    [Header("Story Cutscenes")]
    [SerializeField] private string cinematicSceneName = "Cinematic";
    [SerializeField] private int postStageCutsceneLevelNumber = 13;
    [SerializeField] private int postStageCutsceneStartIndex = 5;
    [SerializeField] private int postStageCutsceneCount = 4;
    [SerializeField] private bool playPostStageCutsceneOnce = true;
    [SerializeField] private string postStageCutscenePlayedKey = "StoryCutsceneAfterStage13Played";

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
        if (StageSelection.IsClassicMode)
        {
            HideStageNavigationButtons();
            return;
        }

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
        string label;
        if (canGoNext)
        {
            label = IsPostStageCutsceneRequired() ? "Next Cutscene" : IsNextStageTutorialRequired() ? "Next Tutorial" : "Next Stage";
        }
        else
        {
            label = hasOneStar ? "Last Stage" : "Need 1 Star";
        }

        SetNextStageButtonState(canGoNext, label);
    }

    private void HideStageNavigationButtons()
    {
        if (gameOverActionsRoot != null)
        {
            gameOverActionsRoot.gameObject.SetActive(false);
        }

        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(false);
        }

        if (storyModeSelectButton != null)
        {
            storyModeSelectButton.gameObject.SetActive(false);
        }
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

        string nextSceneName = GetNextStageSceneName(nextStage);
        bool playPostStageCutscene = IsPostStageCutsceneRequired();

        StageSelection.SelectLevel(nextStage);
        if (playPostStageCutscene)
        {
            MarkPostStageCutscenePlayed();
            CutsceneManager.SetPlaybackOverride(nextSceneName, postStageCutsceneStartIndex, postStageCutsceneCount);
            SceneManager.LoadScene(cinematicSceneName);
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private string GetNextStageSceneName(StageLevelConfig nextStage)
    {
        return IsSecondTutorialRequired(nextStage)
            ? StageSelection.GetTutorialSceneName(nextStage)
            : StageSelection.GetGameplaySceneName(nextStage);
    }

    private bool IsNextStageTutorialRequired()
    {
        return IsSecondTutorialRequired(GetNextStage());
    }

    private bool IsPostStageCutsceneRequired()
    {
        StageLevelConfig selectedLevel = StageSelection.SelectedLevel;
        if (selectedLevel == null || selectedLevel.LevelNumber != postStageCutsceneLevelNumber || string.IsNullOrWhiteSpace(cinematicSceneName))
        {
            return false;
        }

        return !playPostStageCutsceneOnce || PlayerPrefs.GetInt(postStageCutscenePlayedKey, 0) == 0;
    }

    private void MarkPostStageCutscenePlayed()
    {
        if (!playPostStageCutsceneOnce || string.IsNullOrWhiteSpace(postStageCutscenePlayedKey))
        {
            return;
        }

        PlayerPrefs.SetInt(postStageCutscenePlayedKey, 1);
        PlayerPrefs.Save();
    }

    private bool IsSecondTutorialRequired(StageLevelConfig nextStage)
    {
        if (nextStage == null || levelDatabase == null)
        {
            return false;
        }

        int nextStageIndex = levelDatabase.IndexOf(nextStage);
        return nextStageIndex == 3
            && !SaveSystem.IsTutorialCompleted(StageUnlockSystem.SecondTutorialId);
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
        Debug.Log("Navigating to Leaderboard...");
        Debug.Log("Timescale:" + Time.timeScale);

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
