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
                if (!isDragging) 
                {
                    SoundManager.instance.PlaySFX("SFX_WhenPickUpItem");
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
        
        if (_hit.collider != null)
        {
            if (_hit.collider.GetComponent<Plate>() is Plate plate) 
            {

                if (currentDraggingFood == null) return;

                if (!plate.CurrentCustomer.IsOrdering)
                {
                    print("test");
                    currentDraggingFood.GetComponent<Rigidbody2D>().gravityScale = 1;
                }
                else
                {
                    plate.PrepareToEat(currentDraggingFood);
                    currentDraggingFood = null;
                }

                return;
            }

            if (_hit.collider.GetComponent<Trash>() is Trash trash)
            {
                if (currentDraggingFood == null) return;
                Destroy(currentDraggingFood.gameObject);
                currentDraggingFood = null;
                return;
            }

        }

        if (currentDraggingFood == null)
        {
            return;
        }
        currentDraggingFood.GetComponent<Rigidbody2D>().gravityScale = 1;


    }
}
