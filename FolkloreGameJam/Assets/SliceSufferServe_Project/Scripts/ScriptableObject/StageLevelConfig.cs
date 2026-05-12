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
    [SerializeField] private Ghost[] allowedGhosts = Array.Empty<Ghost>();
    [SerializeField] private StageSpawnPhase[] spawnPhases =
    {
        new StageSpawnPhase(0f, 20f, 1.5f, 0.8f, 0f, new[] { FoodState.Normal }),
        new StageSpawnPhase(20f, 40f, 1.5f, 1f, 0.35f, new[] { FoodState.Normal, FoodState.MediumRotten }),
        new StageSpawnPhase(40f, 60f, 0.8f, 1.25f, 0f, new[] { FoodState.Normal, FoodState.MediumRotten, FoodState.SuperRotten })
    };
    [SerializeField] private float humanSpawnDelayAfterGhost = 0.35f;

    public string LevelId => string.IsNullOrWhiteSpace(levelId) ? name : levelId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public int LevelNumber => Mathf.Max(1, levelNumber);
    public string GameplaySceneName => string.IsNullOrWhiteSpace(gameplaySceneName) ? "GameplayScene" : gameplaySceneName;
    public StageGoal StageGoal => stageGoal;
    public HumanBodyPartType[] EnabledBodyParts => enabledBodyParts;
    public Ghost[] AllowedGhosts => allowedGhosts;
    public StageSpawnPhase[] SpawnPhases => spawnPhases;
    public float HumanSpawnDelayAfterGhost => Mathf.Max(0f, humanSpawnDelayAfterGhost);

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
        humanSpawnDelayAfterGhost = Mathf.Max(0f, humanSpawnDelayAfterGhost);

        if (spawnPhases == null || spawnPhases.Length == 0)
        {
            return;
        }

        for (int i = 0; i < spawnPhases.Length; i++)
        {
            if (spawnPhases[i] != null)
            {
                spawnPhases[i].Validate();
            }
        }
    }
}

[Serializable]
public class StageSpawnPhase
{
    [SerializeField] private float startTime;
    [SerializeField] private float endTime = 20f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float humanSpeedMultiplier = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float doubleSpawnChance;
    [SerializeField] private float doubleSpawnDelay = 0.25f;
    [SerializeField] private FoodState[] allowedFoodStates =
    {
        FoodState.Normal,
        FoodState.MediumRotten,
        FoodState.SuperRotten
    };

    public StageSpawnPhase(float startTime, float endTime, float spawnInterval, float humanSpeedMultiplier, float doubleSpawnChance)
        : this(startTime, endTime, spawnInterval, humanSpeedMultiplier, doubleSpawnChance, null)
    {
    }

    public StageSpawnPhase(float startTime, float endTime, float spawnInterval, float humanSpeedMultiplier, float doubleSpawnChance, FoodState[] allowedFoodStates)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.spawnInterval = spawnInterval;
        this.humanSpeedMultiplier = humanSpeedMultiplier;
        this.doubleSpawnChance = doubleSpawnChance;
        this.allowedFoodStates = allowedFoodStates;
    }

    public float StartTime => startTime;
    public float EndTime => endTime;
    public float SpawnInterval => Mathf.Max(0.05f, spawnInterval);
    public float HumanSpeedMultiplier => Mathf.Max(0.01f, humanSpeedMultiplier);
    public float DoubleSpawnChance => Mathf.Clamp01(doubleSpawnChance);
    public float DoubleSpawnDelay => Mathf.Max(0f, doubleSpawnDelay);
    public FoodState[] AllowedFoodStates => allowedFoodStates;

    public bool Contains(float elapsedTime)
    {
        return elapsedTime >= startTime && elapsedTime < endTime;
    }

    public void Validate()
    {
        startTime = Mathf.Max(0f, startTime);
        endTime = Mathf.Max(startTime, endTime);
        spawnInterval = Mathf.Max(0.05f, spawnInterval);
        humanSpeedMultiplier = Mathf.Max(0.01f, humanSpeedMultiplier);
        doubleSpawnChance = Mathf.Clamp01(doubleSpawnChance);
        doubleSpawnDelay = Mathf.Max(0f, doubleSpawnDelay);

        if (allowedFoodStates == null || allowedFoodStates.Length == 0)
        {
            allowedFoodStates = new[]
            {
                FoodState.Normal,
                FoodState.MediumRotten,
                FoodState.SuperRotten
            };
        }
    }
}

public enum HumanBodyPartType
{
    Head,
    Neck,
    Body,
    Leg
}
