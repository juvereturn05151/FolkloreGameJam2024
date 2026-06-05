using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HumanStoreCatalog", menuName = "Slice Suffer Serve/Human Store Catalog")]
public class HumanStoreCatalog : ScriptableObject
{
    private const string DefaultResourcePath = "HumanStoreCatalog";

    [SerializeField] private List<HumanStoreSectionConfig> sections = new List<HumanStoreSectionConfig>();

    public IReadOnlyList<HumanStoreSectionConfig> Sections => sections;

    public static HumanStoreCatalog LoadDefault()
    {
        return Resources.Load<HumanStoreCatalog>(DefaultResourcePath);
    }
}

[Serializable]
public class HumanStoreSectionConfig
{
    [SerializeField] private bool enabled = true;
    [SerializeField] private string sectionTitle;
    [SerializeField] private HumanType humanType;
    [SerializeField] private BodyPartType bodyPart;
    [SerializeField] private int price = 100;
    [SerializeField] private int skipUnlockableCount;
    [SerializeField] private int maxItems;
    [SerializeField] private string itemNameFormat;
    [SerializeField] private List<string> itemDisplayNames = new List<string>();

    public bool Enabled => enabled;
    public string SectionTitle => sectionTitle;
    public HumanType HumanType => humanType;
    public BodyPartType BodyPart => bodyPart;
    public int Price => Mathf.Max(0, price);
    public int SkipUnlockableCount => Mathf.Max(0, skipUnlockableCount);
    public int MaxItems => Mathf.Max(0, maxItems);

    public string GetDisplayName(int unlockableIndex, Sprite sprite)
    {
        if (sprite != null && !string.IsNullOrWhiteSpace(sprite.name))
        {
            return sprite.name;
        }

        if (unlockableIndex >= 0 && unlockableIndex < itemDisplayNames.Count && !string.IsNullOrWhiteSpace(itemDisplayNames[unlockableIndex]))
        {
            return itemDisplayNames[unlockableIndex];
        }

        string partName = GetPartDisplayName(bodyPart);
        string humanName = CharacterCustomizer.GetHumanTypeDisplayName(humanType);
        string format = string.IsNullOrWhiteSpace(itemNameFormat)
            ? (humanType == HumanType.NormalHuman ? "{index} {part}" : "{human} {part} {number}")
            : itemNameFormat;

        return format
            .Replace("{human}", humanName)
            .Replace("{part}", partName)
            .Replace("{index}", $"Unlockable {unlockableIndex + 1}")
            .Replace("{number}", (unlockableIndex + 1).ToString());
    }

    private static string GetPartDisplayName(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head:
                return "Head";
            case BodyPartType.Neck:
                return "Neck";
            case BodyPartType.Stomach:
                return "Stomach";
            case BodyPartType.Leg:
                return "Leg";
            default:
                return "Part";
        }
    }
}
