using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    private const string DefaultSequenceResourcePath = "CutsceneSequence";
    private static string nextSceneNameOverride;
    private static int? startCutsceneIndexOverride;
    private static int? cutsceneCountOverride;

    [Header("Cutscene Data")]
    [SerializeField] private CutsceneSequence sequence;
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private int startCutsceneIndex;
    [SerializeField] private int cutsceneCount = 5;
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

        if (startCutsceneIndexOverride.HasValue)
        {
            startCutsceneIndex = startCutsceneIndexOverride.Value;
            startCutsceneIndexOverride = null;
        }

        if (cutsceneCountOverride.HasValue)
        {
            cutsceneCount = cutsceneCountOverride.Value;
            cutsceneCountOverride = null;
        }

        sequence ??= Resources.Load<CutsceneSequence>(DefaultSequenceResourcePath);
        ValidateUIReferences();
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
        cutsceneIndex = GetPlaybackStartIndex();
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

    public static void SetPlaybackOverride(string sceneName, int startIndex, int count)
    {
        nextSceneNameOverride = sceneName;
        startCutsceneIndexOverride = startIndex;
        cutsceneCountOverride = count;
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

        if (cutsceneIndex >= GetPlaybackEndIndex())
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
            else if (cutsceneIndex == GetPlaybackStartIndex() && !isTransitioning)
            {
                SetFadeAlpha(1f);
            }
        }

        if (useFade && cutsceneIndex == GetPlaybackStartIndex())
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

    private int GetPlaybackStartIndex()
    {
        if (sequence == null || sequence.Count == 0)
        {
            return 0;
        }

        return Mathf.Clamp(startCutsceneIndex, 0, sequence.Count - 1);
    }

    private int GetPlaybackEndIndex()
    {
        if (sequence == null || sequence.Count == 0)
        {
            return 0;
        }

        int startIndex = GetPlaybackStartIndex();
        int safeCount = cutsceneCount <= 0 ? sequence.Count - startIndex : cutsceneCount;
        return Mathf.Clamp(startIndex + safeCount, startIndex + 1, sequence.Count);
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

    private void ValidateUIReferences()
    {
        if (backgroundImage == null || dialogueBoxImage == null || dialogueText == null || fadeImage == null)
        {
            Debug.LogWarning("CutsceneManager needs scene UI references assigned: Background Image, Dialogue Box Image, Dialogue Text, and Fade Image.", this);
        }
    }
}
