using System;
using System.Collections.Generic;
using UnityEngine;

public class FoodRotting : MonoBehaviour
{
    public event Action<FoodState> OnStateChanged;
    public event Action OnExpired;

    [SerializeField] 
    private float baseRottenTime = 10f;

    [SerializeField]
    private GameObject rottenEffect;

    private float remaining;
    private FoodState state = FoodState.Normal;
    private bool canRot = true;

    private readonly List<IRotModifier> modifiers = new();

    public FoodState State => state;
    public float Remaining => remaining;
    public float BaseRottenTime => baseRottenTime;
    public bool CanRot => canRot;

    private void OnEnable()
    {
        remaining = baseRottenTime;
    }

    public void SetCanRot(bool value)
    {
        canRot = value;

        if (canRot)
        {
            return;
        }

        modifiers.Clear();
        remaining = baseRottenTime;

        if (rottenEffect != null)
        {
            rottenEffect.SetActive(false);
        }
    }

    public void AddModifier(IRotModifier mod)
    {
        if (!canRot || mod == null || modifiers.Contains(mod))
        {
            return;
        }

        modifiers.Add(mod);
        if (rottenEffect != null)
        {
            rottenEffect.SetActive(true);
        }
    }

    public void RemoveModifier(IRotModifier mod)
    {
        if (mod != null) 
        {
            modifiers.Remove(mod);
            if (rottenEffect != null)
            {
                rottenEffect.SetActive(modifiers.Count > 0);
            }
        }
    }

    private float EffectiveMultiplier
    {
        get
        {
            float mul = 1f;
            for (int i = 0; i < modifiers.Count; i++)
                mul *= Mathf.Max(0f, modifiers[i].Multiplier);
            return mul;
        }
    }

    public void Tick(float dt)
    {
        if (!canRot) return;
        if (state == FoodState.Disappear) return;

        remaining -= dt * EffectiveMultiplier;

        if (remaining <= 0f)
        {
            AdvanceState();
        }
    }

    private void AdvanceState()
    {
        state++;

        OnStateChanged?.Invoke(state);

        if (state == FoodState.Disappear)
        {
            OnExpired?.Invoke();
            return;
        }

        remaining = baseRottenTime; // reset for next stage
    }
}
