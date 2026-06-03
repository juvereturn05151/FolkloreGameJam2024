using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(FoodRotting))]
[RequireComponent(typeof(FoodEating))]
public class Food : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private Menu menu;
    public Menu Menu => menu;

    [Header("References")]
    [SerializeField] 
    private Rigidbody2D rigidBody2D;
    [SerializeField] 
    private Collider2D servingCollision;
    [SerializeField] 
    private FoodRotting foodRotting;
    public FoodRotting FoodRotting => foodRotting;
    [SerializeField] 
    private FoodEating foodEating;
    [SerializeField] 
    private FoodVisuals foodVisuals;
    [SerializeField] 
    private FoodScoringOnExpire foodScoring;
    [SerializeField] 
    private Draggable2D draggable2D;
    [SerializeField, FormerlySerializedAs("IsPremiumFood")]
    private bool isPremiumFood = false;
    public bool IsPremiumFood => isPremiumFood;

    [Header("Universal Food")]
    [SerializeField] private bool isUniversalFood;
    [SerializeField] private int universalScore = 40;
    [SerializeField] private float universalScoreMultiplier = 1f;
    [SerializeField] private Sprite universalSprite;
    [SerializeField] private bool useUniversalGlow = true;

    private CustomerFoodPlace currentPlate;

    public float UniversalScoreMultiplier => universalScoreMultiplier;
    public bool IsUniversalFood => isUniversalFood;
    public bool IsReadyToEat { get; private set; } 
    public bool IsFinished { get; private set; }

    private void OnEnable()
    {
        IsFinished = false;
        IsReadyToEat = false;

        if (foodRotting != null)
        {
            foodRotting.SetCanRot(!IsPremiumFood);
        }

        if (IsPremiumFood)
        {
            foodVisuals?.HideRotUI();
        }

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

        if (isUniversalFood)
        {
            universalScoreMultiplier = Mathf.Max(1f, universalScoreMultiplier);
            foodVisuals?.ApplyUniversalFoodVisuals(universalSprite, useUniversalGlow);
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
        if (IsReadyToEat)
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
            foodVisuals?.HideRotUI();
        }
    }

    private void HandleDragStarted()
    {
        if (IsReadyToEat) 
        {
            return;
        }
            
        SoundManager.instance.PlaySFX("SFX_WhenPickUpItem");

        if (GameUtility.DragAndDropManagerExists())
        {
            DragAndDropManager.Instance.isDragging = true;
            DragAndDropManager.Instance.UseHandCursor();
        }
    }

    private void HandleDragEnded()
    {
        if (GameUtility.DragAndDropManagerExists())
        {
            DragAndDropManager.Instance.isDragging = false;
            DragAndDropManager.Instance.UseKnifeCursor();
        }

        TryServeCurrentPlate();
    }

    private void HandleDragCancelled()
    {
        if (GameUtility.DragAndDropManagerExists())
        {
            DragAndDropManager.Instance.isDragging = false;
            DragAndDropManager.Instance.UseKnifeCursor();
        }
    }

    private void HandleFinishedEating()
    {
        IsFinished = true;
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
            draggable2D.SnapTo(plate.transform, new Vector3(0f, 1.0f, 0f));
            draggable2D.SetCanDrag(false);
        }
        else
        {
            transform.SetParent(plate.transform);
            transform.localPosition = new Vector3(0f, 1.0f, 0f);
        }

        if (rigidBody2D != null)
        {
            rigidBody2D.linearVelocity = Vector2.zero;
            rigidBody2D.gravityScale = 0f;
            rigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }

        IsReadyToEat = true;

        SoundManager.instance.PlaySFX("Eating");

        if (servingCollision != null)
        {
            servingCollision.enabled = false;
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
        {
            return;
        }

        if (currentPlate == null)
        {
            currentPlate = plate;
            currentPlate.OnFoodInOnPlate();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out CustomerFoodPlace plate))
            return;

        if (currentPlate == plate)
        {
            currentPlate.OnFoodIsOffPlate();
            currentPlate = null;
        }
    }

    private void TryServeCurrentPlate()
    {
        if (IsReadyToEat || currentPlate == null)
        {
            return;
        }

        if (currentPlate.canBeDropped())
        {
            currentPlate.PrepareToEat(this);
        }
    }

    public int GetServeScore(int patienceBonus)
    {
        int baseScore = IsUniversalFood ? Mathf.Max(0, universalScore) : menu != null ? menu.Score : 0;
        int score = baseScore + patienceBonus;
        return IsUniversalFood ? Mathf.RoundToInt(score * UniversalScoreMultiplier) : score;
    }
}
