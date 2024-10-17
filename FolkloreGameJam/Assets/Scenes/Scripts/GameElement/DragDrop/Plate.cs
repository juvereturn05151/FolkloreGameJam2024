using System;
using UnityEngine;
using UnityEngine.Events;

public class Plate : MonoBehaviour
{
    [Serializable] public class FoodPlacedEvent : UnityEvent<Food> { }
    public FoodPlacedEvent OnFoodPlaced;

    [SerializeField] private Food _foodOnPlate;
    public Food FoodOnPlate => _foodOnPlate;

    [SerializeField]private bool _isOccupied = false;
    public bool IsOccupied => _isOccupied;

    private Customer currentCustomer;
    public Customer CurrentCustomer { get => currentCustomer; set => currentCustomer = value; }

    public void SetIsOccupied(bool occupy) 
    {
        _isOccupied = occupy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Food>() is Food food)
        {
            if(currentCustomer != null && currentCustomer.IsOrdering)
                PrepareToEat(food);
        }
    }

    public void PrepareToEat(Food food) 
    {
        if (!IsOccupied) 
        {
            return;
        }

        if (_foodOnPlate == null)
        {
            SoundManager.instance.PlaySFX("SFX_ServeCustomer");
            _foodOnPlate = food;
            _foodOnPlate.SetFoodToBeEaten(this, currentCustomer.IsEatingRightFood);
            OnFoodPlaced?.Invoke(_foodOnPlate); // Pass the food object as a parameter to the event
        }
    }
}
