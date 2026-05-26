using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterCustomizationManager customizationManager;

    [Header("Preview")]
    [Tooltip("Assign Head, Neck, Stomach, and Leg preview targets. Each target can use either a SpriteRenderer or a UI Image.")]
    [SerializeField] private List<BodyPartPreviewTarget> previewTargets = new List<BodyPartPreviewTarget>();

    [Header("Initial Selection")]
    [SerializeField] private HumanType selectedHumanType = HumanType.NormalHuman;
    [SerializeField, Range(0, CharacterCustomizationManager.MaxPresetsPerHuman - 1)]
    private int selectedPresetIndex;
    [SerializeField] private BodyPartType selectedBodyPart = BodyPartType.Head;

    private CharacterCustomizationPreset workingPreset;

    public HumanType SelectedHumanType => selectedHumanType;
    public int SelectedPresetIndex => selectedPresetIndex;
    public BodyPartType SelectedBodyPart => selectedBodyPart;
    public CharacterCustomizationPreset WorkingPreset => workingPreset;

    private void Awake()
    {
        if (customizationManager == null)
        {
            customizationManager = CharacterCustomizationManager.Instance;
        }
    }

    private void Start()
    {
        LoadCustomizationData();
        SelectHumanType(selectedHumanType);
    }

    // UI setup notes:
    // - Human type buttons can call SelectNormalHuman, SelectRockThrowerHuman, SelectObeseHuman, or SelectRobotHuman.
    // - Preset buttons should call SelectPresetButton with values 1 through 10.
    // - Body part buttons can call SelectHead, SelectNeck, SelectStomach, or SelectLeg.
    // - The Default button should call SelectDefaultSprite.
    // - Generated sprite slot buttons should call SelectGeneratedSpriteButton with values 1 through 30.
    // - The Save button should call SaveCurrentPreset.

    public void SelectHumanType(HumanType type)
    {
        selectedHumanType = type;
        LoadWorkingPreset();
        ApplyPresetToPreview();
    }

    public void SelectPreset(int index)
    {
        selectedPresetIndex = Mathf.Clamp(index, 0, CharacterCustomizationManager.MaxPresetsPerHuman - 1);
        LoadWorkingPreset();
        ApplyPresetToPreview();
    }

    public void SelectBodyPart(BodyPartType part)
    {
        selectedBodyPart = part;
        ApplyPresetToPreview();
    }

    public void SelectDefaultSprite()
    {
        BodyPartSpriteSelection selection = GetSelectedBodyPartSelection();
        selection.SelectDefault();
        ApplyPresetToPreview();
    }

    public void SelectGeneratedSprite(int slotIndex)
    {
        slotIndex = Mathf.Clamp(slotIndex, 0, CharacterCustomizationManager.MaxGeneratedSpriteSlotsPerPart - 1);
        GeneratedSpriteSlot slot = customizationManager.GetGeneratedSpriteSlot(selectedHumanType, selectedBodyPart, slotIndex);

        if (slot == null || !slot.HasSprite)
        {
            Debug.LogWarning($"Generated sprite slot {slotIndex + 1} is empty for {selectedHumanType} {selectedBodyPart}.");
            return;
        }

        BodyPartSpriteSelection selection = GetSelectedBodyPartSelection();
        selection.SelectGenerated(slotIndex);
        ApplyPresetToPreview();
    }

    public void SaveCurrentPreset()
    {
        EnsureWorkingPreset();
        customizationManager.SavePreset(selectedHumanType, selectedPresetIndex, workingPreset);
    }

    public void LoadCustomizationData()
    {
        ResolveManager();
        if (customizationManager == null)
        {
            Debug.LogError("CharacterCustomizationUIController needs a CharacterCustomizationManager in the scene.");
            return;
        }

        customizationManager.LoadCustomizationData();
        LoadWorkingPreset();
    }

    public void SaveCustomizationData()
    {
        ResolveManager();
        if (customizationManager == null)
        {
            Debug.LogError("CharacterCustomizationUIController needs a CharacterCustomizationManager in the scene.");
            return;
        }

        customizationManager.SaveCustomizationData();
    }

    public void ApplyPresetToPreview()
    {
        EnsureWorkingPreset();

        for (int i = 0; i < previewTargets.Count; i++)
        {
            BodyPartPreviewTarget target = previewTargets[i];
            if (target == null)
            {
                continue;
            }

            BodyPartSpriteSelection selection = workingPreset.GetSelection(target.BodyPartType);
            Sprite sprite = customizationManager.ResolveSprite(selectedHumanType, target.BodyPartType, selection);
            target.SetSprite(sprite);
        }
    }

    public void SelectNormalHuman()
    {
        SelectHumanType(HumanType.NormalHuman);
    }

    public void SelectRockThrowerHuman()
    {
        SelectHumanType(HumanType.RockThrowerHuman);
    }

    public void SelectObeseHuman()
    {
        SelectHumanType(HumanType.ObeseHuman);
    }

    public void SelectRobotHuman()
    {
        SelectHumanType(HumanType.RobotHuman);
    }

    public void SelectHead()
    {
        SelectBodyPart(BodyPartType.Head);
    }

    public void SelectNeck()
    {
        SelectBodyPart(BodyPartType.Neck);
    }

    public void SelectStomach()
    {
        SelectBodyPart(BodyPartType.Stomach);
    }

    public void SelectLeg()
    {
        SelectBodyPart(BodyPartType.Leg);
    }

    public void SelectPresetButton(int oneBasedIndex)
    {
        SelectPreset(oneBasedIndex - 1);
    }

    public void SelectGeneratedSpriteButton(int oneBasedSlotIndex)
    {
        SelectGeneratedSprite(oneBasedSlotIndex - 1);
    }

    public void AssignGeneratedSpriteToSlot(int zeroBasedSlotIndex, string spriteId, string displayName = null)
    {
        customizationManager.SetGeneratedSpriteSlot(selectedHumanType, selectedBodyPart, zeroBasedSlotIndex, spriteId, displayName);
    }

    private void LoadWorkingPreset()
    {
        ResolveManager();
        if (customizationManager == null)
        {
            return;
        }

        CharacterCustomizationPreset savedPreset = customizationManager.GetPreset(selectedHumanType, selectedPresetIndex);
        workingPreset = ClonePreset(savedPreset);
    }

    private void EnsureWorkingPreset()
    {
        if (workingPreset == null)
        {
            LoadWorkingPreset();
        }

        workingPreset.EnsureBodyPartSelections();
    }

    private BodyPartSpriteSelection GetSelectedBodyPartSelection()
    {
        EnsureWorkingPreset();
        return workingPreset.GetSelection(selectedBodyPart);
    }

    private void ResolveManager()
    {
        if (customizationManager == null)
        {
            customizationManager = CharacterCustomizationManager.Instance;
        }

        if (customizationManager == null)
        {
            customizationManager = FindFirstObjectByType<CharacterCustomizationManager>();
        }
    }

    private static CharacterCustomizationPreset ClonePreset(CharacterCustomizationPreset source)
    {
        if (source == null)
        {
            return new CharacterCustomizationPreset(0);
        }

        string json = JsonUtility.ToJson(source);
        CharacterCustomizationPreset clone = JsonUtility.FromJson<CharacterCustomizationPreset>(json);
        clone.EnsureBodyPartSelections();
        return clone;
    }
}

[Serializable]
public class BodyPartPreviewTarget
{
    [SerializeField] private BodyPartType bodyPartType;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image image;

    public BodyPartType BodyPartType => bodyPartType;

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }

        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
        }
    }
}
