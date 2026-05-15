using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private StageLevelDatabase levelDatabase;

    public void SetLevelDatabase(StageLevelDatabase database)
    {
        levelDatabase = database;
    }

    public void SelectLevel(int levelIndex)
    {
        StageLevelConfig selectedLevel = GetLevel(levelIndex);

        if (selectedLevel == null)
        {
            Debug.LogWarning($"Cannot select level at index {levelIndex}.");
            return;
        }

        StageSelection.SelectLevel(selectedLevel);
        SceneManager.LoadScene(StageSelection.GetEntrySceneName(selectedLevel));
    }

    public void SelectTutorialForLevel(int levelIndex)
    {
        StageLevelConfig selectedLevel = GetLevel(levelIndex);

        if (selectedLevel == null)
        {
            Debug.LogWarning($"Cannot select tutorial for level at index {levelIndex}.");
            return;
        }

        StageSelection.SelectLevel(selectedLevel);
        SceneManager.LoadScene(StageSelection.GetEntrySceneName(selectedLevel));
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
}
