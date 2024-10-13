using System;
using Unity.VisualScripting;
using UnityEngine;

public class DragAndDropManager : MonoBehaviour
{
    [SerializeField] private bool isDragging;
    
    [SerializeField] private Food currentDraggingObject;
    
    [SerializeField] private LayerMask dragableLayer;
    [SerializeField] private LayerMask dropLayer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OnDragThings();
    }

    private void OnDragThings()
    {
        if (Input.GetMouseButton(0))
        {
            var _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var _hit = Physics2D.Raycast(_ray.origin, _ray.direction, Mathf.Infinity, dragableLayer);

            if (_hit.collider != null && _hit.collider.GetComponent<Food>() is Food food) 
            {
                if (food.IsReadyToEat) 
                {
                    return;
                }

                isDragging = true;
                currentDraggingObject = food;

                var _rb = currentDraggingObject.GetComponent<Rigidbody2D>();
                _rb.gravityScale = 0;
            }
           
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnDropThings();
        }
    }

    private void OnDropThings()
    {
        var _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D _hit = Physics2D.Raycast(_ray.origin, _ray.direction, Mathf.Infinity, dropLayer);
        isDragging = false;

        if (_hit.collider != null && _hit.collider.GetComponent<Plate>() is Plate plate)
        {
            if (currentDraggingObject != null)
            {
                plate.SetFoodPosition(currentDraggingObject);
            }
            currentDraggingObject = null;
        }
        else 
        {
            if (currentDraggingObject == null) 
            {
                return;
            } 
            var _rb = currentDraggingObject.GetComponent<Rigidbody2D>();
            _rb.gravityScale = 1;
        }
       

    }
}
