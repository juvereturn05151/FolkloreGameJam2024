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

    void Start()
    {
        levelConfig = StageSelection.SelectedLevel;
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
        // Choose a random human prefab from the array
        HumanBody selectedPrefab = humanPrefabs[Random.Range(0, humanPrefabs.Length)];

        // Instantiate the selected human prefab at the specified spawn point
        HumanBody spawnedHuman = Instantiate(selectedPrefab, spawnPoint.position, selectedPrefab.transform.rotation);
        spawnedHuman.ApplyLevelConfig(levelConfig);
        spawnedHuman.ApplyMovementSpeedMultiplier(movementSpeedMultiplier);
    }

    public bool CanSpawnMenu(Menu menu, StageLevelConfig config)
    {
        if (menu == null || humanPrefabs == null)
        {
            return false;
        }

        for (int i = 0; i < humanPrefabs.Length; i++)
        {
            HumanBody prefab = humanPrefabs[i];
            if (prefab == null)
            {
                continue;
            }

            HumanPart[] parts = prefab.GetComponentsInChildren<HumanPart>(true);
            for (int j = 0; j < parts.Length; j++)
            {
                HumanPart part = parts[j];
                if (part == null || !IsPartEnabledForLevel(part.name, config))
                {
                    continue;
                }

                if (part.GetProducedMenu() == menu)
                {
                    return true;
                }
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
