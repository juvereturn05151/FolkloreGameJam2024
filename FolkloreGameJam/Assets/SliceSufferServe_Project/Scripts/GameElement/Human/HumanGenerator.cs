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

    void Start()
    {
        // Initialize the timer
        spawnTimer = firstSpawnTime;
    }

    void Update()
    {
        // Countdown the spawn timer
        spawnTimer -= Time.deltaTime;

        // If the timer hits zero, try to spawn a human
        if (spawnTimer <= 0f)
        {
            SpawnHuman();
            // Reset the timer for the next potential spawn
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnHuman()
    {
        // Choose a random human prefab from the array
        HumanBody selectedPrefab = humanPrefabs[Random.Range(0, humanPrefabs.Length)];

        // Instantiate the selected human prefab at the specified spawn point
        Instantiate(selectedPrefab, spawnPoint.position, selectedPrefab.transform.rotation);
    }
}
