using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer renderer2D;
    [SerializeField] private Animator animator;
    [SerializeField] private Slider rottenSlider;

    [Header("Sprites / Menu")]
    [SerializeField] private Menu menu;

    [Header("Effects")]
    [SerializeField] private GameObject foodStateEffectPrefab;
    [SerializeField] private GameObject dustPrefab;

    [Header("Animator Settings")]
    [SerializeField] private float almostDisappearThreshold = 3f;

    private FoodState _state = FoodState.Normal;
    private SpriteRenderer _innerGlowRenderer;
    private SpriteRenderer _outerGlowRenderer;
    private Transform _innerGlowTransform;
    private Transform _outerGlowTransform;
    private float _glowPulseOffset;

    private const float InnerGlowScale = 1.13f;
    private const float OuterGlowScale = 1.28f;
    private static readonly Color InnerGlowColor = new Color(1f, 0.86f, 0.25f, 0.42f);
    private static readonly Color OuterGlowColor = new Color(1f, 0.58f, 0.05f, 0.22f);

    private void Awake()
    {
        if (ShouldUseObeseGlow())
        {
            CreateGlowRenderers();
        }
    }

    private void LateUpdate()
    {
        if (_innerGlowRenderer == null || _outerGlowRenderer == null)
            return;

        float pulse = Mathf.Sin((Time.time + _glowPulseOffset) * 3.25f) * 0.5f + 0.5f;
        float innerScale = InnerGlowScale + pulse * 0.035f;
        float outerScale = OuterGlowScale + pulse * 0.06f;

        _innerGlowTransform.localScale = new Vector3(innerScale, innerScale, 1f);
        _outerGlowTransform.localScale = new Vector3(outerScale, outerScale, 1f);

        Color innerColor = InnerGlowColor;
        innerColor.a = Mathf.Lerp(0.32f, 0.48f, pulse);
        _innerGlowRenderer.color = innerColor;

        Color outerColor = OuterGlowColor;
        outerColor.a = Mathf.Lerp(0.14f, 0.26f, pulse);
        _outerGlowRenderer.color = outerColor;
    }

    public void UpdateRotSlider(float remaining, float baseRottenTime)
    {
        if (rottenSlider == null) return;

        rottenSlider.maxValue = baseRottenTime;
        // Direct assignment is usually best for UI; DOTween is for transitions.
        rottenSlider.value = Mathf.Clamp(remaining, 0f, baseRottenTime);

        if (_state == FoodState.SuperRotten && animator != null)
        {
            animator.SetBool("almost_disappear", remaining <= almostDisappearThreshold);
        }
    }

    public void ApplyState(FoodState newState)
    {
        _state = newState;

        if (rottenSlider != null)
            rottenSlider.transform.DOShakePosition(0.5f, 0.5f);

        if (foodStateEffectPrefab != null)
            Instantiate(foodStateEffectPrefab, transform.position, Quaternion.identity, transform);

        // Swap sprite based on state
        if (renderer2D != null && menu != null)
        {
            if (newState == FoodState.MediumRotten)
                renderer2D.sprite = menu.MediumRottenSprite;
            else if (newState == FoodState.SuperRotten)
                renderer2D.sprite = menu.SuperRottenSprite;

            UpdateGlowSprite();
        }

        // Smoothly refill slider for next stage (if not disappearing)
        if (newState != FoodState.Disappear && rottenSlider != null)
        {
            rottenSlider.DOValue(rottenSlider.maxValue, 0.25f).SetEase(Ease.InQuart);
        }
    }

    public void HideRotUI()
    {
        if (rottenSlider != null)
            rottenSlider.gameObject.SetActive(false);
    }

    public void SpawnDust(Vector3 position, Quaternion rotation)
    {
        if (dustPrefab != null)
        {
            Instantiate(dustPrefab, position, rotation);
        }
    }

    private bool ShouldUseObeseGlow()
    {
        return renderer2D != null && menu != null && menu.name.StartsWith("Obese ");
    }

    private void CreateGlowRenderers()
    {
        _glowPulseOffset = Random.Range(0f, 1f);
        _outerGlowRenderer = CreateGlowRenderer("Obese Outer Glow", OuterGlowColor, OuterGlowScale);
        _outerGlowTransform = _outerGlowRenderer.transform;

        _innerGlowRenderer = CreateGlowRenderer("Obese Inner Glow", InnerGlowColor, InnerGlowScale);
        _innerGlowTransform = _innerGlowRenderer.transform;

        UpdateGlowSprite();
    }

    private SpriteRenderer CreateGlowRenderer(string objectName, Color color, float scale)
    {
        GameObject glow = new GameObject(objectName);
        glow.transform.SetParent(renderer2D.transform, false);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localRotation = Quaternion.identity;
        glow.transform.localScale = new Vector3(scale, scale, 1f);

        SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = renderer2D.sprite;
        glowRenderer.color = color;
        glowRenderer.flipX = renderer2D.flipX;
        glowRenderer.flipY = renderer2D.flipY;
        glowRenderer.sortingLayerID = renderer2D.sortingLayerID;
        glowRenderer.sortingOrder = renderer2D.sortingOrder - 1;
        glowRenderer.maskInteraction = renderer2D.maskInteraction;
        return glowRenderer;
    }

    private void UpdateGlowSprite()
    {
        if (_innerGlowRenderer != null)
            _innerGlowRenderer.sprite = renderer2D.sprite;

        if (_outerGlowRenderer != null)
            _outerGlowRenderer.sprite = renderer2D.sprite;
    }
}
