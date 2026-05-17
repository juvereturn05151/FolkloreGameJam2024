using UnityEngine;

[RequireComponent(typeof(HumanBody))]
public class KnightHuman : MonoBehaviour
{
    [Header("Premium Food")]
    [SerializeField] private GameObject premiumBrainPrefab;
    [SerializeField] private GameObject premiumBloodPrefab;
    [SerializeField] private GameObject premiumIntestinePrefab;
    [SerializeField] private GameObject premiumShitPrefab;

    [Header("Armor Break Feedback")]
    [SerializeField] private GameObject armorBreakEffectPrefab;
    [SerializeField] private float armorBreakEffectScale = 0.45f;

    [SerializeField] private int cutsRequiredToDestroy = 2;

    private void Awake()
    {
        HumanPart[] parts = GetComponentsInChildren<HumanPart>(true);
        for (int i = 0; i < parts.Length; i++)
        {
            ConfigurePart(parts[i]);
        }
    }

    private void ConfigurePart(HumanPart part)
    {
        if (part == null)
        {
            return;
        }

        part.SetCutsRequiredToDestroy(cutsRequiredToDestroy);

        string partName = part.name;
        if (partName.Contains("Head"))
        {
            ConfigurePart(part, premiumBrainPrefab);
        }
        else if (partName.Contains("Neck"))
        {
            ConfigurePart(part, premiumBloodPrefab);
        }
        else if (partName.Contains("Body"))
        {
            ConfigurePart(part, premiumIntestinePrefab);
        }
        else if (partName.Contains("Leg"))
        {
            ConfigurePart(part, premiumShitPrefab);
        }
    }

    private void ConfigurePart(HumanPart part, GameObject premiumFoodPrefab)
    {
        if (premiumFoodPrefab != null)
        {
            part.SetFoodPrefab(premiumFoodPrefab);
        }

        KnightArmorLayer armorLayer = part.GetComponent<KnightArmorLayer>();
        if (armorLayer == null)
        {
            armorLayer = part.gameObject.AddComponent<KnightArmorLayer>();
        }

        armorLayer.Configure(FindArmorVisual(part.transform), armorBreakEffectPrefab, armorBreakEffectScale);
    }

    private GameObject FindArmorVisual(Transform partTransform)
    {
        Transform armorTransform = partTransform.Find("ArmorVisual");
        if (armorTransform == null)
        {
            armorTransform = partTransform.Find("ArmoreVisual");
        }

        return armorTransform != null ? armorTransform.gameObject : null;
    }
}
