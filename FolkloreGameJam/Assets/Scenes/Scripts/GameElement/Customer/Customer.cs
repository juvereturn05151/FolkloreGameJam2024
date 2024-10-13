using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Define the customer class
public class Customer : MonoBehaviour
{
    [Serializable] public class LeaveRestaurant : UnityEvent<CustomerSpot> { }
    public LeaveRestaurant onLeaveRestaurant;

    [Serializable] public class EatRightFood : UnityEvent<Customer> { }
    public EatRightFood onEatRightFood;

    [SerializeField]
    private Ghost _ghostType;
    [SerializeField]
    private int _patience; // Patience level (higher means more time before getting angry)
    [SerializeField]
    private float _orderTime; // Time it takes for the customer to place an order
    [SerializeField]
    private float _eatTime;   // Time it takes for the customer to finish eating

    [SerializeField]
    private SpriteRenderer _visual;

    private Plate _currentPlate;
    private CustomerSpot _currentSpot;

    private bool _isEating;
    private bool _isEatingRightFood = false;

    private void Start()
    {
        _visual.sprite = _ghostType.Sprite;
    }

    private void Update()
    {
        if (_isEating) 
        {
            if (_currentPlate && _currentPlate.FoodOnPlate) 
            {
                if (_currentPlate.FoodOnPlate.IsFinished) 
                {
                    Eat(_currentPlate.FoodOnPlate);
                    if (_isEatingRightFood)
                    {
                        Satisfy(_currentPlate.FoodOnPlate);
                    }
                    else 
                    {
                        Anger(_currentPlate.FoodOnPlate);
                    }
                }
            }
        }
    }

    public void SetPlate(Plate plate) 
    {
        _currentPlate = plate;
        plate.SetIsOccupied(true);
        plate.OnFoodPlaced.AddListener(CheckFood); // Customer will check if it's the right food
    }

    public void SetSpot(CustomerSpot spot) 
    {
        _currentSpot = spot;
    }

    private void CheckFood(Food food)
    {
        FoodType incomingMenu = food.Menu.FoodType; 

        // Check if the food is in the FavoriteMenu list
        foreach (var menuRating in _ghostType.FavoriteMenu)
        {
            //Found Designated Food
            if (menuRating.Menu.FoodType == incomingMenu)
            {
                _isEatingRightFood = true;
            }
        }

        //---Wrong Food----

        // Check if the food is in the UnfavoriteMenu list
        foreach (var menuRating in _ghostType.UnfavoriteMenu)
        {
            if (menuRating.Menu.FoodType == incomingMenu)
            {
                Debug.Log("Unfavorite food detected! Score deducted: " + menuRating.Menu.Score);
                _isEatingRightFood = false;
            }
        }

        _isEating = true;
    }

    private void Satisfy(Food food) 
    {
        if (onEatRightFood != null)
        {
            onEatRightFood.Invoke(null);
        }
        if (onLeaveRestaurant != null)
        {
            onLeaveRestaurant.Invoke(_currentSpot);
        }
    }

    private void Anger(Food food) 
    {
        //Reduce score, anger the customer ,and whatever here
    }

    private void Eat(Food food) 
    {
        _currentPlate.SetIsOccupied(false);
        Destroy(food.gameObject);
    }
}