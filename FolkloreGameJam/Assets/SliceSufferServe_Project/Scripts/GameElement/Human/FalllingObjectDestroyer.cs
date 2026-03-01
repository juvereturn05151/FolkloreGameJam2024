using UnityEngine;

public class FalllingObjectDestroyer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<SliceableObject>() is SliceableObject sliceableObject)
        {
            Destroy(sliceableObject.gameObject);

            if (sliceableObject.OnPartDestroyed != null)
            {
                sliceableObject.OnPartDestroyed.Invoke();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            Destroy(other.gameObject);
        }
    }
}
