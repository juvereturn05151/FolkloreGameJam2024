using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(HumanBody))]
public class ObeseRapidSliceEvent : MonoBehaviour
{
    [Header("Whole Body Target")]
    [SerializeField] private Vector2 hitboxSize = new Vector2(7.5f, 8.5f);
    [SerializeField] private Vector2 hitboxOffset = new Vector2(0f, 1.5f);
    [SerializeField] private float travelSpeed = 2f;
    [SerializeField] private float gravityScale = 0.5f;
    [SerializeField] private float linearDamping = 5f;
    [SerializeField] private Vector3 focusPosition = new Vector3(0f, 1f, 0f);
    [SerializeField] private float focusedScale = 1.2f;

    [Header("Rapid Slice Event")]
    [SerializeField] private float eventDuration = 5f;
    [SerializeField] private float slowMotionScale = 0.18f;
    [SerializeField] private float slashRegisterInterval = 0.055f;
    [SerializeField] private Color flashColor = new Color(1f, 0.26f, 0.18f, 1f);
    [SerializeField] private Color blobColor = new Color(1f, 0.62f, 0.38f, 1f);

    [Header("Biomass Reward")]
    [SerializeField] private GameObject biomassPrefab;
    [SerializeField] private Sprite biomassSprite;
    [SerializeField] private int requiredSlicesForBiomass = 30;
    [SerializeField] private float biomassScoreMultiplier = 1f;

