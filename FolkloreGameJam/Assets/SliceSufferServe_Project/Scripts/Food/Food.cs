using UnityEngine;

[RequireComponent(typeof(FoodRotting))]
[RequireComponent(typeof(FoodEating))]
public class Food : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private Menu menu;
    public Menu Menu => menu;

    [Header("References")]
    [SerializeField] private Rigidbody2D rigidBody2D;
    [SerializeField] private Collider2D collider2D;
    [SerializeField] private FoodRotting foodRotting;
    [SerializeField] private FoodEating foodEating;
    [SerializeField] private FoodVisuals foodVisuals;
    [SerializeField] private FoodScoringOnExpire foodScoring;
    [SerializeField] private Draggable2D draggable2D;

    public FoodRotting FoodRotting => foodRotting;
    public bool IsUniversalFood { get; private set; }
    public bool IsPremiumFood;
    public float UniversalScoreMultiplier { get; private set; } = 1f;

    private bool _isReadyToEat;
    public bool IsReadyToEat => _isReadyToEat;

    private bool _isFinished;
    public bool IsFinished => _isFinished;

    public bool IsDragging => draggable2D != null && draggable2D.IsDragging;
    public bool IsSnapped => draggable2D != null && draggable2D.IsSnapped;
    private CustomerFoodPlace _currentPlate;

    private void Reset()
    {
        foodRotting = GetComponent<FoodRotting>();
        foodEating = GetComponent<FoodEating>();
        rigidBody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        _isFinished = false;
        _isReadyToEat = false;

        if (foodRotting != null && foodVisuals != null)
        {
            foodRotting.OnStateChanged += foodVisuals.ApplyState;
            foodRotting.OnExpired += HandleExpired;
        }

        if (foodEating != null)
        {
            foodEating.OnFinished += HandleFinishedEating;
        }

        if (draggable2D != null)
        {
            draggable2D.DragStarted += HandleDragStarted;
            draggable2D.DragEnded += HandleDragEnded;
            draggable2D.DragCancelled += HandleDragCancelled;
        }
    }

    private void OnDisable()
    {
        if (foodRotting != null && foodVisuals != null)
        {
            foodRotting.OnStateChanged -= foodVisuals.ApplyState;
            foodRotting.OnExpired -= HandleExpired;
        }

        if (foodEating != null)
        {
            foodEating.OnFinished -= HandleFinishedEating;
        }

        if (draggable2D != null)
        {
            draggable2D.DragStarted -= HandleDragStarted;
            draggable2D.DragEnded -= HandleDragEnded;
            draggable2D.DragCancelled -= HandleDragCancelled;
        }
    }

    private void Update()
    {
        if (_isReadyToEat)
        {
            foodEating.Tick(Time.deltaTime);
            return;
        }

        if (!IsPremiumFood)
        {
            foodRotting.Tick(Time.deltaTime);

            if (foodVisuals != null)
            {
                foodVisuals.UpdateRotSlider(foodRotting.Remaining, foodRotting.BaseRottenTime);
            }
        }
        else 
        {
            foodVisuals.HideRotUI();
        }


    }

    private void HandleDragStarted()
    {
        if (_isReadyToEat)
            return;

        SoundManager.instance.PlaySFX("SFX_WhenPickUpItem");

        if (GameUtility.DragAndDropManagerExists())
        {
            DragAndDropManager.Instance.isDragging = true;
        }
    }

    private void HandleDragEnded()
    {
        if (GameUtility.DragAndDropManagerExists())
        {
            DragAndDropManager.Instance.isDragging = false;
        }

        TryServeCurrentPlate();
    }

    private void HandleDragCancelled()
    {
        if (GameUtility.DragAndDropManagerExists())
        {
            DragAndDropManager.Instance.isDragging = false;
        }
    }

    private void HandleFinishedEating()
    {
        _isFinished = true;
    }

    private void HandleExpired()
    {
        if (foodScoring != null)
        {
            foodScoring.ApplyPenalty(transform.position, transform.rotation);
        }

        if (foodVisuals != null)
        {
            foodVisuals.SpawnDust(transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    public void SnapToPlate(CustomerFoodPlace plate, bool eatingRightFood)
    {
        if (draggable2D != null)
        {
            draggable2D.SnapTo(plate.transform, new Vector3(0f, 0.86f, 0f));
            draggable2D.SetCanDrag(false);
        }
        else
        {
            transform.SetParent(plate.transform);
            transform.localPosition = new Vector3(0f, 0.86f, 0f);
        }

        if (rigidBody2D != null)
        {
            rigidBody2D.linearVelocity = Vector2.zero;
            rigidBody2D.gravityScale = 0f;
            rigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }

        _isReadyToEat = true;

        SoundManager.instance.PlaySFX("Eating");

        if (collider2D != null)
        {
            collider2D.enabled = false;
        }

        if (foodVisuals != null)
        {
            foodVisuals.HideRotUI();
        }

        if (foodEating != null)
        {
            foodEating.Begin(eatingRightFood);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out CustomerFoodPlace plate))
            return;

        if (_currentPlate == null)
        {
            _currentPlate = plate;
            _currentPlate.OnFoodInOnPlate();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out CustomerFoodPlace plate))
            return;

        if (_currentPlate == plate)
        {
            _currentPlate.OnFoodIsOffPlate();
            _currentPlate = null;
        }
    }

    private void TryServeCurrentPlate()
    {
        if (_isReadyToEat || _currentPlate == null)
        {
            return;
        }

        if (_currentPlate.canBeDropped())
        {
            _currentPlate.PrepareToEat(this);
        }
    }

    public void MakeGoldenOrgan(float scoreMultiplier = 2f, Sprite goldenSprite = null)
    {
        MakeUniversalFood(scoreMultiplier, goldenSprite, true);
    }

    public void MakeUniversalFood(float scoreMultiplier = 1f, Sprite universalSprite = null, bool useGlow = false)
    {
        IsUniversalFood = true;
        UniversalScoreMultiplier = Mathf.Max(1f, scoreMultiplier);
        foodVisuals?.ApplyUniversalFoodVisuals(universalSprite, useGlow);
    }

    public int GetServeScore(int patienceBonus)
    {
        int baseScore = menu != null ? menu.Score : 0;
        int score = baseScore + patienceBonus;
        return IsUniversalFood ? Mathf.RoundToInt(score * UniversalScoreMultiplier) : score;
    }
}
