using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Define the customer class
public class Customer : MonoBehaviour
{
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

    private bool _isEating;

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
                }
            }
        }
    }

    public void SetPlate(Plate plate) 
    {
        _currentPlate = plate;
        plate.OnFoodPlaced.AddListener(CheckFood); // Customer will check if it's the right food
    }

    private void CheckFood(Food food)
    {
        // Assume Food has a Menu reference (which could be linked to the in-game menu)
        FoodType incomingMenu = food.Menu.FoodType; // Assuming Food has a 'Menu' reference

        // Check if the food is in the FavoriteMenu list
        foreach (var menuRating in _ghostType.FavoriteMenu)
        {
            if (menuRating.Menu.FoodType == incomingMenu)
            {
                Debug.Log("Favorite food detected! Score added: " + menuRating.Menu.Score);

                _isEating = true;
                //return menuRating.Menu.Score; // Positive score for favorite food
            }
        }

        // Check if the food is in the UnfavoriteMenu list
        foreach (var menuRating in _ghostType.UnfavoriteMenu)
        {
            if (menuRating.Menu.FoodType == incomingMenu)
            {
                Debug.Log("Unfavorite food detected! Score deducted: " + menuRating.Menu.Score);
                //return menuRating.Menu.Score; // Negative score for unfavorite food
            }
        }

        // If not found in either list, return 0 (neutral score)
        Debug.Log("Neutral food. No score change.");
        //return 0f;
    }

    void Eat(Food food) 
    {
        Destroy(food.gameObject);
        if (onEatRightFood != null)
        {
            onEatRightFood.Invoke(null);
        }
    }
}