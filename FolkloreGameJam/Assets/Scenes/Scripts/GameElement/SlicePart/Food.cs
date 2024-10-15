using System;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;

public enum FoodState
{
    Rare,
    MediumRare,
    WellDone,
    Burnt
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

    [SerializeField] private float _rottenTime = 10.0f;

    [SerializeField] private Slider rottenSlider;

    private FoodState _foodState = FoodState.Rare;
    public FoodState FoodState => _foodState;

    private bool _isReadyToEat = false;
    public bool IsReadyToEat => _isReadyToEat;
    private bool _isFinished = false;
    public bool IsFinished => _isFinished;
    private float _eatingTime = 10.0f;

    private bool isStartingRotten = false;
    private float _currentRottenTime = 10.0f;
    public float RottenTime => _currentRottenTime;


    private Color _initialColor;

    private void OnEnable()
    {
        if (!_isReadyToEat) 
        {
            _currentRottenTime = _rottenTime; 
            _initialColor = _renderer.color;
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
            _eatingTime -= Time.deltaTime;
            if (_eatingTime <= 0)
            {
                _isFinished = true;
            }
        }
        else 
        {
            if (isStartingRotten) 
            {
                _currentRottenTime -= Time.deltaTime;
                rottenSlider.value = Mathf.Lerp(rottenSlider.value, _currentRottenTime, Time.deltaTime);
                
                // Calculate the darkness factor based on the remaining rotten time
                float darknessFactor = Mathf.Clamp01(_currentRottenTime / 10.0f); // Normalized value between 0 and 1
                _renderer.color = Color.Lerp(Color.black, _initialColor, darknessFactor); // Interpolate between black and the initial color

                if (_currentRottenTime <= 0)
                {
                    if(rottenSlider.value > 0) return;
                    ChangeFoodState();
                    _textState.text = _foodState.ToString();

                    if (_foodState == FoodState.Burnt)
                    {
                        CustomerGenerator.Instance.PissCustomer();
                        Destroy(this.gameObject);
                    }
                    else 
                    {
                        _currentRottenTime = _rottenTime;
                        rottenSlider.DOValue(_currentRottenTime, 0.25f).SetEase(Ease.InQuart);
                    }
                }
            }
        }
    }

    private void OnMouseDrag()
    {
        if (IsReadyToEat)
        {
            return;
        }

        var _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(_mousePos.x, _mousePos.y, transform.position.z);
    }

    private void ChangeFoodState() 
    {
        if (_foodState == FoodState.Burnt)
            return;

        _foodState++;
    }

    public void SetIsReadyToEat(bool isReadyToEat) 
    {
        this._isReadyToEat = isReadyToEat;
    }

    public void SetFoodToBeEaten(Plate plate)
    {
        transform.position = plate.transform.position;
        transform.SetParent(plate.transform);
        transform.localPosition = Vector3.zero;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.gravityScale = 0;
        _rigidBody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX;
        _isReadyToEat = true;
        
        rottenSlider.gameObject.SetActive(false);
    }
}

