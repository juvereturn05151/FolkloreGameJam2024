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
    [SerializeField] private bool allowSuperMeter;
    [SerializeField] private HumanBodyPartType[] enabledBodyParts = Array.Empty<HumanBodyPartType>();
    [Tooltip("Zero-based indexes from CustomerGenerator customer spots. In the current 3-plate layout, 1 is the middle plate.")]
    [SerializeField] private int[] activeCustomerSpotIndexes = { 0, 1, 2 };
    [SerializeField] private Ghost[] allowedGhosts = Array.Empty<Ghost>();
    [Tooltip("Optional level-specific human prefab list. Empty means use the HumanGenerator's default list.")]
    [SerializeField] private GameObject[] humanPrefabOverrides = Array.Empty<GameObject>();
    [SerializeField]
    private StageSpawnPhase[] spawnPhases =
    {
        new StageSpawnPhase(0f, 20f, 1.5f, 0.8f, 0f, new[] { FoodState.Normal }),
        new StageSpawnPhase(20f, 40f, 1.5f, 1f, 0.35f, new[] { FoodState.Normal, FoodState.MediumRotten }),
        new StageSpawnPhase(40f, 60f, 0.8f, 1.25f, 0f, new[] { FoodState.Normal, FoodState.MediumRotten, FoodState.SuperRotten })
    };
    [Tooltip("Multiplies spawned customers' patience. Values below 1 create faster, hotter-headed customers.")]
    [SerializeField] private float customerPatienceMultiplier = 1f;
    [SerializeField, HideInInspector] private int customerMinOrderCount;
    [SerializeField, HideInInspector] private int customerMaxOrderCount;
    [SerializeField] private float humanSpawnDelayAfterGhost = 0.35f;
    [SerializeField] private float duration = 180f;

    public string LevelId => string.IsNullOrWhiteSpace(levelId) ? name : levelId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public int LevelNumber => Mathf.Max(1, levelNumber);
    public string GameplaySceneName => string.IsNullOrWhiteSpace(gameplaySceneName) ? "GameplayScene" : gameplaySceneName;
    public StageGoal StageGoal => stageGoal;
    public bool AllowSuperMeter => allowSuperMeter;
    public HumanBodyPartType[] EnabledBodyParts => enabledBodyParts;
    public int[] ActiveCustomerSpotIndexes => activeCustomerSpotIndexes;
    public Ghost[] AllowedGhosts => allowedGhosts;
    public GameObject[] HumanPrefabOverrides => humanPrefabOverrides;
    public StageSpawnPhase[] SpawnPhases => spawnPhases;
    public float CustomerPatienceMultiplier => Mathf.Max(0.01f, customerPatienceMultiplier);
    public float HumanSpawnDelayAfterGhost => Mathf.Max(0f, humanSpawnDelayAfterGhost);
    public float Duration => Mathf.Max(0f, duration);

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

    public bool IsCustomerSpotEnabled(int customerSpotIndex)
    {
        if (activeCustomerSpotIndexes == null || activeCustomerSpotIndexes.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < activeCustomerSpotIndexes.Length; i++)
        {
            if (activeCustomerSpotIndexes[i] == customerSpotIndex)
            {
                return true;
            }
        }

        return false;
    }

    private void OnValidate()
    {
        levelNumber = Mathf.Max(1, levelNumber);
        customerPatienceMultiplier = Mathf.Max(0.01f, customerPatienceMultiplier);
        customerMinOrderCount = Mathf.Max(0, customerMinOrderCount);
        customerMaxOrderCount = Mathf.Max(customerMinOrderCount, customerMaxOrderCount);
        humanSpawnDelayAfterGhost = Mathf.Max(0f, humanSpawnDelayAfterGhost);

        if (activeCustomerSpotIndexes != null)
        {
            for (int i = 0; i < activeCustomerSpotIndexes.Length; i++)
            {
                activeCustomerSpotIndexes[i] = Mathf.Clamp(activeCustomerSpotIndexes[i], 0, 2);
            }
        }

        if (spawnPhases == null || spawnPhases.Length == 0)
        {
            return;
        }

        bool hasLegacyOrderCountOverride = customerMaxOrderCount > 0;
        bool hasPhaseOrderCountOverride = false;
        for (int i = 0; i < spawnPhases.Length; i++)
        {
            if (spawnPhases[i] != null)
            {
                if (spawnPhases[i].HasCustomerOrderCountOverride)
                {
                    hasPhaseOrderCountOverride = true;
                }

                spawnPhases[i].Validate();
            }
        }

        if (hasLegacyOrderCountOverride && !hasPhaseOrderCountOverride)
        {
            for (int i = 0; i < spawnPhases.Length; i++)
            {
                if (spawnPhases[i] != null)
                {
                    spawnPhases[i].SetCustomerOrderCountRange(customerMinOrderCount, customerMaxOrderCount);
                }
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
    [Tooltip("Optional minimum number of foods a customer can order during this phase. Set both min and max to 0 to use customer prefab appetite.")]
    [SerializeField] private int customerMinOrderCount;
    [Tooltip("Optional maximum number of foods a customer can order during this phase. Set both min and max to 0 to use customer prefab appetite.")]
    [SerializeField] private int customerMaxOrderCount;
    [SerializeField]
    private FoodState[] allowedFoodStates =
    {
        FoodState.Normal,
        FoodState.MediumRotten,
        FoodState.SuperRotten
    };
    [Tooltip("Optional human prefab list for this phase. Falls back to level config overrides, then HumanGenerator defaults.")]
    [SerializeField] private GameObject[] humanPrefabOverrides = Array.Empty<GameObject>();

    public StageSpawnPhase(float startTime, float endTime, float spawnInterval, float humanSpeedMultiplier, float doubleSpawnChance)
        : this(startTime, endTime, spawnInterval, humanSpeedMultiplier, doubleSpawnChance, null, null)
    {
    }

    public StageSpawnPhase(float startTime, float endTime, float spawnInterval, float humanSpeedMultiplier, float doubleSpawnChance, FoodState[] allowedFoodStates)
        : this(startTime, endTime, spawnInterval, humanSpeedMultiplier, doubleSpawnChance, allowedFoodStates, null)
    {
    }

    public StageSpawnPhase(float startTime, float endTime, float spawnInterval, float humanSpeedMultiplier, float doubleSpawnChance, FoodState[] allowedFoodStates, GameObject[] humanPrefabOverrides)
        : this(startTime, endTime, spawnInterval, humanSpeedMultiplier, doubleSpawnChance, 0, 0, allowedFoodStates, humanPrefabOverrides)
    {
    }

    public StageSpawnPhase(float startTime, float endTime, float spawnInterval, float humanSpeedMultiplier, float doubleSpawnChance, int customerMinOrderCount, int customerMaxOrderCount, FoodState[] allowedFoodStates, GameObject[] humanPrefabOverrides)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.spawnInterval = spawnInterval;
        this.humanSpeedMultiplier = humanSpeedMultiplier;
        this.doubleSpawnChance = doubleSpawnChance;
        this.customerMinOrderCount = customerMinOrderCount;
        this.customerMaxOrderCount = customerMaxOrderCount;
        this.allowedFoodStates = allowedFoodStates;
        this.humanPrefabOverrides = humanPrefabOverrides ?? Array.Empty<GameObject>();
    }

    public float StartTime => startTime;
    public float EndTime => endTime;
    public float SpawnInterval => Mathf.Max(0.05f, spawnInterval);
    public float HumanSpeedMultiplier => Mathf.Max(0.01f, humanSpeedMultiplier);
    public float DoubleSpawnChance => Mathf.Clamp01(doubleSpawnChance);
    public float DoubleSpawnDelay => Mathf.Max(0f, doubleSpawnDelay);
    public int CustomerMinOrderCount => Mathf.Max(0, customerMinOrderCount);
    public int CustomerMaxOrderCount => Mathf.Max(0, customerMaxOrderCount);
    public FoodState[] AllowedFoodStates => allowedFoodStates;
    public GameObject[] HumanPrefabOverrides => humanPrefabOverrides;
    public bool HasCustomerOrderCountOverride => CustomerMaxOrderCount > 0;
    public bool HasHumanPrefabOverrides => humanPrefabOverrides != null && humanPrefabOverrides.Length > 0;

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
        customerMinOrderCount = Mathf.Max(0, customerMinOrderCount);
        customerMaxOrderCount = Mathf.Max(customerMinOrderCount, customerMaxOrderCount);

        if (humanPrefabOverrides == null)
        {
            humanPrefabOverrides = Array.Empty<GameObject>();
        }

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

    public void SetCustomerOrderCountRange(int minCount, int maxCount)
    {
        customerMinOrderCount = Mathf.Max(0, minCount);
        customerMaxOrderCount = Mathf.Max(customerMinOrderCount, maxCount);
    }
}

public enum HumanBodyPartType
{
    Head,
    Neck,
    Stomach,
    Leg
}
