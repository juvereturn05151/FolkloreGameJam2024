using UnityEngine;

public class Blade : MonoBehaviour
{
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

    private void StartCutting()
    {
        isCutting = true;
        currentBladeTrail = Instantiate(bladeTrailPrefab, transform);
        previousPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        circleCollider.enabled = false;
        SoundManager.instance.PlaySFX("SFX_Slice");
    }

    private void StopCutting()
    {
        isCutting = false;
        currentBladeTrail.transform.SetParent(null);
        Destroy(currentBladeTrail, 2f);
        circleCollider.enabled = false;
    }
}
