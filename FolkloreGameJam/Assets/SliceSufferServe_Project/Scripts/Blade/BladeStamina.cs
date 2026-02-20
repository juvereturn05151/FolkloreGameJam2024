/*
File Name:    BladeStamina.cs
Author(s):    Ju-ve Chankasemporn
Copyright:    (c) MyLoyalFans. All rights reserved.
*/

using System;
using UnityEngine;

public class BladeStamina : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float sliceCost = 50f;
    [SerializeField] private float regenPerSecond = 5f;

    [Header("Behavior")]
    [SerializeField] private bool regenOnlyWhenNotSlicing = false; // optional

    public event Action<float, float> OnStaminaChanged; // (current, max)
    public event Action OnSliceFailed;                  // not enough stamina
    public event Action OnSliceSucceeded;               // for SFX/FX if you want

    public float MaxStamina => maxStamina;
    public float CurrentStamina { get; private set; }

    // Optional: if your blade has a state like "is slicing" you can set this
    public bool IsSlicing { get; set; }

    private void Awake()
    {
        CurrentStamina = maxStamina;
        NotifyChanged();
    }

    private void Update()
    {
        // Regen
        if (CurrentStamina < maxStamina)
        {
            if (!regenOnlyWhenNotSlicing || !IsSlicing)
            {
                CurrentStamina += regenPerSecond * Time.deltaTime;
                if (CurrentStamina > maxStamina) CurrentStamina = maxStamina;
                NotifyChanged();
            }
        }
    }

    public bool CanSlice()
    {
        return CurrentStamina >= sliceCost;
    }

    /// <summary>
    /// Call this when a slice is attempted (e.g., on swipe start or when you confirm a cut).
    /// Returns true if stamina was spent successfully.
    /// </summary>
    public bool TryConsumeForSlice()
    {
        if (CurrentStamina < sliceCost)
        {
            OnSliceFailed?.Invoke();
            return false;
        }

        CurrentStamina -= sliceCost;
        if (CurrentStamina < 0f) CurrentStamina = 0f;

        NotifyChanged();
        OnSliceSucceeded?.Invoke();
        return true;
    }

    public void RefillToMax()
    {
        CurrentStamina = maxStamina;
        NotifyChanged();
    }

    public void SetMaxStamina(float newMax, bool keepRatio = true)
    {
        newMax = Mathf.Max(1f, newMax);

        float ratio = (maxStamina <= 0f) ? 1f : (CurrentStamina / maxStamina);
        maxStamina = newMax;

        CurrentStamina = keepRatio ? ratio * maxStamina : Mathf.Min(CurrentStamina, maxStamina);
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }
}