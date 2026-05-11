using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StageLevelConfig", menuName = "Scriptable Objects/Stage Level Config")]
public class StageLevelConfig : ScriptableObject
{
    [SerializeField] private string levelId;
    [SerializeField] private string displayName;
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string gameplaySceneName = "GameplayScene";
    [SerializeField] private StageGoal stageGoal;
    [SerializeField] private HumanBodyPartType[] enabledBodyParts = Array.Empty<HumanBodyPartType>();

    public string LevelId => string.IsNullOrWhiteSpace(levelId) ? name : levelId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public int LevelNumber => Mathf.Max(1, levelNumber);
    public string GameplaySceneName => string.IsNullOrWhiteSpace(gameplaySceneName) ? "GameplayScene" : gameplaySceneName;
    public StageGoal StageGoal => stageGoal;
    public HumanBodyPartType[] EnabledBodyParts => enabledBodyParts;

    public bool IsBodyPartEnabled(HumanBodyPartType bodyPartType)
    {
        if (enabledBodyParts == null || enabledBodyParts.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < enabledBodyParts.Length; i++)
        {
            if (enabledBodyParts[i] == bodyPartType)
            {
                return true;
            }
        }

        return false;
    }

    private void OnValidate()
    {
        levelNumber = Mathf.Max(1, levelNumber);
    }
}

public enum HumanBodyPartType
{
    Head,
    Neck,
    Body,
    Leg
}
