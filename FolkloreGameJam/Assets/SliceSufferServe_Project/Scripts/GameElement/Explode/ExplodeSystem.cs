using System.Collections;
using UnityEngine;

public class ExplodeSystem : MonoBehaviour
{
    public static ExplodeSystem Instance { get; private set; }

    [Header("Default Explosion")]
    [SerializeField] private GameObject defaultExplosionParticlePrefab;
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private int defaultScorePenalty = 50;
    [SerializeField] private float explosionVisualScale = 1.85f;
    [SerializeField] private float explosionFeedbackDuration = 6f;
    [SerializeField] private float particleLifetimeMultiplier = 3f;
    [SerializeField] private float particleSizeMultiplier = 1.45f;
    [SerializeField] private bool shakeCamera = true;
    [SerializeField] private float cameraShakeInterval = 0.35f;
    [SerializeField] private float cameraShakeDuration = 0.35f;
    [SerializeField] private float cameraShakeStrength = 0.85f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void ExplodeAt(Vector3 position, GameObject particlePrefab, int damage, int scorePenalty)
    {
        if (Instance != null)
        {
            Instance.Explode(position, particlePrefab, damage, scorePenalty);
            return;
        }

        ApplyExplosion(position, particlePrefab, damage, scorePenalty, true, 1.85f, 6f, 3f, 1.45f, 0.35f, 0.35f, 0.85f);
    }

    public void Explode(Vector3 position, GameObject particlePrefab = null, int damage = -1, int scorePenalty = -1)
    {
        GameObject selectedParticlePrefab = particlePrefab != null ? particlePrefab : defaultExplosionParticlePrefab;
        int selectedDamage = damage >= 0 ? damage : defaultDamage;
        int selectedScorePenalty = scorePenalty >= 0 ? scorePenalty : defaultScorePenalty;

        ApplyExplosion(
            position,
            selectedParticlePrefab,
            selectedDamage,
            selectedScorePenalty,
            shakeCamera,
            explosionVisualScale,
            explosionFeedbackDuration,
            particleLifetimeMultiplier,
            particleSizeMultiplier,
            cameraShakeInterval,
            cameraShakeDuration,
            cameraShakeStrength);
    }

    private static void ApplyExplosion(
        Vector3 position,
        GameObject particlePrefab,
        int damage,
        int scorePenalty,
        bool shouldShakeCamera,
        float visualScale,
        float feedbackDuration,
        float lifetimeMultiplier,
        float sizeMultiplier,
        float shakeInterval,
        float shakeDuration,
        float shakeStrength)
    {
        if (particlePrefab != null)
        {
            GameObject explosionObject = Instantiate(particlePrefab, position, Quaternion.identity);
            NuclearExplosionFeedback.Enhance(
                explosionObject,
                visualScale,
                feedbackDuration,
                lifetimeMultiplier,
                sizeMultiplier,
                shouldShakeCamera,
                shakeInterval,
                shakeDuration,
                shakeStrength);
        }

        if (damage > 0 && GameUtility.HPManagerExists())
        {
            HPManager.Instance.TakeDamage(damage);
        }

        if (scorePenalty > 0)
        {
            if (GameUtility.GameManagerExists())
            {
                GameManager.Instance.DecreaseScore(scorePenalty);
            }
            else if (GameUtility.ScoreManagerExists())
            {
                ScoreManager.Instance.SubtractScore(scorePenalty);
            }
        }

        if (shouldShakeCamera && particlePrefab == null && GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(shakeDuration, shakeStrength);
        }
    }
}

public class NuclearExplosionFeedback : MonoBehaviour
{
    private float feedbackDuration = 6f;
    private bool shakeCamera = true;
    private float shakeInterval = 0.35f;
    private float shakeDuration = 0.35f;
    private float shakeStrength = 0.85f;

    public static void Enhance(
        GameObject explosionObject,
        float visualScale,
        float duration,
        float lifetimeMultiplier,
        float sizeMultiplier,
        bool shouldShakeCamera,
        float cameraShakeInterval,
        float cameraShakeDuration,
        float cameraShakeStrength)
    {
        if (explosionObject == null)
        {
            return;
        }

        NuclearExplosionFeedback feedback = explosionObject.GetComponent<NuclearExplosionFeedback>();
        if (feedback == null)
        {
            feedback = explosionObject.AddComponent<NuclearExplosionFeedback>();
        }

        feedback.Configure(duration, shouldShakeCamera, cameraShakeInterval, cameraShakeDuration, cameraShakeStrength);
        feedback.EnhanceParticles(visualScale, duration, lifetimeMultiplier, sizeMultiplier);
    }

    private void Configure(float duration, bool shouldShakeCamera, float cameraShakeInterval, float cameraShakeDuration, float cameraShakeStrength)
    {
        feedbackDuration = Mathf.Max(0.5f, duration);
        shakeCamera = shouldShakeCamera;
        shakeInterval = Mathf.Max(0.05f, cameraShakeInterval);
        shakeDuration = Mathf.Max(0.05f, cameraShakeDuration);
        shakeStrength = Mathf.Max(0f, cameraShakeStrength);
    }

    private void EnhanceParticles(float visualScale, float duration, float lifetimeMultiplier, float sizeMultiplier)
    {
        transform.localScale *= Mathf.Max(0.1f, visualScale);

        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem particleSystem = particleSystems[i];
            if (particleSystem == null)
            {
                continue;
            }

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = Mathf.Max(main.duration, duration);
            main.startLifetime = MultiplyMinMaxCurve(main.startLifetime, Mathf.Max(1f, lifetimeMultiplier));
            main.startSize = MultiplyMinMaxCurve(main.startSize, Mathf.Max(1f, sizeMultiplier));
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = true;

            particleSystem.Play(true);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(FeedbackCoroutine());
    }

    private IEnumerator FeedbackCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < feedbackDuration)
        {
            if (shakeCamera && GameUtility.FeedbackManagerExists())
            {
                float t = 1f - (elapsed / feedbackDuration);
                FeedbackManager.Instance.ShakeCameraFeedback(shakeDuration, shakeStrength * Mathf.Max(0.2f, t));
            }

            yield return new WaitForSeconds(shakeInterval);
            elapsed += shakeInterval;
        }

        Destroy(gameObject, 2f);
    }

    private static ParticleSystem.MinMaxCurve MultiplyMinMaxCurve(ParticleSystem.MinMaxCurve curve, float multiplier)
    {
        curve.constant *= multiplier;
        curve.constantMin *= multiplier;
        curve.constantMax *= multiplier;
        curve.curveMultiplier *= multiplier;
        return curve;
    }
}
