using System;
using UnityEngine;

public class FoodEating : MonoBehaviour
{
    public event Action OnFinished;

    [SerializeField] 
    private float eatingTimeRightFood = 10f;
    [SerializeField] 
    private float eatingTimeWrongFood = 2f;

    private float _remainingTime;
    private bool _isEating;

    public bool IsEating => _isEating;
    public float RemainingTime => _remainingTime;

    public void Begin(bool eatingRightFood)
    {
        _remainingTime = eatingRightFood ? eatingTimeRightFood : eatingTimeWrongFood;
        _isEating = true;
    }

    public void Tick(float deltaTime)
    {
        if (!_isEating)
            return;

        _remainingTime -= deltaTime;

        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            _isEating = false;
            OnFinished?.Invoke();
        }
    }

    public void Stop()
    {
        _isEating = false;
        _remainingTime = 0f;
    }
}