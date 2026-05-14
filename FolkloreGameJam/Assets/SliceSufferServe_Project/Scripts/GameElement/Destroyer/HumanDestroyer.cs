using System;
using UnityEngine;

public class HumanDestroyer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HumanPart>() is HumanPart part)
        {
            if (part.OwnerBody != null)
            {
                part.OwnerBody.NotifyPartMissedDestroyer(part);
            }
            else
            {
                part.DestroyWithoutFood();
                if (CustomerGenerator.Instance != null)
                {
                    CustomerGenerator.Instance.RequestReplacementHumanNextFrame();
                }
            }

            return;
        }
    }
}
