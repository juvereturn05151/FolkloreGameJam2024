using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    [SerializeField] private float travelDuration = 1.15f;
    [SerializeField] private float startScale = 0.3f;
    [SerializeField] private float impactScale = 4.5f;
    [SerializeField] private int damage = 1;

    private static Sprite rockSprite;
    public static readonly Color RockColor = new Color(0.42f, 0.42f, 0.42f, 1f);

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Vector3 startViewportPosition;
    private float elapsed;
    private bool hasImpacted;

    public static RockProjectile Create(Vector3 position, Quaternion rotation)
    {
        GameObject rockObject = new GameObject("Rock Projectile");
        rockObject.transform.SetPositionAndRotation(position, rotation);
        return rockObject.AddComponent<RockProjectile>();
    }

    public static Sprite GetRockSprite()
    {
        if (rockSprite != null)
        {
            return rockSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = "Runtime Rock Circle"
        };

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.43f;
        float softEdge = 3.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((radius - distance + softEdge) / softEdge);
                Color pixel = RockColor;
                pixel.a = alpha;
                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        rockSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        rockSprite.name = "Runtime Rock Circle";
        return rockSprite;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = GetRockSprite();
        spriteRenderer.color = RockColor;
        spriteRenderer.sortingOrder = 100;

        if (mainCamera != null)
        {
            startViewportPosition = mainCamera.WorldToViewportPoint(transform.position);
            startViewportPosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        }

        transform.localScale = Vector3.one * startScale;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, travelDuration));
        float easedProgress = progress * progress * (3f - 2f * progress);

        Vector3 viewportPosition = Vector3.Lerp(startViewportPosition, new Vector3(0.5f, 0.5f, startViewportPosition.z), easedProgress);
        transform.position = mainCamera.ViewportToWorldPoint(viewportPosition);
        transform.localScale = Vector3.one * Mathf.Lerp(startScale, impactScale, easedProgress);

        if (!hasImpacted && progress >= 1f)
        {
            ImpactPlayer();
        }
    }

    private void ImpactPlayer()
    {
        hasImpacted = true;

        if (damage > 0 && GameUtility.HPManagerExists())
        {
            HPManager.Instance.TakeDamage(damage);
        }

        if (GameUtility.FeedbackManagerExists() && FeedbackManager.Instance.DamageFeedback != null)
        {
            FeedbackManager.Instance.DamageFeedback.PlayFeedbacks();
        }

        Destroy(gameObject);
    }
}
