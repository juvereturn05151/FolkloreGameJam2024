using System;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public class Plate : MonoBehaviour
{
    [Serializable] public class FoodPlacedEvent : UnityEvent<Food> { }
    public FoodPlacedEvent OnFoodPlaced;

    private Food _foodOnPlate;
    public Food FoodOnPlate => _foodOnPlate;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Food>() is Food food)
        {
            if (_foodOnPlate == null)
            {
                _foodOnPlate = food;
                if (_foodOnPlate != null)
                {
                    _foodOnPlate.SetFoodToBeEaten(this); 
                    OnFoodPlaced?.Invoke(_foodOnPlate); // Pass the food object as a parameter to the event
                }
            }
        }
    }
}
