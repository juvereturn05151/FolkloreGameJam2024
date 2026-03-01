using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // Array of human prefabs
    [SerializeField]
    private SpawnableObject[] spawnablePrefabs;
    // Where the human will be spawned
    [SerializeField] 
    private Transform spawnPoint; 
    [SerializeField]
    private float firstSpawnTime = 2.0f;
    // Time in seconds before trying to spawn again
    [SerializeField] 
    private float spawnInterval = 5f;

    private SpawnableObject _currentHuman; 
    private float _spawnTimer; 

    private void Start()
    {
        //_spawnTimer = firstSpawnTime;
    }

    void Update()
    {
        //if (_currentHuman == null)
        //{
        //    _spawnTimer -= Time.deltaTime;

        //    if (_spawnTimer <= 0f)
        //    {
        //        SpawnHuman();
        //        _spawnTimer = spawnInterval;
        //    }
        //}
    }

    // Method to spawn the human if none exists
    public void SpawnHuman()
    {
        if (_currentHuman == null)
        {
            SpawnableObject selectedPrefab = spawnablePrefabs[Random.Range(0, spawnablePrefabs.Length)];
            _currentHuman = Instantiate(selectedPrefab, spawnPoint.position, selectedPrefab.transform.rotation);
        }
    }

    // Call this when the human is removed or destroyed
    public void RemoveHuman()
    {
        if (_currentHuman != null)
        {
            Destroy(_currentHuman);
            _currentHuman = null;
        }
    }
}
