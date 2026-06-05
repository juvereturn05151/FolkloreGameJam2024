using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterCustomizer : MonoBehaviour
{
    public const int FreeNormalHumanPartCount = 5;

    private const string NormalHumanResourceRoot = "Characters/Human/Normal";
    private const string RockThrowerHumanResourceRoot = "Characters/Human/RockThrower";
    private const string BigHumanResourceRoot = "Characters/Human/Big";
    private const string KnightHumanResourceRoot = "Characters/Human/Knight";
    private const string RobotHumanResourceRoot = "Characters/Human/Robot";
    private const string FreeHeadsResourcePath = "Characters/Human/Normal/Free/Head";
    private const string FreeNecksResourcePath = "Characters/Human/Normal/Free/Neck";
    private const string FreeStomachsResourcePath = "Characters/Human/Normal/Free/Stomach";
    private const string FreeLegsResourcePath = "Characters/Human/Normal/Free/Leg";
    private const string UnlockableHeadsResourcePath = "Characters/Human/Normal/Unlockables/Head";
    private const string UnlockableNecksResourcePath = "Characters/Human/Normal/Unlockables/Neck";
    private const string UnlockableStomachsResourcePath = "Characters/Human/Normal/Unlockables/Stomach";
    private const string UnlockableLegsResourcePath = "Characters/Human/Normal/Unlockables/Leg";

    [Header("Preview UI Images")]
    [SerializeField] private Image headImage;
    [SerializeField] private Image neckImage;
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image legsImage;

    [Header("Optional Sprite Overrides")]
    [SerializeField] private Sprite[] headOptions;
    [SerializeField] private Sprite[] neckOptions;
    [SerializeField] private Sprite[] bodyOptions;
    [SerializeField] private Sprite[] legsOptions;

    [Header("Manual UI")]
    [SerializeField] private bool allPartsUnlockedForTesting;
    [SerializeField] private CharacterCustomizationManager customizationManager;
    [SerializeField] private Transform manualControlsRoot;
    [SerializeField] private Text titleText;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject weaponCursorPanel;
    [SerializeField] private Button characterTabButton;
    [SerializeField] private Button weaponTabButton;
    [SerializeField] private GameObject[] characterPartUiObjects;
    [SerializeField] private GameObject[] humanPreviewUiObjects;
    [SerializeField] private GameObject humanPreviewRoot;
    [SerializeField] private GameObject weaponPreviewRoot;
    [SerializeField] private Image weaponPreviewImage;

    private CharacterCustomizationData selectedData;
    private CursorCustomizationCatalog cursorCatalog;
    private Sprite weaponPreviewSprite;
    private bool showingWeaponPreview;
    private Vector3 headPreviewBaseScale = Vector3.one;
    private Vector3 neckPreviewBaseScale = Vector3.one;
    private Vector3 bodyPreviewBaseScale = Vector3.one;
    private Vector3 legsPreviewBaseScale = Vector3.one;

    private void Awake()
    {
        selectedData = CharacterCustomizationData.LoadFromPlayerPrefs();
        ResolveCustomizationManager();
        LoadSprites();
        ResolveSelectedSpriteIds();
        ClampSelectedIndices();
        ResolvePreviewReferences();
        CachePreviewBaseScales();
        BindManualUiButtons();
        BindWeaponCursorUi();
        ApplyPreview();
        UpdateStatusText();
    }

    private void OnEnable()
    {
        GlobalCurrencyPanel.SetVisible(false);
    }

    private void OnDestroy()
    {
        GlobalCurrencyPanel.SetVisible(true);
    }

    public void NextHead()
    {
        selectedData.headIndex = NextUnlockedIndex(BodyPartType.Head, selectedData.headIndex, headOptions);
        LogSelectedPart(BodyPartType.Head, selectedData.headIndex, headOptions);
        UpdateSelectedSpriteIds();
        ApplyHead();
        UpdateStatusText();
    }

    public void PreviousHead()
    {
        selectedData.headIndex = PreviousUnlockedIndex(BodyPartType.Head, selectedData.headIndex, headOptions);
        LogSelectedPart(BodyPartType.Head, selectedData.headIndex, headOptions);
        UpdateSelectedSpriteIds();
        ApplyHead();
        UpdateStatusText();
    }

    public void NextNeck()
    {
        selectedData.neckIndex = NextUnlockedIndex(BodyPartType.Neck, selectedData.neckIndex, neckOptions);
        LogSelectedPart(BodyPartType.Neck, selectedData.neckIndex, neckOptions);
        UpdateSelectedSpriteIds();
        ApplyNeck();
        UpdateStatusText();
    }

    public void PreviousNeck()
    {
        selectedData.neckIndex = PreviousUnlockedIndex(BodyPartType.Neck, selectedData.neckIndex, neckOptions);
        LogSelectedPart(BodyPartType.Neck, selectedData.neckIndex, neckOptions);
        UpdateSelectedSpriteIds();
        ApplyNeck();
        UpdateStatusText();
    }

    public void NextStomach()
    {
        selectedData.bodyIndex = NextUnlockedIndex(BodyPartType.Stomach, selectedData.bodyIndex, bodyOptions);
        LogSelectedPart(BodyPartType.Stomach, selectedData.bodyIndex, bodyOptions);
        UpdateSelectedSpriteIds();
        ApplyBody();
        UpdateStatusText();
    }

    public void NextBody()
    {
        NextStomach();
    }

    public void PreviousStomach()
    {
        selectedData.bodyIndex = PreviousUnlockedIndex(BodyPartType.Stomach, selectedData.bodyIndex, bodyOptions);
        LogSelectedPart(BodyPartType.Stomach, selectedData.bodyIndex, bodyOptions);
        UpdateSelectedSpriteIds();
        ApplyBody();
        UpdateStatusText();
    }

    public void PreviousBody()
    {
        PreviousStomach();
    }

    public void NextLegs()
    {
        selectedData.legsIndex = NextUnlockedIndex(BodyPartType.Leg, selectedData.legsIndex, legsOptions);
        LogSelectedPart(BodyPartType.Leg, selectedData.legsIndex, legsOptions);
        UpdateSelectedSpriteIds();
        ApplyLegs();
        UpdateStatusText();
    }

    public void PreviousLegs()
    {
        selectedData.legsIndex = PreviousUnlockedIndex(BodyPartType.Leg, selectedData.legsIndex, legsOptions);
        LogSelectedPart(BodyPartType.Leg, selectedData.legsIndex, legsOptions);
        UpdateSelectedSpriteIds();
        ApplyLegs();
        UpdateStatusText();
    }

    public void SelectDefaultCharacter()
    {
        selectedData.selectedHumanType = HumanType.NormalHuman;
        selectedData.headIndex = 0;
        selectedData.neckIndex = 0;
        selectedData.bodyIndex = 0;
        selectedData.legsIndex = 0;
        LoadSprites();
        UpdateSelectedSpriteIds();
        selectedData.SaveToPlayerPrefs();
        ApplyPreview();
        UpdateStatusText("Using default normal human.");
        Debug.Log("Manual customization selected default normal human.");
    }

    public void SelectNormalHuman()
    {
        SelectHumanType(HumanType.NormalHuman);
    }

    public void SelectRockThrowerHuman()
    {
        SelectHumanType(HumanType.RockThrowerHuman);
    }

    public void SelectBigHuman()
    {
        SelectHumanType(HumanType.BigHuman);
    }

    public void SelectKnightHuman()
    {
        SelectHumanType(HumanType.KnightHuman);
    }

    public void SelectRobotHuman()
    {
        SelectHumanType(HumanType.RobotHuman);
    }

    public void SaveCustomization()
    {
        ClampSelectedIndices();
        if (!AreSelectedPartsUnlocked())
        {
            UpdateStatusText("Buy selected parts in Store first.");
            return;
        }

        UpdateSelectedSpriteIds();
        selectedData.SaveToPlayerPrefs();
        UpdateStatusText("Saved manual customization.");
    }

    public void ShowCharacterCustomizationTab()
    {
        showingWeaponPreview = false;
        SetCharacterPartUiActive(true);
        SetHumanPreviewUiActive(true);
        ApplyPreview();

        if (weaponCursorPanel != null)
        {
            weaponCursorPanel.SetActive(false);
        }

        SetTabButtonColor(characterTabButton, new Color(0.74f, 0.22f, 0.16f, 1f));
        SetTabButtonColor(weaponTabButton, new Color(0.24f, 0.22f, 0.18f, 1f));
        RefreshHumanTypeTabColors();
        UpdateStatusText();
    }

    public void ShowWeaponCustomizationTab()
    {
        showingWeaponPreview = true;
        SetCharacterPartUiActive(false);
        SetHumanPreviewUiActive(false);
        ClearHumanPreviewSprites();

        if (weaponCursorPanel != null)
        {
            weaponCursorPanel.SetActive(true);
        }

        SetTabButtonColor(characterTabButton, new Color(0.24f, 0.22f, 0.18f, 1f));
        SetTabButtonColor(weaponTabButton, new Color(0.74f, 0.22f, 0.16f, 1f));
        RefreshWeaponCursorButtons();
        ApplyWeaponPreview();
    }

    public void SelectWeaponCursor(string cursorId)
    {
        if (selectedData == null)
        {
            selectedData = CharacterCustomizationData.LoadFromPlayerPrefs();
        }

        string resolvedCursorId = string.IsNullOrWhiteSpace(cursorId) ? CursorCustomizationSelection.DefaultCursorId : cursorId;
        if (!IsWeaponCursorUnlocked(resolvedCursorId))
        {
            UpdateStatusText($"{GetCursorDisplayName(resolvedCursorId)} is locked.");
            RefreshWeaponCursorButtons();
            return;
        }

        selectedData.selectedCursorId = resolvedCursorId;
        selectedData.SaveToPlayerPrefs();
        DragAndDropManager.Instance?.UseKnifeCursor();
        RefreshWeaponCursorButtons();
        ApplyWeaponPreview();
        UpdateStatusText($"Selected {GetCursorDisplayName(selectedData.selectedCursorId)}.");
    }

    public void BackToGameModeSelect()
    {
        SceneManager.LoadScene("GameModeSelect");
    }

    public static void SetNormalHumanPartUnlocked(BodyPartType part, int index, bool unlocked)
    {
        SetHumanPartUnlocked(HumanType.NormalHuman, part, index, unlocked);
    }

    public static bool IsNormalHumanPartUnlocked(BodyPartType part, int index)
    {
        return IsHumanPartUnlocked(HumanType.NormalHuman, part, index);
    }

    public static string GetNormalHumanPartUnlockKey(BodyPartType part, int index)
    {
        return GetHumanPartUnlockKey(HumanType.NormalHuman, part, index);
    }

    public static void SetHumanPartUnlocked(HumanType humanType, BodyPartType part, int index, bool unlocked)
    {
        Sprite[] sprites = LoadHumanPartSprites(humanType, part);
        SetHumanPartUnlocked(humanType, part, index, GetSprite(sprites, index), unlocked);
    }

    public static void SetHumanPartUnlocked(HumanType humanType, BodyPartType part, int index, Sprite sprite, bool unlocked)
    {
        string spriteId = GetHumanPartSpriteId(sprite);
        if (!string.IsNullOrWhiteSpace(spriteId))
        {
            Sprite[] sprites = LoadHumanPartSprites(humanType, part);
            int spriteIndex = FindSpriteIndex(sprites, spriteId);
            if (spriteIndex >= 0)
            {
                index = spriteIndex;
            }
        }

        if (index < 0 || index < GetFreeHumanPartCount(humanType, part))
        {
            return;
        }

        PlayerPrefs.SetInt(GetHumanPartUnlockKey(humanType, part, index), unlocked ? 1 : 0);
        if (!string.IsNullOrWhiteSpace(spriteId))
        {
            PlayerPrefs.SetInt(GetHumanPartUnlockKey(humanType, part, spriteId), unlocked ? 1 : 0);
            PlayerPrefs.SetInt(GetHumanPartUnlockKey(part, spriteId), unlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
        Debug.Log($"Manual customization unlock updated: humanType={humanType}, part={part}, index={index}, sprite={spriteId}, unlocked={unlocked}");
    }

    public static void SetHumanPartUnlocked(HumanType humanType, BodyPartType part, Sprite sprite, bool unlocked)
    {
        if (sprite == null)
        {
            return;
        }

        Sprite[] sprites = LoadHumanPartSprites(humanType, part);
        int index = FindSpriteIndex(sprites, GetHumanPartSpriteId(sprite));
        if (index >= 0 && index < GetFreeHumanPartCount(humanType, part))
        {
            return;
        }

        string spriteId = GetHumanPartSpriteId(sprite);
        if (!string.IsNullOrWhiteSpace(spriteId))
        {
            PlayerPrefs.SetInt(GetHumanPartUnlockKey(humanType, part, spriteId), unlocked ? 1 : 0);
            PlayerPrefs.SetInt(GetHumanPartUnlockKey(part, spriteId), unlocked ? 1 : 0);
        }

        if (index >= 0)
        {
            PlayerPrefs.SetInt(GetHumanPartUnlockKey(humanType, part, index), unlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
        Debug.Log($"Manual customization unlock updated: humanType={humanType}, part={part}, sprite={spriteId}, unlocked={unlocked}");
    }

    public static bool IsHumanPartUnlocked(HumanType humanType, BodyPartType part, int index)
    {
        Sprite[] sprites = LoadHumanPartSprites(humanType, part);
        if (index < 0 || index >= sprites.Length)
        {
            return false;
        }

        if (index < GetFreeHumanPartCount(humanType, part) || PlayerPrefs.GetInt(GetHumanPartUnlockKey(humanType, part, index), 0) == 1)
        {
            return true;
        }

        Sprite sprite = GetSprite(sprites, index);
        string spriteId = GetHumanPartSpriteId(sprite);
        return IsHumanPartSpriteIdUnlocked(humanType, part, spriteId);
    }

    public static bool IsHumanPartUnlocked(HumanType humanType, BodyPartType part, Sprite sprite)
    {
        if (sprite == null)
        {
            return false;
        }

        Sprite[] sprites = LoadHumanPartSprites(humanType, part);
        int index = FindSpriteIndex(sprites, GetHumanPartSpriteId(sprite));
        if (index >= 0 && IsHumanPartUnlocked(humanType, part, index))
        {
            return true;
        }

        string spriteId = GetHumanPartSpriteId(sprite);
        return IsHumanPartSpriteIdUnlocked(humanType, part, spriteId);
    }

    public static string GetHumanPartUnlockKey(HumanType humanType, BodyPartType part, int index)
    {
        return $"Unlocked{humanType}{part}{index}";
    }

    public static string GetHumanPartUnlockKey(HumanType humanType, BodyPartType part, string spriteId)
    {
        return $"Unlocked{humanType}{part}_{spriteId}";
    }

    public static string GetHumanPartUnlockKey(BodyPartType part, string spriteId)
    {
        return $"UnlockedHumanPart{part}_{spriteId}";
    }

    private static bool IsHumanPartSpriteIdUnlocked(HumanType humanType, BodyPartType part, string spriteId)
    {
        if (string.IsNullOrWhiteSpace(spriteId))
        {
            return false;
        }

        if (PlayerPrefs.GetInt(GetHumanPartUnlockKey(humanType, part, spriteId), 0) == 1
            || PlayerPrefs.GetInt(GetHumanPartUnlockKey(part, spriteId), 0) == 1)
        {
            return true;
        }

        foreach (HumanType savedHumanType in System.Enum.GetValues(typeof(HumanType)))
        {
            if (PlayerPrefs.GetInt(GetHumanPartUnlockKey(savedHumanType, part, spriteId), 0) == 1)
            {
                PlayerPrefs.SetInt(GetHumanPartUnlockKey(part, spriteId), 1);
                PlayerPrefs.Save();
                return true;
            }
        }

        return false;
    }

    public static void SetWeaponCursorUnlocked(string cursorId, bool unlocked)
    {
        if (string.IsNullOrWhiteSpace(cursorId) || cursorId == CursorCustomizationSelection.DefaultCursorId)
        {
            return;
        }

        PlayerPrefs.SetInt(GetWeaponCursorUnlockKey(cursorId), unlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool IsWeaponCursorUnlocked(string cursorId)
    {
        return string.IsNullOrWhiteSpace(cursorId)
            || cursorId == CursorCustomizationSelection.DefaultCursorId
            || PlayerPrefs.GetInt(GetWeaponCursorUnlockKey(cursorId), 0) == 1;
    }

    public static string GetWeaponCursorUnlockKey(string cursorId)
    {
        return $"UnlockedWeaponCursor_{cursorId}";
    }

    public static Sprite[] LoadNormalHumanPartSprites(BodyPartType part)
    {
        return LoadHumanPartSprites(HumanType.NormalHuman, part);
    }

    public static Sprite[] LoadHumanPartSprites(HumanType humanType, BodyPartType part)
    {
        Sprite[] freeSprites = LoadPartSprites(humanType, part, true);
        Sprite[] unlockableSprites = LoadUnlockableHumanPartSprites(humanType, part);

        SortSpritesByDefaultFirst(freeSprites);
        Sprite[] sprites = new Sprite[freeSprites.Length + unlockableSprites.Length];
        freeSprites.CopyTo(sprites, 0);
        unlockableSprites.CopyTo(sprites, freeSprites.Length);
        return sprites;
    }

    public static Sprite[] LoadUnlockableHumanPartSprites(HumanType humanType, BodyPartType part)
    {
        Sprite[] freeSprites = LoadPartSprites(humanType, part, true);
        Sprite[] unlockableSprites = LoadPartSprites(humanType, part, false);

        SortSpritesByDefaultFirst(freeSprites);
        SortSpritesByDefaultFirst(unlockableSprites);
        return RemoveFreeSpriteDuplicates(unlockableSprites, freeSprites);
    }

    public static int GetFreeHumanPartCount(HumanType humanType, BodyPartType part)
    {
        return LoadPartSprites(humanType, part, true).Length;
    }

    private static Sprite[] RemoveFreeSpriteDuplicates(Sprite[] unlockableSprites, Sprite[] freeSprites)
    {
        if (unlockableSprites == null || unlockableSprites.Length == 0 || freeSprites == null || freeSprites.Length == 0)
        {
            return unlockableSprites ?? System.Array.Empty<Sprite>();
        }

        System.Collections.Generic.HashSet<string> freeSpriteNames = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < freeSprites.Length; i++)
        {
            if (freeSprites[i] != null && !string.IsNullOrWhiteSpace(freeSprites[i].name))
            {
                freeSpriteNames.Add(freeSprites[i].name);
            }
        }

        if (freeSpriteNames.Count == 0)
        {
            return unlockableSprites;
        }

        System.Collections.Generic.List<Sprite> filtered = new System.Collections.Generic.List<Sprite>();
        for (int i = 0; i < unlockableSprites.Length; i++)
        {
            Sprite sprite = unlockableSprites[i];
            if (sprite != null && freeSpriteNames.Contains(sprite.name))
            {
                continue;
            }

            filtered.Add(sprite);
        }

        return filtered.ToArray();
    }

    private void ResolvePreviewReferences()
    {
        headImage ??= FindImageByName("HeadPreviewImage");
        neckImage ??= FindImageByName("NeckPreviewImage");
        bodyImage ??= FindImageByName("StomachPreviewImage");
        legsImage ??= FindImageByName("LegPreviewImage");
        titleText ??= FindTextByName("Title");
        statusText ??= FindTextByName("SelectedHumanLabel");
        humanPreviewRoot ??= FindTransformByName("Human")?.gameObject;
        weaponPreviewRoot ??= FindTransformByName("Weapon")?.gameObject;
        weaponPreviewImage ??= FindChildImageByName(weaponPreviewRoot, "Image") ?? FindImageByName("WeaponPreviewImage");
    }

    private void CachePreviewBaseScales()
    {
        headPreviewBaseScale = GetImageScale(headImage);
        neckPreviewBaseScale = GetImageScale(neckImage);
        bodyPreviewBaseScale = GetImageScale(bodyImage);
        legsPreviewBaseScale = GetImageScale(legsImage);
    }

    private void ApplyHumanPreviewScales()
    {
        bool widenBigHumanBodyParts = selectedData != null && selectedData.selectedHumanType == HumanType.BigHuman;
        ApplyPreviewScale(headImage, headPreviewBaseScale, false);
        ApplyPreviewScale(neckImage, neckPreviewBaseScale, widenBigHumanBodyParts);
        ApplyPreviewScale(bodyImage, bodyPreviewBaseScale, widenBigHumanBodyParts);
        ApplyPreviewScale(legsImage, legsPreviewBaseScale, widenBigHumanBodyParts);
    }

    private void ResetHumanPreviewScales()
    {
        ApplyPreviewScale(headImage, headPreviewBaseScale, false);
        ApplyPreviewScale(neckImage, neckPreviewBaseScale, false);
        ApplyPreviewScale(bodyImage, bodyPreviewBaseScale, false);
        ApplyPreviewScale(legsImage, legsPreviewBaseScale, false);
    }

    private static Vector3 GetImageScale(Image image)
    {
        return image != null ? image.rectTransform.localScale : Vector3.one;
    }

    private static void ApplyPreviewScale(Image image, Vector3 baseScale, bool widen)
    {
        if (image == null)
        {
            return;
        }

        Vector3 scale = baseScale;
        if (widen)
        {
            scale.x *= 1.50f;
        }

        image.rectTransform.localScale = scale;
    }

    private void BindManualUiButtons()
    {
        BindButton("HeadPreviousButton", PreviousHead);
        BindButton("HeadNextButton", NextHead);
        BindButton("NeckPreviousButton", PreviousNeck);
        BindButton("NeckNextButton", NextNeck);
        BindButton("StomachPreviousButton", PreviousStomach);
        BindButton("StomachNextButton", NextStomach);
        BindButton("LegPreviousButton", PreviousLegs);
        BindButton("LegNextButton", NextLegs);
        BindButton("DefaultButton", SelectDefaultCharacter);
        BindButton("SaveButton", SaveCustomization);
        BindButton("BackButton", BackToGameModeSelect);
        BindButton("NormalHumanButton", SelectNormalHuman);
        BindButton("KnightHumanButton", SelectKnightHuman);
        BindButton("RockThrowerHumanButton", SelectRockThrowerHuman);
        BindButton("BigHumanButton", SelectBigHuman);
        BindButton("RobotHumanButton", SelectRobotHuman);
    }

    private void BindWeaponCursorUi()
    {
        cursorCatalog = CursorCustomizationCatalog.LoadDefault();
        if (cursorCatalog == null || cursorCatalog.Options.Length == 0)
        {
            Debug.LogWarning("Cursor customization catalog is missing or empty.");
            return;
        }

        weaponCursorPanel ??= FindTransformByName("WeaponCursorPanel")?.gameObject;
        characterTabButton ??= FindButtonByName("CharacterCustomizationTabButton");
        weaponTabButton ??= FindButtonByName("WeaponCursorTabButton");
        ResolveCharacterPartUiObjects();
        ResolveHumanPreviewUiObjects();

        if (characterTabButton != null)
        {
            characterTabButton.onClick.RemoveListener(ShowCharacterCustomizationTab);
            characterTabButton.onClick.AddListener(ShowCharacterCustomizationTab);
        }

        if (weaponTabButton != null)
        {
            weaponTabButton.onClick.RemoveListener(ShowWeaponCustomizationTab);
            weaponTabButton.onClick.AddListener(ShowWeaponCustomizationTab);
        }

        foreach (CursorCustomizationOption option in cursorCatalog.Options)
        {
            if (option == null)
            {
                continue;
            }

            Button button = FindButtonByName($"{option.Id}CursorOptionButton");
            if (button == null)
            {
                continue;
            }

            EnsureWeaponCursorOptionImage(button, option);
            SetButtonLabel(button, IsWeaponCursorUnlocked(option.Id) ? option.DisplayName : $"{option.DisplayName} (Locked)");
            string capturedId = option.Id;
            button.onClick.AddListener(() => SelectWeaponCursor(capturedId));
        }

        ShowCharacterCustomizationTab();
    }

    private void ResolveCharacterPartUiObjects()
    {
        if (characterPartUiObjects != null && characterPartUiObjects.Length > 0)
        {
            return;
        }

        string[] objectNames =
        {
            "HumanTypeTabs",
            "ControlsTitle",
            "HeadLabel",
            "HeadPreviousButton",
            "HeadNextButton",
            "NeckLabel",
            "NeckPreviousButton",
            "NeckNextButton",
            "StomachLabel",
            "StomachPreviousButton",
            "StomachNextButton",
            "LegLabel",
            "LegPreviousButton",
            "LegNextButton",
            "DefaultButton",
            "SaveButton"
        };

        characterPartUiObjects = new GameObject[objectNames.Length];
        for (int i = 0; i < objectNames.Length; i++)
        {
            characterPartUiObjects[i] = FindTransformByName(objectNames[i])?.gameObject;
        }
    }

    private void SetCharacterPartUiActive(bool active)
    {
        ResolveCharacterPartUiObjects();

        if (characterPartUiObjects == null)
        {
            return;
        }

        for (int i = 0; i < characterPartUiObjects.Length; i++)
        {
            if (characterPartUiObjects[i] != null)
            {
                characterPartUiObjects[i].SetActive(active);
            }
        }
    }

    private void ResolveHumanPreviewUiObjects()
    {
        if (humanPreviewUiObjects != null && humanPreviewUiObjects.Length > 0)
        {
            return;
        }

        string[] objectNames =
        {
            "PreviewLabel",
            "ManualPreview"
        };

        humanPreviewUiObjects = new GameObject[objectNames.Length];
        for (int i = 0; i < objectNames.Length; i++)
        {
            humanPreviewUiObjects[i] = FindTransformByName(objectNames[i])?.gameObject;
        }
    }

    private void SetHumanPreviewUiActive(bool active)
    {
        ResolveHumanPreviewUiObjects();

        if (humanPreviewUiObjects != null)
        {
            for (int i = 0; i < humanPreviewUiObjects.Length; i++)
            {
                if (humanPreviewUiObjects[i] != null)
                {
                    humanPreviewUiObjects[i].SetActive(true);
                }
            }
        }

        if (humanPreviewRoot != null)
        {
            humanPreviewRoot.SetActive(active);
        }

        if (weaponPreviewRoot != null)
        {
            weaponPreviewRoot.SetActive(!active);
        }
    }

    private void LoadSprites()
    {
        HumanType humanType = selectedData != null ? selectedData.selectedHumanType : HumanType.NormalHuman;
        Sprite[] resourceHeads = LoadHumanPartOptions(humanType, BodyPartType.Head);
        Sprite[] resourceNecks = LoadHumanPartOptions(humanType, BodyPartType.Neck);
        Sprite[] resourceStomachs = LoadHumanPartOptions(humanType, BodyPartType.Stomach);
        Sprite[] resourceLegs = LoadHumanPartOptions(humanType, BodyPartType.Leg);

        if (resourceHeads.Length > 0)
        {
            headOptions = resourceHeads;
        }
        else
        {
            headOptions = System.Array.Empty<Sprite>();
        }

        if (resourceNecks.Length > 0)
        {
            neckOptions = resourceNecks;
        }
        else
        {
            neckOptions = System.Array.Empty<Sprite>();
        }

        if (resourceStomachs.Length > 0)
        {
            bodyOptions = resourceStomachs;
        }
        else
        {
            bodyOptions = System.Array.Empty<Sprite>();
        }

        if (resourceLegs.Length > 0)
        {
            legsOptions = resourceLegs;
        }
        else
        {
            legsOptions = System.Array.Empty<Sprite>();
        }

        Debug.Log($"Manual customization loaded {humanType} sprites: heads={headOptions.Length}, necks={neckOptions.Length}, stomachs={bodyOptions.Length}, legs={legsOptions.Length}");
    }

    private Sprite[] LoadHumanPartOptions(HumanType humanType, BodyPartType part)
    {
        Sprite[] resourceSprites = LoadHumanPartSprites(humanType, part);
        if (resourceSprites.Length > 0)
        {
            return resourceSprites;
        }

        ResolveCustomizationManager();
        Sprite defaultSprite = customizationManager != null ? customizationManager.GetDefaultSprite(humanType, part) : null;
        return defaultSprite != null ? new[] { defaultSprite } : System.Array.Empty<Sprite>();
    }

    private void ResolveCustomizationManager()
    {
        if (customizationManager != null)
        {
            return;
        }

        customizationManager = CharacterCustomizationManager.Instance ?? FindAnyObjectByType<CharacterCustomizationManager>();
    }

    private void SelectHumanType(HumanType humanType)
    {
        if (selectedData == null)
        {
            selectedData = CharacterCustomizationData.LoadFromPlayerPrefs();
        }

        selectedData.selectedHumanType = humanType;
        selectedData.headIndex = 0;
        selectedData.neckIndex = 0;
        selectedData.bodyIndex = 0;
        selectedData.legsIndex = 0;
        selectedData.SetSpriteId(BodyPartType.Head, string.Empty);
        selectedData.SetSpriteId(BodyPartType.Neck, string.Empty);
        selectedData.SetSpriteId(BodyPartType.Stomach, string.Empty);
        selectedData.SetSpriteId(BodyPartType.Leg, string.Empty);
        LoadSprites();
        ClampSelectedIndices();
        ApplyPreview();
        RefreshHumanTypeTabColors();
        UpdateStatusText($"Selected {GetHumanTypeDisplayName(humanType)}.");
    }

    private void ClampSelectedIndices()
    {
        selectedData.headIndex = ClampIndex(selectedData.headIndex, headOptions);
        selectedData.neckIndex = ClampIndex(selectedData.neckIndex, neckOptions);
        selectedData.bodyIndex = ClampIndex(selectedData.bodyIndex, bodyOptions);
        selectedData.legsIndex = ClampIndex(selectedData.legsIndex, legsOptions);
    }

    private void ResolveSelectedSpriteIds()
    {
        selectedData.headIndex = ResolveSelectedIndex(BodyPartType.Head, selectedData.headIndex, headOptions);
        selectedData.neckIndex = ResolveSelectedIndex(BodyPartType.Neck, selectedData.neckIndex, neckOptions);
        selectedData.bodyIndex = ResolveSelectedIndex(BodyPartType.Stomach, selectedData.bodyIndex, bodyOptions);
        selectedData.legsIndex = ResolveSelectedIndex(BodyPartType.Leg, selectedData.legsIndex, legsOptions);
    }

    private void UpdateSelectedSpriteIds()
    {
        selectedData.SetSpriteId(BodyPartType.Head, GetHumanPartSpriteId(GetSprite(headOptions, selectedData.headIndex)));
        selectedData.SetSpriteId(BodyPartType.Neck, GetHumanPartSpriteId(GetSprite(neckOptions, selectedData.neckIndex)));
        selectedData.SetSpriteId(BodyPartType.Stomach, GetHumanPartSpriteId(GetSprite(bodyOptions, selectedData.bodyIndex)));
        selectedData.SetSpriteId(BodyPartType.Leg, GetHumanPartSpriteId(GetSprite(legsOptions, selectedData.legsIndex)));
    }

    private int ResolveSelectedIndex(BodyPartType part, int fallbackIndex, Sprite[] options)
    {
        string spriteId = selectedData.GetSpriteId(part);
        int resolvedIndex = FindSpriteIndex(options, spriteId);
        return resolvedIndex >= 0 ? resolvedIndex : ClampIndex(fallbackIndex, options);
    }

    private void ApplyPreview()
    {
        if (showingWeaponPreview)
        {
            ApplyWeaponPreview();
            return;
        }

        ApplyHumanPreviewScales();
        ApplyHead();
        ApplyNeck();
        ApplyBody();
        ApplyLegs();
    }

    private void ApplyWeaponPreview()
    {
        if (!showingWeaponPreview)
        {
            return;
        }

        cursorCatalog ??= CursorCustomizationCatalog.LoadDefault();
        CursorCustomizationOption option = cursorCatalog != null ? cursorCatalog.GetOption(selectedData.selectedCursorId) : null;
        Texture2D cursorTexture = option != null ? option.cursorTexture : null;
        Image targetPreviewImage = weaponPreviewImage != null ? weaponPreviewImage : headImage;
        ClearHumanPreviewSprites();
        ResetHumanPreviewScales();

        if (cursorTexture != null)
        {
            if (weaponPreviewSprite == null || weaponPreviewSprite.texture != cursorTexture)
            {
                weaponPreviewSprite = Sprite.Create(
                    cursorTexture,
                    new Rect(0f, 0f, cursorTexture.width, cursorTexture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }

            SetSprite(targetPreviewImage, weaponPreviewSprite);
        }

        if (targetPreviewImage == headImage)
        {
            SetSprite(neckImage, null);
            SetSprite(bodyImage, null);
            SetSprite(legsImage, null);
        }
    }

    private void ApplyHead()
    {
        ApplyHumanPreviewScales();
        SetSprite(headImage, GetSprite(headOptions, selectedData.headIndex));
    }

    private void ClearHumanPreviewSprites()
    {
        SetSprite(headImage, null);
        SetSprite(neckImage, null);
        SetSprite(bodyImage, null);
        SetSprite(legsImage, null);
    }

    private void ApplyNeck()
    {
        ApplyHumanPreviewScales();
        SetSprite(neckImage, GetSprite(neckOptions, selectedData.neckIndex));
    }

    private void ApplyBody()
    {
        ApplyHumanPreviewScales();
        SetSprite(bodyImage, GetSprite(bodyOptions, selectedData.bodyIndex));
    }

    private void ApplyLegs()
    {
        ApplyHumanPreviewScales();
        SetSprite(legsImage, GetSprite(legsOptions, selectedData.legsIndex));
    }

    private void UpdateStatusText(string prefix = null)
    {
        string humanDisplayName = GetHumanTypeDisplayName(selectedData.selectedHumanType);

        if (titleText != null)
        {
            titleText.text = selectedData.selectedHumanType == HumanType.NormalHuman
                ? humanDisplayName
                : $"{humanDisplayName} Human";
        }

        if (statusText != null)
        {
            string status = $"{humanDisplayName} | Head {DisplayIndex(selectedData.headIndex, headOptions)} | Neck {DisplayIndex(selectedData.neckIndex, neckOptions)} | Stomach {DisplayIndex(selectedData.bodyIndex, bodyOptions)} | Leg {DisplayIndex(selectedData.legsIndex, legsOptions)} | Weapon {GetCursorDisplayName(selectedData.selectedCursorId)}";
            if (!AreSelectedPartsUnlocked())
            {
                status = $"{status} | Locked";
            }

            statusText.text = string.IsNullOrWhiteSpace(prefix) ? status : $"{prefix} {status}";
        }
    }

    private void RefreshWeaponCursorButtons()
    {
        string selectedCursorId = selectedData == null ? CursorCustomizationSelection.GetSelectedCursorId() : selectedData.selectedCursorId;

        if (cursorCatalog == null)
        {
            return;
        }

        foreach (CursorCustomizationOption option in cursorCatalog.Options)
        {
            if (option == null)
            {
                continue;
            }

            Button button = FindButtonByName($"{option.Id}CursorOptionButton");
            Image background = button != null ? button.GetComponent<Image>() : null;
            if (background == null)
            {
                continue;
            }

            bool selected = option.Id == selectedCursorId;
            bool unlocked = IsWeaponCursorUnlocked(option.Id);
            button.interactable = unlocked;
            RefreshWeaponCursorOptionImage(button, option, unlocked);
            SetButtonLabel(button, unlocked ? option.DisplayName : $"{option.DisplayName} (Locked)");
            background.color = !unlocked
                ? new Color(0.09f, 0.08f, 0.07f, 0.85f)
                : selected ? new Color(0.74f, 0.22f, 0.16f, 1f) : new Color(0.18f, 0.16f, 0.13f, 1f);
        }
    }

    private void EnsureWeaponCursorOptionImage(Button button, CursorCustomizationOption option)
    {
        if (button == null)
        {
            return;
        }

        RawImage preview = FindCursorOptionRawImage(button.gameObject);
        if (preview == null)
        {
            GameObject previewObject = new GameObject("CursorPreviewImage", typeof(RectTransform), typeof(RawImage));
            previewObject.transform.SetParent(button.transform, false);

            RectTransform rectTransform = previewObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, 20f);
            rectTransform.sizeDelta = new Vector2(92f, 54f);

            preview = previewObject.GetComponent<RawImage>();
            preview.raycastTarget = false;
        }

        RefreshWeaponCursorOptionImage(button, option, IsWeaponCursorUnlocked(option.Id));
    }

    private void RefreshWeaponCursorOptionImage(Button button, CursorCustomizationOption option, bool unlocked)
    {
        RawImage preview = FindCursorOptionRawImage(button != null ? button.gameObject : null);
        if (preview == null || option == null)
        {
            return;
        }

        preview.texture = option.cursorTexture;
        preview.enabled = option.cursorTexture != null;
        preview.color = unlocked ? Color.white : new Color(0.55f, 0.52f, 0.48f, 1f);
    }

    private static RawImage FindCursorOptionRawImage(GameObject parent)
    {
        if (parent == null)
        {
            return null;
        }

        RawImage[] rawImages = parent.GetComponentsInChildren<RawImage>(true);
        for (int i = 0; i < rawImages.Length; i++)
        {
            if (rawImages[i] != null && rawImages[i].name == "CursorPreviewImage")
            {
                return rawImages[i];
            }
        }

        return null;
    }

    private string GetCursorDisplayName(string cursorId)
    {
        cursorCatalog ??= CursorCustomizationCatalog.LoadDefault();
        CursorCustomizationOption option = cursorCatalog != null ? cursorCatalog.GetOption(cursorId) : null;
        return option != null ? option.DisplayName : "Knife";
    }

    private static void SetTabButtonColor(Button button, Color color)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
    }

    private void RefreshHumanTypeTabColors()
    {
        HumanType selectedType = selectedData != null ? selectedData.selectedHumanType : HumanType.NormalHuman;
        SetHumanTypeTabColor("NormalHumanButton", selectedType == HumanType.NormalHuman);
        SetHumanTypeTabColor("BigHumanButton", selectedType == HumanType.BigHuman);
        SetHumanTypeTabColor("KnightHumanButton", selectedType == HumanType.KnightHuman);
        SetHumanTypeTabColor("RockThrowerHumanButton", selectedType == HumanType.RockThrowerHuman);
        SetHumanTypeTabColor("RobotHumanButton", selectedType == HumanType.RobotHuman);
    }

    private static void SetHumanTypeTabColor(string buttonName, bool selected)
    {
        Button button = FindButtonByName(buttonName);
        if (button == null)
        {
            return;
        }

        SetTabButtonColor(button, selected
            ? new Color(0.74f, 0.22f, 0.16f, 1f)
            : new Color(0.24f, 0.22f, 0.18f, 1f));
    }

    private int NextUnlockedIndex(BodyPartType part, int currentIndex, Sprite[] options)
    {
        if (options == null || options.Length == 0)
        {
            return 0;
        }

        for (int step = 1; step <= options.Length; step++)
        {
            int candidate = (currentIndex + step) % options.Length;
            if (IsPartUnlocked(part, candidate))
            {
                return candidate;
            }
        }

        return 0;
    }

    private int PreviousUnlockedIndex(BodyPartType part, int currentIndex, Sprite[] options)
    {
        if (options == null || options.Length == 0)
        {
            return 0;
        }

        for (int step = 1; step <= options.Length; step++)
        {
            int candidate = (currentIndex - step + options.Length) % options.Length;
            if (IsPartUnlocked(part, candidate))
            {
                return candidate;
            }
        }

        return 0;
    }

    private bool IsPartUnlocked(BodyPartType part, int index)
    {
        if (allPartsUnlockedForTesting)
        {
            return true;
        }

        HumanType humanType = selectedData != null ? selectedData.selectedHumanType : HumanType.NormalHuman;
        return IsHumanPartUnlocked(humanType, part, index);
    }

    private void LogSelectedPart(BodyPartType part, int index, Sprite[] options)
    {
        HumanType humanType = selectedData != null ? selectedData.selectedHumanType : HumanType.NormalHuman;
        string spriteId = GetHumanPartSpriteId(GetSprite(options, index));
        Debug.Log($"Manual customization selected {humanType} {part}: index={index}, sprite={spriteId}");
    }

    private bool AreSelectedPartsUnlocked()
    {
        return IsPartUnlocked(BodyPartType.Head, selectedData.headIndex)
            && IsPartUnlocked(BodyPartType.Neck, selectedData.neckIndex)
            && IsPartUnlocked(BodyPartType.Stomach, selectedData.bodyIndex)
            && IsPartUnlocked(BodyPartType.Leg, selectedData.legsIndex);
    }

    private static int ClampIndex(int index, Sprite[] options)
    {
        return options == null || options.Length == 0 ? 0 : Mathf.Clamp(index, 0, options.Length - 1);
    }

    private static Sprite GetSprite(Sprite[] options, int index)
    {
        if (options == null || options.Length == 0)
        {
            return null;
        }

        return options[Mathf.Clamp(index, 0, options.Length - 1)];
    }

    public static string GetHumanPartSpriteId(Sprite sprite)
    {
        return sprite != null ? sprite.name : string.Empty;
    }

    public static int FindSpriteIndex(Sprite[] options, string spriteId)
    {
        if (options == null || string.IsNullOrWhiteSpace(spriteId))
        {
            return -1;
        }

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null && options[i].name == spriteId)
            {
                return i;
            }
        }

        return -1;
    }

    private static string DisplayIndex(int index, Sprite[] options)
    {
        int count = options == null ? 0 : options.Length;
        return count == 0 ? "0/0" : $"{Mathf.Clamp(index, 0, count - 1) + 1}/{count}";
    }

    private static void SortSpritesByDefaultFirst(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length <= 1)
        {
            return;
        }

        System.Array.Sort(sprites, CompareSpritesByDefaultFirst);
    }

    private static int CompareSpritesByDefaultFirst(Sprite left, Sprite right)
    {
        if (left == right)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        bool leftDefault = IsDefaultSpriteName(left.name);
        bool rightDefault = IsDefaultSpriteName(right.name);

        if (leftDefault != rightDefault)
        {
            return leftDefault ? -1 : 1;
        }

        return string.CompareOrdinal(left.name, right.name);
    }

    private static bool IsDefaultSpriteName(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
        {
            return false;
        }

        return !System.Text.RegularExpressions.Regex.IsMatch(spriteName, @"\d+$");
    }

    private static string GetFreeResourcePath(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head:
                return FreeHeadsResourcePath;
            case BodyPartType.Neck:
                return FreeNecksResourcePath;
            case BodyPartType.Stomach:
                return FreeStomachsResourcePath;
            case BodyPartType.Leg:
                return FreeLegsResourcePath;
            default:
                return string.Empty;
        }
    }

    private static string GetUnlockableResourcePath(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head:
                return UnlockableHeadsResourcePath;
            case BodyPartType.Neck:
                return UnlockableNecksResourcePath;
            case BodyPartType.Stomach:
                return UnlockableStomachsResourcePath;
            case BodyPartType.Leg:
                return UnlockableLegsResourcePath;
            default:
                return string.Empty;
        }
    }

    private static string GetFreeResourcePath(HumanType humanType, BodyPartType part)
    {
        if (humanType == HumanType.NormalHuman)
        {
            return GetFreeResourcePath(part);
        }

        return $"{GetHumanResourceRoot(humanType)}/Free/{GetPartResourceFolder(part)}";
    }

    private static string GetFlatFreeResourcePath(HumanType humanType)
    {
        return humanType == HumanType.NormalHuman ? $"{NormalHumanResourceRoot}/Free" : $"{GetHumanResourceRoot(humanType)}/Free";
    }

    private static string GetUnlockableResourcePath(HumanType humanType, BodyPartType part)
    {
        if (humanType == HumanType.NormalHuman)
        {
            return GetUnlockableResourcePath(part);
        }

        return $"{GetHumanResourceRoot(humanType)}/Unlockables/{GetPartResourceFolder(part)}";
    }

    private static string GetFlatUnlockableResourcePath(HumanType humanType)
    {
        return humanType == HumanType.NormalHuman ? $"{NormalHumanResourceRoot}/Unlockables" : $"{GetHumanResourceRoot(humanType)}/Unlockables";
    }

    private static string GetHumanResourceRoot(HumanType humanType)
    {
        switch (humanType)
        {
            case HumanType.RockThrowerHuman:
                return RockThrowerHumanResourceRoot;
            case HumanType.BigHuman:
                return BigHumanResourceRoot;
            case HumanType.KnightHuman:
                return KnightHumanResourceRoot;
            case HumanType.RobotHuman:
                return RobotHumanResourceRoot;
            case HumanType.NormalHuman:
                return NormalHumanResourceRoot;
            default:
                return string.Empty;
        }
    }

    private static Sprite[] FilterSpritesByPart(Sprite[] sprites, BodyPartType part)
    {
        if (sprites == null || sprites.Length == 0)
        {
            return System.Array.Empty<Sprite>();
        }

        System.Collections.Generic.List<Sprite> filtered = new System.Collections.Generic.List<Sprite>();
        string token = GetPartNameToken(part);
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite sprite = sprites[i];
            if (sprite != null && sprite.name.ToLowerInvariant().Contains(token))
            {
                filtered.Add(sprite);
            }
        }

        Sprite[] result = filtered.ToArray();
        SortSpritesByDefaultFirst(result);
        return result;
    }

    private static Sprite[] LoadPartSprites(HumanType humanType, BodyPartType part, bool free)
    {
        string partFolderPath = free ? GetFreeResourcePath(humanType, part) : GetUnlockableResourcePath(humanType, part);
        Sprite[] sprites = Resources.LoadAll<Sprite>(partFolderPath);
        if (sprites.Length > 0)
        {
            return sprites;
        }

        string flatFolderPath = free ? GetFlatFreeResourcePath(humanType) : GetFlatUnlockableResourcePath(humanType);
        return FilterSpritesByPart(Resources.LoadAll<Sprite>(flatFolderPath), part);
    }

    private static string GetPartResourceFolder(BodyPartType part)
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
                return string.Empty;
        }
    }

    public static string GetHumanTypeDisplayName(HumanType humanType)
    {
        switch (humanType)
        {
            case HumanType.RockThrowerHuman:
                return "Rock Thrower";
            case HumanType.BigHuman:
                return "Big";
            case HumanType.KnightHuman:
                return "Knight";
            case HumanType.RobotHuman:
                return "Robot";
            default:
                return "Normal Human";
        }
    }

    private static string GetPartNameToken(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head:
                return "head";
            case BodyPartType.Neck:
                return "neck";
            case BodyPartType.Stomach:
                return "stomach";
            case BodyPartType.Leg:
                return "leg";
            default:
                return string.Empty;
        }
    }

    private static Image FindImageByName(string objectName)
    {
        Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == objectName)
            {
                return images[i];
            }
        }

        return null;
    }

    private static Text FindTextByName(string objectName)
    {
        Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == objectName)
            {
                return texts[i];
            }
        }

        return null;
    }

    private static Button FindButtonByName(string objectName)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].name == objectName)
            {
                return buttons[i];
            }
        }

        return null;
    }

    private static Transform FindTransformByName(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && transforms[i].name == objectName)
            {
                return transforms[i];
            }
        }

        return null;
    }

    private static Image FindChildImageByName(GameObject parent, string objectName)
    {
        if (parent == null)
        {
            return null;
        }

        Image[] images = parent.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == objectName)
            {
                return images[i];
            }
        }

        return null;
    }

    private static void SetButtonLabel(Button button, string label)
    {
        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label;
        }
    }

    private static void BindButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        Button button = FindButtonByName(objectName);
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static void SetSprite(Image image, Sprite sprite)
    {
        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
        }
    }
}
