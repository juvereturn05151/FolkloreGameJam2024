using UnityEngine;
using System.Collections.Generic;

public class RottenPot : MonoBehaviour, IRotModifier
{
    [SerializeField] private float multiplier = 2f;
    private readonly HashSet<FoodRotting> affectedFoods = new();

    public float Multiplier => multiplier;
    public int Priority => 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Food food) && food.IsPremiumFood)
        {
            return;
        }

        if (other.TryGetComponent(out FoodRotting rotting) && rotting.CanRot && affectedFoods.Add(rotting))
        {
            rotting.AddModifier(this);

            if (GameUtility.SSSAdvancedTutorialManagerExists())
            {
                SSSAdvancedTutorialManager.Instance.ReportProgress(TutorialType.PutFoodInRottenPot);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out FoodRotting rotting))
        {
            RemoveFood(rotting);
        }
    }

    private void OnDisable()
    {
        foreach (FoodRotting rotting in affectedFoods)
        {
            if (rotting != null)
            {
                rotting.RemoveModifier(this);
            }
        }

        affectedFoods.Clear();
    }

    private void RemoveFood(FoodRotting rotting)
    {
        if (rotting != null && affectedFoods.Remove(rotting))
        {
            rotting.RemoveModifier(this);
        }
    }
}
