using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationSceneUIBinder : MonoBehaviour
{
    [SerializeField] private CharacterCustomizationUIController controller;
    [SerializeField] private Text presetLabel;
    [SerializeField] private Text generatedSlotLabel;
    [SerializeField] private Image headPreviewImage;
    [SerializeField] private Image neckPreviewImage;
    [SerializeField] private Image stomachPreviewImage;
    [SerializeField] private Image legPreviewImage;

    private int selectedGeneratedSlotIndex;

    private void Awake()
    {
        if (controller == null)
        {
            controller = FindFirstObjectByType<CharacterCustomizationUIController>();
        }

        BindButtons();
        RefreshAll();
    }

    private void BindButtons()
    {
        Bind("NormalHumanButton", () => SelectHuman(HumanType.NormalHuman));
        Bind("RockThrowerHumanButton", () => SelectHuman(HumanType.RockThrowerHuman));
        Bind("ObeseHumanButton", () => SelectHuman(HumanType.ObeseHuman));
        Bind("RobotHumanButton", () => SelectHuman(HumanType.RobotHuman));

        Bind("PresetPreviousButton", SelectPreviousPreset);
        Bind("PresetNextButton", SelectNextPreset);

        Bind("HeadButton", () => SelectBodyPart(BodyPartType.Head));
        Bind("NeckButton", () => SelectBodyPart(BodyPartType.Neck));
        Bind("StomachButton", () => SelectBodyPart(BodyPartType.Stomach));
        Bind("LegButton", () => SelectBodyPart(BodyPartType.Leg));

        Bind("DefaultSpriteButton", SelectDefaultSprite);
        Bind("GeneratedSlotPreviousButton", SelectPreviousGeneratedSlot);
        Bind("GeneratedSlotNextButton", SelectNextGeneratedSlot);
        Bind("SelectGeneratedSlotButton", SelectGeneratedSprite);
        Bind("SavePresetButton", SaveCurrentPreset);
    }

    private void SelectHuman(HumanType humanType)
    {
        controller.SelectHumanType(humanType);
        RefreshAll();
    }

    private void SelectPreviousPreset()
    {
        controller.SelectPreset(Mathf.Max(0, controller.SelectedPresetIndex - 1));
        RefreshAll();
    }

    private void SelectNextPreset()
    {
        controller.SelectPreset(Mathf.Min(CharacterCustomizationManager.MaxPresetsPerHuman - 1, controller.SelectedPresetIndex + 1));
        RefreshAll();
    }

    private void SelectBodyPart(BodyPartType bodyPartType)
    {
        controller.SelectBodyPart(bodyPartType);
        RefreshAll();
    }

    private void SelectDefaultSprite()
    {
        controller.SelectDefaultSprite();
        RefreshAll();
    }

    private void SelectPreviousGeneratedSlot()
    {
        selectedGeneratedSlotIndex = Mathf.Max(0, selectedGeneratedSlotIndex - 1);
        RefreshLabels();
    }

    private void SelectNextGeneratedSlot()
    {
        selectedGeneratedSlotIndex = Mathf.Min(CharacterCustomizationManager.MaxGeneratedSpriteSlotsPerPart - 1, selectedGeneratedSlotIndex + 1);
        RefreshLabels();
    }

    private void SelectGeneratedSprite()
    {
        controller.SelectGeneratedSprite(selectedGeneratedSlotIndex);
        RefreshAll();
    }

    private void SaveCurrentPreset()
    {
        controller.SaveCurrentPreset();
        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshLabels();
        RefreshPreview();
    }

    private void RefreshLabels()
    {
        if (presetLabel != null)
        {
            presetLabel.text = $"Preset {controller.SelectedPresetIndex + 1}";
        }

        if (generatedSlotLabel != null)
        {
            generatedSlotLabel.text = $"Generated Slot {selectedGeneratedSlotIndex + 1}";
        }
    }

    private void RefreshPreview()
    {
        CharacterCustomizationManager manager = CharacterCustomizationManager.Instance;
        CharacterCustomizationPreset preset = controller.WorkingPreset;

        if (manager == null || preset == null)
        {
            return;
        }

        SetPreview(headPreviewImage, manager, preset, BodyPartType.Head);
        SetPreview(neckPreviewImage, manager, preset, BodyPartType.Neck);
        SetPreview(stomachPreviewImage, manager, preset, BodyPartType.Stomach);
        SetPreview(legPreviewImage, manager, preset, BodyPartType.Leg);
    }

    private void SetPreview(Image image, CharacterCustomizationManager manager, CharacterCustomizationPreset preset, BodyPartType bodyPartType)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = manager.ResolveSprite(controller.SelectedHumanType, bodyPartType, preset.GetSelection(bodyPartType));
        image.enabled = image.sprite != null;
        image.preserveAspect = true;
    }

    private void Bind(string objectName, UnityEngine.Events.UnityAction action)
    {
        Button button = FindNamedButton(objectName);
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private Button FindNamedButton(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == objectName)
            {
                return children[i].GetComponent<Button>();
            }
        }

        return null;
    }
}
