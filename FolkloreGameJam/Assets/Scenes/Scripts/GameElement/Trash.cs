using UnityEngine;

public class Trash : MonoBehaviour
{
    [SerializeField] private GameObject trashFX;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Food>() is Food food)
        {
            SoundManager.instance.PlaySFX("Trash");
            Instantiate(trashFX, transform.position, Quaternion.identity);
            Destroy(food.gameObject);
        }
    }
}