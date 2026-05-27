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
    private const float DemandCheckInterval = 2.0f;

    private float _spawnTimer; // Timer to track the spawn interval
    private bool _isGenerating = true; // Flag to control customer generation
    private float elapsedStageTime;
    private StageLevelConfig levelConfig;
    private readonly Dictionary<Ghost, List<Customer>> customersByGhost = new Dictionary<Ghost, List<Customer>>();
    private readonly List<CustomerSpot> activeCustomerSpots = new List<CustomerSpot>();
    private HumanGenerator[] humanGenerators;
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

        BuildCustomersByGhost();
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
        if (pendingDemandHumanSpawns > 0) return;

        Debug.Log("Outstanding demand detected. Spawning human to help meet demand.");
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
            Customer randomCustomer = GetRandomCustomer(activePhase); // Get a random customer from the list
            if (randomCustomer == null)
            {
                return;
            }

            Customer newCustomer = Instantiate(randomCustomer); // Instantiate the customer
            if (levelConfig != null)
            {
                newCustomer.SetPatienceMultiplier(levelConfig.CustomerPatienceMultiplier);
            }

            if (activePhase.HasCustomerOrderCountOverride)
            {
                newCustomer.SetOrderCountRange(activePhase.CustomerMinOrderCount, activePhase.CustomerMaxOrderCount);
            }

            newCustomer.SetAllowedDesiredFoodStates(activePhase.AllowedFoodStates);
            emptySpot.SetCustomer(newCustomer); // Set the new customer in the spot
            newCustomer.onLeaveRestaurant.AddListener(ClearCustomerSpot); // Listen for when the customer leaves
            Debug.Log($"Spawned new customer: {newCustomer.name} at spot {emptySpot.name}. Active phase: {activePhase.StartTime}-{activePhase.EndTime}s.");
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
            // Resolve the prefab override pool using priority: phase > level config > generator defaults
            StageHumanPrefabSpawnEntry[] phaseOverrides = activePhase.HasHumanPrefabSpawnOverrides ? activePhase.HumanPrefabSpawnOverrides : null;
            Debug.Log($"Spawning human to meet demand. Pending demand spawns remaining: {pendingDemandHumanSpawns}. Active phase: {activePhase.StartTime}-{activePhase.EndTime}s. Using {(phaseOverrides != null ? "phase overrides" : "generator defaults")}.");
            generator.SpawnHuman(activePhase.HumanSpeedMultiplier, phaseOverrides);
        }
    }

    public void SetRapidSlicePaused(bool paused)
    {
        isRapidSlicePaused = paused;
    }

    // Get a random customer from the list of possible customers
    Customer GetRandomCustomer(StageSpawnPhase activePhase)
    {
        StageGhostSpawnEntry[] ghostSpawnEntries = GetGhostSpawnEntries(activePhase);
        if (HasValidGhostSpawnEntries(ghostSpawnEntries))
        {
            return GetWeightedGhostCustomer(ghostSpawnEntries);
        }

        return GetRandomCustomerFromList(_possibleCustomers);
    }

    private Customer GetWeightedGhostCustomer(StageGhostSpawnEntry[] ghostSpawnEntries)
    {
        float totalWeight = 0f;
        for (int i = 0; i < ghostSpawnEntries.Length; i++)
        {
            StageGhostSpawnEntry entry = ghostSpawnEntries[i];
            if (entry == null || !entry.IsValid || !HasCustomersForGhost(entry.Ghost))
            {
                continue;
            }

            totalWeight += entry.SpawnPercentage;
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("No eligible customers match the active ghost spawn percentages.");
            return null;
        }

        float randomWeight = Random.Range(0f, totalWeight);
        for (int i = 0; i < ghostSpawnEntries.Length; i++)
        {
            StageGhostSpawnEntry entry = ghostSpawnEntries[i];
            if (entry == null || !entry.IsValid || !HasCustomersForGhost(entry.Ghost))
            {
                continue;
            }

            randomWeight -= entry.SpawnPercentage;
            if (randomWeight <= 0f)
            {
                return GetRandomCustomerFromList(customersByGhost[entry.Ghost]);
            }
        }

        return null;
    }

    private StageGhostSpawnEntry[] GetGhostSpawnEntries(StageSpawnPhase activePhase)
    {
        if (activePhase != null && activePhase.HasGhostSpawnOverrides)
        {
            return activePhase.GhostSpawnOverrides;
        }

        if (levelConfig != null && levelConfig.HasGhostSpawnPercentages)
        {
            return levelConfig.GhostSpawnPercentages;
        }

        return null;
    }

    private static bool HasValidGhostSpawnEntries(StageGhostSpawnEntry[] entries)
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

    private bool HasCustomersForGhost(Ghost ghost)
    {
        return ghost != null && customersByGhost.TryGetValue(ghost, out List<Customer> customers) && customers.Count > 0;
    }

    private static Customer GetRandomCustomerFromList(List<Customer> source)
    {
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

    private void BuildCustomersByGhost()
    {
        customersByGhost.Clear();

        for (int i = 0; i < _possibleCustomers.Count; i++)
        {
            Customer customer = _possibleCustomers[i];
            if (customer == null || customer.GhostType == null)
            {
                continue;
            }

            if (!customersByGhost.TryGetValue(customer.GhostType, out List<Customer> customers))
            {
                customers = new List<Customer>();
                customersByGhost[customer.GhostType] = customers;
            }

            customers.Add(customer);
        }
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

        if (stuff.Length >= 5)
        {
            return false; // If there are already 5 or more available items, we likely don't need more humans to meet demand
        }

        Dictionary<Menu, int> supply = GetAvailableSupplyCounts();

        StageSpawnPhase activePhase = GetActivePhase();

        foreach (KeyValuePair<Menu, int> orderCount in demand)
        {
            supply.TryGetValue(orderCount.Key, out int availableCount);
            if (CanPendingHumansProvide(orderCount.Key, activePhase))
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

    private bool CanPendingHumansProvide(Menu menu, StageSpawnPhase activePhase)
    {
        if (menu == null || humanGenerators == null)
        {
            return false;
        }

        // Resolve phase-level overrides for the demand check, mirroring spawn logic
        StageHumanPrefabSpawnEntry[] phaseOverrides = activePhase.HasHumanPrefabSpawnOverrides ? activePhase.HumanPrefabSpawnOverrides : null;

        for (int i = 0; i < humanGenerators.Length; i++)
        {
            if (humanGenerators[i] != null && humanGenerators[i].CanSpawnMenu(menu, levelConfig, phaseOverrides))
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
