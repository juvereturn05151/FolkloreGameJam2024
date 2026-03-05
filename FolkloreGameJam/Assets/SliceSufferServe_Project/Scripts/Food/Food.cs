using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FoodState
{
    Normal,
    MediumRotten,
    SuperRotten,
    Disappear
}

public class Food : MonoBehaviour
{
    [SerializeField]
    private Menu menu;
    public Menu Menu => menu;

    [SerializeField]
    private SpriteRenderer _renderer;

    [SerializeField]
    private Rigidbody2D _rigidBody;

    [SerializeField]
    private TextMeshProUGUI _textState;

    [SerializeField] 
    private float _rottenTime = 10.0f;

    [SerializeField] 
    private Slider rottenSlider;

    [SerializeField] 
    private GameObject _dust;

    [SerializeField] 
    private GameObject foodStateEffect;
    [SerializeField] 
    private BoxCollider2D _boxCollider;
    [SerializeField] 
    private Animator _animator;
    [SerializeField] 
    private ScoreFeedback _scoreFeedback;

    [SerializeField]
    private float _eatingTime = 10.0f;

    [SerializeField]
    private int _decreaseScoreOnBurnt = 10;

    [SerializeField]
    private FoodRotting foodRotting;
    [SerializeField] 
    private FoodVisuals foodVisuals;
    [SerializeField] 
    private FoodScoringOnExpire foodScoring;


    private FoodState _foodState = FoodState.Normal;
    public FoodState FoodState => _foodState;

    private bool _isReadyToEat = false;
    public bool IsReadyToEat => _isReadyToEat;
    private bool _isFinished = false;
    public bool IsFinished => _isFinished;


    private bool isStartingRotten = false;
    private float _currentRottenTime = 10.0f;
    public float RottenTime => _currentRottenTime;



    private bool isDragging = false;
    public bool IsDragging => isDragging;
    private bool canDrag = true;

    private Plate currentPlate;

    private void OnEnable()
    {
        foodRotting.OnStateChanged += foodVisuals.ApplyState;   
        foodRotting.OnExpired += HandleExpired;            

        if (!_isReadyToEat) 
        {
            _currentRottenTime = _rottenTime; 
            isStartingRotten = true;
        }
    }

    private void Start()
    {
        rottenSlider.maxValue = _rottenTime;
        rottenSlider.value = rottenSlider.maxValue;
    }

    private void Update()
    {
        if (_isReadyToEat)
        {
            UpdateEaten();
            return;
        }
        
        foodRotting.Tick(Time.deltaTime);
        foodVisuals.UpdateRotSlider(foodRotting.Remaining, foodRotting.BaseRottenTime);
    }

    private void OnMouseDown()
    {
        if (!canDrag || _isReadyToEat)
        {
            return;
        }
        SoundManager.instance.PlaySFX("SFX_WhenPickUpItem");

        if (GameUtility.DragAndDropManagerExists()) 
        {
            DragAndDropManager.Instance.isDragging = true;
        }

        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging && !IsReadyToEat)
        {
            var _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(_mousePos.x, _mousePos.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void HandleExpired()
    {
        foodScoring.ApplyPenalty(transform.position);
        foodVisuals.SpawnDustAndDestroy(gameObject);
    }

    private void UpdateEaten() 
    {
        _eatingTime -= Time.deltaTime;
        if (_eatingTime <= 0)
        {
            _isFinished = true;
        }
    }

    public void SetFoodToBeEaten(Plate plate, bool eatingRightFood)
    {
        transform.position = plate.transform.position ;
        transform.SetParent(plate.transform);
        transform.localPosition = Vector3.zero + new Vector3(0.0f, 0.86f, 0.0f);
        _rigidBody.linearVelocity = Vector2.zero;
        _rigidBody.gravityScale = 0;
        _rigidBody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX;
        _isReadyToEat = true;
        SoundManager.instance.PlaySFX("Eating");
        if (_boxCollider != null) 
        {
            _boxCollider.enabled = false;
        }

        if (!eatingRightFood) 
        {
            _eatingTime = 2.0f;
        }
        
        rottenSlider.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Plate>() is Plate plate)
        {
            if (currentPlate == null) // Only register if there�s no current plate
            {
                currentPlate = plate;
                currentPlate.OnFoodInOnPlate();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Plate>() is Plate plate && currentPlate == plate)
        {
            currentPlate.OnFoodIsOffPlate();
            currentPlate = null; // Clear the reference when food leaves the plate
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<Plate>() is Plate plate && plate == currentPlate)
        {
            if (currentPlate.canBeDropped() && !IsDragging)
            {
                currentPlate.PrepareToEat(this);
            }
        }
    }
}

