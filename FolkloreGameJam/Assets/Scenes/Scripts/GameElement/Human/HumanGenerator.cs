using UnityEngine;

public class HumanGenerator : MonoBehaviour
{
    [SerializeField] 
    private HumanBody humanPrefab; // Prefab for the human body
    [SerializeField] 
    private Transform spawnPoint; // Where the human will be spawned
    [SerializeField]
    private float firstSpawnTime = 2.0f;
    [SerializeField] 
    private float spawnInterval = 5f; // Time in seconds before trying to spawn again

    private HumanBody currentHuman; // Reference to the current human on stage
    private float spawnTimer; // Timer to track spawn interval

    void Start()
    {
        // Initialize the timer
        spawnTimer = firstSpawnTime;
    }

    void Update()
    {
        // Check if there is already a human on the stage
        if (currentHuman == null)
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
    }

    // Method to spawn the human if none exists
    private void SpawnHuman()
    {
        // Ensure that we only spawn if there is no human present
        if (currentHuman == null)
        {
            // Instantiate the human at the specified spawn point
            currentHuman = Instantiate(humanPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    // Call this when the human is removed or destroyed
    public void RemoveHuman()
    {
        if (currentHuman != null)
        {
            Destroy(currentHuman);
            currentHuman = null;
            Debug.Log("Human removed.");
        }
    }
}
