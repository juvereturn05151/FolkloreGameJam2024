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

    [SerializeField] private GameObject _dust;

    private FoodState _foodState = FoodState.Rare;
    public FoodState FoodState => _foodState;

    private bool _isReadyToEat = false;
    public bool IsReadyToEat => _isReadyToEat;
    private bool _isFinished = false;
    public bool IsFinished => _isFinished;
    [SerializeField]
    private float _eatingTime = 10.0f;

    [SerializeField]
    private int _decreaseScoreOnBurnt = 10;

    private bool isStartingRotten = false;
    private float _currentRottenTime = 10.0f;
    public float RottenTime => _currentRottenTime;

    [SerializeField] private GameObject foodStateEffect;

    private void OnEnable()
    {
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
        }
        else 
        {
            if (isStartingRotten) 
            {
                UpdateRotten();
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



    private void UpdateEaten() 
    {
        _eatingTime -= Time.deltaTime;
        if (_eatingTime <= 0)
        {
            _isFinished = true;
        }
    }

    private void UpdateRotten() 
    {
        _currentRottenTime -= Time.deltaTime;
        rottenSlider.value = Mathf.Lerp(rottenSlider.value, _currentRottenTime, Time.deltaTime);

        if (_currentRottenTime <= 0)
        {
            if (rottenSlider.value > 0) 
            {
                return;
            } 

            ChangeFoodState();
        }
    }

    private void ChangeFoodState()
    {
        if (_foodState == FoodState.Burnt) 
        {
            return;
        }

        rottenSlider.transform.DOShakePosition(0.5f, 0.5f);
        SoundManager.instance.PlaySFX("ChangeFoodState");
        Instantiate(foodStateEffect, transform.position, Quaternion.identity, transform);
        _foodState++;

        if (_foodState == FoodState.MediumRare)
        {
            _renderer.sprite = Menu.MediumRottenSprite;
        }
        else if (_foodState == FoodState.WellDone)
        {
            _renderer.sprite = Menu.SuperRottenSprite;
        }

        if (_foodState == FoodState.Burnt)
        {
            if (GameUtility.GameManagerExists())
            {
                if (!GameManager.Instance.IsGameOver)
                {
                    ScoreManager.Instance.SubtractScore(_decreaseScoreOnBurnt);
                }

                if (GameUtility.AdvancedTutorialManagerExists()) 
                {
                    if (GameManager.Instance.IsTutorial && AdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.WaitForRotten)
                    {
                        AdvancedTutorialManager.Instance.rottenCount++;
                    }
                }
            }

            Instantiate(_dust, this.transform.position, this.transform.rotation);
            Destroy(this.gameObject);
        }
        else
        {
            _currentRottenTime = _rottenTime;
            rottenSlider.DOValue(_currentRottenTime, 0.25f).SetEase(Ease.InQuart);
        }
    }

    public void SetFoodToBeEaten(Plate plate, bool eatingRightFood)
    {
        transform.position = plate.transform.position ;
        transform.SetParent(plate.transform);
        transform.localPosition = Vector3.zero + new Vector3(0.0f, 0.86f, 0.0f);
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.gravityScale = 0;
        _rigidBody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX;
        _isReadyToEat = true;
        SoundManager.instance.PlaySFX("Eating");
        if (!eatingRightFood) 
        {
            _eatingTime = 2.0f;
        }
        
        rottenSlider.gameObject.SetActive(false);
    }
}

