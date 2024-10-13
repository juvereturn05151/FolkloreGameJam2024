using UnityEngine;
using static Customer;

public class CustomerSpot : MonoBehaviour
{
    [SerializeField]
    private Plate _plate;

    private Customer _customer;

    // Set a customer in this spot
    public void SetCustomer(Customer customer)
    {
        if (customer != null)
        {
            _customer = customer;
            _customer.transform.parent = transform; // Parent the customer to this spot
            _customer.transform.localPosition = Vector3.zero; // Center the customer in the spot
            _customer.SetPlate(_plate);
            _customer.SetSpot(this);
            _customer.onEatRightFood.AddListener(SetCustomer);
        }
        else
        {
            if (_customer != null)
            {
                Destroy(_customer.gameObject); // Remove the customer from the spot
                _customer = null;
            }
        }
    }

    // Check if this spot has a customer
    public bool HasCustomer()
    {
        return _customer != null;
    }
}