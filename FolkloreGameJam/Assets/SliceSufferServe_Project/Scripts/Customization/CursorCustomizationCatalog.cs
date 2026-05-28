using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CursorCustomizationCatalog", menuName = "Slice Suffer Serve/Cursor Customization Catalog")]
public class CursorCustomizationCatalog : ScriptableObject
{
    private const string DefaultResourcesPath = "CursorCustomizationCatalog";

    [SerializeField] private CursorCustomizationOption[] options = Array.Empty<CursorCustomizationOption>();

    public CursorCustomizationOption[] Options => options ?? Array.Empty<CursorCustomizationOption>();

    public static CursorCustomizationCatalog LoadDefault()
    {
        return Resources.Load<CursorCustomizationCatalog>(DefaultResourcesPath);
    }

    public CursorCustomizationOption GetOption(string cursorId)
    {
        if (string.IsNullOrWhiteSpace(cursorId))
        {
            cursorId = CursorCustomizationSelection.DefaultCursorId;
        }

        CursorCustomizationOption[] cursorOptions = Options;
        for (int i = 0; i < cursorOptions.Length; i++)
        {
            CursorCustomizationOption option = cursorOptions[i];
            if (option != null && string.Equals(option.Id, cursorId, StringComparison.Ordinal))
            {
                return option;
            }
        }

        return GetDefaultOption();
    }

    public CursorCustomizationOption GetDefaultOption()
    {
        CursorCustomizationOption[] cursorOptions = Options;
        for (int i = 0; i < cursorOptions.Length; i++)
        {
            CursorCustomizationOption option = cursorOptions[i];
            if (option != null && string.Equals(option.Id, CursorCustomizationSelection.DefaultCursorId, StringComparison.Ordinal))
            {
                return option;
            }
        }

        return cursorOptions.Length > 0 ? cursorOptions[0] : null;
    }
}

[Serializable]
public class CursorCustomizationOption
{
    public string id;
    public string displayName;
    public Texture2D cursorTexture;

    public string Id => string.IsNullOrWhiteSpace(id) ? CursorCustomizationSelection.DefaultCursorId : id;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? Id : displayName;
}

public static class CursorCustomizationSelection
{
    public const string DefaultCursorId = "knife";

    private const string PlayerPrefsKey = "SelectedWeaponCursorId";

    public static string GetSelectedCursorId()
    {
        string cursorId = PlayerPrefs.GetString(PlayerPrefsKey, DefaultCursorId);
        if (string.IsNullOrWhiteSpace(cursorId) || !CharacterCustomizer.IsWeaponCursorUnlocked(cursorId))
        {
            return DefaultCursorId;
        }

        return cursorId;
    }

    public static void SetSelectedCursorId(string cursorId)
    {
        if (string.IsNullOrWhiteSpace(cursorId) || !CharacterCustomizer.IsWeaponCursorUnlocked(cursorId))
        {
            cursorId = DefaultCursorId;
        }

        PlayerPrefs.SetString(PlayerPrefsKey, cursorId);
        PlayerPrefs.Save();
    }
}
