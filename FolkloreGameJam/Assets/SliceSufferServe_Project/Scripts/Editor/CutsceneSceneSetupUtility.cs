using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class CutsceneSceneSetupUtility
{
    private const string ScenePath = "Assets/SliceSufferServe_Project/Scenes/Cinematic.unity";
    private const string SequencePath = "Assets/SliceSufferServe_Project/Resources/CutsceneSequence.asset";

    [MenuItem("Tools/Slice Suffer Serve/Cutscenes/Create Cinematic UI")]
    public static void CreateAndAssignCutsceneUI()
    {
        EditorSceneManager.OpenScene(ScenePath);

        CutsceneManager manager = Object.FindFirstObjectByType<CutsceneManager>();
        if (manager == null)
        {
            GameObject managerObject = new GameObject("CutsceneManager");
            manager = managerObject.AddComponent<CutsceneManager>();
        }

        Transform existingCanvas = manager.transform.Find("CutsceneCanvas");
        if (existingCanvas != null)
        {
            Object.DestroyImmediate(existingCanvas.gameObject);
        }

        GameObject canvasObject = new GameObject("CutsceneCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(manager.transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        Stretch(canvasRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Image backgroundImage = CreateImage(canvasRect, "CutsceneBackground", Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        backgroundImage.preserveAspect = true;

        Image dialogueBoxImage = CreateImage(canvasRect, "DialogueBox", new Color(0.04f, 0f, 0f, 0.88f), new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.28f), Vector2.zero, Vector2.zero);
        TextMeshProUGUI dialogueText = CreateDialogueText(dialogueBoxImage.transform as RectTransform);

        Image fadeImage = CreateImage(canvasRect, "FadeImage", new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        fadeImage.transform.SetAsLastSibling();

        EnsureEventSystem();
        AssignManagerReferences(manager, backgroundImage, dialogueBoxImage, dialogueText, fadeImage);

        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        EditorSceneManager.SaveScene(manager.gameObject.scene);
        AssetDatabase.SaveAssets();
    }

    private static Image CreateImage(RectTransform parent, string objectName, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        Stretch(rectTransform, anchorMin, anchorMax, offsetMin, offsetMax);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.type = Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateDialogueText(RectTransform parent)
    {
        GameObject textObject = new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        Stretch(rectTransform, Vector2.zero, Vector2.one, new Vector2(42f, 28f), new Vector2(-42f, -28f));

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = string.Empty;
        text.color = new Color(0.95f, 0.88f, 0.78f, 1f);
        text.fontSize = 42f;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition3D = Vector3.zero;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static void AssignManagerReferences(CutsceneManager manager, Image backgroundImage, Image dialogueBoxImage, TextMeshProUGUI dialogueText, Image fadeImage)
    {
        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("sequence").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CutsceneSequence>(SequencePath);
        serializedManager.FindProperty("nextSceneName").stringValue = "MainMenu";
        serializedManager.FindProperty("startCutsceneIndex").intValue = 0;
        serializedManager.FindProperty("cutsceneCount").intValue = 5;
        serializedManager.FindProperty("playOnStart").boolValue = true;
        serializedManager.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
        serializedManager.FindProperty("dialogueBoxImage").objectReferenceValue = dialogueBoxImage;
        serializedManager.FindProperty("dialogueText").objectReferenceValue = dialogueText;
        serializedManager.FindProperty("fadeImage").objectReferenceValue = fadeImage;
        serializedManager.FindProperty("useFade").boolValue = true;
        serializedManager.FindProperty("fadeDuration").floatValue = 0.35f;
        serializedManager.FindProperty("inputCooldown").floatValue = 0.12f;
        serializedManager.FindProperty("fadeColor").colorValue = Color.black;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
    }
}
