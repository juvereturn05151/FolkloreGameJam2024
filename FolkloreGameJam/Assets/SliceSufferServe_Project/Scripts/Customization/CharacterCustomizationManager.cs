using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomizationManager : MonoBehaviour
{
    public const int GeneratedSlotsPerHuman = 10;

    private const string PlayerPrefsKey = "CharacterCustomizationData";

    public static CharacterCustomizationManager Instance { get; private set; }

    [Header("Default Sprites")]
    [Tooltip("Assign one default sprite per human type and body part. Saved data stores only generated sprite IDs.")]
    [SerializeField] private List<CharacterDefaultSpriteSet> defaultSpriteSets = new List<CharacterDefaultSpriteSet>();

    public CharacterCustomizationSaveData CustomizationData { get; private set; }

    public static CharacterCustomizationManager GetOrCreateRuntimeInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        CharacterCustomizationManager existingManager = FindAnyObjectByType<CharacterCustomizationManager>();
        if (existingManager != null)
        {
            return existingManager;
        }

        GameObject managerObject = new GameObject("CharacterCustomizationManager");
        DontDestroyOnLoad(managerObject);
        return managerObject.AddComponent<CharacterCustomizationManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadCustomizationData();
    }

    public void LoadCustomizationData()
    {
        string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);

        if (string.IsNullOrWhiteSpace(json))
        {
            CustomizationData = new CharacterCustomizationSaveData();
        }
        else
        {
            try
            {
                CustomizationData = JsonUtility.FromJson<CharacterCustomizationSaveData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load character customization data. A fresh save will be used. {exception.Message}");
                CustomizationData = new CharacterCustomizationSaveData();
            }
        }

        if (CustomizationData == null)
        {
            CustomizationData = new CharacterCustomizationSaveData();
        }

        CustomizationData.EnsureData(GeneratedSlotsPerHuman);
    }

    public void SaveCustomizationData()
    {
        EnsureLoaded();
        CustomizationData.EnsureData(GeneratedSlotsPerHuman);
        PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(CustomizationData));
        PlayerPrefs.Save();
    }

    public HumanTypeCustomizationData GetHumanData(HumanType type)
    {
        EnsureLoaded();
        return CustomizationData.GetHumanData(type, GeneratedSlotsPerHuman);
    }

    public CharacterSpriteSlot GetGeneratedSlot(HumanType type, int slotIndex)
    {
        slotIndex = Mathf.Clamp(slotIndex, 0, GeneratedSlotsPerHuman - 1);
        return GetHumanData(type).GetGeneratedSlot(slotIndex, GeneratedSlotsPerHuman);
    }

    public void SetSlotEnabledForSpawn(HumanType type, bool isDefaultSlot, int slotIndex, bool enabled)
    {
        HumanTypeCustomizationData humanData = GetHumanData(type);

        if (isDefaultSlot)
        {
            // The default slot always exists, but the player can still decide whether it enters the spawn pool.
            humanData.defaultSlot.isEnabledForSpawn = enabled;
        }
        else
        {
            CharacterSpriteSlot slot = humanData.GetGeneratedSlot(Mathf.Clamp(slotIndex, 0, GeneratedSlotsPerHuman - 1), GeneratedSlotsPerHuman);
            if (slot != null)
            {
                slot.isEnabledForSpawn = enabled;
            }
        }

        SaveCustomizationData();
    }

    public CharacterSpriteSlot GetRandomEnabledSlotForSpawn(HumanType type)
    {
        HumanTypeCustomizationData humanData = GetHumanData(type);
        List<CharacterSpriteSlot> candidates = new List<CharacterSpriteSlot>();

        if (humanData.defaultSlot != null && humanData.defaultSlot.isEnabledForSpawn)
        {
            candidates.Add(humanData.defaultSlot);
        }

        for (int i = 0; i < humanData.generatedSlots.Count; i++)
        {
            CharacterSpriteSlot slot = humanData.generatedSlots[i];
            if (slot != null && slot.isEnabledForSpawn)
            {
                candidates.Add(slot);
            }
        }

        if (candidates.Count == 0)
        {
            return humanData.defaultSlot;
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    public CharacterSpriteSet ResolveSpriteSet(HumanType type, CharacterSpriteSlot slot)
    {
        CharacterSpriteSlot resolvedSlot = slot ?? GetHumanData(type).defaultSlot;

        return new CharacterSpriteSet
        {
            head = ResolveSprite(type, BodyPartType.Head, resolvedSlot),
            neck = ResolveSprite(type, BodyPartType.Neck, resolvedSlot),
            stomach = ResolveSprite(type, BodyPartType.Stomach, resolvedSlot),
            leg = ResolveSprite(type, BodyPartType.Leg, resolvedSlot)
        };
    }

    public bool TryGetGeneratedSpriteSetForSpawn(HumanType type, out CharacterSpriteSet spriteSet)
    {
        spriteSet = null;

        CharacterSpriteSlot slot = GetRandomEnabledSlotForSpawn(type);
        if (slot == null || slot.isDefaultSlot || !slot.HasAnyGeneratedSpriteId())
        {
            return false;
        }

        spriteSet = new CharacterSpriteSet
        {
            head = LoadGeneratedSprite(slot.headSpriteId),
            neck = LoadGeneratedSprite(slot.neckSpriteId),
            stomach = LoadGeneratedSprite(slot.stomachSpriteId),
            leg = LoadGeneratedSprite(slot.legSpriteId)
        };

        return spriteSet.HasAnySprite();
    }

    public void SaveGeneratedWholeBody(HumanType type, int slotIndex, string headId, string neckId, string stomachId, string legId)
    {
        CharacterSpriteSlot slot = GetGeneratedSlot(type, slotIndex);
        if (slot == null)
        {
            return;
        }

        slot.headSpriteId = headId ?? string.Empty;
        slot.neckSpriteId = neckId ?? string.Empty;
        slot.stomachSpriteId = stomachId ?? string.Empty;
        slot.legSpriteId = legId ?? string.Empty;
        slot.isGenerated = slot.HasAnyGeneratedSpriteId();
        slot.displayName = $"Generated Slot {slot.slotIndex + 1}";
        SaveCustomizationData();
    }

    public void SaveGeneratedPart(HumanType type, int slotIndex, BodyPartType part, string spriteId)
    {
        CharacterSpriteSlot slot = GetGeneratedSlot(type, slotIndex);
        if (slot == null)
        {
            return;
        }

        slot.SetSpriteId(part, spriteId ?? string.Empty);
        slot.displayName = $"Generated Slot {slot.slotIndex + 1}";
        SaveCustomizationData();
    }

    public Sprite GetDefaultSprite(HumanType type, BodyPartType part)
    {
        for (int i = 0; i < defaultSpriteSets.Count; i++)
        {
            CharacterDefaultSpriteSet spriteSet = defaultSpriteSets[i];
            if (spriteSet != null && spriteSet.HumanType == type)
            {
                return spriteSet.GetSprite(part);
            }
        }

        return null;
    }

    public Sprite LoadGeneratedSprite(string spriteId)
    {
        if (string.IsNullOrWhiteSpace(spriteId))
        {
            return null;
        }

        // Current convention: store a Resources path such as "GeneratedSprites/NormalHuman/Slot01/Head".
        // If generated files later live elsewhere, replace this resolver without changing save data.
        return Resources.Load<Sprite>(spriteId);
    }

    private Sprite ResolveSprite(HumanType type, BodyPartType part, CharacterSpriteSlot slot)
    {
        if (slot == null || slot.isDefaultSlot)
        {
            return GetDefaultSprite(type, part);
        }

        Sprite generatedSprite = LoadGeneratedSprite(slot.GetSpriteId(part));
        return generatedSprite != null ? generatedSprite : GetDefaultSprite(type, part);
    }

    private void EnsureLoaded()
    {
        if (CustomizationData == null)
        {
            LoadCustomizationData();
        }
    }
}

[Serializable]
public class CharacterDefaultSpriteSet
{
    [SerializeField] private HumanType humanType;
    [SerializeField] private List<BodyPartSpriteReference> bodyPartSprites = new List<BodyPartSpriteReference>();

    public HumanType HumanType => humanType;

    public Sprite GetSprite(BodyPartType bodyPartType)
    {
        for (int i = 0; i < bodyPartSprites.Count; i++)
        {
            BodyPartSpriteReference reference = bodyPartSprites[i];
            if (reference != null && reference.BodyPartType == bodyPartType)
            {
                return reference.Sprite;
            }
        }

        return null;
    }
}

[Serializable]
public class BodyPartSpriteReference
{
    [SerializeField] private BodyPartType bodyPartType;
    [SerializeField] private Sprite sprite;

    public BodyPartType BodyPartType => bodyPartType;
    public Sprite Sprite => sprite;
}
