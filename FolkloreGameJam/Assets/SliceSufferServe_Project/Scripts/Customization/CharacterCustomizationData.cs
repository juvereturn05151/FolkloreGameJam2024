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

public enum GenerationMode
{
    WholeBody,
    HeadOnly,
    NeckOnly,
    StomachOnly,
    LegOnly
}

[Serializable]
public class CharacterSpriteSlot
{
    public int slotIndex;
    public bool isDefaultSlot;
    public bool isGenerated;
    public bool isEnabledForSpawn;
    public string displayName;
    public string headSpriteId;
    public string neckSpriteId;
    public string stomachSpriteId;
    public string legSpriteId;

    public CharacterSpriteSlot()
    {
    }

    public CharacterSpriteSlot(int slotIndex, bool isDefaultSlot)
    {
        this.slotIndex = slotIndex;
        this.isDefaultSlot = isDefaultSlot;
        isGenerated = false;
        isEnabledForSpawn = isDefaultSlot;
        displayName = isDefaultSlot ? "Default Slot" : $"Generated Slot {slotIndex + 1}";
    }

    public string GetSpriteId(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head:
                return headSpriteId;
            case BodyPartType.Neck:
                return neckSpriteId;
            case BodyPartType.Stomach:
                return stomachSpriteId;
            case BodyPartType.Leg:
                return legSpriteId;
            default:
                return string.Empty;
        }
    }

    public void SetSpriteId(BodyPartType part, string spriteId)
    {
        switch (part)
        {
            case BodyPartType.Head:
                headSpriteId = spriteId;
                break;
            case BodyPartType.Neck:
                neckSpriteId = spriteId;
                break;
            case BodyPartType.Stomach:
                stomachSpriteId = spriteId;
                break;
            case BodyPartType.Leg:
                legSpriteId = spriteId;
                break;
        }

        if (!isDefaultSlot)
        {
            isGenerated = HasAnyGeneratedSpriteId();
        }
    }

    public bool HasAnyGeneratedSpriteId()
    {
        return !string.IsNullOrWhiteSpace(headSpriteId)
            || !string.IsNullOrWhiteSpace(neckSpriteId)
            || !string.IsNullOrWhiteSpace(stomachSpriteId)
            || !string.IsNullOrWhiteSpace(legSpriteId);
    }

    public void EnsureSlotState(int index, bool defaultSlot)
    {
        slotIndex = index;
        isDefaultSlot = defaultSlot;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = defaultSlot ? "Default Slot" : $"Generated Slot {index + 1}";
        }

        if (defaultSlot)
        {
            isGenerated = false;
            isEnabledForSpawn = true;
        }
        else
        {
            isGenerated = HasAnyGeneratedSpriteId();

            if (!isGenerated)
            {
                isEnabledForSpawn = false;
            }
        }
    }
}

[Serializable]
public class HumanTypeCustomizationData
{
    public HumanType humanType;
    public CharacterSpriteSlot defaultSlot = new CharacterSpriteSlot(0, true);
    public List<CharacterSpriteSlot> generatedSlots = new List<CharacterSpriteSlot>();

    public HumanTypeCustomizationData()
    {
    }

    public HumanTypeCustomizationData(HumanType humanType, int generatedSlotCount)
    {
        this.humanType = humanType;
        EnsureData(generatedSlotCount);
    }

    public CharacterSpriteSlot GetGeneratedSlot(int slotIndex, int generatedSlotCount)
    {
        EnsureData(generatedSlotCount);

        if (slotIndex < 0 || slotIndex >= generatedSlots.Count)
        {
            return null;
        }

        return generatedSlots[slotIndex];
    }

    public void EnsureData(int generatedSlotCount)
    {
        if (defaultSlot == null)
        {
            defaultSlot = new CharacterSpriteSlot(0, true);
        }

        defaultSlot.EnsureSlotState(0, true);

        if (generatedSlots == null)
        {
            generatedSlots = new List<CharacterSpriteSlot>();
        }

        if (generatedSlots.Count > generatedSlotCount)
        {
            generatedSlots.RemoveRange(generatedSlotCount, generatedSlots.Count - generatedSlotCount);
        }

        for (int i = generatedSlots.Count; i < generatedSlotCount; i++)
        {
            generatedSlots.Add(new CharacterSpriteSlot(i, false));
        }

        for (int i = 0; i < generatedSlots.Count; i++)
        {
            if (generatedSlots[i] == null)
            {
                generatedSlots[i] = new CharacterSpriteSlot(i, false);
            }

            generatedSlots[i].EnsureSlotState(i, false);
        }
    }
}

[Serializable]
public class CharacterCustomizationSaveData
{
    public int saveVersion = 2;
    public List<HumanTypeCustomizationData> humans = new List<HumanTypeCustomizationData>();

    public HumanTypeCustomizationData GetHumanData(HumanType humanType, int generatedSlotCount)
    {
        EnsureData(generatedSlotCount);

        for (int i = 0; i < humans.Count; i++)
        {
            if (humans[i] != null && humans[i].humanType == humanType)
            {
                return humans[i];
            }
        }

        HumanTypeCustomizationData humanData = new HumanTypeCustomizationData(humanType, generatedSlotCount);
        humans.Add(humanData);
        return humanData;
    }

    public void EnsureData(int generatedSlotCount)
    {
        if (humans == null)
        {
            humans = new List<HumanTypeCustomizationData>();
        }

        foreach (HumanType humanType in Enum.GetValues(typeof(HumanType)))
        {
            HumanTypeCustomizationData humanData = null;

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
                humanData = new HumanTypeCustomizationData(humanType, generatedSlotCount);
                humans.Add(humanData);
            }

            humanData.EnsureData(generatedSlotCount);
        }
    }
}

[Serializable]
public class CharacterSpriteSet
{
    public Sprite head;
    public Sprite neck;
    public Sprite stomach;
    public Sprite leg;

    public Sprite GetSprite(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head:
                return head;
            case BodyPartType.Neck:
                return neck;
            case BodyPartType.Stomach:
                return stomach;
            case BodyPartType.Leg:
                return leg;
            default:
                return null;
        }
    }
}
