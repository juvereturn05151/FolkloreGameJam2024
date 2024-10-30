using System;
using Unity.VisualScripting;
using UnityEngine;

public class DragAndDropManager : MonoBehaviour
{
    public static DragAndDropManager Instance;

    [SerializeField] private Food currentDraggingFood;
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private LayerMask dropLayer;

    public bool isDragging { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
       // if (Input.GetMouseButtonDown(0)) StartDragging();
       // else if (Input.GetMouseButtonUp(0)) StopDragging();
    }

    private void StartDragging()
    {
        if (isDragging) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, draggableLayer);

        if (hit.collider != null && hit.collider.TryGetComponent(out Food food) && !food.IsReadyToEat)
        {
            currentDraggingFood = food;
            isDragging = true;
            SoundManager.instance.PlaySFX("SFX_WhenPickUpItem");
            currentDraggingFood.GetComponent<Rigidbody2D>().gravityScale = 0;
        }
    }

    private void StopDragging()
    {
        if (!isDragging || currentDraggingFood == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, dropLayer);
        isDragging = false;

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out Plate plate) && plate.CurrentCustomer.IsOrdering)
            {
                plate.PrepareToEat(currentDraggingFood);
            }
            else if (hit.collider.TryGetComponent(out Trash _))
            {
                Destroy(currentDraggingFood.gameObject);
            }
            else
            {
                currentDraggingFood.GetComponent<Rigidbody2D>().gravityScale = 1;
            }
        }
        else
        {
            currentDraggingFood.GetComponent<Rigidbody2D>().gravityScale = 1;
        }

        currentDraggingFood = null;
    }
}