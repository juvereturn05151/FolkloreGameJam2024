using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Customer : MonoBehaviour
{
    // Unity Events
    [Serializable] public class LeaveRestaurant : UnityEvent<CustomerSpot> { }
    [Serializable] public class EatRightFood : UnityEvent<Customer> { }

    public LeaveRestaurant onLeaveRestaurant;
    public EatRightFood onEatRightFood;

    // Serialized Fields
    [Header("Customer Settings")]
    [SerializeField] private Ghost _ghostType;
    [SerializeField] private int _patience; // Patience level (higher means more time before getting angry)
    [SerializeField] private float _orderTime; // Time it takes for the customer to place an order
    [SerializeField] private SpriteRenderer _visual;
    [SerializeField] private Animator _animator;
    
    [Header("Customer Request Order Canvas Elements")]
    [SerializeField] private Image orderPrefab;
    [SerializeField] private Image orderImageBG;
    [SerializeField] private Transform content;
    [SerializeField] private Slider patienceSlider;
    [SerializeField] private TextMeshProUGUI _desiredDonenessText;
    [SerializeField] private GameObject eatingIcon;

    [Header("Customer Feedback")]
    [SerializeField] private GameObject heart;
    [SerializeField] private GameObject brokenHeart;
    [SerializeField] private Transform heartLocation;
    [SerializeField] private Transform feedbackParent;
    [SerializeField] private GameObject _satisfyFeedback;
    [SerializeField] private GameObject _unsatisfyFeedback;
    [SerializeField] private ScoreFeedback _scoreFeedback;

    [Header("Patience Settings")]
    [SerializeField] private float decreasePatienceSpeed = 0.2f;

    private Plate _currentPlate;
    private CustomerSpot _currentSpot;
    private FoodState _desiredFoodState;
    private bool _isEating;
    private bool _isEatingRightFood = false;
    private bool _isOrdering = false;

    public bool IsEatingRightFood => _isEatingRightFood;
    public bool IsOrdering => _isOrdering;

    private void Start()
    {
        InitializeCustomer();
    }

    private void Update()
    {
        if(GameManager.Instance.IsGameOver) return;

        HandlePatience();
        UpdateUIElements();
        HandleEating();
    }

    #region Initialization

    private void InitializeCustomer()
    {
        SoundManager.instance.PlaySFX("DoorBell");
        GenerateDesiredFoodState();
        SetupPatienceSlider();

        if (!_isEatingRightFood) 
        {
            StartCoroutine(OrderThePlate());
        }

        patienceSlider.onValueChanged.AddListener(OnPatienceChanged);
    }

    private void GenerateDesiredFoodState()
    {
        int foodStateCount = Enum.GetValues(typeof(FoodState)).Length - 1;
        _desiredFoodState = (FoodState)Random.Range(0, foodStateCount);
        _desiredDonenessText.text = _desiredFoodState.ToString();
    }

    private void SetupPatienceSlider()
    {
        float maxPatience = _patience * ((int)_desiredFoodState + 1);
        patienceSlider.maxValue = maxPatience;
        patienceSlider.value = maxPatience;
    }

    #endregion

    #region Update Logic

    private void HandlePatience()
    {
        if (_isOrdering && !_isEating && patienceSlider.value > 0)
        {
            patienceSlider.value = Mathf.Lerp(patienceSlider.value, patienceSlider.value - 1, Time.deltaTime * decreasePatienceSpeed);
        }
    }

    private void UpdateUIElements()
    {
        eatingIcon.SetActive(_isEating);
        content.gameObject.SetActive(!_isEating);

        if (_animator != null)
        {
            _animator.SetBool("Pick", _isEating);
        }
    }

    private void HandleEating() 
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
                        TriggerSatisfaction(_currentPlate.FoodOnPlate);
                    }
                    else
                    {
                        TriggerAnger(_currentPlate.FoodOnPlate);
                    }
                }
            }
        }
    }

    #endregion

    #region Patience Events

    private void OnPatienceChanged(float value)
    {
        if (value <= 0)
        {
            TriggerAnger();
        }
    }

    #endregion

    #region Plate and Food Interaction
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

    #endregion

    #region Customer Reactions

    private void TriggerSatisfaction(Food food)
    {
        if (GameManager.Instance.IsTutorial && SSSAdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.ServeCustomer)
        {
            SSSAdvancedTutorialManager.Instance.serveCount++;
        }

        if (_animator != null)
        {
            _animator.SetBool("Happy", true);
        }

        SoundManager.instance.PlaySFX("Like");
        Instantiate(heart, heart.transform.position, heart.transform.rotation, heartLocation);
        Instantiate(_satisfyFeedback, feedbackParent);
        orderImageBG.GetComponent<Animator>().SetTrigger("Right");

        StartCoroutine(LeaveAfterDelay(food));
    }

    private void TriggerAnger()
    {
        // Anger without eating food or patience is <= 0
        //decrease health point or something with anger ghost
        if (_animator != null)
        {
            _animator.SetBool("Anger", true);
        }

        HPManager.Instance.TakeDamage(1);
        GameManager.Instance.DecreaseScore(15);
        _currentPlate.SetIsOccupied(false);
        onLeaveRestaurant?.Invoke(_currentSpot);
    }

    private void TriggerAnger(Food food)
    {
        // anger if didn't eat the right food
        //Reduce score, anger the customer ,and whatever here
        if (_animator != null)
        {
            _animator.SetTrigger("Anger");
        }

        SoundManager.instance.PlaySFX("Nah");
        Instantiate(_unsatisfyFeedback, feedbackParent);
        Instantiate(brokenHeart, heartLocation.transform.position, Quaternion.identity, heartLocation);
        orderImageBG.GetComponent<Animator>().SetTrigger("Wrong");
        var _decreaseValue = patienceSlider.value / 2;
        patienceSlider.DOValue(_decreaseValue, 1f).SetEase(Ease.OutSine);
        patienceSlider.gameObject.transform.DOShakePosition(1f, new Vector3(0.25f, 0.25f, 0));
        GameManager.Instance.DecreaseScore(15);
        transform.DOShakePosition(1f, 0.5f);

        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.5f, 1f);
        }

        _isEating = false;
    }

    private void Eat(Food food)
    {
        Destroy(food.gameObject);
    }

    private IEnumerator LeaveAfterDelay(Food food)
    {
        yield return new WaitForSeconds(1.0f); // Adjust the delay time as needed (2 seconds in this case)

        if (onEatRightFood != null)
        {
            var _scoreWithPatience = (food.Menu.Score + (int)patienceSlider.value); // if rotten 

            GameObject scoreFeedbackObj = Instantiate(_scoreFeedback.gameObject, transform.position, transform.rotation);

            if (scoreFeedbackObj.GetComponent<ScoreFeedback>() is ScoreFeedback scoreFeedback)
            {
                // Set the score value
                scoreFeedback.SetScore(_scoreWithPatience);  // Example score value
            }

            GameManager.Instance.IncreaseScore(_scoreWithPatience);
            onEatRightFood.Invoke(null);
        }

        if (onLeaveRestaurant != null)
        {
            onLeaveRestaurant.Invoke(_currentSpot);
        }
    }

    #endregion

    #region Ordering Logic

    private IEnumerator OrderThePlate()
    {
        yield return new WaitForSeconds(_orderTime);
        orderImageBG.DOFade(1f, 0.25f);
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

                _isOrdering = true;
                patienceSlider.gameObject.transform.DOScaleY(1f, 0.25f);
            }
        });
    }

    #endregion
}