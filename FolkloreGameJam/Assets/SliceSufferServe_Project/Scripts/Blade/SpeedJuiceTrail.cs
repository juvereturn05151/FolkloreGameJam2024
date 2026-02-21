using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class SpeedJuiceTrail : MonoBehaviour
{
    [Header("Track Transform (optional)")]
    [SerializeField] private Transform targetToTrack; // if null, uses this transform

    [Header("Speed Range (world units/sec)")]
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 20f;

    [Header("Width")]
    [SerializeField] private float widthAtMin = 0.03f;
    [SerializeField] private float widthAtMax = 0.14f;

    [Header("Lifetime")]
    [SerializeField] private float timeAtMin = 0.06f;
    [SerializeField] private float timeAtMax = 0.22f;

    [Header("Alpha")]
    [SerializeField] private float alphaAtMin = 0.2f;
    [SerializeField] private float alphaAtMax = 1.0f;

    private TrailRenderer tr;
    private Gradient baseGradient;

    private Vector3 prevPos;
    private bool hasPrev;

    private void Awake()
    {
        tr = GetComponent<TrailRenderer>();
        baseGradient = tr.colorGradient;

        if (targetToTrack == null)
            targetToTrack = transform;
    }

    private void OnEnable()
    {
        // Reset on enable so you don't get a huge speed spike
        prevPos = targetToTrack.position;
        hasPrev = true;
    }

    private void LateUpdate()
    {
        if (targetToTrack == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 pos = targetToTrack.position;

        if (!hasPrev)
        {
            prevPos = pos;
            hasPrev = true;
            return;
        }

        float speed = Vector3.Distance(pos, prevPos) / dt;
        prevPos = pos;

        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

        tr.startWidth = Mathf.Lerp(widthAtMin, widthAtMax, t);
        tr.time = Mathf.Lerp(timeAtMin, timeAtMax, t);

        tr.colorGradient = ScaleGradientAlpha(baseGradient, Mathf.Lerp(alphaAtMin, alphaAtMax, t));
    }

    private static Gradient ScaleGradientAlpha(Gradient g, float a)
    {
        var ng = new Gradient();

        var cks = g.colorKeys;
        var aks = g.alphaKeys;

        for (int i = 0; i < aks.Length; i++)
            aks[i].alpha = Mathf.Clamp01(aks[i].alpha * a);

        ng.SetKeys(cks, aks);
        return ng;
    }
}