using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private SpriteRenderer renderer2D;
    [SerializeField] 
    private Animator animator;
    [SerializeField] 
    private Slider rottenSlider;

    [Header("Sprites / Menu")]
    [SerializeField] private Menu menu;

    [Header("Effects")]
    [SerializeField] private GameObject foodStateEffectPrefab;
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private SpriteRenderer outerGlowRenderer;
    [SerializeField] private SpriteRenderer innerGlowRenderer;

    [Header("Animator Settings")]
    [SerializeField] private float almostDisappearThreshold = 3f;

    private FoodState _state = FoodState.Normal;
    private float _glowPulseOffset;
    private bool lockUniversalSprite;

    private const float InnerGlowScale = 1.13f;
    private const float OuterGlowScale = 1.28f;
    private static readonly Color InnerGlowColor = new Color(1f, 0.86f, 0.25f, 0.42f);
    private static readonly Color OuterGlowColor = new Color(1f, 0.58f, 0.05f, 0.22f);

    private void Awake()
    {
        _glowPulseOffset = Random.Range(0f, 1f);
    }

    private void LateUpdate()
    {
        if (innerGlowRenderer == null || outerGlowRenderer == null) 
        {
            return;
        }

        float pulse = Mathf.Sin((Time.time + _glowPulseOffset) * 3.25f) * 0.5f + 0.5f;
        float innerScale = InnerGlowScale + pulse * 0.035f;
        float outerScale = OuterGlowScale + pulse * 0.06f;

        innerGlowRenderer.transform.localScale = new Vector3(innerScale, innerScale, 1f);
        outerGlowRenderer.transform.localScale = new Vector3(outerScale, outerScale, 1f);

        Color innerColor = InnerGlowColor;
        innerColor.a = Mathf.Lerp(0.32f, 0.48f, pulse);
        innerGlowRenderer.color = innerColor;

        Color outerColor = OuterGlowColor;
        outerColor.a = Mathf.Lerp(0.14f, 0.26f, pulse);
        outerGlowRenderer.color = outerColor;
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
        {
            rottenSlider.transform.DOShakePosition(0.5f, 0.5f);
        }


        if (foodStateEffectPrefab != null) 
        {
            Instantiate(foodStateEffectPrefab, transform.position, Quaternion.identity, transform);
        }
            
        // Swap sprite based on state
        if (!lockUniversalSprite && renderer2D != null && menu != null)
        {
            if (newState == FoodState.MediumRotten)
            {
                renderer2D.sprite = menu.MediumRottenSprite;
            }
            else if (newState == FoodState.SuperRotten) 
            {
                renderer2D.sprite = menu.SuperRottenSprite;
            }

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
        {
            rottenSlider.gameObject.SetActive(false);
        }
    }

    public void SpawnDust(Vector3 position, Quaternion rotation)
    {
        if (dustPrefab != null)
        {
            Instantiate(dustPrefab, position, rotation);
        }
    }

    public void ApplyUniversalFoodVisuals(Sprite universalSprite = null, bool useGlow = false)
    {
        lockUniversalSprite = universalSprite != null;

        if (renderer2D != null)
        {
            if (universalSprite != null)
            {
                renderer2D.sprite = universalSprite;
            }

            renderer2D.color = Color.white;
        }

        UpdateGlowSprite();
        SetGlowActive(useGlow);
        HideRotUI();
    }

    private void UpdateGlowSprite()
    {
        if (renderer2D == null)
            return;

        if (innerGlowRenderer != null)
            innerGlowRenderer.sprite = renderer2D.sprite;

        if (outerGlowRenderer != null)
            outerGlowRenderer.sprite = renderer2D.sprite;
    }

    private void SetGlowActive(bool active)
    {
        if (innerGlowRenderer != null)
            innerGlowRenderer.gameObject.SetActive(active);

        if (outerGlowRenderer != null)
            outerGlowRenderer.gameObject.SetActive(active);
    }
}
