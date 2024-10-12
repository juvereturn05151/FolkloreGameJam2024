using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Food : MonoBehaviour
{
    [SerializeField]
    private Menu menu;
    public Menu Menu => menu;

    private bool _isReadyToEat = false;
    private bool _isFinished = false;
    public bool IsFinished => _isFinished;
    private float _eatingTime = 10.0f;

    private void FixedUpdate()
    {
        if (_isReadyToEat) 
        {
            _eatingTime -= Time.deltaTime;
            if(_eatingTime <= 0) 
            {
                _isFinished = true;
            }
        }
    }

    private void OnMouseDrag()
    {
        var _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(_mousePos.x, _mousePos.y, transform.position.z);
    }

    public void SetIsReadyToEat(bool isReadyToEat) 
    {
        this._isReadyToEat = isReadyToEat;
    }
}

