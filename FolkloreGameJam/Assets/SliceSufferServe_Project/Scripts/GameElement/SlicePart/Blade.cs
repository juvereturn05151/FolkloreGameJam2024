/*
 * Auther: Ju-ve Chankasemporn
 * E-mail: juvereturn@gmail.com
 * @Copyright (c) 2026 by Ju-ve Chankasemporn. All rights reserved.
 */

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Blade : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject bladeTrailPrefab;
    [SerializeField]
    private BladeTrailCustomizationOption[] bladeTrailOptions;
    [SerializeField]
    private float minCuttingVelocity = .001f;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private CircleCollider2D circleCollider;

    private Vector2 previousPosition;
    private GameObject currentBladeTrail;
    private Camera cam;
    private bool isCutting = false;

    private void Start()
    {
        cam = Camera.main;

        if (rb == null) 
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCutting();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopCutting();
        }

        if (isCutting)
        {
            UpdateCut();
        }
    }

    private void UpdateCut()
    {
        Vector2 newPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        rb.position = newPosition;

        float velocity = (newPosition - previousPosition).magnitude / Time.deltaTime;

        if (velocity > minCuttingVelocity && !DragAndDropManager.Instance.isDragging)
        {
            circleCollider.enabled = true;
        }
        else
        {
            circleCollider.enabled = false;
        }

        previousPosition = newPosition;
    }

    private void StartCutting()
    {
        // Check if the click landed on a Draggable2D — if so, don't cut
        Vector2 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(worldPoint);
        if (hit != null && hit.GetComponent<Draggable2D>() != null)
        {
            // finger is on food, abort entirely
            return; 
        }

        isCutting = true;
        currentBladeTrail = Instantiate(GetSelectedBladeTrailPrefab(), transform);
        previousPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        circleCollider.enabled = false;
        SoundManager.instance.PlaySFX("SFX_Slice");
    }

    private void StopCutting()
    {
        isCutting = false;

        if (currentBladeTrail != null) 
        {
            currentBladeTrail.transform.SetParent(null);
            Destroy(currentBladeTrail, 2f);
        }
        
        circleCollider.enabled = false;
    }

    private GameObject GetSelectedBladeTrailPrefab()
    {
        string cursorId = CursorCustomizationSelection.GetSelectedCursorId();

        if (bladeTrailOptions != null)
        {
            for (int i = 0; i < bladeTrailOptions.Length; i++)
            {
                BladeTrailCustomizationOption option = bladeTrailOptions[i];
                if (option != null && option.Matches(cursorId) && option.BladeTrailPrefab != null)
                {
                    return option.BladeTrailPrefab;
                }
            }
        }

        return bladeTrailPrefab;
    }
}

[System.Serializable]
public class BladeTrailCustomizationOption
{
    [SerializeField] private string cursorId;
    [SerializeField] private GameObject bladeTrailPrefab;

    public GameObject BladeTrailPrefab => bladeTrailPrefab;

    public bool Matches(string selectedCursorId)
    {
        return !string.IsNullOrWhiteSpace(cursorId)
            && string.Equals(cursorId, selectedCursorId, System.StringComparison.Ordinal);
    }
}
