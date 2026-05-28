using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplaySuperComboUI : MonoBehaviour
{
    [Header("Super Meter UI")]
    [SerializeField] private Slider superMeterSlider;
    [SerializeField] private TextMeshProUGUI superMeterText;
    [SerializeField] private Image superMeterGraphic;
    [SerializeField] private GameObject superMeterRoot;
    [SerializeField] private GameObject activatedIcon;
    [SerializeField] private Color superChargingColor = new Color(0.94f, 0.18f, 0.14f, 0.95f);
    [SerializeField] private Color superReadyColor = new Color(1f, 0.75f, 0.12f, 1f);
    [SerializeField] private Color superActiveColor = new Color(0.1f, 0.85f, 1f, 1f);

    [Header("Combo UI")]
    [SerializeField] private GameObject comboRoot;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Image comboBackground;
    [SerializeField] private GameObject comboSpecialEffectRoot;
    [SerializeField] private Color comboLostBackgroundColor = new Color(0.85f, 0.05f, 0.03f, 0.9f);
    [SerializeField] private float comboPunchScale = 0.18f;
    [SerializeField] private float comboPunchDuration = 0.2f;
    [SerializeField] private float comboLostDisplayDuration = 0.6f;

    private int previousCombo;
    private Vector3 comboTextInitialScale = Vector3.one;
    private Color comboBackgroundInitialColor;
    private bool hasComboBackgroundInitialColor;

    public void BindSuperMeter()
    {
        ResolveSuperMeterUI();

        if (GameManager.Instance == null)
        {
            return;
        }

        ApplySuperMeterVisibility();

        GameManager.Instance.OnSuperMeterChanged += UpdateSuperMeterUI;
        GameManager.Instance.OnSuperActiveTimeChanged += UpdateSuperActiveUI;
        GameManager.Instance.OnSuperActivated += HandleSuperActivated;
        GameManager.Instance.OnSuperEnded += HandleSuperEnded;
        UpdateSuperMeterUI(GameManager.Instance.CurrentSuperMeter, GameManager.Instance.SuperMeterThreshold);
    }

    public void UnbindSuperMeter()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnSuperMeterChanged -= UpdateSuperMeterUI;
        GameManager.Instance.OnSuperActiveTimeChanged -= UpdateSuperActiveUI;
        GameManager.Instance.OnSuperActivated -= HandleSuperActivated;
        GameManager.Instance.OnSuperEnded -= HandleSuperEnded;
    }

    public void BindCombo()
    {
        ResolveComboUI();
        CacheComboVisualState();
        previousCombo = ComboSystem.CurrentCombo;
        ComboSystem.OnComboChanged += UpdateComboUI;
        ComboSystem.ResetCombo();
    }

    public void UnbindCombo()
    {
        ComboSystem.OnComboChanged -= UpdateComboUI;
    }

    private void ResolveSuperMeterUI()
    {
        Transform content = transform.Find("SuperMeter/Content");
        if (content == null)
        {
            return;
        }

        superMeterRoot = content.parent != null ? content.parent.gameObject : content.gameObject;

        if (superMeterSlider == null)
        {
            superMeterSlider = content.GetComponent<Slider>();
        }

        if (superMeterGraphic == null)
        {
            superMeterGraphic = content.GetComponent<Image>();
        }

        if (superMeterText == null)
        {
            superMeterText = content.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (activatedIcon == null)
        {
            Transform icon = transform.Find("SuperMeter/ActivatedIcon");
            activatedIcon = icon == null ? null : icon.gameObject;
        }
    }

    private void ResolveComboUI()
    {
        if (comboBackground == null && comboRoot != null)
        {
            comboBackground = comboRoot.GetComponentInChildren<Image>(true);
        }
    }

    private void CacheComboVisualState()
    {
        if (comboText != null)
        {
            comboTextInitialScale = comboText.transform.localScale;
        }

        if (comboBackground != null)
        {
            comboBackgroundInitialColor = comboBackground.color;
            hasComboBackgroundInitialColor = true;
        }
    }

    private bool IsSuperMeterUIAllowed()
    {
        return GameManager.Instance == null || GameManager.Instance.IsSuperMeterAllowed;
    }

    private void ApplySuperMeterVisibility()
    {
        if (superMeterRoot == null)
        {
            return;
        }

        bool isAllowed = IsSuperMeterUIAllowed();
        superMeterRoot.SetActive(isAllowed);

        if (!isAllowed)
        {
            SetActivatedIconActive(false);
        }
    }

    private void UpdateSuperMeterUI(float currentValue, float threshold)
    {
        ApplySuperMeterVisibility();

        if (!IsSuperMeterUIAllowed())
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsSuperScoreMultiplierActive)
        {
            return;
        }

        float normalizedValue = threshold <= 0f ? 0f : Mathf.Clamp01(currentValue / threshold);
        bool isReady = normalizedValue >= 1f;

        if (superMeterSlider != null)
        {
            superMeterSlider.minValue = 0f;
            superMeterSlider.maxValue = Mathf.Max(1f, threshold);
            superMeterSlider.value = Mathf.Clamp(currentValue, superMeterSlider.minValue, superMeterSlider.maxValue);
        }

        SetSuperMeterColor(isReady ? superReadyColor : superChargingColor);
        SetActivatedIconActive(isReady);

        if (superMeterText != null)
        {
            superMeterText.text = isReady ? "EVIL ENERGY READY" : $"EVIL ENERGY {Mathf.RoundToInt(normalizedValue * 100f)}%";
        }
    }

    private void UpdateSuperActiveUI(float remainingTime, float duration)
    {
        ApplySuperMeterVisibility();

        if (!IsSuperMeterUIAllowed())
        {
            return;
        }

        if (superMeterSlider != null)
        {
            superMeterSlider.minValue = 0f;
            superMeterSlider.maxValue = Mathf.Max(0.01f, duration);
            superMeterSlider.value = Mathf.Clamp(remainingTime, superMeterSlider.minValue, superMeterSlider.maxValue);
        }

        SetSuperMeterColor(superActiveColor);
        SetActivatedIconActive(false);

        if (superMeterText != null)
        {
            superMeterText.text = $"EVIL POWER x2 {Mathf.CeilToInt(remainingTime)}s";
        }

    }

    private void HandleSuperActivated()
    {
        if (!IsSuperMeterUIAllowed())
        {
            return;
        }

        Transform target = superMeterText != null ? superMeterText.transform : superMeterSlider != null ? superMeterSlider.transform : null;
        if (target != null)
        {
            target.DOPunchScale(Vector3.one * 0.15f, 0.2f, 4, 0.5f);
        }


        if (comboSpecialEffectRoot != null)
        {
            comboSpecialEffectRoot.SetActive(true);
        }
    }

    private void HandleSuperEnded()
    {
        if (GameManager.Instance != null)
        {
            UpdateSuperMeterUI(GameManager.Instance.CurrentSuperMeter, GameManager.Instance.SuperMeterThreshold);
        }
    }

    private void SetSuperMeterColor(Color color)
    {
        if (superMeterGraphic != null)
        {
            superMeterGraphic.color = color;
        }
    }

    private void SetActivatedIconActive(bool isActive)
    {
        if (activatedIcon != null)
        {
            activatedIcon.SetActive(isActive);
        }
    }

    private void UpdateComboUI(int combo)
    {
        bool comboIncreased = combo > previousCombo;
        bool comboLost = previousCombo > 0 && combo == 0;

        if (comboRoot != null)
        {
            comboRoot.SetActive(combo > 0 || comboLost);
        }

        if (comboText != null)
        {
            comboText.text = comboLost ? "Combo Lost" : $"Combo {combo}  Score x{ComboSystem.GetScoreMultiplier(combo)}";
        }

        if (comboIncreased)
        {
            RestoreComboBackground();
            PlayComboIncreaseFeedback();
        }
        else if (comboLost)
        {
            PlayComboLostFeedback();
        }

        if (comboSpecialEffectRoot != null)
        {
            // TODO: Replace this placeholder object with the final combo 10+ special effect.
            comboSpecialEffectRoot.SetActive(ComboSystem.IsSpecialEffectActive);
        }

        previousCombo = combo;
    }

    private void RestoreComboBackground()
    {
        if (comboBackground != null && hasComboBackgroundInitialColor)
        {
            comboBackground.DOKill();
            comboBackground.color = comboBackgroundInitialColor;
        }
    }

    private void PlayComboIncreaseFeedback()
    {
        if (comboText == null)
            return;

        Transform textTransform = comboText.transform;
        textTransform.DOKill();
        textTransform.localScale = comboTextInitialScale;
        textTransform
            .DOPunchScale(Vector3.one * Mathf.Max(0f, comboPunchScale), Mathf.Max(0.01f, comboPunchDuration), 8, 0.5f)
            .OnComplete(() =>
            {
                if (textTransform != null)
                {
                    textTransform.localScale = comboTextInitialScale;
                }
            });
    }

    private void PlayComboLostFeedback()
    {
        if (comboBackground != null)
        {
            comboBackground.DOKill();
            comboBackground.color = comboLostBackgroundColor;
            if (hasComboBackgroundInitialColor)
            {
                comboBackground.DOColor(comboBackgroundInitialColor, Mathf.Max(0.01f, comboLostDisplayDuration));
            }
        }

        if (comboRoot != null)
        {
            DOVirtual.DelayedCall(Mathf.Max(0.01f, comboLostDisplayDuration), () =>
            {
                if (comboRoot != null && ComboSystem.CurrentCombo == 0)
                {
                    comboRoot.SetActive(false);
                }
            });
        }
    }
}
