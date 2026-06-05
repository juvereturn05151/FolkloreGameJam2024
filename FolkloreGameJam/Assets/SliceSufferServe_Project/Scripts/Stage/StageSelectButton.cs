using UnityEngine;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
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

    private SelectMode selectMode;
    private bool isUnlocked;

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

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(SelectLevel);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SelectLevel);
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
