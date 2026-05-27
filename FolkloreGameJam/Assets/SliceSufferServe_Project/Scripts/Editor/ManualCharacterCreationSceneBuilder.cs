#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ManualCharacterCreationSceneBuilder
{
    private const string ScenePath = "Assets/SliceSufferServe_Project/Scenes/Actual/ManualCharacterCustomizationScene.unity";

    [InitializeOnLoadMethod]
    private static void RebuildMissingUiInOpenManualScene()
    {
        EditorSceneManager.sceneOpened -= RebuildMissingUiIfNeeded;
        EditorSceneManager.sceneOpened += RebuildMissingUiIfNeeded;
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying)
            {
                return;
            }

            RebuildMissingUiIfNeeded(SceneManager.GetActiveScene(), OpenSceneMode.Single);
        };
    }

    private static void RebuildMissingUiIfNeeded(Scene scene, OpenSceneMode mode)
    {
        if (Application.isPlaying || scene.path != ScenePath || GameObject.Find("ManualCharacterCreationCanvas") != null)
        {
            return;
        }

        RebuildManualCharacterCreationUI();
    }

    [MenuItem("Tools/Slice Suffer Serve/Rebuild Manual Character Creation UI")]
    public static void RebuildManualCharacterCreationUI()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        CharacterCustomizer customizer = Object.FindFirstObjectByType<CharacterCustomizer>();

        if (customizer == null)
        {
            GameObject systems = GameObject.Find("CharacterCustomizationSystems") ?? new GameObject("CharacterCustomizationSystems");
            customizer = systems.AddComponent<CharacterCustomizer>();
        }

        RemoveExistingManualUi();

        Canvas canvas = CreateCanvas();
        RectTransform root = CreateRect("ManualCharacterCreationUI", canvas.transform);
        Stretch(root);

        CreateImage("Background", root, Vector2.zero, Vector2.zero, new Color(0.05f, 0.045f, 0.04f, 1f), stretch: true);
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        CreateText("Title", root, "Normal Human", font, new Vector2(0f, 450f), new Vector2(760f, 64f), 34);
        CreateText("Subtitle", root, "Manual Character Creation", font, new Vector2(0f, 404f), new Vector2(760f, 42f), 20);

        RectTransform previewPanel = CreateImage("ManualPreview", root, new Vector2(-420f, -20f), new Vector2(520f, 760f), new Color(0.11f, 0.10f, 0.09f, 0.95f));
        CreateText("PreviewLabel", root, "Preview", font, new Vector2(-420f, 380f), new Vector2(520f, 40f), 26);

        Image legImage = CreatePartImage("LegPreviewImage", previewPanel);
        Image stomachImage = CreatePartImage("StomachPreviewImage", previewPanel);
        Image neckImage = CreatePartImage("NeckPreviewImage", previewPanel);
        Image headImage = CreatePartImage("HeadPreviewImage", previewPanel);

        RectTransform controls = CreateImage("ManualCustomizationControls", root, new Vector2(520f, 0f), new Vector2(380f, 520f), new Color(0.08f, 0.07f, 0.06f, 0.90f));
        CreateText("ControlsTitle", controls, "Choose Parts", font, new Vector2(0f, 220f), new Vector2(320f, 42f), 24);

        CreatePartRow(controls, font, "Head", new Vector2(0f, 142f), customizer.PreviousHead, customizer.NextHead);
        CreatePartRow(controls, font, "Neck", new Vector2(0f, 66f), customizer.PreviousNeck, customizer.NextNeck);
        CreatePartRow(controls, font, "Stomach", new Vector2(0f, -10f), customizer.PreviousStomach, customizer.NextStomach);
        CreatePartRow(controls, font, "Leg", new Vector2(0f, -86f), customizer.PreviousLegs, customizer.NextLegs);

        Button defaultButton = CreateButton("DefaultButton", controls, "Default", font, new Vector2(-96f, -206f), new Vector2(150f, 48f), new Color(0.24f, 0.22f, 0.18f, 1f));
        UnityEventTools.AddPersistentListener(defaultButton.onClick, customizer.SelectDefaultCharacter);

        Button saveButton = CreateButton("SaveButton", controls, "Save", font, new Vector2(96f, -206f), new Vector2(150f, 48f), new Color(0.74f, 0.22f, 0.16f, 1f));
        UnityEventTools.AddPersistentListener(saveButton.onClick, customizer.SaveCustomization);

        Text statusText = CreateText("SelectedHumanLabel", root, "Head 1/5 | Neck 1/5 | Stomach 1/5 | Leg 1/5", font, new Vector2(-420f, 34f), new Vector2(760f, 42f), 20);

        SerializedObject serializedCustomizer = new SerializedObject(customizer);
        serializedCustomizer.FindProperty("createDefaultManualControls").boolValue = false;
        serializedCustomizer.FindProperty("allPartsUnlockedForTesting").boolValue = true;
        serializedCustomizer.FindProperty("manualControlsRoot").objectReferenceValue = controls;
        serializedCustomizer.FindProperty("statusText").objectReferenceValue = statusText;
        serializedCustomizer.FindProperty("headImage").objectReferenceValue = headImage;
        serializedCustomizer.FindProperty("neckImage").objectReferenceValue = neckImage;
        serializedCustomizer.FindProperty("bodyImage").objectReferenceValue = stomachImage;
        serializedCustomizer.FindProperty("legsImage").objectReferenceValue = legImage;
        serializedCustomizer.ApplyModifiedPropertiesWithoutUndo();

        EnsureEventSystem();

        EditorUtility.SetDirty(customizer);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Rebuilt manual character creation UI in scene.");
    }

    private static void RemoveExistingManualUi()
    {
        foreach (string name in new[] { "ManualCharacterCreationCanvas", "ManualCharacterCustomizationCanvas", "ManualCharacterCreationUI", "ManualCharacterCustomizationUI" })
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("ManualCharacterCreationCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<RectTransform>();
    }

    private static RectTransform CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Color color, bool stretch = false)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();

        if (stretch)
        {
            Stretch(rect);
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        gameObject.GetComponent<Image>().color = color;
        return rect;
    }

    private static Image CreatePartImage(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(420f, 746f);

        Image image = gameObject.GetComponent<Image>();
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(string name, Transform parent, string value, Font font, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = gameObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.95f, 0.91f, 0.82f, 1f);
        return text;
    }

    private static void CreatePartRow(RectTransform parent, Font font, string label, Vector2 center, UnityEngine.Events.UnityAction previousAction, UnityEngine.Events.UnityAction nextAction)
    {
        CreateText(label + "Label", parent, label, font, center + new Vector2(0f, 20f), new Vector2(300f, 30f), 18);
        Button previous = CreateButton(label + "PreviousButton", parent, "<", font, center + new Vector2(-88f, -14f), new Vector2(64f, 38f), new Color(0.24f, 0.22f, 0.18f, 1f));
        UnityEventTools.AddPersistentListener(previous.onClick, previousAction);
        Button next = CreateButton(label + "NextButton", parent, ">", font, center + new Vector2(88f, -14f), new Vector2(64f, 38f), new Color(0.24f, 0.22f, 0.18f, 1f));
        UnityEventTools.AddPersistentListener(next.onClick, nextAction);
    }

    private static Button CreateButton(string name, Transform parent, string label, Font font, Vector2 position, Vector2 size, Color color)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        buttonObject.GetComponent<Image>().color = color;

        Text text = CreateText("Text", buttonObject.transform, label, font, Vector2.zero, size, 20);
        Stretch(text.GetComponent<RectTransform>());
        text.color = Color.white;
        return buttonObject.GetComponent<Button>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
#endif
