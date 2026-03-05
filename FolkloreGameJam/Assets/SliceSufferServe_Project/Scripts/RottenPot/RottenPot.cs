using UnityEngine;

public class RottenPot : MonoBehaviour, IRotModifier
{
    [SerializeField] private float multiplier = 2f;
    public float Multiplier => multiplier;
    public int Priority => 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<FoodRotting>() is FoodRotting rotting)
            rotting.AddModifier(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<FoodRotting>() is FoodRotting rotting)
            rotting.RemoveModifier(this);
    }
}