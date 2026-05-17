using UnityEngine;

[RequireComponent(typeof(HumanBody))]
public class HumanRockThrower : MonoBehaviour
{
    [Header("Screen Timing")]
    [SerializeField, Range(0f, 1f)] private float pickUpViewportY = 0.8f;
    [SerializeField, Range(0f, 1f)] private float throwViewportY = 0.5f;

    [Header("Rock")]
    [SerializeField] private GameObject heldRockSpawner;
    [SerializeField] private RockProjectile rockPrefab;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private Vector3 heldRockLocalPosition = new Vector3(1.35f, 1.4f, -0.05f);

    private HumanBody humanBody;
    private Camera mainCamera;
    private bool pickedRock;
    private bool threwRock;

    private void Awake()
    {
        humanBody = GetComponent<HumanBody>();
        mainCamera = Camera.main;
        EnsureHeldRockSpawner();

        if (heldRockSpawner != null)
        {
            heldRockSpawner.SetActive(false);
        }
    }

    private void Update()
    {
        if (humanBody != null && humanBody.IsBeingDestroyed)
        {
            HideHeldRock();
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

        if (rockPrefab != null)
        {
            Instantiate(rockPrefab, spawnPosition, spawnRotation);
        }
        else
        {
            RockProjectile.Create(spawnPosition, spawnRotation);
        }

        HideHeldRock();
    }

    private void HideHeldRock()
    {
        if (heldRockSpawner != null)
        {
            heldRockSpawner.SetActive(false);
        }
    }

    private void EnsureHeldRockSpawner()
    {
        if (heldRockSpawner != null)
        {
            if (throwOrigin == null)
            {
                throwOrigin = heldRockSpawner.transform;
            }

            return;
        }

        Transform parent = FindBodyLikeTransform();
        GameObject rockObject = new GameObject("Held Rock Spawner");
        rockObject.transform.SetParent(parent != null ? parent : transform, false);
        rockObject.transform.localPosition = heldRockLocalPosition;
        rockObject.transform.localRotation = Quaternion.identity;
        rockObject.transform.localScale = Vector3.one;

        SpriteRenderer spriteRenderer = rockObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = RockProjectile.GetRockSprite();
        spriteRenderer.color = RockProjectile.RockColor;
        spriteRenderer.sortingOrder = 20;

        heldRockSpawner = rockObject;
        throwOrigin = rockObject.transform;
    }

    private Transform FindBodyLikeTransform()
    {
        HumanPart[] parts = GetComponentsInChildren<HumanPart>(true);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] != null && parts[i].name.Contains("Body"))
            {
                return parts[i].transform;
            }
        }

        return transform;
    }
}
