using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CharacterCustomizationTogglePrefabUtility
{
    private const string CanvasPrefabPath = "Assets/SliceSufferServe_Project/Prefabs/UI/CharacterCustomizationCanvas.prefab";
    private const string TogglePrefabPath = "Assets/SliceSufferServe_Project/Prefabs/UI/CharacterCustomization/GeneratedSlotToggle.prefab";
    private const string CheckmarkPrefabPath = "Assets/SliceSufferServe_Project/Prefabs/UI/CharacterCustomization/GeneratedSlotToggleCheckmark.prefab";
    private const string AppliedSessionKey = "SliceSufferServe.GeneratedSlotTogglePrefabUtility.Applied";

    [InitializeOnLoadMethod]
    private static void ApplyWhenUnityImports()
    {
        if (SessionState.GetBool(AppliedSessionKey, false))
        {
            return;
        }

        SessionState.SetBool(AppliedSessionKey, true);
        EditorApplication.delayCall += ApplyGeneratedSlotTogglePrefab;
    }

    [MenuItem("Slice Suffer Serve/Character Customization/Apply Generated Slot Toggle Prefab")]
    public static void ApplyGeneratedSlotTogglePrefab()
    {
        GameObject canvasRoot = PrefabUtility.LoadPrefabContents(CanvasPrefabPath);
        GameObject togglePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TogglePrefabPath);

        if (canvasRoot == null || togglePrefab == null)
        {
            Debug.LogError("Could not load Character Customization canvas or Generated Slot Toggle prefab.");
            if (canvasRoot != null)
            {
                PrefabUtility.UnloadPrefabContents(canvasRoot);
            }

            return;
        }

        try
        {
            for (int i = 1; i <= CharacterCustomizationManager.GeneratedSlotsPerHuman; i++)
            {
                ReplaceToggle(canvasRoot.transform, togglePrefab, $"GeneratedSlot{i}Toggle");
            }

            PrefabUtility.SaveAsPrefabAsset(canvasRoot, CanvasPrefabPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Generated slot toggles now use the shared GeneratedSlotToggle prefab.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(canvasRoot);
        }
    }

    [MenuItem("Slice Suffer Serve/Character Customization/Open Generated Slot Toggle Prefab")]
    public static void OpenGeneratedSlotTogglePrefab()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(TogglePrefabPath);
        AssetDatabase.OpenAsset(Selection.activeObject);
    }

    [MenuItem("Slice Suffer Serve/Character Customization/Open Generated Slot Checkmark Prefab")]
    public static void OpenGeneratedSlotCheckmarkPrefab()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(CheckmarkPrefabPath);
        AssetDatabase.OpenAsset(Selection.activeObject);
    }

    private static void ReplaceToggle(Transform root, GameObject togglePrefab, string toggleName)
    {
        Transform oldToggle = FindDeepChild(root, toggleName);
        if (oldToggle == null)
        {
            Debug.LogWarning($"Could not find {toggleName} in CharacterCustomizationCanvas.");
            return;
        }

        RectTransform oldRect = oldToggle as RectTransform;
        Toggle oldToggleComponent = oldToggle.GetComponent<Toggle>();
        Transform parent = oldToggle.parent;
        int siblingIndex = oldToggle.GetSiblingIndex();
        bool wasActive = oldToggle.gameObject.activeSelf;
        bool wasOn = oldToggleComponent == null || oldToggleComponent.isOn;

        RectTransformSnapshot snapshot = new RectTransformSnapshot(oldRect);

        GameObject newToggleObject = (GameObject)PrefabUtility.InstantiatePrefab(togglePrefab, parent);
        newToggleObject.name = toggleName;
        newToggleObject.SetActive(wasActive);
        newToggleObject.transform.SetSiblingIndex(siblingIndex);

        RectTransform newRect = newToggleObject.transform as RectTransform;
        snapshot.ApplyTo(newRect);

        Toggle newToggle = newToggleObject.GetComponent<Toggle>();
        if (newToggle != null)
        {
            newToggle.SetIsOnWithoutNotify(wasOn);
        }

        Object.DestroyImmediate(oldToggle.gameObject);
    }

    private static Transform FindDeepChild(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == objectName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindDeepChild(root.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private readonly struct RectTransformSnapshot
    {
        private readonly Vector2 anchorMin;
        private readonly Vector2 anchorMax;
        private readonly Vector2 anchoredPosition;
        private readonly Vector2 sizeDelta;
        private readonly Vector2 pivot;
        private readonly Vector3 localScale;
        private readonly Quaternion localRotation;

        public RectTransformSnapshot(RectTransform source)
        {
            anchorMin = source != null ? source.anchorMin : Vector2.zero;
            anchorMax = source != null ? source.anchorMax : Vector2.zero;
            anchoredPosition = source != null ? source.anchoredPosition : Vector2.zero;
            sizeDelta = source != null ? source.sizeDelta : Vector2.zero;
            pivot = source != null ? source.pivot : Vector2.zero;
            localScale = source != null ? source.localScale : Vector3.one;
            localRotation = source != null ? source.localRotation : Quaternion.identity;
        }

        public void ApplyTo(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.anchoredPosition = anchoredPosition;
            target.sizeDelta = sizeDelta;
            target.pivot = pivot;
            target.localScale = localScale;
            target.localRotation = localRotation;
        }
    }
}
