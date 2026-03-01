using UnityEngine;
using System.Collections;

public class BellUI : MonoBehaviour
{
    [Header("Bell Animation")]
    [SerializeField] private Animator bellAnimator;
    [SerializeField] private string boolName = "IsRinging";

    [Header("Cooldown")]
    [SerializeField] private float ringDuration = 0.5f;   // how long bell rings
    [SerializeField] private float cooldown = 1.0f;       // how long before next allowed trigger

    private bool isCoolingDown = false;

    public void TriggerBell()
    {
        if (bellAnimator == null) return;
        if (isCoolingDown) return;   // prevent spam

        StartCoroutine(RingRoutine());
    }

    private IEnumerator RingRoutine()
    {
        isCoolingDown = true;

        // Turn ON animation
        bellAnimator.SetBool(boolName, true);
        Debug.Log("Bell ringing!");

        // Wait for animation duration
        yield return new WaitForSeconds(ringDuration);

        // Turn OFF animation
        bellAnimator.SetBool(boolName, false);

        // Wait for cooldown before allowing next ring
        yield return new WaitForSeconds(cooldown);

        isCoolingDown = false;
    }
}