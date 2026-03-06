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
    [Serializable] public class LeaveRestaurant : UnityEvent<CustomerSpot> { }
    [Serializable] public class EatRightFood : UnityEvent<Customer> { }

    private enum CustomerState
    {
        Arriving,
        Ordering,
        WaitingForFood,
        Eating,
        Leaving,
        Angry
    }

    public LeaveRestaurant onLeaveRestaurant;
    public EatRightFood onEatRightFood;

    [Header("Customer Settings")]
    [SerializeField] private Ghost ghostType;
    [SerializeField] private int patience = 10;
    [SerializeField] private float orderTime = 1f;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Animator animator;

    [Header("Customer Request Order Canvas Elements")]
    [SerializeField] private Image orderPrefab;
    [SerializeField] private Image orderImageBG;
    [SerializeField] private Transform content;
    [SerializeField] private Slider patienceSlider;
    [SerializeField] private TextMeshProUGUI desiredDonenessText;
    [SerializeField] private GameObject eatingIcon;

    [Header("Customer Feedback")]
    [SerializeField] private GameObject heart;
    [SerializeField] private GameObject brokenHeart;
    [SerializeField] private Transform heartLocation;
    [SerializeField] private Transform feedbackParent;
    [SerializeField] private GameObject satisfyFeedback;
    [SerializeField] private GameObject unsatisfyFeedback;
    [SerializeField] private ScoreFeedback scoreFeedback;

    [Header("Patience Settings")]
    [SerializeField] private float decreasePatienceSpeed = 0.2f;

    private static readonly FoodState[] DesiredFoodStates =
    {
        FoodState.Normal,
        FoodState.MediumRotten,
        FoodState.SuperRotten
    };

    private CustomerFoodPlace currentPlate;
    private CustomerSpot currentSpot;
    private FoodState desiredFoodState;
    private CustomerState currentState = CustomerState.Arriving;

    private bool isEatingRightFood;

    public bool IsEatingRightFood => isEatingRightFood;
    public bool IsOrdering => currentState == CustomerState.Ordering || currentState == CustomerState.WaitingForFood;

    private void OnEnable()
    {
        if (patienceSlider != null)
        {
            patienceSlider.onValueChanged.AddListener(OnPatienceChanged);
        }
    }

    private void OnDisable()
    {
        if (patienceSlider != null)
        {
            patienceSlider.onValueChanged.RemoveListener(OnPatienceChanged);
        }

        if (currentPlate != null)
        {
            currentPlate.OnFoodPlaced.RemoveListener(CheckFood);
        }
    }

    private void Start()
    {
        InitializeCustomer();
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        switch (currentState)
        {
            case CustomerState.Ordering:
            case CustomerState.WaitingForFood:
                HandlePatience();
                break;

            case CustomerState.Eating:
                HandleEating();
                break;
        }

        UpdateUIElements();
    }

    private void InitializeCustomer()
    {
        SoundManager.instance.PlaySFX("DoorBell");

        GenerateDesiredFoodState();
        SetupPatienceSlider();

        currentState = CustomerState.Ordering;
        StartCoroutine(OrderThePlate());
    }

    private void GenerateDesiredFoodState()
    {
        desiredFoodState = DesiredFoodStates[Random.Range(0, DesiredFoodStates.Length)];

        if (desiredDonenessText != null)
        {
            desiredDonenessText.text = desiredFoodState.ToString();
        }
    }

    private void SetupPatienceSlider()
    {
        float maxPatience = patience * ((int)desiredFoodState + 1);

        patienceSlider.maxValue = maxPatience;
        patienceSlider.value = maxPatience;
    }

    private void HandlePatience()
    {
        if (patienceSlider.value <= 0f)
            return;

        float nextValue = Mathf.Lerp(
            patienceSlider.value,
            patienceSlider.value - 1f,
            Time.deltaTime * decreasePatienceSpeed
        );

        patienceSlider.value = nextValue;
    }

    private void HandleEating()
    {
        if (currentPlate == null || currentPlate.FoodOnPlate == null)
            return;

        if (!currentPlate.FoodOnPlate.IsFinished)
            return;

        Food finishedFood = currentPlate.FoodOnPlate;
        Eat(finishedFood);

        if (isEatingRightFood)
        {
            TriggerSatisfaction(finishedFood);
        }
        else
        {
            TriggerAnger(finishedFood);
        }
    }

    private void UpdateUIElements()
    {
        if (eatingIcon != null)
        {
            eatingIcon.SetActive(currentState == CustomerState.Eating);
        }

        if (content != null)
        {
            content.gameObject.SetActive(currentState != CustomerState.Eating);
        }

        if (animator != null)
        {
            animator.SetBool("Pick", currentState == CustomerState.Eating);
        }
    }

    private void OnPatienceChanged(float value)
    {
        if (value <= 0f &&
            currentState != CustomerState.Angry &&
            currentState != CustomerState.Leaving)
        {
            TriggerAnger();
        }
    }

    public void SetPlate(CustomerFoodPlace plate)
    {
        if (currentPlate != null)
        {
            currentPlate.OnFoodPlaced.RemoveListener(CheckFood);
        }

        currentPlate = plate;
        currentPlate.CurrentCustomer = this;
        currentPlate.SetIsOccupied(true);
        currentPlate.OnFoodPlaced.AddListener(CheckFood);
    }

    public void SetSpot(CustomerSpot spot)
    {
        currentSpot = spot;
    }

    private void CheckFood(Food food)
    {
        isEatingRightFood = false;

        FoodType incomingFoodType = food.Menu.FoodType;

        foreach (var menuRating in ghostType.FavoriteMenu)
        {
            if (menuRating.Menu.FoodType != incomingFoodType)
                continue;

            isEatingRightFood = food.FoodRotting.State == desiredFoodState;

            if (isEatingRightFood)
            {
                patienceSlider.DOValue(patienceSlider.value + menuRating.Value, 1f);
            }

            break;
        }

        currentState = CustomerState.Eating;
    }

    private void TriggerSatisfaction(Food food)
    {
        currentState = CustomerState.Leaving;

        if (GameUtility.SSSAdvancedTutorialManagerExists())
        {
            if (GameManager.Instance.IsTutorial &&
                SSSAdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.ServeCustomer)
            {
                SSSAdvancedTutorialManager.Instance.serveCount++;
            }
        }

        if (animator != null)
        {
            animator.SetBool("Happy", true);
        }

        SoundManager.instance.PlaySFX("Like");

        if (heart != null && heartLocation != null)
        {
            Instantiate(heart, heartLocation.position, Quaternion.identity, heartLocation);
        }

        if (satisfyFeedback != null && feedbackParent != null)
        {
            Instantiate(satisfyFeedback, feedbackParent);
        }

        if (orderImageBG != null && orderImageBG.TryGetComponent(out Animator bgAnimator))
        {
            bgAnimator.SetTrigger("Right");
        }

        StartCoroutine(LeaveAfterDelay(food));
    }

    private void TriggerAnger()
    {
        currentState = CustomerState.Angry;

        if (animator != null)
        {
            animator.SetBool("Anger", true);
        }

        HPManager.Instance.TakeDamage(1);
        GameManager.Instance.DecreaseScore(15);

        if (currentPlate != null)
        {
            currentPlate.SetIsOccupied(false);
        }

        onLeaveRestaurant?.Invoke(currentSpot);
    }

    private void TriggerAnger(Food food)
    {
        if (animator != null)
        {
            animator.SetTrigger("Anger");
        }

        SoundManager.instance.PlaySFX("Nah");

        if (unsatisfyFeedback != null && feedbackParent != null)
        {
            Instantiate(unsatisfyFeedback, feedbackParent);
        }

        if (brokenHeart != null && heartLocation != null)
        {
            Instantiate(brokenHeart, heartLocation.position, Quaternion.identity, heartLocation);
        }

        if (orderImageBG != null && orderImageBG.TryGetComponent(out Animator bgAnimator))
        {
            bgAnimator.SetTrigger("Wrong");
        }

        float decreaseValue = patienceSlider.value / 2f;
        patienceSlider.DOValue(decreaseValue, 1f).SetEase(Ease.OutSine);
        patienceSlider.transform.DOShakePosition(1f, new Vector3(0.25f, 0.25f, 0f));

        GameManager.Instance.DecreaseScore(15);
        transform.DOShakePosition(1f, 0.5f);

        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.5f, 1f);
        }

        currentState = CustomerState.WaitingForFood;
    }

    private void Eat(Food food)
    {
        Destroy(food.gameObject);
    }

    private IEnumerator LeaveAfterDelay(Food food)
    {
        yield return new WaitForSeconds(1f);

        int scoreWithPatience = food.Menu.Score + (int)patienceSlider.value;

        if (scoreFeedback != null)
        {
            GameObject scoreFeedbackObj = Instantiate(scoreFeedback.gameObject, transform.position, transform.rotation);

            if (scoreFeedbackObj.TryGetComponent(out ScoreFeedback feedback))
            {
                feedback.SetScore(scoreWithPatience);
            }
        }

        GameManager.Instance.IncreaseScore(scoreWithPatience);
        onEatRightFood?.Invoke(this);
        onLeaveRestaurant?.Invoke(currentSpot);
    }

    private IEnumerator OrderThePlate()
    {
        yield return new WaitForSeconds(orderTime);

        if (orderImageBG != null)
        {
            orderImageBG.DOFade(1f, 0.25f);

            Tween activeTween = orderImageBG.transform
                .DOMoveY(orderImageBG.transform.position.y + 0.5f, 0.25f)
                .SetEase(Ease.InBounce);

            activeTween.OnComplete(() =>
            {
                SpawnOrderImages();
                currentState = CustomerState.WaitingForFood;

                if (patienceSlider != null)
                {
                    patienceSlider.transform.DOScaleY(1f, 0.25f);
                }
            });
        }
        else
        {
            SpawnOrderImages();
            currentState = CustomerState.WaitingForFood;
        }
    }

    private void SpawnOrderImages()
    {
        foreach (var request in ghostType.FavoriteMenu)
        {
            Image order = Instantiate(orderPrefab, content);

            switch (desiredFoodState)
            {
                case FoodState.MediumRotten:
                    order.sprite = request.Menu.MediumRottenSprite;
                    break;

                case FoodState.SuperRotten:
                    order.sprite = request.Menu.SuperRottenSprite;
                    break;

                default:
                    order.sprite = request.Menu.Sprite;
                    break;
            }
        }
    }
}