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

    [Header("Fade Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Fade Interaction Settings")]
    [SerializeField] private float fadeSliceCooldown = 0.08f;

    private HumanBody _ownerBody;
    private bool _sliced;
    private bool _isFading;
    private float _lastFadeSliceTime = -999f;

    public void SetOwner(HumanBody owner)
    {
        _ownerBody = owner;
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
        }

        if (!atMainMenu && GameUtility.SSSAdvancedTutorialManagerExists())
        {
            SSSAdvancedTutorialManager.Instance.Hook(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag(GameTagContainer.BladeTag)) return;
        if (_sliced) return;

        // While fading, still allow interaction,
        // but do not count as a real slice again.
        if (_isFading)
        {
            HandleFadeSlice();
            return;
        }

        _sliced = true;

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

        Sliced?.Invoke(transform.position, feedbackRequests);
        OnPartDestroyed?.Invoke();

        Destroy(gameObject);
    }

    public void StartFadeAndDestroy()
    {
        if (_sliced || _isFading) return;
        _isFading = true;

        OnPartDestroyed?.Invoke();

        StartCoroutine(FadeAndDestroyCoroutine());
    }

    private void HandleFadeSlice()
    {
        if (Time.time - _lastFadeSliceTime < fadeSliceCooldown)
            return;

        _lastFadeSliceTime = Time.time;

        if (foodPrefab != null)
        {
            Instantiate(foodPrefab, transform.position, Quaternion.identity);
        }

        Sliced?.Invoke(transform.position, feedbackRequests);
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
}