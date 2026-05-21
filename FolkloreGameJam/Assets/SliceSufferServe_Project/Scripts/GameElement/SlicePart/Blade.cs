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
            return; // finger is on food, abort entirely
        }

        isCutting = true;
        currentBladeTrail = Instantiate(bladeTrailPrefab, transform);
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
}
