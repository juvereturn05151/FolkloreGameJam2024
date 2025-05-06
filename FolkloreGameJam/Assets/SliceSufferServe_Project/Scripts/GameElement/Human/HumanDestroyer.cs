using System;
using UnityEngine;

public class HumanDestroyer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HumanPart>() is HumanPart part)
        {
            Destroy(part.gameObject);
            if (part.OnPartDestroyed != null)
            {
                part.OnPartDestroyed.Invoke();
            }
            return;
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
