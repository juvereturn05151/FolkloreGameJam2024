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
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private FoodRotting foodRotting;
    [SerializeField] private FoodEating foodEating;
    [SerializeField] private FoodVisuals foodVisuals;
    [SerializeField] private FoodScoringOnExpire foodScoring;

    public FoodRotting FoodRotting => foodRotting;

    private bool _isReadyToEat;
    public bool IsReadyToEat => _isReadyToEat;

    private bool _isFinished;
    public bool IsFinished => _isFinished;

    private bool _isDragging;
    public bool IsDragging => _isDragging;

    private bool _canDrag = true;
    private Plate _currentPlate;

    private void Reset()
    {
        foodRotting = GetComponent<FoodRotting>();
        foodEating = GetComponent<FoodEating>();
        rigidBody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        _isFinished = false;
        _isReadyToEat = false;
        _isDragging = false;

        if (foodRotting != null && foodVisuals != null)
        {
            foodRotting.OnStateChanged += foodVisuals.ApplyState;
            foodRotting.OnExpired += HandleExpired;
        }

        if (foodEating != null)
        {
            foodEating.OnFinished += HandleFinishedEating;
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
    }

    private void Update()
    {
        if (_isReadyToEat)
        {
            foodEating.Tick(Time.deltaTime);
            return;
        }

        foodRotting.Tick(Time.deltaTime);

        if (foodVisuals != null)
        {
            foodVisuals.UpdateRotSlider(foodRotting.Remaining, foodRotting.BaseRottenTime);
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

    private void OnMouseDown()
    {
        if (!_canDrag || _isReadyToEat)
            return;

        SoundManager.instance.PlaySFX("SFX_WhenPickUpItem");

        if (GameUtility.DragAndDropManagerExists())
        {
            DragAndDropManager.Instance.isDragging = true;
        }

        _isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (_isDragging && !IsReadyToEat)
        {
            var _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(_mousePos.x, _mousePos.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        _isDragging = false;
    }

    public void SetFoodToBeEaten(Plate plate, bool eatingRightFood)
    {
        transform.position = plate.transform.position;
        transform.SetParent(plate.transform);
        transform.localPosition = new Vector3(0f, 0.86f, 0f);

        if (rigidBody2D != null)
        {
            rigidBody2D.linearVelocity = Vector2.zero;
            rigidBody2D.gravityScale = 0f;
            rigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }

        _isReadyToEat = true;

        SoundManager.instance.PlaySFX("Eating");

        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = false;
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
        if (!other.TryGetComponent(out Plate plate))
            return;

        if (_currentPlate == null)
        {
            _currentPlate = plate;
            _currentPlate.OnFoodInOnPlate();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Plate plate))
            return;

        if (_currentPlate == plate)
        {
            _currentPlate.OnFoodIsOffPlate();
            _currentPlate = null;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Plate plate))
            return;

        if (_currentPlate == plate && _currentPlate.canBeDropped() && !_isDragging)
        {
            _currentPlate.PrepareToEat(this);
        }
    }
}