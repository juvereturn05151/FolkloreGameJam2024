using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private enum SelectMode
    {
        Level,
        Tutorial,
        OpeningCutscene,
        PostStageCutscene
    }

    [SerializeField] private StageSelectManager stageSelectManager;
    [SerializeField] private int levelIndex;
    [SerializeField] private Button button;
    [SerializeField] private Image previewImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text starsText;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private float cutSelectDistance = 80f;
    [SerializeField] private float cutEffectDuration = 0.25f;
    [SerializeField] private float cutEffectSeparation = 80f;
    [SerializeField] private float cutEffectRotation = 9f;

    private SelectMode selectMode;
    private bool isUnlocked;
    private bool isTrackingCut;
    private bool cutSelectionTriggered;
    private Vector2 cutStartScreenPosition;
    private Vector2 lastScreenPosition;
    private bool isPlayingCutEffect;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        EnsurePreviewImage();

        Text[] texts = GetComponentsInChildren<Text>(true);
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].font == null)
            {
                texts[i].font = defaultFont;
            }
        }
    }

    private void OnDisable()
    {
        StopTrackingCut();
    }

    private void Update()
    {
        if (!isTrackingCut)
        {
            return;
        }

        if (!Input.GetMouseButton(0))
        {
            StopTrackingCut();
            return;
        }

        UpdateCutSelection(Input.mousePosition);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        BeginTrackingCut(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopTrackingCut();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData == null || !Input.GetMouseButton(0))
        {
            return;
        }

        BeginTrackingCut(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData != null)
        {
            UpdateCutSelection(eventData.position);
        }
    }

    public void Configure(StageSelectManager manager, int index, StageLevelConfig levelConfig)
    {
        stageSelectManager = manager;
        levelIndex = index;
        selectMode = SelectMode.Level;
        isUnlocked = StageUnlockSystem.IsLevelUnlocked(GetLevelDatabase(manager), index);

        if (titleText != null && levelConfig != null)
        {
            titleText.text = levelConfig.DisplayName;
        }

        if (starsText != null && levelConfig != null)
        {
            int stars = levelConfig.StageGoal == null
                ? GetSavedStars(levelConfig.LevelId)
                : levelConfig.StageGoal.GetBestStars();
            starsText.text = isUnlocked ? $"{stars} / 3" : string.Empty;
        }

        SetButtonState();
        Sprite previewSprite = isUnlocked && levelConfig != null ? levelConfig.StoryModeSelectionSprite : lockSprite;
        SetPreviewSprite(previewSprite);
    }

    public void ConfigureTutorial(StageSelectManager manager, int targetLevelIndex, string tutorialName, StageLevelDatabase levelDatabase)
    {
        stageSelectManager = manager;
        levelIndex = targetLevelIndex;
        selectMode = SelectMode.Tutorial;
        isUnlocked = StageUnlockSystem.IsTutorialUnlocked(levelDatabase, targetLevelIndex);

        if (titleText != null)
        {
            titleText.text = tutorialName;
        }

        if (starsText != null)
        {
            starsText.text = string.Empty;
        }

        SetButtonState();
        SetPreviewSprite(isUnlocked ? null : lockSprite);
    }

    public void ConfigureCutscene(StageSelectManager manager, int targetLevelIndex, string cutsceneName, StageLevelDatabase levelDatabase, Sprite previewSprite)
    {
        stageSelectManager = manager;
        levelIndex = targetLevelIndex;
        selectMode = SelectMode.OpeningCutscene;
        isUnlocked = StageUnlockSystem.IsTutorialUnlocked(levelDatabase, targetLevelIndex);

        if (titleText != null)
        {
            titleText.text = cutsceneName;
        }

        if (starsText != null)
        {
            starsText.text = string.Empty;
        }

        SetButtonState();
        SetPreviewSprite(isUnlocked ? previewSprite : lockSprite);
    }

    public void ConfigurePostStageCutscene(StageSelectManager manager, int completedLevelIndex, string cutsceneName, StageLevelDatabase levelDatabase, Sprite previewSprite)
    {
        stageSelectManager = manager;
        levelIndex = completedLevelIndex;
        selectMode = SelectMode.PostStageCutscene;
        StageLevelConfig completedLevel = levelDatabase != null ? levelDatabase.GetLevel(completedLevelIndex) : null;
        isUnlocked = StageUnlockSystem.IsLevelCompleted(completedLevel);

        if (titleText != null)
        {
            titleText.text = cutsceneName;
        }

        if (starsText != null)
        {
            starsText.text = string.Empty;
        }

        SetButtonState();
        SetPreviewSprite(isUnlocked ? previewSprite : lockSprite);
    }

    private void EnsurePreviewImage()
    {
        if (previewImage != null)
        {
            previewImage.raycastTarget = false;
            previewImage.preserveAspect = true;
            return;
        }

        Transform existingPreview = transform.Find("LevelPreviewImage");
        if (existingPreview != null)
        {
            previewImage = existingPreview.GetComponent<Image>();
        }

        if (previewImage == null)
        {
            GameObject previewObject = new GameObject("LevelPreviewImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewObject.transform.SetParent(transform, false);
            previewObject.transform.SetAsFirstSibling();

            RectTransform previewRect = previewObject.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.anchoredPosition = new Vector2(0f, 10f);
            previewRect.sizeDelta = new Vector2(220f, 120f);

            previewImage = previewObject.GetComponent<Image>();
        }

        previewImage.raycastTarget = false;
        previewImage.preserveAspect = true;
        previewImage.gameObject.SetActive(false);
    }

    private void SetPreviewSprite(Sprite sprite)
    {
        EnsurePreviewImage();

        if (previewImage == null)
        {
            return;
        }

        previewImage.sprite = sprite;
        previewImage.enabled = sprite != null;
        previewImage.gameObject.SetActive(sprite != null);
    }

    private void SetButtonState()
    {
        if (button != null)
        {
            button.interactable = isUnlocked;
        }
    }

    private void SelectLevel()
    {
        if (stageSelectManager == null || !isUnlocked)
        {
            return;
        }

        if (selectMode == SelectMode.Tutorial)
        {
            stageSelectManager.SelectTutorialForLevel(levelIndex);
            return;
        }

        if (selectMode == SelectMode.OpeningCutscene)
        {
            stageSelectManager.SelectCutsceneBeforeTutorial(levelIndex);
            return;
        }

        if (selectMode == SelectMode.PostStageCutscene)
        {
            stageSelectManager.SelectCutsceneAfterLevel(levelIndex);
            return;
        }

        stageSelectManager.SelectLevel(levelIndex);
    }

    private void BeginTrackingCut(Vector2 screenPosition)
    {
        if (!isUnlocked || stageSelectManager == null)
        {
            return;
        }

        isTrackingCut = true;
        cutSelectionTriggered = false;
        cutStartScreenPosition = screenPosition;
        lastScreenPosition = screenPosition;
    }

    private void UpdateCutSelection(Vector2 screenPosition)
    {
        if (!isTrackingCut || cutSelectionTriggered)
        {
            return;
        }

        float distanceFromStart = Vector2.Distance(cutStartScreenPosition, screenPosition);
        float distanceFromLastPosition = Vector2.Distance(lastScreenPosition, screenPosition);
        lastScreenPosition = screenPosition;

        if (distanceFromStart < cutSelectDistance && distanceFromLastPosition < cutSelectDistance)
        {
            return;
        }

        cutSelectionTriggered = true;
        StopTrackingCut();
        StartCoroutine(PlayCutEffectThenSelect());
    }

    private void StopTrackingCut()
    {
        isTrackingCut = false;
        cutSelectionTriggered = false;
    }

    private IEnumerator PlayCutEffectThenSelect()
    {
        if (isPlayingCutEffect)
        {
            yield break;
        }

        isPlayingCutEffect = true;

        RectTransform sourceRect = transform as RectTransform;
        if (sourceRect == null || sourceRect.rect.width <= 0f || sourceRect.rect.height <= 0f)
        {
            SelectLevel();
            yield break;
        }

        Transform originalParent = transform.parent;
        int originalSiblingIndex = transform.GetSiblingIndex();
        CanvasGroup originalGroup = GetOrAddComponent<CanvasGroup>(gameObject);
        originalGroup.alpha = 0f;
        if (button != null)
        {
            button.interactable = false;
        }

        RectTransform leftPiece = CreateCutPiece("LeftCutPiece", sourceRect, originalParent, originalSiblingIndex, true);
        RectTransform rightPiece = CreateCutPiece("RightCutPiece", sourceRect, originalParent, originalSiblingIndex + 1, false);
        CanvasGroup leftGroup = leftPiece.GetComponent<CanvasGroup>();
        CanvasGroup rightGroup = rightPiece.GetComponent<CanvasGroup>();

        Vector2 leftStart = leftPiece.anchoredPosition;
        Vector2 rightStart = rightPiece.anchoredPosition;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, cutEffectDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);

            leftPiece.anchoredPosition = leftStart + new Vector2(-cutEffectSeparation, -cutEffectSeparation * 0.35f) * eased;
            rightPiece.anchoredPosition = rightStart + new Vector2(cutEffectSeparation, cutEffectSeparation * 0.35f) * eased;
            leftPiece.localRotation = Quaternion.Euler(0f, 0f, cutEffectRotation * eased);
            rightPiece.localRotation = Quaternion.Euler(0f, 0f, -cutEffectRotation * eased);

            float alpha = 1f - t;
            leftGroup.alpha = alpha;
            rightGroup.alpha = alpha;
            yield return null;
        }

        Destroy(leftPiece.gameObject);
        Destroy(rightPiece.gameObject);
        SelectLevel();
    }

    private RectTransform CreateCutPiece(string pieceName, RectTransform sourceRect, Transform parent, int siblingIndex, bool leftSide)
    {
        GameObject maskObject = new GameObject(pieceName, typeof(RectTransform), typeof(RectMask2D), typeof(CanvasGroup));
        maskObject.transform.SetParent(parent, false);
        maskObject.transform.SetSiblingIndex(siblingIndex);

        RectTransform maskRect = maskObject.GetComponent<RectTransform>();
        maskRect.anchorMin = sourceRect.anchorMin;
        maskRect.anchorMax = sourceRect.anchorMax;
        maskRect.pivot = sourceRect.pivot;
        maskRect.anchoredPosition = sourceRect.anchoredPosition + new Vector2(leftSide ? -sourceRect.rect.width * 0.25f : sourceRect.rect.width * 0.25f, 0f);
        maskRect.sizeDelta = new Vector2(sourceRect.rect.width * 0.5f, sourceRect.rect.height);
        maskRect.localScale = sourceRect.localScale;

        GameObject clone = Instantiate(gameObject, maskObject.transform);
        clone.name = leftSide ? "LeftVisual" : "RightVisual";
        StripInteractiveComponents(clone);
        ResetCanvasGroups(clone);

        RectTransform cloneRect = clone.transform as RectTransform;
        cloneRect.anchorMin = new Vector2(0.5f, 0.5f);
        cloneRect.anchorMax = new Vector2(0.5f, 0.5f);
        cloneRect.pivot = sourceRect.pivot;
        cloneRect.anchoredPosition = new Vector2(leftSide ? sourceRect.rect.width * 0.25f : -sourceRect.rect.width * 0.25f, 0f);
        cloneRect.sizeDelta = sourceRect.sizeDelta;
        cloneRect.localScale = Vector3.one;
        cloneRect.localRotation = Quaternion.identity;

        return maskRect;
    }

    private void StripInteractiveComponents(GameObject clone)
    {
        StageSelectButton[] stageButtons = clone.GetComponentsInChildren<StageSelectButton>(true);
        for (int i = 0; i < stageButtons.Length; i++)
        {
            Destroy(stageButtons[i]);
        }

        Selectable[] selectables = clone.GetComponentsInChildren<Selectable>(true);
        for (int i = 0; i < selectables.Length; i++)
        {
            Destroy(selectables[i]);
        }

        UIClickSoundPlayer[] clickSounds = clone.GetComponentsInChildren<UIClickSoundPlayer>(true);
        for (int i = 0; i < clickSounds.Length; i++)
        {
            Destroy(clickSounds[i]);
        }
    }

    private void ResetCanvasGroups(GameObject clone)
    {
        CanvasGroup[] canvasGroups = clone.GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            canvasGroups[i].alpha = 1f;
            canvasGroups[i].interactable = false;
            canvasGroups[i].blocksRaycasts = false;
        }
    }

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        if (target.TryGetComponent(out T component))
        {
            return component;
        }

        return target.AddComponent<T>();
    }

    private static int GetSavedStars(string levelId)
    {
        LevelProgressSaveData progress = SaveSystem.GetLevelProgress(levelId);
        return progress == null ? 0 : progress.bestStars;
    }

    private static StageLevelDatabase GetLevelDatabase(StageSelectManager manager)
    {
        return manager == null ? null : manager.LevelDatabase;
    }
}
