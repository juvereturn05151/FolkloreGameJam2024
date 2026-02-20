/*
File Name:    Blade.cs
Author(s):    Ju-ve Chankasemporn
Copyright:    (c) MyLoyalFans. All rights reserved.
*/

using UnityEngine;

public class Blade : MonoBehaviour
{
    [SerializeField]
    private GameObject bladeTrailPrefab;
    [SerializeField]
    private float minCuttingVelocity = .001f;
    [SerializeField]
    private BladeStamina bladeStamina;

    private CircleCollider2D circleCollider;
    public CircleCollider2D CircleCollider => circleCollider;

    private Vector2 previousPosition;
    private GameObject currentBladeTrail;
    private Rigidbody2D rb;
    private Camera cam;
    private bool isCutting = false;

    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
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

    void UpdateCut()
    {
        Vector2 newPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        rb.position = newPosition;

        float velocity = (newPosition - previousPosition).magnitude * Time.deltaTime;
        if (velocity > minCuttingVelocity)
        {
            circleCollider.enabled = !DragAndDropManager.Instance.isDragging;
        }
        else
        {
            circleCollider.enabled = false;
        }


        previousPosition = newPosition;
    }

    void StartCutting()
    {
        if(DragAndDropManager.Instance.isDragging)
            return;

        //if (!bladeStamina.TryConsumeForSlice())
        //    return;

        isCutting = true;
        currentBladeTrail = Instantiate(bladeTrailPrefab, transform);
        previousPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        circleCollider.enabled = false;
        SoundManager.instance.PlaySFX("SFX_Slice");
    }

    void StopCutting()
    {
        if (!isCutting) return; // nothing to stop

        isCutting = false;

        if (currentBladeTrail != null)
        {
            currentBladeTrail.transform.SetParent(null);
            Destroy(currentBladeTrail, 2f);
            currentBladeTrail = null;
        }

        circleCollider.enabled = false;
    }
}
