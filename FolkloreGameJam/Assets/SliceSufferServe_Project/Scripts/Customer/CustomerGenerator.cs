using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CustomerGenerator : MonoBehaviour
{
    public static CustomerGenerator Instance { get; private set; }

    [SerializeField]
    private List<Customer> _possibleCustomers = new List<Customer>(); // List of possible customer prefabs

    [SerializeField]
    private List<CustomerSpot> _customerSpots = new List<CustomerSpot>(); // List of customer spots

    [SerializeField]
    private float _spawnInterval = 5f; // Interval between spawning customers

    private float _spawnTimer; // Timer to track the spawn interval
    private bool _isGenerating = true; // Flag to control customer generation
    private float elapsedStageTime;
    private StageLevelConfig levelConfig;
    private readonly List<Customer> eligibleCustomers = new List<Customer>();
    private HumanGenerator[] humanGenerators;
    private bool hasGhostFilter;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        levelConfig = StageSelection.SelectedLevel;
        humanGenerators = FindObjectsByType<HumanGenerator>(FindObjectsSortMode.None);
        for (int i = 0; i < humanGenerators.Length; i++)
        {
            if (humanGenerators[i] != null)
            {
                humanGenerators[i].SetExternallyControlled(true);
            }
        }

        BuildEligibleCustomers();
        _spawnTimer = GetActivePhase().SpawnInterval;

        TimeManager.Instance.OnRushTime.AddListener(DoubleSpawnInterval);
    }

    private void Update()
    {
        if(GameManager.Instance.IsGameOver) return;
        
        elapsedStageTime += Time.deltaTime;

        if (_isGenerating)
        {
            _spawnTimer -= Time.deltaTime; // Countdown the spawn timer

            if (_spawnTimer <= 0f) // If the timer reaches zero
            {
                StageSpawnPhase activePhase = GetActivePhase();
                GenerateSpawnPair(activePhase);
                _spawnTimer = activePhase.SpawnInterval;
            }
        }
    }

    private void DoubleSpawnInterval() 
    {
        _spawnTimer = GetActivePhase().SpawnInterval;
    }

    // Generate a random customer and place them in an empty spot
    private void GenerateSpawnPair(StageSpawnPhase activePhase)
    {
        GenerateRandomCustomer(activePhase);

        if (Random.value <= activePhase.DoubleSpawnChance)
        {
            StartCoroutine(GenerateDelayedSpawnPair(activePhase, activePhase.DoubleSpawnDelay));
        }
    }

    private IEnumerator GenerateDelayedSpawnPair(StageSpawnPhase activePhase, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!GameManager.Instance.IsGameOver)
        {
            GenerateRandomCustomer(activePhase);
        }
    }

    void GenerateRandomCustomer(StageSpawnPhase activePhase)
    {
        // Find an available (empty) customer spot
        CustomerSpot emptySpot = GetEmptySpot();

        if (emptySpot != null) // If there's an available spot
        {
            Customer randomCustomer = GetRandomCustomer(); // Get a random customer from the list
            if (randomCustomer == null)
            {
                return;
            }

            Customer newCustomer = Instantiate(randomCustomer); // Instantiate the customer
            newCustomer.SetAllowedDesiredFoodStates(activePhase.AllowedFoodStates);
            emptySpot.SetCustomer(newCustomer); // Set the new customer in the spot
            newCustomer.onLeaveRestaurant.AddListener(ClearCustomerSpot); // Listen for when the customer leaves
            StartCoroutine(SpawnHumanAfterDelay(activePhase));
        }
        else
        {
            _isGenerating = false; // Stop generating if no empty spots are available
        }
    }

    private IEnumerator SpawnHumanAfterDelay(StageSpawnPhase activePhase)
    {
        float delay = levelConfig == null ? 0.35f : levelConfig.HumanSpawnDelayAfterGhost;
        yield return new WaitForSeconds(delay);

        if (GameManager.Instance.IsGameOver || humanGenerators == null || humanGenerators.Length == 0)
        {
            yield break;
        }

        HumanGenerator generator = humanGenerators[Random.Range(0, humanGenerators.Length)];
        if (generator != null)
        {
            generator.SpawnHuman(activePhase.HumanSpeedMultiplier);
        }
    }

    // Get a random customer from the list of possible customers
    Customer GetRandomCustomer()
    {
        List<Customer> source = hasGhostFilter ? eligibleCustomers : _possibleCustomers;
        if (source == null || source.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, source.Count); // Pick a random index
        return source[randomIndex]; // Return the randomly selected customer prefab
    }

    // Find an empty customer spot
    CustomerSpot GetEmptySpot()
    {
        foreach (CustomerSpot spot in _customerSpots)
        {
            if (!spot.HasCustomer()) // If the spot is empty
            {
                return spot;
            }
        }

        return null; // Return null if no empty spots are available
    }

    // Clear a customer spot and start generating customers again
    public void ClearCustomerSpot(CustomerSpot spot)
    {
        spot.SetCustomer(null); // Clear the customer from the spot
        _isGenerating = true; // Allow customer generation again
    }

    private void BuildEligibleCustomers()
    {
        eligibleCustomers.Clear();

        hasGhostFilter = levelConfig != null && levelConfig.AllowedGhosts != null && levelConfig.AllowedGhosts.Length > 0;

        if (!hasGhostFilter)
        {
            eligibleCustomers.AddRange(_possibleCustomers);
            return;
        }

        for (int i = 0; i < _possibleCustomers.Count; i++)
        {
            Customer customer = _possibleCustomers[i];
            if (customer != null && IsGhostAllowed(customer.GhostType))
            {
                eligibleCustomers.Add(customer);
            }
        }

        if (eligibleCustomers.Count == 0)
        {
            Debug.LogWarning("No eligible customers match the selected level's allowed ghosts.");
        }
    }

    private bool IsGhostAllowed(Ghost ghost)
    {
        if (ghost == null)
        {
            return false;
        }

        for (int i = 0; i < levelConfig.AllowedGhosts.Length; i++)
        {
            if (levelConfig.AllowedGhosts[i] == ghost)
            {
                return true;
            }
        }

        return false;
    }

    private StageSpawnPhase GetActivePhase()
    {
        if (levelConfig == null || levelConfig.SpawnPhases == null || levelConfig.SpawnPhases.Length == 0)
        {
            return new StageSpawnPhase(0f, 60f, _spawnInterval, 1f, 0f);
        }

        StageSpawnPhase fallback = levelConfig.SpawnPhases[levelConfig.SpawnPhases.Length - 1];
        if (fallback == null)
        {
            fallback = new StageSpawnPhase(0f, 60f, _spawnInterval, 1f, 0f);
        }
        for (int i = 0; i < levelConfig.SpawnPhases.Length; i++)
        {
            StageSpawnPhase phase = levelConfig.SpawnPhases[i];
            if (phase != null && phase.Contains(elapsedStageTime))
            {
                return phase;
            }
        }

        return fallback;
    }
}
