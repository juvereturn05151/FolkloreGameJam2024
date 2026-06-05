using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    private const string DefaultSequenceResourcePath = "CutsceneSequence";
    private static string nextSceneNameOverride;

    [Header("Cutscene Data")]
    [SerializeField] private CutsceneSequence sequence;
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private bool playOnStart = true;

    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image dialogueBoxImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image fadeImage;

    [Header("Presentation")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float inputCooldown = 0.12f;
    [SerializeField] private Color dialogueBoxColor = new Color(0.04f, 0f, 0f, 0.88f);
    [SerializeField] private Color dialogueTextColor = new Color(0.95f, 0.88f, 0.78f, 1f);
    [SerializeField] private Color fadeColor = Color.black;

    private int cutsceneIndex;
    private int dialogueIndex;
    private bool isPlaying;
    private bool isTransitioning;
    private float nextInputTime;

    private void Awake()
    {
        if (!string.IsNullOrWhiteSpace(nextSceneNameOverride))
        {
            nextSceneName = nextSceneNameOverride;
            nextSceneNameOverride = null;
        }

        sequence ??= Resources.Load<CutsceneSequence>(DefaultSequenceResourcePath);
        EnsureUI();
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying || isTransitioning || Time.unscaledTime < nextInputTime)
        {
            return;
        }

        if (GetAdvanceInput())
        {
            Advance();
        }
    }

    public void Play()
    {
        if (sequence == null || sequence.Count == 0)
        {
            Debug.LogWarning("CutsceneManager needs a CutsceneSequence with at least one cutscene.", this);
            LoadNextScene();
            return;
        }

        isPlaying = true;
        cutsceneIndex = 0;
        dialogueIndex = 0;
        ShowCurrentCutscene();
    }

    public void Advance()
    {
        if (!isPlaying || sequence == null)
        {
            return;
        }

        nextInputTime = Time.unscaledTime + inputCooldown;
        CutsceneEntry current = sequence.GetCutscene(cutsceneIndex);
        dialogueIndex++;

        if (current != null && dialogueIndex < current.DialogueLineCount)
        {
            ShowCurrentDialogueLine();
            return;
        }

        StartCoroutine(GoToNextCutscene());
    }

    public void SkipToEnd()
    {
        if (!isPlaying)
        {
            return;
        }

        StopAllCoroutines();
        LoadNextScene();
    }

    public static void SetNextSceneNameOverride(string sceneName)
    {
        nextSceneNameOverride = sceneName;
    }

    private IEnumerator GoToNextCutscene()
    {
        isTransitioning = true;

        if (useFade)
        {
            yield return Fade(1f);
        }

        cutsceneIndex++;
        dialogueIndex = 0;

        if (cutsceneIndex >= sequence.Count)
        {
            LoadNextScene();
            yield break;
        }

        ShowCurrentCutscene();

        if (useFade)
        {
            yield return Fade(0f);
        }

        isTransitioning = false;
    }

    private void ShowCurrentCutscene()
    {
        CutsceneEntry current = sequence.GetCutscene(cutsceneIndex);
        if (current == null)
        {
            StartCoroutine(GoToNextCutscene());
            return;
        }

        if (backgroundImage != null)
        {
            backgroundImage.sprite = current.Background;
            backgroundImage.enabled = current.Background != null;
            backgroundImage.preserveAspect = true;
        }

        ShowCurrentDialogueLine();

        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;

            if (!useFade)
            {
                SetFadeAlpha(0f);
            }
            else if (cutsceneIndex == 0 && !isTransitioning)
            {
                SetFadeAlpha(1f);
            }
        }

        if (useFade && cutsceneIndex == 0)
        {
            StartCoroutine(FadeInFromStart());
        }
    }

    private IEnumerator FadeInFromStart()
    {
        isTransitioning = true;
        yield return Fade(0f);
        isTransitioning = false;
    }

    private void ShowCurrentDialogueLine()
    {
        CutsceneEntry current = sequence.GetCutscene(cutsceneIndex);
        if (dialogueText == null || current == null)
        {
            return;
        }

        dialogueText.text = current.GetDialogueLine(dialogueIndex);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null || fadeDuration <= 0f)
        {
            SetFadeAlpha(targetAlpha);
            yield break;
        }

        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetFadeAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }

        SetFadeAlpha(targetAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = fadeColor;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void LoadNextScene()
    {
        isPlaying = false;

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("CutsceneManager has no next scene name set.", this);
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private static bool GetAdvanceInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            return true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                return true;
            }
        }

        return false;
    }

    private void EnsureUI()
    {
        if (backgroundImage != null && dialogueText != null && dialogueBoxImage != null && fadeImage != null)
        {
            ApplyUIStyle();
            return;
        }

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("CutsceneCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform canvasRect = canvas.transform as RectTransform;
        backgroundImage ??= CreateImage(canvasRect, "CutsceneBackground", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        dialogueBoxImage ??= CreateImage(canvasRect, "DialogueBox", new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.28f), Vector2.zero, Vector2.zero);
        dialogueText ??= CreateDialogueText(dialogueBoxImage.transform as RectTransform);
        fadeImage ??= CreateImage(canvasRect, "FadeImage", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        ApplyUIStyle();
    }

    private void ApplyUIStyle()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
            backgroundImage.type = Image.Type.Simple;
            backgroundImage.preserveAspect = true;
            backgroundImage.raycastTarget = false;
        }

        if (dialogueBoxImage != null)
        {
            dialogueBoxImage.color = dialogueBoxColor;
            dialogueBoxImage.raycastTarget = false;
        }

        if (dialogueText != null)
        {
            dialogueText.color = dialogueTextColor;
            dialogueText.fontSize = 42f;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            dialogueText.enableWordWrapping = true;
            dialogueText.raycastTarget = false;
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.raycastTarget = false;
            fadeImage.transform.SetAsLastSibling();
        }
    }

    private static Image CreateImage(RectTransform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;

        return imageObject.GetComponent<Image>();
    }

    private static TextMeshProUGUI CreateDialogueText(RectTransform parent)
    {
        GameObject textObject = new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(42f, 28f);
        rectTransform.offsetMax = new Vector2(-42f, -28f);

        return textObject.GetComponent<TextMeshProUGUI>();
    }
}
