using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomerPatienceController : MonoBehaviour
{
    [SerializeField] private Slider patienceSlider;
    public Slider PatienceSlider => patienceSlider;
    [SerializeField] private float decreasePatienceSpeed = 0.2f;

    public UnityEvent onPatienceDepleted;

    public float CurrentValue => patienceSlider != null ? patienceSlider.value : 0f;
    public float MaxValue => patienceSlider != null ? patienceSlider.maxValue : 0f;

    private void OnEnable()
    {
        if (patienceSlider != null)
        {
            patienceSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnDisable()
    {
        if (patienceSlider != null)
        {
            patienceSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    public void Setup(int basePatience, FoodState desiredFoodState, int orderCount)
    {
        if (patienceSlider == null)
            return;

        float maxPatience = basePatience * ((int)desiredFoodState + 1) * Mathf.Max(1, orderCount);
        patienceSlider.maxValue = maxPatience;
        patienceSlider.value = maxPatience;
    }

    public void Tick()
    {
        if (patienceSlider == null || patienceSlider.value <= 0f)
            return;

        float nextValue = Mathf.Lerp(
            patienceSlider.value,
            patienceSlider.value - 1f,
            Time.deltaTime * decreasePatienceSpeed
        );

        patienceSlider.value = nextValue;
    }

    public void Reward(int amount, float duration = 1f)
    {
        if (patienceSlider == null)
            return;

        float targetValue = Mathf.Min(patienceSlider.maxValue, patienceSlider.value + amount);
        patienceSlider.DOValue(targetValue, duration);
    }

    public void PenalizeHalf(float duration = 1f)
    {
        if (patienceSlider == null)
            return;

        float decreasedValue = patienceSlider.value / 2f;
        patienceSlider.DOValue(decreasedValue, duration).SetEase(Ease.OutSine);
        patienceSlider.transform.DOShakePosition(duration, new Vector3(0.25f, 0.25f, 0f));
    }

    private void OnSliderValueChanged(float value)
    {
        if (value <= 0f)
        {
            onPatienceDepleted?.Invoke();
        }
    }
}