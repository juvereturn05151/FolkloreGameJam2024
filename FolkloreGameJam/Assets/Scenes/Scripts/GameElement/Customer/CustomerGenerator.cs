using UnityEngine;
using System.Collections.Generic;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _spawnTimer = _spawnInterval; // Initialize the spawn timer
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.IsGameOver) return;
        
        if (_isGenerating)
        {
            _spawnTimer -= Time.deltaTime; // Countdown the spawn timer

            if (_spawnTimer <= 0f) // If the timer reaches zero
            {
                GenerateRandomCustomer(); // Attempt to generate a customer
                _spawnTimer = _spawnInterval; // Reset the spawn timer
            }
        }
    }

    // Generate a random customer and place them in an empty spot
    void GenerateRandomCustomer()
    {
        // Find an available (empty) customer spot
        CustomerSpot emptySpot = GetEmptySpot();

        if (emptySpot != null) // If there's an available spot
        {
            Customer randomCustomer = GetRandomCustomer(); // Get a random customer from the list
            Customer newCustomer = Instantiate(randomCustomer); // Instantiate the customer
            emptySpot.SetCustomer(newCustomer); // Set the new customer in the spot
            newCustomer.onLeaveRestaurant.AddListener(ClearCustomerSpot); // Listen for when the customer leaves
        }
        else
        {
            _isGenerating = false; // Stop generating if no empty spots are available
        }
    }

    // Get a random customer from the list of possible customers
    Customer GetRandomCustomer()
    {
        int randomIndex = Random.Range(0, _possibleCustomers.Count); // Pick a random index
        return _possibleCustomers[randomIndex]; // Return the randomly selected customer prefab
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

    public void PissCustomer() 
    {
        foreach (CustomerSpot spot in _customerSpots) 
        {
            if (spot.HasCustomer() && spot.Customer != null) 
            {
                if (!spot.Customer.IsEating) 
                {
                    spot.Customer.PatienceSlider.value -= 10.0f;
                }
            }
            
        }
    }
}