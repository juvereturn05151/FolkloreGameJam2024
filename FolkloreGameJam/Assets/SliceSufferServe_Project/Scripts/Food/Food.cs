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
    private Rigidbody2D _rigidBody;

    [SerializeField]
    private TextMeshProUGUI _textState;

    [SerializeField] 
    private BoxCollider2D _boxCollider;

    [SerializeField]
    private float _eatingTime = 10.0f;

    [SerializeField]
    private FoodRotting foodRotting;
    public FoodRotting FoodRotting => foodRotting;
    [SerializeField] 
    private FoodVisuals foodVisuals;
    [SerializeField] 
    private FoodScoringOnExpire foodScoring;


    private bool _isReadyToEat = false;
    public bool IsReadyToEat => _isReadyToEat;
    private bool _isFinished = false;
    public bool IsFinished => _isFinished;

    private bool isDragging = false;
    public bool IsDragging => isDragging;
    private bool canDrag = true;

    private Plate currentPlate;

    private void OnEnable()
    {
        foodRotting.OnStateChanged += foodVisuals.ApplyState;   
        foodRotting.OnExpired += HandleExpired;            
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
        
        foodVisuals.HideRotUI();
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
            currentPlate = null; 
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

