using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Draggable2D : MonoBehaviour
{
    public event Action DragStarted;
    public event Action DragEnded;
    public event Action DragCancelled;
    public event Action<Transform> Snapped;

    [SerializeField] private bool canDrag = true;
    [SerializeField] private float zOffset = 0f;

    private Camera _mainCamera;
    private bool _isDragging;
    private bool _isSnapped;

    private Transform _snapTarget;

    public bool IsDragging => _isDragging;
    public bool IsSnapped => _isSnapped;
    public bool CanDrag => canDrag;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    private void OnMouseDown()
    {
        if (!canDrag || _isSnapped)
            return;

        BeginDrag();
    }

    private void OnMouseDrag()
    {
        if (!_isDragging || _isSnapped)
            return;

        Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z + zOffset);
    }

    private void OnMouseUp()
    {
        // Important:
        // OnMouseUp may still happen even after we were force-snapped.
        // So only end drag if we're actually still dragging.
        if (_isDragging)
        {
            EndDrag();
        }
    }

    public void BeginDrag()
    {
        if (!canDrag || _isDragging || _isSnapped)
            return;

        _isDragging = true;
        DragStarted?.Invoke();
    }

    public void EndDrag()
    {
        if (!_isDragging)
            return;

        _isDragging = false;
        DragEnded?.Invoke();
    }

    public void CancelDrag()
    {
        if (!_isDragging)
            return;

        _isDragging = false;
        DragCancelled?.Invoke();
    }

    public void SnapTo(Transform target, Vector3 localPosition)
    {
        // If the object was being dragged, stop dragging immediately.
        if (_isDragging)
        {
            CancelDrag();
        }

        _isSnapped = true;
        _snapTarget = target;

        transform.SetParent(target);
        transform.localPosition = localPosition;

        Snapped?.Invoke(target);
    }

    public void ReleaseFromSnap()
    {
        if (!_isSnapped)
            return;

        _isSnapped = false;
        _snapTarget = null;
        transform.SetParent(null);
    }
}