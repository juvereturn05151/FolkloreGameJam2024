using System;
using Unity.VisualScripting;
using UnityEngine;

public class DragAndDropManager : MonoBehaviour
{
    public static DragAndDropManager Instance;
    
    [SerializeField] private Food currentDraggingFood;
    public bool isDragging;
    
    [SerializeField] private LayerMask dragableLayer;
    [SerializeField] private LayerMask dropLayer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
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
                currentDraggingFood = food;

                var _rb = currentDraggingFood.GetComponent<Rigidbody2D>();
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
            if (currentDraggingFood != null)
            {
                if (!plate.CurrentCustomer.IsOrdering)
                {
                    var _rb = currentDraggingFood.GetComponent<Rigidbody2D>();
                    _rb.gravityScale = 1;
                }
                else
                {
                    plate.PrepareToEat(currentDraggingFood);
                    currentDraggingFood = null;
                }
            }
        }
        else 
        {
            if (currentDraggingFood == null) 
            {
                return;
            } 
            var _rb = currentDraggingFood.GetComponent<Rigidbody2D>();
            _rb.gravityScale = 1;
        }
       

    }
}
