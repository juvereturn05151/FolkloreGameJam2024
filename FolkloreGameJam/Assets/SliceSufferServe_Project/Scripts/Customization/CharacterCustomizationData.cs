using System;
using System.Collections.Generic;
using UnityEngine;

public enum HumanType
{
    NormalHuman,
    RockThrowerHuman,
    ObeseHuman,
    RobotHuman
}

public enum BodyPartType
{
    Head,
    Neck,
    Stomach,
    Leg
}

[Serializable]
public class GeneratedSpriteSlot
{
    public int slotIndex;
    public string spriteId;
    public string displayName;

    public bool HasSprite => !string.IsNullOrWhiteSpace(spriteId);

    public GeneratedSpriteSlot()
    {
    }

    public GeneratedSpriteSlot(int slotIndex)
    {
        this.slotIndex = slotIndex;
        spriteId = string.Empty;
        displayName = $"Generated {slotIndex + 1}";
    }
}

[Serializable]
public class BodyPartSpriteSelection
{
    public BodyPartType bodyPartType;
    public bool useDefaultSprite = true;
    public int generatedSlotIndex = -1;

    public BodyPartSpriteSelection()
    {
    }

    public BodyPartSpriteSelection(BodyPartType bodyPartType)
    {
        this.bodyPartType = bodyPartType;
    }

    public void SelectDefault()
    {
        useDefaultSprite = true;
        generatedSlotIndex = -1;
    }

    public void SelectGenerated(int slotIndex)
    {
        useDefaultSprite = false;
        generatedSlotIndex = slotIndex;
    }
}

[Serializable]
public class CharacterCustomizationPreset
{
    public int presetIndex;
    public string presetName;
    public List<BodyPartSpriteSelection> bodyPartSelections = new List<BodyPartSpriteSelection>();

    public CharacterCustomizationPreset()
    {
    }

    public CharacterCustomizationPreset(int presetIndex)
    {
        this.presetIndex = presetIndex;
        presetName = $"Preset {presetIndex + 1}";
        EnsureBodyPartSelections();
    }

    public BodyPartSpriteSelection GetSelection(BodyPartType bodyPartType)
    {
        EnsureBodyPartSelections();

        for (int i = 0; i < bodyPartSelections.Count; i++)
        {
            if (bodyPartSelections[i] != null && bodyPartSelections[i].bodyPartType == bodyPartType)
            {
                return bodyPartSelections[i];
            }
        }

        BodyPartSpriteSelection selection = new BodyPartSpriteSelection(bodyPartType);
        bodyPartSelections.Add(selection);
        return selection;
    }

    public void EnsureBodyPartSelections()
    {
        foreach (BodyPartType bodyPartType in Enum.GetValues(typeof(BodyPartType)))
        {
            bool exists = false;
            for (int i = 0; i < bodyPartSelections.Count; i++)
            {
                if (bodyPartSelections[i] != null && bodyPartSelections[i].bodyPartType == bodyPartType)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                bodyPartSelections.Add(new BodyPartSpriteSelection(bodyPartType));
            }
        }
    }
}

[Serializable]
public class BodyPartGeneratedSpriteSlots
{
    public BodyPartType bodyPartType;
    public List<GeneratedSpriteSlot> generatedSpriteSlots = new List<GeneratedSpriteSlot>();

    public BodyPartGeneratedSpriteSlots()
    {
    }

    public BodyPartGeneratedSpriteSlots(BodyPartType bodyPartType, int maxSlots)
    {
        this.bodyPartType = bodyPartType;
        EnsureSlots(maxSlots);
    }

    public GeneratedSpriteSlot GetSlot(int slotIndex, int maxSlots)
    {
        EnsureSlots(maxSlots);

        if (slotIndex < 0 || slotIndex >= generatedSpriteSlots.Count)
        {
            return null;
        }

        return generatedSpriteSlots[slotIndex];
    }

    public void EnsureSlots(int maxSlots)
    {
        if (generatedSpriteSlots.Count > maxSlots)
        {
            generatedSpriteSlots.RemoveRange(maxSlots, generatedSpriteSlots.Count - maxSlots);
        }

        for (int i = generatedSpriteSlots.Count; i < maxSlots; i++)
        {
            generatedSpriteSlots.Add(new GeneratedSpriteSlot(i));
        }

        for (int i = 0; i < generatedSpriteSlots.Count; i++)
        {
            if (generatedSpriteSlots[i] == null)
            {
                generatedSpriteSlots[i] = new GeneratedSpriteSlot(i);
            }

            generatedSpriteSlots[i].slotIndex = i;
        }
    }
}

