/*
File Name:    BladeStaminaUI.cs
Author(s):    Ju-ve Chankasemporn
Copyright:    (c) MyLoyalFans. All rights reserved.
*/

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BladeStaminaUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BladeStamina stamina;
    [SerializeField] private Image fillImage;      // Image.type = Filled recommended
    [SerializeField] private TMP_Text valueText;   // optional
    [SerializeField] private CanvasGroup failFlash; // optional (set alpha flash)

    [Header("Formatting")]
    [SerializeField] private bool showAsInteger = true;

    [Header("Fail Flash")]
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float flashAlpha = 1f;
    [SerializeField] private Image backgroundImage;

    [Header("Background Colors")]
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color inactiveColor = Color.gray;

    private Coroutine flashRoutine;

    private void OnEnable()
    {
        if (stamina == null) return;

        stamina.OnStaminaChanged += HandleStaminaChanged;
        stamina.OnSliceFailed += HandleSliceFailed;

        // initialize UI immediately
        HandleStaminaChanged(stamina.CurrentStamina, stamina.MaxStamina);
    }

    private void OnDisable()
    {
        if (stamina == null) return;

        stamina.OnStaminaChanged -= HandleStaminaChanged;
        stamina.OnSliceFailed -= HandleSliceFailed;
    }

    private void HandleStaminaChanged(float current, float max)
    {
        float t = (max <= 0f) ? 0f : current / max;

        if (fillImage != null)
            fillImage.fillAmount = t;

        if (valueText != null)
        {
            if (showAsInteger)
                valueText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            else
                valueText.text = $"{current:0.0} / {max:0.0}";
        }

        // Dim fill slightly if low stamina
        if (fillImage != null)
        {
            var c = fillImage.color;
            c.a = (current < 99f) ? 0.7f : 1f;
            fillImage.color = c;
        }

        // THIS PART IS NEW
        if (backgroundImage != null)
        {
            bool canSlice = stamina.CanSlice();
            Color target = canSlice ? activeColor : inactiveColor;
            backgroundImage.color = Color.Lerp(backgroundImage.color, target, 10f * Time.deltaTime);
        }
    }

    private void HandleSliceFailed()
    {
        if (failFlash == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashFail());
    }

    private IEnumerator FlashFail()
    {
        failFlash.alpha = flashAlpha;
        yield return new WaitForSecondsRealtime(flashDuration);
        failFlash.alpha = 0f;
    }
}