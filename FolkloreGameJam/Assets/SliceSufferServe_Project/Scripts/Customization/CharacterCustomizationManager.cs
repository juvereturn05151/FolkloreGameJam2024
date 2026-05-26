using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomizationManager : MonoBehaviour
{
    public const int MaxPresetsPerHuman = 10;
    public const int MaxGeneratedSpriteSlotsPerPart = 30;

    private const string PlayerPrefsKey = "CharacterCustomizationData";

    public static CharacterCustomizationManager Instance { get; private set; }

    [Header("Default Sprites")]
    [Tooltip("Assign one default sprite per human type and body part. These are not saved; saved data stores only generated sprite IDs.")]
    [SerializeField] private List<CharacterDefaultSpriteSet> defaultSpriteSets = new List<CharacterDefaultSpriteSet>();

    public CharacterCustomizationSaveData CustomizationData { get; private set; }

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

        CustomizationData.EnsureData(MaxPresetsPerHuman, MaxGeneratedSpriteSlotsPerPart);
    }

    public void SaveCustomizationData()
    {
        if (CustomizationData == null)
        {
            CustomizationData = new CharacterCustomizationSaveData();
            CustomizationData.EnsureData(MaxPresetsPerHuman, MaxGeneratedSpriteSlotsPerPart);
        }

        string json = JsonUtility.ToJson(CustomizationData);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    public HumanCustomizationData GetHumanData(HumanType humanType)
    {
        EnsureLoaded();
        return CustomizationData.GetHumanData(humanType, MaxPresetsPerHuman, MaxGeneratedSpriteSlotsPerPart);
    }

    public CharacterCustomizationPreset GetPreset(HumanType humanType, int presetIndex)
    {
        presetIndex = Mathf.Clamp(presetIndex, 0, MaxPresetsPerHuman - 1);
        return GetHumanData(humanType).GetPreset(presetIndex, MaxPresetsPerHuman);
    }

    public GeneratedSpriteSlot GetGeneratedSpriteSlot(HumanType humanType, BodyPartType bodyPartType, int slotIndex)
    {
        slotIndex = Mathf.Clamp(slotIndex, 0, MaxGeneratedSpriteSlotsPerPart - 1);
        BodyPartGeneratedSpriteSlots slots = GetHumanData(humanType).GetSlotsForPart(bodyPartType, MaxGeneratedSpriteSlotsPerPart);
        return slots.GetSlot(slotIndex, MaxGeneratedSpriteSlotsPerPart);
    }

    public void SetGeneratedSpriteSlot(HumanType humanType, BodyPartType bodyPartType, int slotIndex, string spriteId, string displayName = null)
    {
        GeneratedSpriteSlot slot = GetGeneratedSpriteSlot(humanType, bodyPartType, slotIndex);
        if (slot == null)
        {
            return;
        }

        slot.spriteId = spriteId ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            slot.displayName = displayName;
        }

        SaveCustomizationData();
    }

    public void SavePreset(HumanType humanType, int presetIndex, CharacterCustomizationPreset preset)
    {
        if (preset == null)
        {
            return;
        }

        presetIndex = Mathf.Clamp(presetIndex, 0, MaxPresetsPerHuman - 1);
        HumanCustomizationData humanData = GetHumanData(humanType);
        humanData.EnsureData(MaxPresetsPerHuman, MaxGeneratedSpriteSlotsPerPart);
        humanData.presets[presetIndex] = preset;
        humanData.presets[presetIndex].presetIndex = presetIndex;
        humanData.presets[presetIndex].EnsureBodyPartSelections();
        SaveCustomizationData();
    }

    public Sprite ResolveSprite(HumanType humanType, BodyPartType bodyPartType, BodyPartSpriteSelection selection)
    {
        if (selection == null || selection.useDefaultSprite)
        {
            return GetDefaultSprite(humanType, bodyPartType);
        }

        GeneratedSpriteSlot slot = GetGeneratedSpriteSlot(humanType, bodyPartType, selection.generatedSlotIndex);
        if (slot == null || !slot.HasSprite)
        {
            return GetDefaultSprite(humanType, bodyPartType);
        }

        Sprite generatedSprite = LoadGeneratedSprite(slot.spriteId);
        return generatedSprite != null ? generatedSprite : GetDefaultSprite(humanType, bodyPartType);
    }

    public Sprite GetDefaultSprite(HumanType humanType, BodyPartType bodyPartType)
    {
        for (int i = 0; i < defaultSpriteSets.Count; i++)
        {
            CharacterDefaultSpriteSet spriteSet = defaultSpriteSets[i];
            if (spriteSet != null && spriteSet.HumanType == humanType)
            {
                return spriteSet.GetSprite(bodyPartType);
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

        // Current convention: store a Resources path such as "GeneratedSprites/NormalHuman/Head_01".
        // If generated files later live elsewhere, replace this one method without changing save data.
        return Resources.Load<Sprite>(spriteId);
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
