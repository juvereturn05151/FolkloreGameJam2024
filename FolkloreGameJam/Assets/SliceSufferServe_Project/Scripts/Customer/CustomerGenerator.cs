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

    private float _demandCheckTimer;
    private const float DemandCheckInterval = 2f;

    private float _spawnTimer; // Timer to track the spawn interval
    private bool _isGenerating = true; // Flag to control customer generation
    private float elapsedStageTime;
    private StageLevelConfig levelConfig;
    private readonly List<Customer> eligibleCustomers = new List<Customer>();
    private readonly List<CustomerSpot> activeCustomerSpots = new List<CustomerSpot>();
    private HumanGenerator[] humanGenerators;
    private bool hasGhostFilter;
    private int pendingDemandHumanSpawns;
    private bool isRapidSlicePaused;

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
        ApplyActiveCustomerSpots();
        _spawnTimer = GetActivePhase().SpawnInterval;

        TimeManager.Instance.OnRushTime.AddListener(DoubleSpawnInterval);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (isRapidSlicePaused) return;

        elapsedStageTime += Time.deltaTime;

        if (_isGenerating)
        {
            _spawnTimer -= Time.deltaTime;

            if (_spawnTimer <= 0f)
            {
                StageSpawnPhase activePhase = GetActivePhase();
                GenerateSpawnPair(activePhase);
                _spawnTimer = activePhase.SpawnInterval;
            }
        }

        // Demand check
        _demandCheckTimer -= Time.deltaTime;
        if (_demandCheckTimer <= 0f)
        {
            _demandCheckTimer = DemandCheckInterval;
            CheckAndFulfillOutstandingDemand();
        }
    }

    private void CheckAndFulfillOutstandingDemand()
    {
        if (!CanProcessReplacementRequest()) return;
        if (!NeedsAnotherHuman()) return;

        QueueHumanSpawn(GetActivePhase());
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
            if (levelConfig != null)
            {
                newCustomer.SetPatienceMultiplier(levelConfig.CustomerPatienceMultiplier);
                if (levelConfig.HasCustomerOrderCountOverride)
                {
                    newCustomer.SetOrderCountRange(levelConfig.CustomerMinOrderCount, levelConfig.CustomerMaxOrderCount);
                }
            }

            newCustomer.SetAllowedDesiredFoodStates(activePhase.AllowedFoodStates);
            emptySpot.SetCustomer(newCustomer); // Set the new customer in the spot
            newCustomer.onLeaveRestaurant.AddListener(ClearCustomerSpot); // Listen for when the customer leaves
            QueueHumanSpawn(activePhase);
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

        while (isRapidSlicePaused)
        {
            yield return null;
        }

        pendingDemandHumanSpawns = Mathf.Max(0, pendingDemandHumanSpawns - 1);

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

    public void SetRapidSlicePaused(bool paused)
    {
        isRapidSlicePaused = paused;
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
        foreach (CustomerSpot spot in activeCustomerSpots)
        {
            if (!spot.HasCustomer()) // If the spot is empty
            {
                return spot;
            }
        }

        return null; // Return null if no empty spots are available
    }

    private void ApplyActiveCustomerSpots()
    {
        activeCustomerSpots.Clear();

        for (int i = 0; i < _customerSpots.Count; i++)
        {
            CustomerSpot spot = _customerSpots[i];
            if (spot == null)
            {
                continue;
            }

            bool isActive = levelConfig == null || levelConfig.IsCustomerSpotEnabled(i);
            spot.SetGameplayActive(isActive);

            if (isActive)
            {
                activeCustomerSpots.Add(spot);
            }
        }

        if (activeCustomerSpots.Count == 0)
        {
            Debug.LogWarning("No active customer spots are configured for this level.");
        }
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

    private void QueueHumanSpawn(StageSpawnPhase activePhase)
    {
        if (!CanProcessReplacementRequest())
        {
            return;
        }

        pendingDemandHumanSpawns++;
        StartCoroutine(SpawnHumanAfterDelay(activePhase));
    }

    private bool CanProcessReplacementRequest()
    {
        return isActiveAndEnabled
            && GameManager.Instance != null
            && !GameManager.Instance.IsGameOver;
    }

    private bool NeedsAnotherHuman()
    {
        Dictionary<Menu, int> demand = GetOutstandingOrderCounts();
        if (demand.Count == 0)
        {
            return false;
        }

        Food[] stuff = FindObjectsByType<Food>(FindObjectsSortMode.None);
        Debug.Log("Current outstanding demand:" + stuff.Length);
        if (stuff.Length >= 5)
        {
            Debug.Log("Supply is sufficient with " + stuff.Length + " available items. No need to spawn more humans for demand.");
            return false; // If there are already 5 or more available items, we likely don't need more humans to meet demand
        }

        Dictionary<Menu, int> supply = GetAvailableSupplyCounts();


        foreach (KeyValuePair<Menu, int> orderCount in demand)
        {
            supply.TryGetValue(orderCount.Key, out int availableCount);
            if (CanPendingHumansProvide(orderCount.Key))
            {
                availableCount += pendingDemandHumanSpawns;
            }

            if (availableCount < orderCount.Value)
            {
                return true;
            }
        }

        return false;
    }

    private Dictionary<Menu, int> GetOutstandingOrderCounts()
    {
        Dictionary<Menu, int> counts = new Dictionary<Menu, int>();

        for (int i = 0; i < activeCustomerSpots.Count; i++)
        {
            Customer customer = activeCustomerSpots[i] != null ? activeCustomerSpots[i].Customer : null;
            if (customer == null || !customer.IsOrdering)
            {
                continue;
            }

            IReadOnlyList<CustomerOrder> orders = customer.CurrentOrders;
            for (int j = 0; j < orders.Count; j++)
            {
                CustomerOrder order = orders[j];
                if (order == null || order.Menu == null)
                {
                    continue;
                }

                AddMenuCount(counts, order.Menu, 1);
            }
        }

        return counts;
    }

    private Dictionary<Menu, int> GetAvailableSupplyCounts()
    {
        Dictionary<Menu, int> counts = new Dictionary<Menu, int>();

        Food[] foods = FindObjectsByType<Food>(FindObjectsSortMode.None);
        for (int i = 0; i < foods.Length; i++)
        {
            Food food = foods[i];
            if (food == null || food.Menu == null || food.IsReadyToEat || food.IsFinished)
            {
                continue;
            }

            AddMenuCount(counts, food.Menu, 1);
        }

        HumanBody[] humans = FindObjectsByType<HumanBody>(FindObjectsSortMode.None);
        for (int i = 0; i < humans.Length; i++)
        {
            if (humans[i] != null)
            {
                humans[i].AddAvailablePartMenus(counts);
            }
        }

        return counts;
    }

    private bool CanPendingHumansProvide(Menu menu)
    {
        if (menu == null || humanGenerators == null)
        {
            return false;
        }

        for (int i = 0; i < humanGenerators.Length; i++)
        {
            if (humanGenerators[i] != null && humanGenerators[i].CanSpawnMenu(menu, levelConfig))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddMenuCount(Dictionary<Menu, int> counts, Menu menu, int amount)
    {
        if (!counts.ContainsKey(menu))
        {
            counts[menu] = 0;
        }

        counts[menu] += amount;
    }
}
