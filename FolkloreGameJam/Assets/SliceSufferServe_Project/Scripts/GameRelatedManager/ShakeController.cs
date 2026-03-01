using UnityEngine;
using UnityEngine.Events;

public class ShakeController : MonoBehaviour
{
    [Header("Mobile shake")]
    [Tooltip("Higher = harder to trigger")]
    [SerializeField] private float shakeThreshold = 2.2f;

    [Tooltip("How long the shake must be sustained")]
    [SerializeField] private float requiredDuration = 0.12f;

    [Header("PC fallback")]
    [SerializeField] private bool enablePcFallback = true;
    [SerializeField] private KeyCode pcKey = KeyCode.Space;

    [Tooltip("Optional: mouse wiggle triggers too")]
    [SerializeField] private bool enableMouseWiggle = false;
    [SerializeField] private float mouseWiggleThreshold = 25f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 0.5f;

    [Header("Events")]
    public UnityEvent onActivated;

    private float shakeTimer = 0f;
    private float cooldownTimer = 0f;

    private Vector3 lastAccel;
    private Vector3 lastMousePos;

    private void Awake()
    {
        lastAccel = Input.acceleration;
        lastMousePos = Input.mousePosition;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            // still track baseline inputs so it feels stable after cooldown
            lastAccel = Input.acceleration;
            lastMousePos = Input.mousePosition;
            return;
        }

        if (Application.isMobilePlatform)
        {
            CheckMobileShake();
        }
        else
        {
            CheckPcFallback();
        }
    }

    private void CheckMobileShake()
    {
        // High-pass style: compare delta acceleration to reduce constant gravity noise
        Vector3 accel = Input.acceleration;
        Vector3 delta = accel - lastAccel;
        lastAccel = accel;

        float mag = delta.magnitude;

        if (mag >= shakeThreshold)
        {
            shakeTimer += Time.deltaTime;
            if (shakeTimer >= requiredDuration)
            {
                Trigger();
            }
        }
        else
        {
            // decay instead of hard reset, makes it more forgiving
            shakeTimer = Mathf.Max(0f, shakeTimer - Time.deltaTime * 2f);
        }
    }

    private void CheckPcFallback()
    {
        if (enablePcFallback && Input.GetKeyDown(pcKey))
        {
            Trigger();
            return;
        }

        if (enableMouseWiggle && Input.GetKeyDown(pcKey))
        {
            Vector3 mouse = Input.mousePosition;
            float dist = (mouse - lastMousePos).magnitude;
            lastMousePos = mouse;

            if (dist >= mouseWiggleThreshold)
            {
                Trigger();
            }
        }
        else
        {
            lastMousePos = Input.mousePosition;
        }
    }

    private void Trigger()
    {
        cooldownTimer = cooldown;
        shakeTimer = 0f;
        onActivated?.Invoke();
    }
}