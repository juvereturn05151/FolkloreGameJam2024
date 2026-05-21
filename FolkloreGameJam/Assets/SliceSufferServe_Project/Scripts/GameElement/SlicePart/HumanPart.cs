using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HumanPart : MonoBehaviour
{
    public UnityEvent OnPartDestroyed;
    public event Action<Vector3, IReadOnlyList<FeedbackRequest>> Sliced;

    [Header("Slice Feedback Settings")]
    [SerializeField] private List<FeedbackRequest> feedbackRequests = new();

    [Header("Spawn / Motion")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float startForce = 15f;
    [SerializeField] private bool atMainMenu;

    [Header("Durability")]
    [SerializeField] private int cutsRequiredToDestroy = 1;
    [SerializeField] private GameObject durabilityHitEffectPrefab;
    [SerializeField] private float durabilityHitEffectScale = 1f;
    [SerializeField] private bool shakeOnDurabilityHit = true;

    [Header("Fade Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeDuration = 0.5f;

    private HumanBody _ownerBody;
    private bool _sliced;
    private bool _isFading;
    private int cutCount;
    private bool feedbackManagerHooked;

    public HumanBody OwnerBody => _ownerBody;
    public bool CanProduceFood => enabled && !_sliced && !_isFading && gameObject.activeInHierarchy && foodPrefab != null;

    public void SetOwner(HumanBody owner)
    {
        _ownerBody = owner;
    }

    public void SetStartForceMultiplier(float multiplier)
    {
        startForce *= Mathf.Max(0.01f, multiplier);
    }

    public void SetFoodPrefab(GameObject prefab)
    {
        foodPrefab = prefab;
    }

    public void SetCutsRequiredToDestroy(int cutsRequired)
    {
        cutsRequiredToDestroy = Mathf.Max(1, cutsRequired);
        cutCount = 0;
    }

    public Menu GetProducedMenu()
    {
        if (foodPrefab == null || !foodPrefab.TryGetComponent(out Food food))
        {
            return null;
        }

        return food.Menu;
    }

    private void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(startForce, 0);
        }
    }

    private void OnEnable()
    {
        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.Hook(this);
            feedbackManagerHooked = true;
        }

        if (!atMainMenu && GameUtility.SSSAdvancedTutorialManagerExists())
        {
            SSSAdvancedTutorialManager.Instance.Hook(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag(GameTagContainer.BladeTag)) return;
        if (GameManager.Instance != null && GameManager.Instance.IsRapidSliceEventActive) return;
        if (_sliced) return;

        if (_isFading)
        {
            return;
        }

        cutCount++;
        if (cutCount < Mathf.Max(1, cutsRequiredToDestroy))
        {
            PlayDurabilityHitFeedback();
            return;
        }

        _sliced = true;

        if (GameUtility.GameManagerExists()) 
        {
            GameManager.Instance.AddSuperMeter(5);
        }

        if (_ownerBody != null)
        {
            _ownerBody.NotifyPartSliced(this);
        }
        else
        {
            DestroyImmediately();
        }
    }

    public void DestroyImmediately()
    {
        if (foodPrefab != null)
        {
            Instantiate(foodPrefab, transform.position, Quaternion.identity);
        }

        SpawnSliceFeedbackFallback();
        Sliced?.Invoke(transform.position, feedbackRequests);
        OnPartDestroyed?.Invoke();

        Destroy(gameObject);
    }

    public void DestroyWithoutFood()
    {
        OnPartDestroyed?.Invoke();
        Destroy(gameObject);
    }

    public void StartFadeAndDestroy()
    {
        if (_sliced || _isFading) return;
        _isFading = true;

        OnPartDestroyed?.Invoke();

        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(FadeAndDestroyCoroutine());
    }

    private IEnumerator FadeAndDestroyCoroutine()
    {
        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Color startColor = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, t);
            spriteRenderer.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }

    private void PlayDurabilityHitFeedback()
    {
        if (durabilityHitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(durabilityHitEffectPrefab, transform.position, Quaternion.identity);
            hitEffect.transform.localScale *= Mathf.Max(0.01f, durabilityHitEffectScale);
        }

        if (shakeOnDurabilityHit && GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.18f, 0.12f);
        }
    }

    private void SpawnSliceFeedbackFallback()
    {
        bool shouldUseDurabilityFallback = cutsRequiredToDestroy > 1;
        if ((!shouldUseDurabilityFallback && feedbackManagerHooked) || !GameUtility.FeedbackManagerExists() || feedbackRequests == null)
        {
            return;
        }

        for (int i = 0; i < feedbackRequests.Count; i++)
        {
            FeedbackRequest request = feedbackRequests[i];
            if (request.feedbackID == "Blood")
            {
                FeedbackManager.Instance.SpawnBlood(transform.position);
            }
        }
    }
}
