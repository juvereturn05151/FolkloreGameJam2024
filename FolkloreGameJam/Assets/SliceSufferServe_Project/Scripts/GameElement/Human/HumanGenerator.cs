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

    private float spawnTimer; // Timer to track spawn interval
    private StageLevelConfig levelConfig;
    private bool isExternallyControlled;
    private GameObject[] activeHumanPrefabOverrides;

    void Start()
    {
        levelConfig = StageSelection.SelectedLevel;
        activeHumanPrefabOverrides = GetActiveHumanPrefabOverrides();
        // Initialize the timer
        spawnTimer = firstSpawnTime;
    }

    void Update()
    {
        if (isExternallyControlled && !IsTutorialActive())
        {
            return;
        }

        // Countdown the spawn timer
        spawnTimer -= Time.deltaTime;

        // If the timer hits zero, try to spawn a human
        if (spawnTimer <= 0f)
        {
            SpawnHuman(1f);
            // Reset the timer for the next potential spawn
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

    public void SpawnHuman(float movementSpeedMultiplier)
    {
        HumanBody spawnedHuman = SpawnConfiguredHuman();

        if (spawnedHuman == null)
        {
            return;
        }

        spawnedHuman.ApplyLevelConfig(levelConfig);
        spawnedHuman.ApplyMovementSpeedMultiplier(movementSpeedMultiplier);
    }

    public bool CanSpawnMenu(Menu menu, StageLevelConfig config)
    {
        if (menu == null)
        {
            return false;
        }

        GameObject[] overridePrefabs = GetActiveHumanPrefabOverrides(config);
        if (overridePrefabs != null && overridePrefabs.Length > 0)
        {
            for (int i = 0; i < overridePrefabs.Length; i++)
            {
                if (CanPrefabSpawnMenu(overridePrefabs[i], menu, config))
                {
                    return true;
                }
            }

            return false;
        }

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

    private HumanBody SpawnConfiguredHuman()
    {
        if (activeHumanPrefabOverrides != null && activeHumanPrefabOverrides.Length > 0)
        {
            GameObject selectedOverridePrefab = activeHumanPrefabOverrides[Random.Range(0, activeHumanPrefabOverrides.Length)];
            if (selectedOverridePrefab == null)
            {
                return null;
            }

            GameObject spawnedObject = Instantiate(selectedOverridePrefab, spawnPoint.position, selectedOverridePrefab.transform.rotation);
            return spawnedObject.GetComponent<HumanBody>();
        }

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

    private GameObject[] GetActiveHumanPrefabOverrides()
    {
        return GetActiveHumanPrefabOverrides(levelConfig);
    }

    private GameObject[] GetActiveHumanPrefabOverrides(StageLevelConfig config)
    {
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
            if (partName.Contains(bodyPart.ToString()))
            {
                return true;
            }
        }

        return false;
    }
}
