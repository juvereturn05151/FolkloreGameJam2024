using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Define the customer class
public class Customer : MonoBehaviour
{
    [Serializable] public class LeaveRestaurant : UnityEvent<CustomerSpot> { }
    public LeaveRestaurant onLeaveRestaurant;

    [Serializable] public class EatRightFood : UnityEvent<Customer> { }
    public EatRightFood onEatRightFood;

    [SerializeField] private Ghost _ghostType;
    [SerializeField] private int _patience; // Patience level (higher means more time before getting angry)
    [SerializeField] private float _orderTime; // Time it takes for the customer to place an order
    [SerializeField] private float _eatTime;   // Time it takes for the customer to finish eating

    [SerializeField] private SpriteRenderer _visual;
    
    private Plate _currentPlate;
    private CustomerSpot _currentSpot;

    private bool _isEating;
    private bool _isEatingRightFood = false;

    #region -Customer Canvas-

    [Header("Customer Request Order Canvas Elements")]
    [SerializeField] private Image orderPrefab;
    
    [SerializeField] private Image orderImageBG;
    [SerializeField] private Transform content;

    [SerializeField] private Slider patienceSlider;
    [SerializeField] private float decreasePatienceSpeed = 0.2f;
    
    #endregion
    
    private Dictionary<Menu, GameObject> menuOrder = new Dictionary<Menu, GameObject>();

    private bool isOrdering = false;
    public bool IsOrdering => isOrdering;

    private void Start()
    {
        _visual.sprite = _ghostType.Sprite;
        
        patienceSlider.maxValue = _patience;
        patienceSlider.value = _patience;

        if(!_isEatingRightFood)
            StartCoroutine(OrderThePlate());
    }

    private void Update()
    {
        if (isOrdering)
        {
            patienceSlider.value = Mathf.Lerp(patienceSlider.value, patienceSlider.value - 1, Time.deltaTime * decreasePatienceSpeed);
        }
        
        if(patienceSlider.value <= 0 && !_isEating)
            Anger();
        
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
        plate.CurrentCustomer = this;
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
        
        if (menuOrder.TryGetValue(food.Menu, out var _order))
        {
            _order.GetComponent<Image>().color = Color.gray;
        }
        
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

    private void Anger()
    {
        // Anger without eating food
        print($"{_ghostType.Name} is Anger");
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

    private IEnumerator OrderThePlate()
    {
        yield return new WaitForSeconds(_orderTime);
        orderImageBG.DOFade(1f, 0.25f);
        var _active = orderImageBG.gameObject.transform.DOMoveY(orderImageBG.transform.position.y + 0.5f, 0.25f).SetEase(Ease.InBounce);
        _active.OnComplete(() =>
        {
            foreach (var _request in _ghostType.possibleRequest)
            {
                var _order = Instantiate(orderPrefab, content);
                _order.sprite = _request.Sprite;
                menuOrder.Add(_request, _order.gameObject);
                isOrdering = true;
                patienceSlider.gameObject.transform.DOScaleY( 1f, 0.25f);
            }
        });
    }
}