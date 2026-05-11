using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private StageLevelConfig[] levels;

    public void SetLevels(StageLevelConfig[] levelConfigs)
    {
        levels = levelConfigs;
    }

    public void SelectLevel(int levelIndex)
    {
        if (levels == null || levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogWarning($"Cannot select level at index {levelIndex}.");
            return;
        }

        StageLevelConfig selectedLevel = levels[levelIndex];

        if (selectedLevel == null)
        {
            Debug.LogWarning($"Level config at index {levelIndex} is missing.");
            return;
        }

        StageSelection.SelectLevel(selectedLevel);
        SceneManager.LoadScene(selectedLevel.GameplaySceneName);
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
}
