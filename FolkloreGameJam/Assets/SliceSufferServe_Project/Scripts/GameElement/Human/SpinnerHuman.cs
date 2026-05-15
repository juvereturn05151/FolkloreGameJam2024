using UnityEngine;

public class SpinnerHuman : MonoBehaviour
{
    [SerializeField] private float angularSpeed = 180f;
    [SerializeField] private bool stopWhenBeingDestroyed = true;

    private HumanBody humanBody;
    private Rigidbody2D[] rigidbodies;
    private float[] startRotations;
    private float spinAngle;

    private void Awake()
    {
        humanBody = GetComponent<HumanBody>();
        rigidbodies = GetComponentsInChildren<Rigidbody2D>(true);
        startRotations = new float[rigidbodies.Length];

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i] == null)
            {
                continue;
            }

            startRotations[i] = rigidbodies[i].rotation;
        }
    }

    private void FixedUpdate()
    {
        if (rigidbodies == null || rigidbodies.Length == 0)
        {
            return;
        }

        if (stopWhenBeingDestroyed && humanBody != null && humanBody.IsBeingDestroyed)
        {
            StopSpin();
            return;
        }

        spinAngle += angularSpeed * Time.fixedDeltaTime;

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody2D rb = rigidbodies[i];

            if (rb == null || !rb.gameObject.activeInHierarchy)
            {
                continue;
            }

            rb.MoveRotation(startRotations[i] + spinAngle);
        }
    }

    private void StopSpin()
    {
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i] != null)
            {
                rigidbodies[i].angularVelocity = 0f;
            }
        }
    }
}
