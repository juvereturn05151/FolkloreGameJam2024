using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HumanPart))]
public class ExplosiveHumanPart : MonoBehaviour
{
    [SerializeField] private GameObject explosionParticlePrefab;
    [SerializeField] private int damage = 1;
    [SerializeField] private int scorePenalty = 50;
    [SerializeField] private bool explodeOnlyOnce = true;

    private HumanPart humanPart;
    private bool hasExploded;

    public void Configure(GameObject particlePrefab, int explosionDamage, int explosionScorePenalty)
    {
        explosionParticlePrefab = particlePrefab;
        damage = Mathf.Max(0, explosionDamage);
        scorePenalty = Mathf.Max(0, explosionScorePenalty);
    }

    private void Awake()
    {
        humanPart = GetComponent<HumanPart>();
    }

    private void OnEnable()
    {
        if (humanPart == null)
        {
            humanPart = GetComponent<HumanPart>();
        }

        humanPart.Sliced -= OnPartSliced;
        humanPart.Sliced += OnPartSliced;
    }

    private void OnDisable()
    {
        if (humanPart != null)
        {
            humanPart.Sliced -= OnPartSliced;
        }
    }

    private void OnPartSliced(Vector3 position, IReadOnlyList<FeedbackRequest> requests)
    {
        if (explodeOnlyOnce && hasExploded)
        {
            return;
        }

        hasExploded = true;
        ExplodeSystem.ExplodeAt(position, explosionParticlePrefab, damage, scorePenalty);
    }
}
