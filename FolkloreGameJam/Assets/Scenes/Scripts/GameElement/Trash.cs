using UnityEngine;

public class Trash : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Food>() is Food food)
        {
            Destroy(food.gameObject);
        }
    }
}
