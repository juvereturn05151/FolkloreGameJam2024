using UnityEngine;
using static Customer;

public class CustomerSpot : MonoBehaviour
{
    [SerializeField] private CustomerFoodPlace _plate;
    [SerializeField] private GameObject _darkFire;

    private Customer _customer;
    public Customer Customer => _customer;

    public void SetGameplayActive(bool isActive)
    {
        if (!isActive)
        {
            ClearCustomer();
        }

        GameObject root = GetGameplayRoot();
        if (_plate != null && !_plate.transform.IsChildOf(root.transform))
        {
            _plate.gameObject.SetActive(isActive);
        }

        root.SetActive(isActive);
    }

    // Set a customer in this spot
    public void SetCustomer(Customer customer)
    {
        _darkFire.SetActive(false);

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

    public void ClearCustomer()
    {
        SetCustomer(null);

        if (_plate != null)
        {
            _plate.CurrentCustomer = null;
            _plate.SetIsOccupied(false);
        }
    }

    public void ActivateDarkFire() 
    {
        _darkFire.SetActive(true);
    }

    // Check if this spot has a customer
    public bool HasCustomer()
    {
        return _customer != null;
    }

    private GameObject GetGameplayRoot()
    {
        if (_plate != null && transform.parent != null && _plate.transform.parent == transform.parent)
        {
            return transform.parent.gameObject;
        }

        return gameObject;
    }
}
