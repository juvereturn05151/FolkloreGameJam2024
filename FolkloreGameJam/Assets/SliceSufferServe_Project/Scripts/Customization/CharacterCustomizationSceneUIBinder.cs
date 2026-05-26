using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationSceneUIBinder : MonoBehaviour
{
    [SerializeField] private CharacterCustomizationUIController controller;
    [SerializeField] private CharacterGenerationManager generationManager;
    [SerializeField] private Text selectedSlotLabel;
    [SerializeField] private Text selectedModeLabel;
    [SerializeField] private Image headPreviewImage;
    [SerializeField] private Image neckPreviewImage;
    [SerializeField] private Image stomachPreviewImage;
    [SerializeField] private Image legPreviewImage;

    private int selectedGeneratedSlotIndex;
    private GenerationMode selectedGenerationMode = GenerationMode.WholeBody;

    private void Awake()
    {
        if (controller == null)
        {
            controller = FindAnyObjectByType<CharacterCustomizationUIController>();
        }

        if (generationManager == null)
        {
            generationManager = FindAnyObjectByType<CharacterGenerationManager>();
        }

        BindButtons();
        RefreshAll();
    }

    private void BindButtons()
    {
        Bind("EditPageButton", ShowEditPage);
        Bind("GeneratePageButton", ShowGeneratePage);

        Bind("NormalHumanButton", () => SelectHuman(HumanType.NormalHuman));
        Bind("RockThrowerHumanButton", () => SelectHuman(HumanType.RockThrowerHuman));
        Bind("ObeseHumanButton", () => SelectHuman(HumanType.ObeseHuman));
        Bind("RobotHumanButton", () => SelectHuman(HumanType.RobotHuman));

        Bind("GeneratedSlotPreviousButton", SelectPreviousGeneratedSlot);
        Bind("GeneratedSlotNextButton", SelectNextGeneratedSlot);

        Bind("WholeBodyButton", () => SelectGenerationMode(GenerationMode.WholeBody));
        Bind("HeadOnlyButton", () => SelectGenerationMode(GenerationMode.HeadOnly));
        Bind("NeckOnlyButton", () => SelectGenerationMode(GenerationMode.NeckOnly));
        Bind("StomachOnlyButton", () => SelectGenerationMode(GenerationMode.StomachOnly));
        Bind("LegOnlyButton", () => SelectGenerationMode(GenerationMode.LegOnly));
        Bind("GenerateButton", GenerateSelectedCharacter);

        for (int i = 0; i < CharacterCustomizationManager.GeneratedSlotsPerHuman; i++)
        {
            int capturedIndex = i;
            Bind($"GenerateSlot{capturedIndex + 1}Button", () => SelectGeneratedSlot(capturedIndex));
            BindToggle($"GeneratedSlot{capturedIndex + 1}Toggle", enabled => controller?.ToggleSlotForGameplay(capturedIndex, enabled));
        }

        BindToggle("DefaultSlotToggle", enabled => controller?.ToggleDefaultSlotForGameplay(enabled));
        BindInputField("PromptInput", prompt => controller?.SetPromptText(prompt));

        // These names existed in the first temporary UI. They now operate on generated character slots.
        Bind("PresetPreviousButton", SelectPreviousGeneratedSlot);
        Bind("PresetNextButton", SelectNextGeneratedSlot);
        Bind("DefaultSpriteButton", () => controller?.ToggleDefaultSlotForGameplay(true));
        Bind("SelectGeneratedSlotButton", () => controller?.SelectGeneratedSlot(selectedGeneratedSlotIndex + 1));
        Bind("SavePresetButton", () => controller?.RefreshEditPage());
    }

    private void ShowEditPage()
    {
        controller?.ShowEditPage();
        RefreshAll();
    }

    private void ShowGeneratePage()
    {
        controller?.ShowGeneratePage();
        RefreshAll();
    }

    private void SelectHuman(HumanType humanType)
    {
        controller?.SelectHumanType(humanType);
        RefreshAll();
    }

    private void SelectPreviousGeneratedSlot()
    {
        SelectGeneratedSlot(Mathf.Max(0, selectedGeneratedSlotIndex - 1));
    }

    private void SelectNextGeneratedSlot()
    {
        SelectGeneratedSlot(Mathf.Min(CharacterCustomizationManager.GeneratedSlotsPerHuman - 1, selectedGeneratedSlotIndex + 1));
    }

    private void SelectGeneratedSlot(int zeroBasedIndex)
    {
        selectedGeneratedSlotIndex = Mathf.Clamp(zeroBasedIndex, 0, CharacterCustomizationManager.GeneratedSlotsPerHuman - 1);
        controller?.SelectGeneratedSlot(selectedGeneratedSlotIndex + 1);
        RefreshAll();
    }

    private void SelectGenerationMode(GenerationMode mode)
    {
        selectedGenerationMode = mode;
        controller?.SelectGenerationMode(mode);
        RefreshAll();
    }

    private void GenerateSelectedCharacter()
    {
        generationManager?.GenerateSelectedCharacter();
    }

    private void RefreshAll()
    {
        if (controller == null)
        {
            return;
        }

        if (selectedSlotLabel != null)
        {
            selectedSlotLabel.text = $"Generated Slot {selectedGeneratedSlotIndex + 1}";
        }

        if (selectedModeLabel != null)
        {
            selectedModeLabel.text = selectedGenerationMode.ToString();
        }

        RefreshPreview();
    }

    private void RefreshPreview()
    {
        CharacterCustomizationManager manager = CharacterCustomizationManager.Instance;
        if (manager == null || controller == null)
        {
            return;
        }

        CharacterSpriteSlot slot = manager.GetGeneratedSlot(controller.SelectedHumanType, selectedGeneratedSlotIndex);
        CharacterSpriteSet spriteSet = manager.ResolveSpriteSet(controller.SelectedHumanType, slot);
        SetPreview(headPreviewImage, spriteSet.head);
        SetPreview(neckPreviewImage, spriteSet.neck);
        SetPreview(stomachPreviewImage, spriteSet.stomach);
        SetPreview(legPreviewImage, spriteSet.leg);
    }

    private static void SetPreview(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
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

    private void BindToggle(string objectName, UnityEngine.Events.UnityAction<bool> action)
    {
        Toggle toggle = FindNamedToggle(objectName);
        if (toggle == null)
        {
            return;
        }

        toggle.onValueChanged.RemoveListener(action);
        toggle.onValueChanged.AddListener(action);
    }

    private Toggle FindNamedToggle(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == objectName)
            {
                return children[i].GetComponent<Toggle>();
            }
        }

        return null;
    }

    private void BindInputField(string objectName, UnityEngine.Events.UnityAction<string> action)
    {
        InputField inputField = FindNamedInputField(objectName);
        if (inputField == null)
        {
            return;
        }

        inputField.onValueChanged.RemoveListener(action);
        inputField.onValueChanged.AddListener(action);
    }

    private InputField FindNamedInputField(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == objectName)
            {
                return children[i].GetComponent<InputField>();
            }
        }

        return null;
    }
}
