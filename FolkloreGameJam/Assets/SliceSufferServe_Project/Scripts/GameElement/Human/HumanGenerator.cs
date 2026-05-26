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
    private GameObject[] levelHumanPrefabOverrides; // Level-config overrides, resolved once at Start
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

    private bool IsTutorialActive()
    {
        return GameManager.Instance != null && GameManager.Instance.IsTutorial;
    }

    /// <summary>
    /// Spawns a human using the three-tier prefab priority:
    /// phase overrides > level config overrides > humanPrefabs serialized list.
    /// </summary>
    public void SpawnHuman(float movementSpeedMultiplier, GameObject[] phaseOverrides = null)
    {
        HumanBody spawnedHuman = SpawnConfiguredHuman(phaseOverrides);

        if (spawnedHuman == null)
        {
            return;
        }

        spawnedHuman.ApplyLevelConfig(levelConfig);
        ApplyNormalHumanCustomization(spawnedHuman);
        spawnedHuman.ApplyMovementSpeedMultiplier(movementSpeedMultiplier);
    }

    /// <summary>
    /// Checks whether any prefab in the resolved pool can produce the given menu,
    /// respecting the same three-tier priority as SpawnHuman.
    /// </summary>
    public bool CanSpawnMenu(Menu menu, StageLevelConfig config, GameObject[] phaseOverrides = null)
    {
        if (menu == null)
        {
            return false;
        }

        GameObject[] pool = ResolveHumanPrefabOverrides(config, phaseOverrides);

        if (pool != null && pool.Length > 0)
        {
            for (int i = 0; i < pool.Length; i++)
            {
                if (CanPrefabSpawnMenu(pool[i], menu, config))
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
    private HumanBody SpawnConfiguredHuman(GameObject[] phaseOverrides = null)
    {
        Debug.Log("Spawn");
        // Tier 1: phase overrides
        if (phaseOverrides != null && phaseOverrides.Length > 0)
        {
            return InstantiateFromGameObjectPool(phaseOverrides);
        }

        // Tier 2: level config overrides (resolved at Start)
        if (levelHumanPrefabOverrides != null && levelHumanPrefabOverrides.Length > 0)
        {
            return InstantiateFromGameObjectPool(levelHumanPrefabOverrides);
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

    private HumanBody InstantiateFromGameObjectPool(GameObject[] pool)
    {
        GameObject selectedPrefab = pool[Random.Range(0, pool.Length)];
        if (selectedPrefab == null)
        {
            return null;
        }

        GameObject spawnedObject = Instantiate(selectedPrefab, spawnPoint.position, selectedPrefab.transform.rotation);
        return spawnedObject.GetComponent<HumanBody>();
    }

    /// <summary>
    /// Resolves the active prefab pool following three-tier priority.
    /// Returns null if no overrides are set at any tier (caller should fall back to humanPrefabs).
    /// </summary>
    private static GameObject[] ResolveHumanPrefabOverrides(StageLevelConfig config, GameObject[] phaseOverrides)
    {
        // Tier 1: phase overrides
        if (phaseOverrides != null && phaseOverrides.Length > 0)
        {
            return phaseOverrides;
        }

        // Tier 2: level config overrides
        if (config != null && config.HumanPrefabOverrides != null && config.HumanPrefabOverrides.Length > 0)
        {
            return config.HumanPrefabOverrides;
        }

        return null;
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

        if (prefab.GetComponent<ObeseRapidSliceEvent>() != null)
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

    private void ApplyNormalHumanCustomization(HumanBody spawnedHuman)
    {
        if (spawnedHuman == null || !IsNormalHuman(spawnedHuman))
        {
            return;
        }

        if (customizationManager == null)
        {
            customizationManager = CharacterCustomizationManager.GetOrCreateRuntimeInstance();
        }

        if (customizationManager != null
            && customizationManager.TryGetGeneratedSpriteSetForSpawn(HumanType.NormalHuman, out CharacterSpriteSet spriteSet))
        {
            spawnedHuman.ApplyCustomizationSpriteSet(spriteSet);
        }
    }

    private static bool IsNormalHuman(HumanBody humanBody)
    {
        string humanName = humanBody.gameObject.name;
        return humanName == "Human" || humanName.StartsWith("Human(");
    }
}
