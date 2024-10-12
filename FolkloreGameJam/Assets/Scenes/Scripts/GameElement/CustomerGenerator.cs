using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CustomerGenerator : MonoBehaviour
{
    [SerializeField]
    private List<Customer> _possibleCustomers = new List<Customer>(); // List of possible ghost prefabs

    [SerializeField]
    private List<CustomerSpot> _customerSpots = new List<CustomerSpot>(); // List of customer spots

    [SerializeField]
    private float _spawnInterval = 5f; // Interval between spawning customers

    private bool _isGenerating = true; // Flag to control customer generation

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateCustomersRoutine());
    }

    // Coroutine to generate customers at intervals
    IEnumerator GenerateCustomersRoutine()
    {
        while (true)
        {
            if (_isGenerating)
            {
                GenerateRandomCustomer(); // Attempt to generate a customer
            }

            yield return new WaitForSeconds(_spawnInterval); // Wait for the next spawn interval
        }
    }

    // Generate a random customer and place them in an empty spot
    void GenerateRandomCustomer()
    {
        // Find an available (empty) customer spot
        CustomerSpot emptySpot = GetEmptySpot();

        if (emptySpot != null) // If there's an available spot
        {
            Customer randomGhost = GetRandomCustomer(); // Get a random ghost from the list
            Customer newCustomer = Instantiate(randomGhost); // Instantiate the ghost
            emptySpot.SetCustomer(newCustomer); // Set the new customer in the spot
        }
        else
        {
            _isGenerating = false; // Stop generating if no empty spots are available
        }
    }

    // Get a random ghost from the list of possible ghosts
    Customer GetRandomCustomer()
    {
        int randomIndex = Random.Range(0, _possibleCustomers.Count); // Pick a random index
        return _possibleCustomers[randomIndex]; // Return the randomly selected ghost prefab
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
        spot.SetCustomer(null); // Clear the customer
        _isGenerating = true; // Start generating customers again
    }
}