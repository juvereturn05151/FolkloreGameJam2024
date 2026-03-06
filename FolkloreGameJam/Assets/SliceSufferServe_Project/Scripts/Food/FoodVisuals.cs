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
}