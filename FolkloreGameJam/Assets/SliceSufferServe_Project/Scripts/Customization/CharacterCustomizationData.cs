using System;
using System.Collections.Generic;
using UnityEngine;

public enum HumanType
{
    NormalHuman = 0,
    RockThrowerHuman = 1,
    BigHuman = 2,
    RobotHuman = 3,
    KnightHuman = 4
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
    HeadOnly,
    NeckOnly,
    StomachOnly,
    LegOnly
}

[Serializable]
public class CharacterCustomizationData
{
    public const string HeadIndexKey = "CustomHeadIndex";
    public const string NeckIndexKey = "CustomNeckIndex";
    public const string BodyIndexKey = "CustomBodyIndex";
    public const string LegsIndexKey = "CustomLegsIndex";
    public const string CursorIdKey = "CustomWeaponCursorId";
    public const string HumanTypeKey = "CustomHumanType";
    public const string HeadSpriteIdKey = "CustomHeadSpriteId";
    public const string NeckSpriteIdKey = "CustomNeckSpriteId";
    public const string BodySpriteIdKey = "CustomBodySpriteId";
    public const string LegsSpriteIdKey = "CustomLegsSpriteId";

    public int headIndex;
    public int neckIndex;
    public int bodyIndex;
    public int legsIndex;
    public HumanType selectedHumanType = HumanType.NormalHuman;
    public string selectedCursorId = CursorCustomizationSelection.DefaultCursorId;
    public string headSpriteId;
    public string neckSpriteId;
    public string bodySpriteId;
    public string legsSpriteId;

    public static CharacterCustomizationData LoadFromPlayerPrefs()
    {
        CharacterCustomizationData data = new CharacterCustomizationData
        {
            headIndex = PlayerPrefs.GetInt(HeadIndexKey, 0),
            neckIndex = PlayerPrefs.GetInt(NeckIndexKey, 0),
            bodyIndex = PlayerPrefs.GetInt(BodyIndexKey, 0),
            legsIndex = PlayerPrefs.GetInt(LegsIndexKey, 0),
            selectedHumanType = (HumanType)PlayerPrefs.GetInt(HumanTypeKey, (int)HumanType.NormalHuman),
            selectedCursorId = PlayerPrefs.GetString(CursorIdKey, CursorCustomizationSelection.GetSelectedCursorId()),
            headSpriteId = PlayerPrefs.GetString(HeadSpriteIdKey, string.Empty),
            neckSpriteId = PlayerPrefs.GetString(NeckSpriteIdKey, string.Empty),
            bodySpriteId = PlayerPrefs.GetString(BodySpriteIdKey, string.Empty),
            legsSpriteId = PlayerPrefs.GetString(LegsSpriteIdKey, string.Empty)
        };

        if (!Enum.IsDefined(typeof(HumanType), data.selectedHumanType))
        {
            data.selectedHumanType = HumanType.NormalHuman;
        }

        if (string.IsNullOrWhiteSpace(data.selectedCursorId) || !CharacterCustomizer.IsWeaponCursorUnlocked(data.selectedCursorId))
        {
            data.selectedCursorId = CursorCustomizationSelection.DefaultCursorId;
        }

        Debug.Log($"Manual character customization loaded: type={data.selectedHumanType}, head={data.headIndex}, neck={data.neckIndex}, body={data.bodyIndex}, legs={data.legsIndex}, cursor={data.selectedCursorId}");
        return data;
    }

    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt(HeadIndexKey, headIndex);
        PlayerPrefs.SetInt(NeckIndexKey, neckIndex);
        PlayerPrefs.SetInt(BodyIndexKey, bodyIndex);
        PlayerPrefs.SetInt(LegsIndexKey, legsIndex);
        PlayerPrefs.SetInt(HumanTypeKey, (int)selectedHumanType);
        PlayerPrefs.SetString(HeadSpriteIdKey, headSpriteId ?? string.Empty);
        PlayerPrefs.SetString(NeckSpriteIdKey, neckSpriteId ?? string.Empty);
        PlayerPrefs.SetString(BodySpriteIdKey, bodySpriteId ?? string.Empty);
        PlayerPrefs.SetString(LegsSpriteIdKey, legsSpriteId ?? string.Empty);
        string cursorId = string.IsNullOrWhiteSpace(selectedCursorId) || !CharacterCustomizer.IsWeaponCursorUnlocked(selectedCursorId)
            ? CursorCustomizationSelection.DefaultCursorId
            : selectedCursorId;
        selectedCursorId = cursorId;
        PlayerPrefs.SetString(CursorIdKey, cursorId);
        CursorCustomizationSelection.SetSelectedCursorId(cursorId);
        PlayerPrefs.Save();
        Debug.Log($"Manual character customization saved: type={selectedHumanType}, head={headIndex}, neck={neckIndex}, body={bodyIndex}, legs={legsIndex}, cursor={selectedCursorId}");
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
                return bodySpriteId;
            case BodyPartType.Leg:
                return legsSpriteId;
            default:
                return string.Empty;
        }
    }

    public void SetSpriteId(BodyPartType part, string spriteId)
    {
        switch (part)
        {
            case BodyPartType.Head:
                headSpriteId = spriteId ?? string.Empty;
                break;
            case BodyPartType.Neck:
                neckSpriteId = spriteId ?? string.Empty;
                break;
            case BodyPartType.Stomach:
                bodySpriteId = spriteId ?? string.Empty;
                break;
            case BodyPartType.Leg:
                legsSpriteId = spriteId ?? string.Empty;
                break;
        }
    }
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

    public bool HasAnySprite()
    {
        return head != null || neck != null || stomach != null || leg != null;
    }

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
