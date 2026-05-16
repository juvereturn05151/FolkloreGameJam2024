using UnityEngine;

public class ExplodeSystem : MonoBehaviour
{
    public static ExplodeSystem Instance { get; private set; }

    [Header("Default Explosion")]
    [SerializeField] private GameObject defaultExplosionParticlePrefab;
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private int defaultScorePenalty = 50;
    [SerializeField] private bool shakeCamera = true;

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

        ApplyExplosion(position, particlePrefab, damage, scorePenalty, true);
    }

    public void Explode(Vector3 position, GameObject particlePrefab = null, int damage = -1, int scorePenalty = -1)
    {
        GameObject selectedParticlePrefab = particlePrefab != null ? particlePrefab : defaultExplosionParticlePrefab;
        int selectedDamage = damage >= 0 ? damage : defaultDamage;
        int selectedScorePenalty = scorePenalty >= 0 ? scorePenalty : defaultScorePenalty;

        ApplyExplosion(position, selectedParticlePrefab, selectedDamage, selectedScorePenalty, shakeCamera);
    }

    private static void ApplyExplosion(Vector3 position, GameObject particlePrefab, int damage, int scorePenalty, bool shouldShakeCamera)
    {
        if (particlePrefab != null)
        {
            Instantiate(particlePrefab, position, Quaternion.identity);
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

        if (shouldShakeCamera && GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.5f, 0.5f);
        }
    }
}
