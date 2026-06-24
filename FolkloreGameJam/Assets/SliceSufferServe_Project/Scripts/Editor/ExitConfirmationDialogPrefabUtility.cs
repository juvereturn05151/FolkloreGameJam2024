#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ExitConfirmationDialogPrefabUtility
{
    private const string PrefabPath = "Assets/SliceSufferServe_Project/Resources/ExitConfirmationDialog.prefab";
    private const string AutoCreateSessionKey = "SliceSufferServe.ExitConfirmationDialogPrefabUtility.AutoCreated";

    static ExitConfirmationDialogPrefabUtility()
    {
        EditorApplication.delayCall += AutoCreatePrefabIfMissing;
    }

    [MenuItem("Slice Suffer Serve/UI/Create Exit Confirmation Dialog Prefab")]
    public static void CreatePrefab()
    {
        GameObject dialogObject = CreateDialog();
        try
        {
            PrefabUtility.SaveAsPrefabAsset(dialogObject, PrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Created exit confirmation dialog prefab at {PrefabPath}.");
        }
        finally
        {
            Object.DestroyImmediate(dialogObject);
        }
    }

    private static void AutoCreatePrefabIfMissing()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(AutoCreateSessionKey, false))
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
        {
            return;
        }

        SessionState.SetBool(AutoCreateSessionKey, true);
        CreatePrefab();
    }

    private static GameObject CreateDialog()
    {
        GameObject dialogObject = new GameObject(
            "ExitConfirmationDialog",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(ExitConfirmationDialog));

        RectTransform dialogRect = dialogObject.GetComponent<RectTransform>();
        dialogRect.anchorMin = Vector2.zero;
        dialogRect.anchorMax = Vector2.one;
        dialogRect.offsetMin = Vector2.zero;
        dialogRect.offsetMax = Vector2.zero;

        Canvas canvas = dialogObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        CanvasScaler scaler = dialogObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateImage(dialogObject.transform, "Overlay", new Color(0f, 0f, 0f, 0.68f), Vector2.zero, Vector2.zero, true);

        GameObject panel = CreateImage(dialogObject.transform, "Panel", new Color(0.08f, 0.07f, 0.06f, 0.97f), Vector2.zero, new Vector2(760f, 360f), false).gameObject;
        CreateText(panel.transform, "Title", "Exit Game", new Vector2(0f, 105f), new Vector2(640f, 60f), 46, TextAnchor.MiddleCenter);
        CreateText(panel.transform, "Message", "Are you sure you want to exit?", new Vector2(0f, 25f), new Vector2(640f, 78f), 34, TextAnchor.MiddleCenter);

        Button cancelButton = CreateButton(panel.transform, "NoButton", "No", new Vector2(-155f, -105f), new Vector2(220f, 70f), new Color(0.23f, 0.21f, 0.18f, 1f));
        Button confirmButton = CreateButton(panel.transform, "YesButton", "Yes", new Vector2(155f, -105f), new Vector2(220f, 70f), new Color(0.72f, 0.18f, 0.13f, 1f));

        SerializedObject serializedDialog = new SerializedObject(dialogObject.GetComponent<ExitConfirmationDialog>());
        serializedDialog.FindProperty("confirmButton").objectReferenceValue = confirmButton;
        serializedDialog.FindProperty("cancelButton").objectReferenceValue = cancelButton;
        serializedDialog.ApplyModifiedPropertiesWithoutUndo();

        return dialogObject;
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size, Color color)
    {
        Image image = CreateImage(parent, objectName, color, position, size, false);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        CreateText(image.transform, "Text", label, Vector2.zero, size, 32, TextAnchor.MiddleCenter);
        return button;
    }

    private static Text CreateText(Transform parent, string objectName, string text, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Text textComponent = textObject.GetComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = Color.white;
        textComponent.raycastTarget = false;

        return textComponent;
    }

    private static Image CreateImage(Transform parent, string objectName, Color color, Vector2 position, Vector2 size, bool stretch)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        if (stretch)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
        }

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }
}
#endif
