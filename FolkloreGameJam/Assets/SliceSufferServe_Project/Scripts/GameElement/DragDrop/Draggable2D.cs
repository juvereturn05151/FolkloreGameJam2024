using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Draggable2D : MonoBehaviour
{
    public event Action DragStarted;
    public event Action DragEnded;
    public event Action DragCancelled;
    public event Action<Transform> Snapped;

    [SerializeField]
    private Collider2D pickupCollider;
    [SerializeField] 
    private bool canDrag = true;
    [SerializeField] 
    private float zOffset = 0f;

    private Camera mainCamera;
    private Transform snapTarget;
    private bool isDragging;
    private bool isSnapped;

    public bool IsDragging => isDragging;
    public bool IsSnapped => isSnapped;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // Big collider active at start — waiting for pick-up
        SetPickupColliderActive(true);
    }

    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsRapidSliceEventActive) 
        {
            return;
        }
            
        if (!canDrag || isSnapped) 
        {
            return;
        }

        // Block the blade immediately, before Blade.cs reads this flag
        if (GameUtility.DragAndDropManagerExists())
        {
            DragAndDropManager.Instance.isDragging = true;
        }

        BeginDrag();
    }

    private void OnMouseDrag()
    {
        if (!isDragging || isSnapped)
            return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z + zOffset);
    }

    private void OnMouseUp()
    {
        // Important:
        // OnMouseUp may still happen even after we were force-snapped.
        // So only end drag if we're actually still dragging.
        if (isDragging)
        {
            EndDrag();
        }
    }

    public void BeginDrag()
    {
        if (!canDrag || isDragging || isSnapped)
            return;

        isDragging = true;
        SetPickupColliderActive(false);
        DragStarted?.Invoke();
    }

    public void EndDrag()
    {
        if (!isDragging)
            return;

        isDragging = false;
        SetPickupColliderActive(true);
        DragEnded?.Invoke();
    }

    public void CancelDrag()
    {
        if (!isDragging)
            return;

        isDragging = false;
        SetPickupColliderActive(true);
        DragCancelled?.Invoke();
    }

    public void SnapTo(Transform target, Vector3 localPosition)
    {
        // If the object was being dragged, stop dragging immediately.
        if (isDragging)
        {
            CancelDrag();
        }

        isSnapped = true;
        snapTarget = target;

        transform.SetParent(target);
        transform.localPosition = localPosition;

        SetPickupColliderActive(false);
        Snapped?.Invoke(target);
    }

    public void ReleaseFromSnap()
    {
        if (!isSnapped)
            return;

        isSnapped = false;
        snapTarget = null;
        transform.SetParent(null);
    }

    private void SetPickupColliderActive(bool active)
    {
        if (pickupCollider != null)
            pickupCollider.enabled = active;
    }
}
