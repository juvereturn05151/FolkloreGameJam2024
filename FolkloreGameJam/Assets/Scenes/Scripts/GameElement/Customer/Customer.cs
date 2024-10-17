using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;



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

    [SerializeField] private SpriteRenderer _visual;
    
    private Plate _currentPlate;
    private CustomerSpot _currentSpot;

    private bool _isEating;
    public bool IsEating => _isEating;
    private bool _isEatingRightFood = false;

    #region -Customer Canvas-

    [Header("Customer Request Order Canvas Elements")]
    [SerializeField] private Image orderPrefab;
    
    [SerializeField] private Image orderImageBG;
    [SerializeField] private Transform content;

    [SerializeField] private Slider patienceSlider;
    public Slider PatienceSlider => patienceSlider;
    [SerializeField] private float decreasePatienceSpeed = 0.2f;

    [SerializeField] private TextMeshProUGUI _desiredDonenessText;

    #endregion
    
    private Dictionary<Menu, GameObject> menuOrder = new Dictionary<Menu, GameObject>();

    private bool isOrdering = false;
    public bool IsOrdering => isOrdering;

    private UnityAction OnAngry;

    private FoodState _desiredFoodState;

    private void Start()
    {
        _visual.sprite = _ghostType.Sprite;

        GenerateFoodState();

        patienceSlider.maxValue = _patience * ((int)_desiredFoodState + 1);
        patienceSlider.value = _patience * ((int)_desiredFoodState + 1);

        if(!_isEatingRightFood)
            StartCoroutine(OrderThePlate());

        patienceSlider.onValueChanged.AddListener((_value) =>
        {
            if (_value <= 0)
            {
                Anger();
            }
        });


    }


    private void Update()
    {
        if(GameManager.Instance.IsGameOver) return;
        
        if (isOrdering && !_isEating)
        {
            if(patienceSlider.value <= 0) return;
            patienceSlider.value = Mathf.Lerp(patienceSlider.value, patienceSlider.value - 1, Time.deltaTime * decreasePatienceSpeed);
        }
        
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

    private void GenerateFoodState() 
    {
        int foodStateCount = Enum.GetValues(typeof(FoodState)).Length - 1;

        // Generate a random index
        int randomIndex = Random.Range(0, foodStateCount);

        _desiredFoodState = (FoodState)randomIndex;
        _desiredDonenessText.text = _desiredFoodState.ToString();
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
                if (food.FoodState == _desiredFoodState)
                {
                    _isEatingRightFood = true;
                    patienceSlider.DOValue(patienceSlider.value + menuRating.Value, 1f);
                }
                else 
                {
                    _isEatingRightFood = false;
                }
            }
        }

        _isEating = true;
    }

    private void Satisfy(Food food) 
    {
        if (GameManager.Instance.IsTutorial && AdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.ServeCustomer) 
        {
            AdvancedTutorialManager.Instance.serveCount++;
        }

        if (onEatRightFood != null)
        {
            var _scoreWithPatience = (food.Menu.Score + (int) patienceSlider.value); // if rotten 
            GameManager.Instance.IncreaseScore(_scoreWithPatience);
            onEatRightFood.Invoke(null);
        }
        if (onLeaveRestaurant != null)
        {
            onLeaveRestaurant.Invoke(_currentSpot);
        }
    }

    private void Anger()
    {
        // Anger without eating food or patience is <= 0
        //decrease health point or something with anger ghost
        
        HPManager.Instance.TakeDamage(1);
        GameManager.Instance.DecreaseScore(15);
        _currentPlate.SetIsOccupied(false);
        onLeaveRestaurant?.Invoke(_currentSpot);
    }

    private void Anger(Food food)
    {
        // anger if didn't eat the right food
        //Reduce score, anger the customer ,and whatever here
        var _decreaseValue = patienceSlider.value / 2; 
        patienceSlider.DOValue(_decreaseValue, 1f).SetEase(Ease.OutSine);
        patienceSlider.gameObject.transform.DOShakePosition(1f, new Vector3(0.25f, 0.25f, 0));
        Camera.main.DOShakePosition(0.5f, 1f);

        GameManager.Instance.DecreaseScore(15);
        // HPManager.Instance.TakeDamage(1);


        _isEating = false;
    }

    private void Eat(Food food) 
    {
        // _currentPlate.SetIsOccupied(false);
        Destroy(food.gameObject);
    }

    private IEnumerator OrderThePlate()
    {
        yield return new WaitForSeconds(_orderTime);
        orderImageBG.DOFade(1f, 0.25f);
        //_desiredDonenessText.gameObject.SetActive(true);
        var _active = orderImageBG.gameObject.transform.DOMoveY(orderImageBG.transform.position.y + 0.5f, 0.25f).SetEase(Ease.InBounce);
        _active.OnComplete(() =>
        {
            foreach (var _request in _ghostType.FavoriteMenu)
            {
                var _order = Instantiate(orderPrefab, content);


                if (_desiredFoodState == FoodState.MediumRare)
                {
                    _order.sprite = _request.Menu.MediumRottenSprite;
                }
                else if (_desiredFoodState == FoodState.WellDone)
                {
                    _order.sprite = _request.Menu.SuperRottenSprite;
                }
                else 
                {
                    _order.sprite = _request.Menu.Sprite;
                }

                menuOrder.Add(_request.Menu, _order.gameObject);
                isOrdering = true;
                patienceSlider.gameObject.transform.DOScaleY( 1f, 0.25f);
            }
        });
    }
}