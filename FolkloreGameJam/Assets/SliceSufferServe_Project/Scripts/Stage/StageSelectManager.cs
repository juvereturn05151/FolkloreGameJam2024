using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private StageLevelDatabase levelDatabase;

    private bool loadingSelectedStage;
    private string pendingSceneName;

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

        StageSelection.SelectLevel(selectedLevel);
        LoadSelectedStageWithFade(StageSelection.GetEntrySceneName(selectedLevel));
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

        StageSelection.SelectLevel(selectedLevel);
        LoadSelectedStageWithFade(StageSelection.GetEntrySceneName(selectedLevel));
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
