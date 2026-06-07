using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    [SerializeField] private float travelDuration = 1.15f;
    [SerializeField] private float startScale = 0.3f;
    [SerializeField] private float impactScale = 4.5f;
    [SerializeField] private int damage = 1;

    private Camera mainCamera;
    private Vector3 startViewportPosition;
    private float elapsed;
    private bool hasStartPosition;
    private bool hasImpacted;

    private void Awake()
    {
        mainCamera = Camera.main;
        CaptureStartPosition();
        transform.localScale = Vector3.one * startScale;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsRapidSliceEventActive)
        {
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

        CaptureStartPosition();

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

    private void CaptureStartPosition()
    {
        if (hasStartPosition || mainCamera == null)
        {
            return;
        }

        startViewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        startViewportPosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        hasStartPosition = true;
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
