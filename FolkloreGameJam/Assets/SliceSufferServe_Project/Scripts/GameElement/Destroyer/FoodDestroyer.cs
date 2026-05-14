using UnityEngine;

public class FoodDestroyer : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            Destroy(other.gameObject);

            if (CustomerGenerator.Instance != null)
            {
                CustomerGenerator.Instance.RequestReplacementHumanNextFrame();
            }
        }
    }
}
