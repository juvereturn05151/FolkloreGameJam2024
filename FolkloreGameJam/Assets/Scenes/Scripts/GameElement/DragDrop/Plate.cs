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
                    SetFoodPosition(_foodOnPlate);
                    OnFoodPlaced?.Invoke(_foodOnPlate); // Pass the food object as a parameter to the event
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Food>())
        {
            _foodOnPlate = null;
        }
    }

    public void SetFoodPosition(Food food)
    {
        food.transform.position = transform.position;
        food.transform.SetParent(transform);
        food.transform.localPosition = Vector3.zero;
        food.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        food.GetComponent<Rigidbody2D>().gravityScale = 0;
        food.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;
        food.SetIsReadyToEat(true);
    }
}
