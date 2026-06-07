using UnityEngine;

public class HumanGenerator : MonoBehaviour
{
    [SerializeField]
    private HumanBody[] humanPrefabs; // Array of human prefabs
    [SerializeField]
    private Transform spawnPoint; // Where the human will be spawned
    [SerializeField]
    private float firstSpawnTime = 2.0f;
    [SerializeField]
    private float spawnInterval = 5f; // Time in seconds before trying to spawn again

    [SerializeField]
    private GameObject[] defaultHumanPrefabs; // Default prefabs to use if no overrides are specified

    private float spawnTimer; // Timer to track spawn interval
    private StageLevelConfig levelConfig;
    private bool isExternallyControlled;
    private HumanBody tutorialOnlyHumanPrefab;
    private HumanBody[] originalHumanPrefabs;
    private bool hasTutorialHumanPrefabOverride;
    private StageHumanPrefabSpawnEntry[] levelHumanPrefabOverrides; // Level-config overrides, resolved once at Start
    private CharacterCustomizationManager customizationManager;

    void Start()
    {
        levelConfig = StageSelection.SelectedLevel;
        levelHumanPrefabOverrides = ResolveHumanPrefabOverrides(levelConfig, null);
        spawnTimer = firstSpawnTime;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsRapidSliceEventActive)
        {
            return;
        }

