using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterCustomizationManager customizationManager;
    [SerializeField] private CharacterGenerationManager generationManager;

    [Header("Pages")]
    [SerializeField] private GameObject editPageRoot;
    [SerializeField] private GameObject generatePageRoot;

    [Header("Previews")]
    [SerializeField] private CharacterPreviewView editPreview;
    [SerializeField] private CharacterPreviewView generatePreview;

    [Header("Edit Page Slots")]
    [Tooltip("Optional. Assign Default Slot plus Generated Slot 1-10 rows if you want the controller to refresh labels/toggles automatically.")]
    [SerializeField] private List<CharacterSlotToggleView> editSlotViews = new List<CharacterSlotToggleView>();

    [Header("Generate Page")]
    [SerializeField] private InputField promptInput;
    [SerializeField] private Text selectedHumanLabel;
    [SerializeField] private Text selectedGeneratedSlotLabel;
    [SerializeField] private Text selectedGenerationModeLabel;

    [Header("Initial Selection")]
    [SerializeField] private HumanType selectedHumanType = HumanType.NormalHuman;
    [SerializeField, Range(0, CharacterCustomizationManager.GeneratedSlotsPerHuman - 1)]
    private int selectedGeneratedSlotIndex;
    [SerializeField] private GenerationMode selectedGenerationMode = GenerationMode.WholeBody;

    private string promptText = string.Empty;

    public HumanType SelectedHumanType => selectedHumanType;
    public int SelectedGeneratedSlotIndex => selectedGeneratedSlotIndex;
    public GenerationMode SelectedGenerationMode => selectedGenerationMode;
    public string PromptText => promptText;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        LoadCustomizationData();
        ShowEditPage();
    }

    // UI setup notes:
    // - Page tab buttons: call ShowEditPage() and ShowGeneratePage().
    // - Human type buttons: call SelectNormalHuman(), SelectRockThrowerHuman(), SelectObeseHuman(), SelectRobotHuman().
    // - Generate slot buttons/dropdowns: call SelectGeneratedSlot(oneBasedIndex), where Slot 1 passes 1.
    // - Generation mode buttons/dropdowns: call SelectWholeBody(), SelectHeadOnly(), SelectNeckOnly(), SelectStomachOnly(), SelectLegOnly().
    // - Prompt input field: connect OnValueChanged(string) to SetPromptText(string).
    // - Edit page toggles: Default toggle calls ToggleDefaultSlotForGameplay(bool), generated slot toggles call ToggleSlotForGameplay(slotIndex, bool).
    // - Generate button: call CharacterGenerationManager.GenerateSelectedCharacter().

    public void ShowEditPage()
    {
        SetPageActive(editPageRoot, true);
        SetPageActive(generatePageRoot, false);
        RefreshEditPage();
    }

    public void ShowGeneratePage()
    {
        SetPageActive(editPageRoot, false);
        SetPageActive(generatePageRoot, true);
        RefreshGeneratePage();
    }

    public void SelectHumanType(HumanType type)
    {
        selectedHumanType = type;
        RefreshEditPage();
        RefreshGeneratePage();
    }

    public void SelectGeneratedSlot(int oneBasedIndex)
    {
        selectedGeneratedSlotIndex = Mathf.Clamp(oneBasedIndex - 1, 0, CharacterCustomizationManager.GeneratedSlotsPerHuman - 1);
        RefreshGeneratePage();
    }

    public void SelectGenerationMode(GenerationMode mode)
    {
        selectedGenerationMode = mode;
        RefreshGeneratePage();
    }

    public void SetPromptText(string prompt)
    {
        promptText = prompt ?? string.Empty;
    }

    public void ToggleSlotForGameplay(int slotIndex, bool enabled)
    {
        ResolveReferences();
        customizationManager?.SetSlotEnabledForSpawn(selectedHumanType, false, slotIndex, enabled);
        RefreshEditPage();
    }

    public void ToggleDefaultSlotForGameplay(bool enabled)
    {
        ResolveReferences();
        customizationManager?.SetSlotEnabledForSpawn(selectedHumanType, true, 0, enabled);
        RefreshEditPage();
    }

    public void RefreshEditPage()
    {
        ResolveReferences();
        if (customizationManager == null)
        {
            return;
        }

        HumanTypeCustomizationData humanData = customizationManager.GetHumanData(selectedHumanType);
        CharacterSpriteSlot previewSlot = customizationManager.GetRandomEnabledSlotForSpawn(selectedHumanType) ?? humanData.defaultSlot;
        ApplyPreview(editPreview, previewSlot);

        for (int i = 0; i < editSlotViews.Count; i++)
        {
            CharacterSlotToggleView view = editSlotViews[i];
            if (view == null)
            {
                continue;
            }

            if (view.IsDefaultSlot)
            {
                view.Refresh(humanData.defaultSlot, customizationManager.ResolveSpriteSet(selectedHumanType, humanData.defaultSlot));
            }
            else
            {
                CharacterSpriteSlot slot = customizationManager.GetGeneratedSlot(selectedHumanType, view.SlotIndex);
                view.Refresh(slot, customizationManager.ResolveSpriteSet(selectedHumanType, slot));
            }
        }

        RefreshLabels();
    }

    public void RefreshGeneratePage()
    {
        ResolveReferences();
        if (customizationManager == null)
        {
            return;
        }

        CharacterSpriteSlot slot = customizationManager.GetGeneratedSlot(selectedHumanType, selectedGeneratedSlotIndex);
        ApplyPreview(generatePreview, slot);
        RefreshLabels();
    }

    public void LoadCustomizationData()
    {
        ResolveReferences();
        customizationManager?.LoadCustomizationData();
        RefreshEditPage();
        RefreshGeneratePage();
    }

    public void SaveCustomizationData()
    {
        ResolveReferences();
        customizationManager?.SaveCustomizationData();
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

    public void SelectWholeBody()
    {
        SelectGenerationMode(GenerationMode.WholeBody);
    }

    public void SelectHeadOnly()
    {
        SelectGenerationMode(GenerationMode.HeadOnly);
    }

    public void SelectNeckOnly()
    {
        SelectGenerationMode(GenerationMode.NeckOnly);
    }

    public void SelectStomachOnly()
    {
        SelectGenerationMode(GenerationMode.StomachOnly);
    }

    public void SelectLegOnly()
    {
        SelectGenerationMode(GenerationMode.LegOnly);
    }

    public void OnGenerationDataChanged()
    {
        RefreshEditPage();
        RefreshGeneratePage();
    }

    private void ApplyPreview(CharacterPreviewView preview, CharacterSpriteSlot slot)
    {
        if (preview == null || customizationManager == null)
        {
            return;
        }

        preview.ApplySpriteSet(customizationManager.ResolveSpriteSet(selectedHumanType, slot));
    }

    private void RefreshLabels()
    {
        SetText(selectedHumanLabel, GetHumanDisplayName(selectedHumanType));
        SetText(selectedGeneratedSlotLabel, $"Generated Slot {selectedGeneratedSlotIndex + 1}");
        SetText(selectedGenerationModeLabel, GetGenerationModeDisplayName(selectedGenerationMode));

        if (promptInput != null && promptInput.text != promptText)
        {
            promptInput.text = promptText;
        }
    }

    private void ResolveReferences()
    {
        if (customizationManager == null)
        {
            customizationManager = CharacterCustomizationManager.Instance ?? FindAnyObjectByType<CharacterCustomizationManager>();
        }

        if (generationManager == null)
        {
            generationManager = FindAnyObjectByType<CharacterGenerationManager>();
        }
    }

    private static void SetPageActive(GameObject pageRoot, bool active)
    {
        if (pageRoot != null)
        {
            pageRoot.SetActive(active);
        }
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static string GetHumanDisplayName(HumanType type)
    {
        switch (type)
        {
            case HumanType.NormalHuman:
                return "Normal Human";
            case HumanType.RockThrowerHuman:
                return "Rock Thrower Human";
            case HumanType.ObeseHuman:
                return "Obese Human";
            case HumanType.RobotHuman:
                return "Robot Human";
            default:
                return type.ToString();
        }
    }

    private static string GetGenerationModeDisplayName(GenerationMode mode)
    {
        switch (mode)
        {
            case GenerationMode.WholeBody:
                return "Whole Body";
            case GenerationMode.HeadOnly:
                return "Head Only";
            case GenerationMode.NeckOnly:
                return "Neck Only";
            case GenerationMode.StomachOnly:
                return "Stomach Only";
            case GenerationMode.LegOnly:
                return "Leg Only";
            default:
                return mode.ToString();
        }
    }
}

[Serializable]
public class CharacterSlotToggleView
{
    [SerializeField] private bool isDefaultSlot;
    [SerializeField, Range(0, CharacterCustomizationManager.GeneratedSlotsPerHuman - 1)]
    private int slotIndex;
    [SerializeField] private Text displayNameText;
    [SerializeField] private Text slotTypeText;
    [SerializeField] private Toggle useInGameplayToggle;
    [SerializeField] private CharacterPreviewView preview;

    public bool IsDefaultSlot => isDefaultSlot;
    public int SlotIndex => slotIndex;

    public void Refresh(CharacterSpriteSlot slot, CharacterSpriteSet spriteSet)
    {
        if (slot == null)
        {
            return;
        }

        if (displayNameText != null)
        {
            displayNameText.text = slot.displayName;
        }

        if (slotTypeText != null)
        {
            slotTypeText.text = slot.isDefaultSlot ? "Default" : (slot.isGenerated ? "Generated" : "Generated Empty");
        }

        if (useInGameplayToggle != null)
        {
            useInGameplayToggle.SetIsOnWithoutNotify(slot.isEnabledForSpawn);
        }

        if (preview != null)
        {
            preview.ApplySpriteSet(spriteSet);
        }
    }
}
