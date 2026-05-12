using UnityEngine;

[CreateAssetMenu(fileName = "StageLevelDatabase", menuName = "Scriptable Objects/Stage Level Database")]
public class StageLevelDatabase : ScriptableObject
{
    [SerializeField] private StageLevelConfig[] levels = System.Array.Empty<StageLevelConfig>();

    public StageLevelConfig[] Levels => levels;

    public int Count => levels == null ? 0 : levels.Length;

    public StageLevelConfig GetLevel(int index)
    {
        if (levels == null || index < 0 || index >= levels.Length)
        {
            return null;
        }

        return levels[index];
    }

    public StageLevelConfig GetNextLevel(StageLevelConfig currentLevel)
    {
        int currentIndex = IndexOf(currentLevel);
        if (currentIndex < 0)
        {
            return null;
        }

        return GetLevel(currentIndex + 1);
    }

    public int IndexOf(StageLevelConfig levelConfig)
    {
        if (levelConfig == null || levels == null)
        {
            return -1;
        }

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] == levelConfig)
            {
                return i;
            }
        }

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && levels[i].LevelNumber == levelConfig.LevelNumber)
            {
                return i;
            }
        }

        return -1;
    }
}