        if (isExternallyControlled && !IsTutorialActive())
        {
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnHuman(1f);
            Debug.Log("das");
            spawnTimer = spawnInterval;
        }
    }

    public void SetExternallyControlled(bool controlled)
    {
        isExternallyControlled = controlled;
    }

    public void SetTutorialOnlyHuman(HumanBody humanPrefab)
    {
        if (!hasTutorialHumanPrefabOverride)
        {
            originalHumanPrefabs = humanPrefabs;
            hasTutorialHumanPrefabOverride = true;
        }

        tutorialOnlyHumanPrefab = humanPrefab;
        humanPrefabs = humanPrefab == null ? null : new[] { humanPrefab };
    }

    public void ClearTutorialOnlyHuman()
    {
        tutorialOnlyHumanPrefab = null;
        if (hasTutorialHumanPrefabOverride)
        {
            humanPrefabs = originalHumanPrefabs;
            originalHumanPrefabs = null;
            hasTutorialHumanPrefabOverride = false;
        }
    }

    private bool IsTutorialActive()
    {
        return GameManager.Instance != null && GameManager.Instance.IsTutorial;
    }

    /// <summary>
    /// Spawns a human using the three-tier prefab priority:
    /// phase overrides > level config overrides > humanPrefabs serialized list.
    /// </summary>
    public void SpawnHuman(float movementSpeedMultiplier, StageHumanPrefabSpawnEntry[] phaseOverrides = null)
    {
        HumanBody spawnedHuman = SpawnConfiguredHuman(phaseOverrides);

        if (spawnedHuman == null)
        {
            return;
        }

        spawnedHuman.ApplyLevelConfig(levelConfig);
        ApplyHumanCustomization(spawnedHuman);
        spawnedHuman.ApplyMovementSpeedMultiplier(movementSpeedMultiplier);
    }

    public void SpawnSpecificHuman(HumanBody humanPrefab, float movementSpeedMultiplier = 1f)
    {
        if (humanPrefab == null || spawnPoint == null)
        {
            return;
        }

        HumanBody spawnedHuman = Instantiate(humanPrefab, spawnPoint.position, humanPrefab.transform.rotation);
        spawnedHuman.ApplyLevelConfig(levelConfig);
        ApplyHumanCustomization(spawnedHuman);
        spawnedHuman.ApplyMovementSpeedMultiplier(movementSpeedMultiplier);
    }

    /// <summary>
    /// Checks whether any prefab in the resolved pool can produce the given menu,
    /// respecting the same three-tier priority as SpawnHuman.
    /// </summary>
    public bool CanSpawnMenu(Menu menu, StageLevelConfig config, StageHumanPrefabSpawnEntry[] phaseOverrides = null)
    {
        if (menu == null)
        {
            return false;
        }

        if (tutorialOnlyHumanPrefab != null)
        {
            return CanPrefabSpawnMenu(tutorialOnlyHumanPrefab, menu, config);
        }

        StageHumanPrefabSpawnEntry[] pool = ResolveHumanPrefabOverrides(config, phaseOverrides);

        if (pool != null && pool.Length > 0)
        {
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i] != null && pool[i].IsValid && CanPrefabSpawnMenu(pool[i].Prefab, menu, config))
                {
                    return true;
                }
            }

            // A pool was resolved — don't fall through to humanPrefabs
            return false;
        }

        // No overrides at any tier; check the serialized humanPrefabs list
        if (humanPrefabs == null)
        {
            return false;
        }

        for (int i = 0; i < humanPrefabs.Length; i++)
        {
            if (CanPrefabSpawnMenu(humanPrefabs[i], menu, config))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Instantiates a human from the resolved prefab pool.
    /// Priority: phaseOverrides > levelHumanPrefabOverrides > humanPrefabs.
    /// </summary>
    private HumanBody SpawnConfiguredHuman(StageHumanPrefabSpawnEntry[] phaseOverrides = null)
    {
        Debug.Log("Spawn");
        if (tutorialOnlyHumanPrefab != null)
        {
            return Instantiate(tutorialOnlyHumanPrefab, spawnPoint.position, tutorialOnlyHumanPrefab.transform.rotation);
        }

        // Tier 1: phase overrides
        if (HasValidHumanPrefabSpawnEntries(phaseOverrides))
        {
            return InstantiateFromWeightedPrefabPool(phaseOverrides);
        }

        // Tier 2: level config overrides (resolved at Start)
        if (HasValidHumanPrefabSpawnEntries(levelHumanPrefabOverrides))
        {
            return InstantiateFromWeightedPrefabPool(levelHumanPrefabOverrides);
        }

        // Tier 3: serialized HumanBody prefab list
        if (humanPrefabs == null || humanPrefabs.Length == 0)
        {
            return null;
        }

        HumanBody selectedPrefab = humanPrefabs[Random.Range(0, humanPrefabs.Length)];
        if (selectedPrefab == null)
        {
            return null;
        }

        return Instantiate(selectedPrefab, spawnPoint.position, selectedPrefab.transform.rotation);
    }

    private HumanBody InstantiateFromWeightedPrefabPool(StageHumanPrefabSpawnEntry[] pool)
    {
        GameObject selectedPrefab = GetWeightedPrefab(pool);
        if (selectedPrefab == null)
        {
            return null;
        }

        GameObject spawnedObject = Instantiate(selectedPrefab, spawnPoint.position, selectedPrefab.transform.rotation);
        return spawnedObject.GetComponent<HumanBody>();
    }

    private static GameObject GetWeightedPrefab(StageHumanPrefabSpawnEntry[] pool)
    {
        float totalWeight = 0f;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null && pool[i].IsValid)
            {
                totalWeight += pool[i].SpawnPercentage;
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float randomWeight = Random.Range(0f, totalWeight);
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] == null || !pool[i].IsValid)
            {
                continue;
            }

            randomWeight -= pool[i].SpawnPercentage;
            if (randomWeight <= 0f)
            {
                return pool[i].Prefab;
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves the active prefab pool following three-tier priority.
    /// Returns null if no overrides are set at any tier (caller should fall back to humanPrefabs).
    /// </summary>
    private static StageHumanPrefabSpawnEntry[] ResolveHumanPrefabOverrides(StageLevelConfig config, StageHumanPrefabSpawnEntry[] phaseOverrides)
    {
        // Tier 1: phase overrides
        if (HasValidHumanPrefabSpawnEntries(phaseOverrides))
        {
            return phaseOverrides;
        }

        // Tier 2: level config overrides
        if (config != null && config.HasHumanPrefabSpawnPercentages)
        {
            return config.HumanPrefabSpawnPercentages;
        }

        return null;
    }

    private static bool HasValidHumanPrefabSpawnEntries(StageHumanPrefabSpawnEntry[] entries)
    {
        if (entries == null)
        {
            return false;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] != null && entries[i].IsValid)
            {
                return true;
            }
        }

        return false;
    }

    private bool CanPrefabSpawnMenu(Component prefab, Menu menu, StageLevelConfig config)
    {
        if (prefab == null)
        {
            return false;
        }

        return CanPrefabSpawnMenu(prefab.gameObject, menu, config);
    }

    private bool CanPrefabSpawnMenu(GameObject prefab, Menu menu, StageLevelConfig config)
    {
        if (prefab == null)
        {
            return false;
        }

        if (prefab.GetComponent<BigRapidSliceEvent>() != null)
        {
            return true;
        }

        HumanPart[] parts = prefab.GetComponentsInChildren<HumanPart>(true);
        for (int i = 0; i < parts.Length; i++)
        {
            HumanPart part = parts[i];
            if (part == null || !IsPartEnabledForLevel(part.name, config))
            {
                continue;
            }

            if (part.GetProducedMenu() == menu)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPartEnabledForLevel(string partName, StageLevelConfig config)
    {
        if (config == null || config.EnabledBodyParts == null || config.EnabledBodyParts.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < config.EnabledBodyParts.Length; i++)
        {
            HumanBodyPartType bodyPart = config.EnabledBodyParts[i];
            if (partName.Contains(bodyPart.ToString()) || (bodyPart == HumanBodyPartType.Stomach && partName.Contains("Body")))
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyHumanCustomization(HumanBody spawnedHuman)
    {
        if (spawnedHuman == null)
        {
            return;
        }

        if (customizationManager == null)
        {
            customizationManager = CharacterCustomizationManager.GetOrCreateRuntimeInstance();
        }

        HumanType spawnedType = GetHumanType(spawnedHuman);
        CharacterCustomizationData manualData = CharacterCustomizationData.LoadFromPlayerPrefs();
        if (manualData.selectedHumanType == spawnedType)
        {
            CharacterSpriteSet manualSpriteSet = CharacterCustomizationApplier.LoadSavedSpriteSet();
            if (manualSpriteSet != null && manualSpriteSet.HasAnySprite())
            {
                spawnedHuman.ApplyCustomizationSpriteSet(manualSpriteSet);
                return;
            }
        }

        if (customizationManager != null && customizationManager.TryGetGeneratedSpriteSetForSpawn(spawnedType, out CharacterSpriteSet generatedSpriteSet))
        {
            spawnedHuman.ApplyCustomizationSpriteSet(generatedSpriteSet);
        }
    }

    private static HumanType GetHumanType(HumanBody humanBody)
    {
        if (humanBody.GetComponent<BigRapidSliceEvent>() != null)
        {
            return HumanType.BigHuman;
        }

        if (humanBody.GetComponent<HumanRockThrower>() != null)
        {
            return HumanType.RockThrowerHuman;
        }

        if (humanBody.GetComponent<RobotHuman>() != null)
        {
            return HumanType.RobotHuman;
        }

        if (humanBody.GetComponentInChildren<KnightArmorLayer>(true) != null)
        {
            return HumanType.KnightHuman;
        }

        string humanName = humanBody.gameObject.name;
        if (humanName.Contains("Big"))
        {
            return HumanType.BigHuman;
        }

        if (humanName.Contains("RockThrower") || humanName.Contains("Rock Thrower"))
        {
            return HumanType.RockThrowerHuman;
        }

        if (humanName.Contains("Robot"))
        {
            return HumanType.RobotHuman;
        }

        if (humanName.Contains("Knight"))
        {
            return HumanType.KnightHuman;
        }

        return HumanType.NormalHuman;
    }
}