    private BoxCollider2D wholeBodyHitbox;
    private Rigidbody2D wholeBodyRigidbody;
    private SpriteRenderer[] renderers;
    private Vector3 originalScale;
    private bool eventActive;
    private bool eventFinished;
    private float lastSlashTime;
    private int sliceCount;
    private float previousTimeScale = 1f;
    private float previousFixedDeltaTime;
    private TextMeshPro comboText;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalScale = transform.localScale;
        ConvertToSingleBodyTarget();
    }

    private void FixedUpdate()
    {
        if (!eventActive && !eventFinished)
        {
            wholeBodyRigidbody.linearVelocity = new Vector2(travelSpeed, wholeBodyRigidbody.linearVelocity.y);
        }
    }

    public void ApplyMovementSpeedMultiplier(float multiplier)
    {
        travelSpeed *= Mathf.Max(0.01f, multiplier);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryRegisterSlash(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryRegisterSlash(other);
    }

    private void TryRegisterSlash(Collider2D other)
    {
        if (eventFinished || !other.CompareTag(GameTagContainer.BladeTag))
        {
            return;
        }

        if (!eventActive)
        {
            StartCoroutine(RapidSliceRoutine());
        }

        if (Time.unscaledTime - lastSlashTime < slashRegisterInterval)
        {
            return;
        }

        lastSlashTime = Time.unscaledTime;
        sliceCount++;
        PlaySliceFeedback();
    }

    private IEnumerator RapidSliceRoutine()
    {
        eventActive = true;
        previousTimeScale = Time.timeScale;
        previousFixedDeltaTime = Time.fixedDeltaTime;

        GameManager.Instance?.SetRapidSliceEventActive(true);
        TimeManager.Instance?.SetStageTimerPaused(true);
        CustomerGenerator.Instance?.SetRapidSlicePaused(true);

        Time.timeScale = Mathf.Clamp(slowMotionScale, 0.01f, 1f);
        Time.fixedDeltaTime = previousFixedDeltaTime * Time.timeScale;

        wholeBodyRigidbody.linearVelocity = Vector2.zero;
        wholeBodyRigidbody.bodyType = RigidbodyType2D.Kinematic;
        transform.position = focusPosition;
        transform.localScale = originalScale * focusedScale;
        SetBodyColor(blobColor);
        EnsureComboText();

        yield return new WaitForSecondsRealtime(eventDuration);

        EndEvent();
    }

    private void EndEvent()
    {
        if (eventFinished)
        {
            return;
        }

        eventFinished = true;
        eventActive = false;

        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = previousFixedDeltaTime;

        TimeManager.Instance?.SetStageTimerPaused(false);
        CustomerGenerator.Instance?.SetRapidSlicePaused(false);
        GameManager.Instance?.SetRapidSliceEventActive(false);

        TrySpawnBiomass();

        //if (CustomerGenerator.Instance != null)
        //{
        //    CustomerGenerator.Instance.RequestReplacementHuman();
        //}

        Destroy(gameObject);
    }

    private void ConvertToSingleBodyTarget()
    {
        HumanPart[] parts = GetComponentsInChildren<HumanPart>(true);
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].enabled = false;
        }

        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < childColliders.Length; i++)
        {
            if (childColliders[i].transform != transform)
            {
                childColliders[i].enabled = false;
            }
        }

        Rigidbody2D[] childRigidbodies = GetComponentsInChildren<Rigidbody2D>(true);
        for (int i = 0; i < childRigidbodies.Length; i++)
        {
            if (childRigidbodies[i].transform != transform)
            {
                childRigidbodies[i].simulated = false;
            }
        }

        wholeBodyRigidbody = GetComponent<Rigidbody2D>();
        if (wholeBodyRigidbody == null)
        {
            wholeBodyRigidbody = gameObject.AddComponent<Rigidbody2D>();
        }

        wholeBodyRigidbody.bodyType = RigidbodyType2D.Dynamic;
        wholeBodyRigidbody.gravityScale = gravityScale;
        wholeBodyRigidbody.linearDamping = linearDamping;
        wholeBodyRigidbody.freezeRotation = true;
        wholeBodyRigidbody.linearVelocity = new Vector2(travelSpeed, 0f);

        wholeBodyHitbox = GetComponent<BoxCollider2D>();
        if (wholeBodyHitbox == null)
        {
            wholeBodyHitbox = gameObject.AddComponent<BoxCollider2D>();
        }

        wholeBodyHitbox.isTrigger = true;
        wholeBodyHitbox.size = hitboxSize;
        wholeBodyHitbox.offset = hitboxOffset;
    }

    private void PlaySliceFeedback()
    {
        SetBodyColor(Color.Lerp(blobColor, flashColor, Mathf.Clamp01(sliceCount / 36f)));
        StartCoroutine(RestoreBlobColor());

        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.SpawnBlood(transform.position + Random.insideUnitSphere * 0.8f);
            FeedbackManager.Instance.ShakeCameraFeedback(0.08f, Mathf.Lerp(0.08f, 0.28f, Mathf.Clamp01(sliceCount / 36f)));
        }

        if (GameUtility.SoundManagerExists() && sliceCount % 4 == 0)
        {
            SoundManager.instance.PlaySFX("SFX_Slice");
        }

        if (comboText != null)
        {
            comboText.text = sliceCount.ToString();
            comboText.fontSize = Mathf.Min(8f, 3f + sliceCount * 0.08f);
        }
    }

    private IEnumerator RestoreBlobColor()
    {
        yield return new WaitForSecondsRealtime(0.045f);

        if (eventActive)
        {
            SetBodyColor(blobColor);
        }
    }

    private void SetBodyColor(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = color;
            }
        }
    }

    private void EnsureComboText()
    {
        if (comboText != null)
        {
            return;
        }

        GameObject textObject = new GameObject("Rapid Slice Combo");
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = new Vector3(0f, 5.4f, 0f);
        comboText = textObject.AddComponent<TextMeshPro>();
        comboText.alignment = TextAlignmentOptions.Center;
        comboText.fontSize = 3f;
        comboText.color = Color.yellow;
        comboText.text = "0";
    }

    private void TrySpawnBiomass()
    {
        if (sliceCount < Mathf.Max(1, requiredSlicesForBiomass) || biomassPrefab == null)
        {
            return;
        }

        GameObject reward = Instantiate(biomassPrefab, transform.position, Quaternion.identity);
        if (reward.TryGetComponent(out Food food))
        {
            food.MakeUniversalFood(biomassScoreMultiplier, biomassSprite, false);
        }
    }
}
