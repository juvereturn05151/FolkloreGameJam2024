using UnityEngine;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{
    private enum SelectMode
    {
        Level,
        Tutorial
    }

    [SerializeField] private StageSelectManager stageSelectManager;
    [SerializeField] private int levelIndex;
    [SerializeField] private Button button;
    [SerializeField] private Text titleText;
    [SerializeField] private Text starsText;

    private SelectMode selectMode;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

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

        if (titleText != null && levelConfig != null)
        {
            titleText.text = levelConfig.DisplayName;
        }

        if (starsText != null && levelConfig != null)
        {
            LevelProgressSaveData progress = SaveSystem.GetLevelProgress(levelConfig.LevelId);
            int stars = progress == null ? 0 : progress.bestStars;
            starsText.text = $"{stars} / 3";
        }
    }

    public void ConfigureTutorial(StageSelectManager manager, int targetLevelIndex, string tutorialName)
    {
        stageSelectManager = manager;
        levelIndex = targetLevelIndex;
        selectMode = SelectMode.Tutorial;

        if (titleText != null)
        {
            titleText.text = tutorialName;
        }

        if (starsText != null)
        {
            starsText.text = string.Empty;
        }
    }

    private void SelectLevel()
    {
        if (stageSelectManager == null)
        {
            return;
        }

        if (selectMode == SelectMode.Tutorial)
        {
            stageSelectManager.SelectTutorialForLevel(levelIndex);
            return;
        }

        stageSelectManager.SelectLevel(levelIndex);
    }
}
