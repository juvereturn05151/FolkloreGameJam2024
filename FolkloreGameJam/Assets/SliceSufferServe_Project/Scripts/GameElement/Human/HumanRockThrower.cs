using UnityEngine;

[RequireComponent(typeof(HumanBody))]
public class HumanRockThrower : MonoBehaviour
{
    [Header("Screen Timing")]
    [SerializeField, Range(0f, 1f)] private float pickUpViewportY = 1.0f;
    [SerializeField, Range(0f, 1f)] private float throwViewportY = 0.8f;

    [Header("Rock")]
    [SerializeField] private GameObject heldRockSpawner;
    [SerializeField] private RockProjectile rockPrefab;
    [SerializeField] private Transform throwOrigin;

    private HumanBody humanBody;
    private Camera mainCamera;
    private RockProjectile activeRockProjectile;
    private bool pickedRock;
    private bool threwRock;

    private void Awake()
    {
        humanBody = GetComponent<HumanBody>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsRapidSliceEventActive)
        {
            return;
        }

        if (humanBody != null && humanBody.IsBeingDestroyed)
        {
            HideHeldRock();
            DestroyActiveRockProjectile();
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        float viewportY = mainCamera.WorldToViewportPoint(GetReferencePosition()).y;

        Debug.Log($"Viewport Y: {viewportY}, PickUp Threshold: {pickUpViewportY}, Throw Threshold: {throwViewportY}");

        if (!pickedRock && viewportY <= pickUpViewportY)
        {
            PickRock();
        }

        if (!threwRock && viewportY <= throwViewportY)
        {
            ThrowRock();
        }
    }

    private Vector3 GetReferencePosition()
    {
        return throwOrigin != null ? throwOrigin.position : transform.position;
    }

    private void PickRock()
    {
        pickedRock = true;

        if (heldRockSpawner != null)
        {
            heldRockSpawner.SetActive(true);
        }
    }

    private void ThrowRock()
    {
        threwRock = true;

        Vector3 spawnPosition = heldRockSpawner != null ? heldRockSpawner.transform.position : GetReferencePosition();
        Quaternion spawnRotation = heldRockSpawner != null ? heldRockSpawner.transform.rotation : Quaternion.identity;

        if (rockPrefab == null)
        {
            Debug.LogWarning($"{nameof(HumanRockThrower)} on {name} needs a rock projectile prefab assigned.", this);
            HideHeldRock();
            return;
        }

        activeRockProjectile = Instantiate(rockPrefab, spawnPosition, spawnRotation);
        HideHeldRock();
    }

    private void DestroyActiveRockProjectile()
    {
        if (activeRockProjectile == null)
        {
            return;
        }

        Destroy(activeRockProjectile.gameObject);
        activeRockProjectile = null;
    }

    private void HideHeldRock()
    {
        if (heldRockSpawner != null)
        {
            heldRockSpawner.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        DestroyActiveRockProjectile();
    }
}
