using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private StageLevelDatabase levelDatabase;
    [SerializeField] private string cutsceneSceneName = "Cinematic";

    private bool loadingSelectedStage;
    private string pendingSceneName;

    public StageLevelDatabase LevelDatabase => levelDatabase;

    public void SetLevelDatabase(StageLevelDatabase database)
    {
        levelDatabase = database;
    }

    public void SelectLevel(int levelIndex)
    {
        if (loadingSelectedStage)
        {
            return;
        }

        StageLevelConfig selectedLevel = GetLevel(levelIndex);

        if (selectedLevel == null)
        {
            Debug.LogWarning($"Cannot select level at index {levelIndex}.");
            return;
        }

        if (!StageUnlockSystem.IsLevelUnlocked(levelDatabase, levelIndex))
        {
            Debug.LogWarning($"Level at index {levelIndex} is locked.");
            return;
        }

        StageSelection.SelectLevel(selectedLevel);
        LoadSelectedStageWithFade(StageSelection.GetGameplaySceneName(selectedLevel));
    }

    public void SelectTutorialForLevel(int levelIndex)
    {
        if (loadingSelectedStage)
        {
            return;
        }

        StageLevelConfig selectedLevel = GetLevel(levelIndex);

        if (selectedLevel == null)
        {
            Debug.LogWarning($"Cannot select tutorial for level at index {levelIndex}.");
            return;
        }

        if (!StageUnlockSystem.IsTutorialUnlocked(levelDatabase, levelIndex))
        {
            Debug.LogWarning($"Tutorial for level index {levelIndex} is locked.");
            return;
        }

        StageSelection.SelectLevel(selectedLevel);
        LoadSelectedStageWithFade(StageSelection.GetTutorialSceneName(selectedLevel));
    }

    public void SelectCutsceneBeforeTutorial(int levelIndex)
    {
        if (loadingSelectedStage)
        {
            return;
        }

        StageLevelConfig selectedLevel = GetLevel(levelIndex);

        if (selectedLevel == null)
        {
            Debug.LogWarning($"Cannot select cutscene for level at index {levelIndex}.");
            return;
        }

        if (!StageUnlockSystem.IsTutorialUnlocked(levelDatabase, levelIndex))
        {
            Debug.LogWarning($"Cutscene for level index {levelIndex} is locked.");
            return;
        }

        StageSelection.SelectLevel(selectedLevel);
        CutsceneManager.SetNextSceneNameOverride(StageSelection.GetTutorialSceneName(selectedLevel));
        LoadSelectedStageWithFade(cutsceneSceneName);
    }

    public void SelectLevelOne()
    {
        SelectLevel(0);
    }

    public void SelectLevelTwo()
    {
        SelectLevel(1);
    }

    public void SelectLevelThree()
    {
        SelectLevel(2);
    }

    private StageLevelConfig GetLevel(int levelIndex)
    {
        if (levelDatabase == null || levelIndex < 0 || levelIndex >= levelDatabase.Count)
        {
            return null;
        }

        return levelDatabase.GetLevel(levelIndex);
    }

    private void LoadSelectedStageWithFade(string sceneName)
    {
        loadingSelectedStage = true;
        pendingSceneName = sceneName;

        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadPendingScene);
    }

    private void LoadPendingScene()
    {
        SceneManager.LoadScene(pendingSceneName);
    }
}
