#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class WeaponCursorOptionButtonUtility
{
    private const string CanvasPrefabPath = "Assets/SliceSufferServe_Project/Prefabs/UI/CharacterCreationCanvas.prefab";
    private const string CatalogPath = "Assets/SliceSufferServe_Project/Resources/CursorCustomizationCatalog.asset";
    private const string AutoApplySessionKey = "SliceSufferServe.WeaponCursorOptionButtonUtility.Applied";

    static WeaponCursorOptionButtonUtility()
    {
        EditorApplication.delayCall += AutoApplyIfNeeded;
    }

    [MenuItem("Slice Suffer Serve/Character Customization/Apply Weapon Cursor Option Images")]
    public static void ApplyWeaponCursorOptionImages()
    {
        CursorCustomizationCatalog catalog = AssetDatabase.LoadAssetAtPath<CursorCustomizationCatalog>(CatalogPath);
        if (catalog == null)
        {
            Debug.LogWarning("Could not find CursorCustomizationCatalog.asset.");
            return;
        }

        GameObject canvasPrefab = PrefabUtility.LoadPrefabContents(CanvasPrefabPath);
        if (canvasPrefab == null)
        {
            Debug.LogWarning("Could not load CharacterCreationCanvas.prefab.");
            return;
        }

        try
        {
            foreach (CursorCustomizationOption option in catalog.Options)
            {
                if (option == null)
                {
                    continue;
                }

                Button button = FindDescendant<Button>(canvasPrefab.transform, $"{option.Id}CursorOptionButton");
                if (button == null)
                {
                    continue;
                }

                RawImage preview = EnsurePreviewImage(button.transform);
                preview.texture = option.cursorTexture;
                preview.color = Color.white;
            }

            PrefabUtility.SaveAsPrefabAsset(canvasPrefab, CanvasPrefabPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Weapon cursor option buttons now have preview images.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(canvasPrefab);
        }
    }

    private static void AutoApplyIfNeeded()
    {
        if (SessionState.GetBool(AutoApplySessionKey, false) || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CanvasPrefabPath);
        if (canvasPrefab == null)
        {
            return;
        }

        Button knifeButton = FindDescendant<Button>(canvasPrefab.transform, "knifeCursorOptionButton");
        if (knifeButton != null && FindPreviewImage(knifeButton.transform) != null)
        {
            return;
        }

        SessionState.SetBool(AutoApplySessionKey, true);
        ApplyWeaponCursorOptionImages();
    }

    private static RawImage EnsurePreviewImage(Transform buttonTransform)
    {
        RawImage preview = FindPreviewImage(buttonTransform);
        if (preview != null)
        {
            return preview;
        }

        GameObject previewObject = new GameObject("CursorPreviewImage", typeof(RectTransform), typeof(RawImage));
        previewObject.transform.SetParent(buttonTransform, false);

        RectTransform rectTransform = previewObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 20f);
        rectTransform.sizeDelta = new Vector2(92f, 54f);

        preview = previewObject.GetComponent<RawImage>();
        preview.raycastTarget = false;
        return preview;
    }

    private static RawImage FindPreviewImage(Transform parent)
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

    private static T FindDescendant<T>(Transform parent, string objectName) where T : Component
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == objectName)
        {
            return parent.GetComponent<T>();
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            T result = FindDescendant<T>(parent.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
#endif
