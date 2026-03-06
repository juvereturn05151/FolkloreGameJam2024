using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    public enum HungryLevel
    {
        Normal,
        Hungry,
        SuperHungry
    }

    private static readonly FoodState[] DesiredFoodStates =
    {
        FoodState.Normal,
        FoodState.MediumRotten,
        FoodState.SuperRotten
    };

    public LeaveRestaurant onLeaveRestaurant;
    public EatRightFood onEatRightFood;

    [Header("Customer Settings")]
    [SerializeField] private Ghost ghostType;
    [SerializeField] private int patience = 10;
    [SerializeField] private float orderTime = 1f;
    [SerializeField] private SpriteRenderer visual;

    [Header("Customer Appetite")]
    [SerializeField] private HungryLevel hungryLevel = HungryLevel.Normal;

    [Header("Controllers")]
    [SerializeField] private CustomerOrderGenerator orderGenerator;
    [SerializeField] private CustomerPatienceController patienceController;
    [SerializeField] private CustomerOrderUI orderUI;
    [SerializeField] private CustomerFeedbackController feedbackController;

    private readonly List<CustomerOrder> currentOrders = new();

    private CustomerFoodPlace currentPlate;
    private CustomerSpot currentSpot;
    private FoodState desiredFoodState;
    private CustomerState currentState = CustomerState.Arriving;
    private bool isEatingRightFood;

    public bool IsEatingRightFood => isEatingRightFood;
    public bool IsOrdering => currentState == CustomerState.Ordering || currentState == CustomerState.WaitingForFood;

    private void OnEnable()
    {
        if (patienceController != null)
        {
            patienceController.onPatienceDepleted.AddListener(OnPatienceDepleted);
        }
    }

    private void OnDisable()
    {
        if (patienceController != null)
        {
            patienceController.onPatienceDepleted.RemoveListener(OnPatienceDepleted);
        }

        UnregisterPlateListener();
    }

    private void Start()
    {
        InitializeCustomer();
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        UpdateStateLogic();
        UpdateVisualState();
    }

    private void InitializeCustomer()
    {
        SoundManager.instance.PlaySFX("DoorBell");

        ApplyGhostVisual();
        GenerateDesiredFoodState();
        GenerateOrders();
        SetupPatience();

        currentState = CustomerState.Ordering;
        StartCoroutine(OrderRoutine());
    }

    private void UpdateStateLogic()
    {
        switch (currentState)
        {
            case CustomerState.Ordering:
            case CustomerState.WaitingForFood:
                patienceController?.Tick();
                break;

            case CustomerState.Eating:
                ProcessEatingState();
                break;
        }
    }

    private void UpdateVisualState()
    {
        bool isEating = currentState == CustomerState.Eating;
        orderUI?.SetEatingState(isEating);
        feedbackController?.SetEatingAnimation(isEating);
    }

    private void ApplyGhostVisual()
    {
        if (visual != null && ghostType != null && ghostType.Sprite != null)
        {
            visual.sprite = ghostType.Sprite;
        }
    }

    private void GenerateDesiredFoodState()
    {
        desiredFoodState = DesiredFoodStates[Random.Range(0, DesiredFoodStates.Length)];
        orderUI?.SetDesiredFoodState(desiredFoodState);
    }

    private void GenerateOrders()
    {
        currentOrders.Clear();

        if (orderGenerator == null)
            return;

        List<CustomerOrder> generatedOrders = orderGenerator.GenerateOrders(ghostType, hungryLevel);
        currentOrders.AddRange(generatedOrders);
    }

    private void SetupPatience()
    {
        patienceController?.Setup(patience, desiredFoodState, currentOrders.Count);
    }

    public void SetPlate(CustomerFoodPlace plate)
    {
        UnregisterPlateListener();

        currentPlate = plate;

        if (currentPlate == null)
            return;

        currentPlate.CurrentCustomer = this;
        currentPlate.SetIsOccupied(true);
        currentPlate.OnFoodPlaced.AddListener(CheckFood);
    }

    public void SetSpot(CustomerSpot spot)
    {
        currentSpot = spot;
    }

    private void UnregisterPlateListener()
    {
        if (currentPlate != null)
        {
            currentPlate.OnFoodPlaced.RemoveListener(CheckFood);
        }
    }

    private void CheckFood(Food food)
    {
        if (food == null || food.Menu == null)
            return;

        isEatingRightFood = false;

        CustomerOrder matchedOrder = FindMatchingOrder(food);
        if (matchedOrder != null)
        {
            isEatingRightFood = true;
            patienceController?.Reward(matchedOrder.RewardValue);
            currentOrders.Remove(matchedOrder);
            orderUI?.RemoveOrderImage(matchedOrder, desiredFoodState);
        }

        currentState = CustomerState.Eating;
    }

    private CustomerOrder FindMatchingOrder(Food food)
    {
        foreach (CustomerOrder order in currentOrders)
        {
            if (order == null || order.Menu == null)
                continue;

            bool correctMenu = order.Menu == food.Menu;
            bool correctState = food.FoodRotting.State == desiredFoodState;

            if (correctMenu && correctState)
            {
                return order;
            }
        }

        return null;
    }

    private void ProcessEatingState()
    {
        if (!HasFinishedFoodOnPlate(out Food finishedFood))
            return;

        Eat(finishedFood);

        if (!isEatingRightFood)
        {
            HandleWrongFood();
            return;
        }

        HandleSatisfied();

        if (currentOrders.Count > 0)
        {
            currentState = CustomerState.WaitingForFood;
            return;
        }

        HandleLeaving(finishedFood);
    }

    private bool HasFinishedFoodOnPlate(out Food finishedFood)
    {
        finishedFood = null;

        if (currentPlate == null || currentPlate.FoodOnPlate == null)
            return false;

        if (!currentPlate.FoodOnPlate.IsFinished)
            return false;

        finishedFood = currentPlate.FoodOnPlate;
        return true;
    }

    private void HandleWrongFood()
    {
        feedbackController?.PlayWrongFoodFeedback();
        orderUI?.TriggerWrong();
        patienceController?.PenalizeHalf();

        GameManager.Instance.DecreaseScore(15);
        currentState = CustomerState.WaitingForFood;
    }

    private void HandleSatisfied()
    {
        HandleTutorialServeProgress();
        feedbackController?.PlaySatisfiedFeedback();
        orderUI?.TriggerRight();
    }

    private void HandleLeaving(Food food) 
    {
        currentState = CustomerState.Leaving;
        StartCoroutine(LeaveAfterDelay(food));
    }

    private void HandleTutorialServeProgress()
    {
        if (!GameUtility.SSSAdvancedTutorialManagerExists())
            return;

        if (!GameManager.Instance.IsTutorial)
            return;

        if (SSSAdvancedTutorialManager.Instance.CurrentTutorial.Type != TutorialType.ServeCustomer)
            return;

        SSSAdvancedTutorialManager.Instance.serveCount++;
    }

    private void OnPatienceDepleted()
    {
        if (currentState == CustomerState.Angry || currentState == CustomerState.Leaving)
            return;

        currentState = CustomerState.Angry;

        feedbackController?.PlayTimeoutFeedback();

        HPManager.Instance.TakeDamage(1);
        GameManager.Instance.DecreaseScore(15);

        if (currentPlate != null)
        {
            currentPlate.SetIsOccupied(false);
        }

        onLeaveRestaurant?.Invoke(currentSpot);
    }

    private void Eat(Food food)
    {
        if (food != null)
        {
            Destroy(food.gameObject);
        }
    }

    private IEnumerator LeaveAfterDelay(Food food)
    {
        yield return new WaitForSeconds(1f);

        int patienceBonus = patienceController != null ? (int)patienceController.CurrentValue : 0;
        int scoreWithPatience = food.Menu.Score + patienceBonus;

        feedbackController?.SpawnScoreFeedback(scoreWithPatience);

        GameManager.Instance.IncreaseScore(scoreWithPatience);
        onEatRightFood?.Invoke(this);
        onLeaveRestaurant?.Invoke(currentSpot);
    }

    private IEnumerator OrderRoutine()
    {
        yield return new WaitForSeconds(orderTime);

        orderUI?.AnimateOrderPopup(() =>
        {
            orderUI.ShowOrders(currentOrders, desiredFoodState);
            currentState = CustomerState.WaitingForFood;
        });
    }
}