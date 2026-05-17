using UnityEngine;

[RequireComponent(typeof(HumanPart))]
public class KnightArmorLayer : MonoBehaviour
{
    [SerializeField] private GameObject armorVisual;
    [SerializeField] private GameObject armorBreakEffectPrefab;
    [SerializeField] private float armorBreakEffectScale = 0.45f;
    [SerializeField] private bool shakeOnArmorBreak = true;

    private bool armorBroken;

    public void Configure(GameObject visual, GameObject breakEffectPrefab, float breakEffectScale)
    {
        armorVisual = visual;
        armorBreakEffectPrefab = breakEffectPrefab;
        armorBreakEffectScale = breakEffectScale;
        ResetArmor();
    }

    private void Awake()
    {
        if (armorVisual == null)
        {
            armorVisual = FindArmorVisual();
        }

        ResetArmor();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (armorBroken || !col.CompareTag(GameTagContainer.BladeTag))
        {
            return;
        }

        BreakArmor();
    }

    private GameObject FindArmorVisual()
    {
        Transform armorTransform = transform.Find("ArmorVisual");
        if (armorTransform == null)
        {
            armorTransform = transform.Find("ArmoreVisual");
        }

        return armorTransform != null ? armorTransform.gameObject : null;
    }

    private void ResetArmor()
    {
        armorBroken = false;
        if (armorVisual != null)
        {
            armorVisual.SetActive(true);
        }
    }

    private void BreakArmor()
    {
        armorBroken = true;

        if (armorVisual != null)
        {
            armorVisual.SetActive(false);
        }

        SpawnBreakEffect();

        if (shakeOnArmorBreak && GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.18f, 0.12f);
        }
    }

    private void SpawnBreakEffect()
    {
        if (armorBreakEffectPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = armorVisual != null ? armorVisual.transform.position : transform.position;
        GameObject effect = Instantiate(armorBreakEffectPrefab, spawnPosition, Quaternion.identity);
        effect.transform.localScale *= Mathf.Max(0.01f, armorBreakEffectScale);
    }
}