[Serializable]
public class HumanCustomizationData
{
    public HumanType humanType;
    public List<CharacterCustomizationPreset> presets = new List<CharacterCustomizationPreset>();
    public List<BodyPartGeneratedSpriteSlots> generatedSpriteSlotsByPart = new List<BodyPartGeneratedSpriteSlots>();

    public HumanCustomizationData()
    {
    }

    public HumanCustomizationData(HumanType humanType, int maxPresets, int maxGeneratedSlots)
    {
        this.humanType = humanType;
        EnsureData(maxPresets, maxGeneratedSlots);
    }

    public CharacterCustomizationPreset GetPreset(int presetIndex, int maxPresets)
    {
        EnsurePresets(maxPresets);

        if (presetIndex < 0 || presetIndex >= presets.Count)
        {
            return null;
        }

        return presets[presetIndex];
    }

    public BodyPartGeneratedSpriteSlots GetSlotsForPart(BodyPartType bodyPartType, int maxGeneratedSlots)
    {
        EnsureGeneratedSlots(maxGeneratedSlots);

        for (int i = 0; i < generatedSpriteSlotsByPart.Count; i++)
        {
            BodyPartGeneratedSpriteSlots slots = generatedSpriteSlotsByPart[i];
            if (slots != null && slots.bodyPartType == bodyPartType)
            {
                return slots;
            }
        }

        BodyPartGeneratedSpriteSlots createdSlots = new BodyPartGeneratedSpriteSlots(bodyPartType, maxGeneratedSlots);
        generatedSpriteSlotsByPart.Add(createdSlots);
        return createdSlots;
    }

    public void EnsureData(int maxPresets, int maxGeneratedSlots)
    {
        EnsurePresets(maxPresets);
        EnsureGeneratedSlots(maxGeneratedSlots);
    }

    private void EnsurePresets(int maxPresets)
    {
        if (presets.Count > maxPresets)
        {
            presets.RemoveRange(maxPresets, presets.Count - maxPresets);
        }

        for (int i = presets.Count; i < maxPresets; i++)
        {
            presets.Add(new CharacterCustomizationPreset(i));
        }

        for (int i = 0; i < presets.Count; i++)
        {
            if (presets[i] == null)
            {
                presets[i] = new CharacterCustomizationPreset(i);
            }

            presets[i].presetIndex = i;
            presets[i].EnsureBodyPartSelections();
        }
    }

    private void EnsureGeneratedSlots(int maxGeneratedSlots)
    {
        foreach (BodyPartType bodyPartType in Enum.GetValues(typeof(BodyPartType)))
        {
            BodyPartGeneratedSpriteSlots slots = null;
            for (int i = 0; i < generatedSpriteSlotsByPart.Count; i++)
            {
                if (generatedSpriteSlotsByPart[i] != null && generatedSpriteSlotsByPart[i].bodyPartType == bodyPartType)
                {
                    slots = generatedSpriteSlotsByPart[i];
                    break;
                }
            }

            if (slots == null)
            {
                slots = new BodyPartGeneratedSpriteSlots(bodyPartType, maxGeneratedSlots);
                generatedSpriteSlotsByPart.Add(slots);
            }

            slots.EnsureSlots(maxGeneratedSlots);
        }
    }
}

[Serializable]
public class CharacterCustomizationSaveData
{
    public int saveVersion = 1;
    public List<HumanCustomizationData> humans = new List<HumanCustomizationData>();

    public HumanCustomizationData GetHumanData(HumanType humanType, int maxPresets, int maxGeneratedSlots)
    {
        EnsureData(maxPresets, maxGeneratedSlots);

        for (int i = 0; i < humans.Count; i++)
        {
            if (humans[i] != null && humans[i].humanType == humanType)
            {
                return humans[i];
            }
        }

        HumanCustomizationData humanData = new HumanCustomizationData(humanType, maxPresets, maxGeneratedSlots);
        humans.Add(humanData);
        return humanData;
    }

    public void EnsureData(int maxPresets, int maxGeneratedSlots)
    {
        foreach (HumanType humanType in Enum.GetValues(typeof(HumanType)))
        {
            HumanCustomizationData humanData = null;
            for (int i = 0; i < humans.Count; i++)
            {
                if (humans[i] != null && humans[i].humanType == humanType)
                {
                    humanData = humans[i];
                    break;
                }
            }

            if (humanData == null)
            {
                humanData = new HumanCustomizationData(humanType, maxPresets, maxGeneratedSlots);
                humans.Add(humanData);
            }

            humanData.EnsureData(maxPresets, maxGeneratedSlots);
        }
    }
}
