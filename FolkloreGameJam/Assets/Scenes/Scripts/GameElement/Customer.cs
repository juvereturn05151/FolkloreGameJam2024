using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define the customer class
public class Customer : MonoBehaviour
{
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
    [SerializeField]
    private Plate _currentPlate;

    private void Start()
    {
        _visual.sprite = _ghostType.Sprite;
    }

}