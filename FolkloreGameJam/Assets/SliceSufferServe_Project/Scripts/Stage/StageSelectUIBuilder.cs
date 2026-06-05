using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectUIBuilder : MonoBehaviour
{
    [SerializeField] private StageSelectManager stageSelectManager;
    [SerializeField] private StageLevelDatabase levelDatabase;
    [SerializeField] private RectTransform levelListRoot;
    [SerializeField] private StageSelectButton stageNodePrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private Button storeButton;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Text pageText;
    [SerializeField] private string backSceneName = "GameModeSelect";
    [SerializeField] private string storeSceneName = "Store";
    [SerializeField] private Vector2 nodeSize = new Vector2(320, 300);
    [SerializeField] private float nodeSpacing = 360f;
    [SerializeField] private float rowSpacing = 320f;
    [SerializeField] private int maxNodesPerRow = 4;
    [SerializeField] private int levelsPerPage = 6;
    [SerializeField] private string firstCutsceneName = "Opening";
    [SerializeField] private string firstTutorialName = "First Tutorial";
    [SerializeField] private string secondTutorialName = "Second Tutorial";
    [SerializeField] private string postStage13CutsceneName = "Dark Rumors";
    [SerializeField] private Sprite firstCutscenePreviewSprite;
    [SerializeField] private Sprite postStage13CutscenePreviewSprite;

    private int currentPageIndex;
    private bool loadingStore;

    private void Awake()
    {
        if (stageSelectManager == null)
        {
            stageSelectManager = GetComponent<StageSelectManager>();
        }

        if (stageSelectManager != null)
        {
            stageSelectManager.SetLevelDatabase(levelDatabase);
        }

        BuildStageNodes();
    }

    private void OnEnable()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }

        if (storeButton != null)
        {
            storeButton.onClick.AddListener(GoToStore);
        }

        if (previousPageButton != null)
        {
            previousPageButton.onClick.AddListener(GoToPreviousPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(GoToNextPage);
        }
    }

    private void OnDisable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
        }

        if (storeButton != null)
        {
            storeButton.onClick.RemoveListener(GoToStore);
        }

        if (previousPageButton != null)
        {
            previousPageButton.onClick.RemoveListener(GoToPreviousPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveListener(GoToNextPage);
        }
    }

    public void BuildStageNodes()
    {
        if (levelListRoot == null)
        {
            Debug.LogWarning("StageSelectUIBuilder needs a Level List Root.");
            return;
        }

        if (stageNodePrefab == null)
        {
            Debug.LogWarning("StageSelectUIBuilder needs a Stage Node Prefab.");
            return;
        }

        ClearLevelList();

        List<StageSelectEntry> entries = BuildEntries();
        int safeLevelsPerPage = Mathf.Max(1, levelsPerPage);
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(entries.Count / (float)safeLevelsPerPage));
        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pageCount - 1);

        int firstEntryIndex = currentPageIndex * safeLevelsPerPage;
        int lastEntryIndexExclusive = Mathf.Min(entries.Count, firstEntryIndex + safeLevelsPerPage);
        int nodeCount = lastEntryIndexExclusive - firstEntryIndex;
        int nodeIndex = 0;

        for (int i = firstEntryIndex; i < lastEntryIndexExclusive; i++)
        {
            CreateEntryNode(entries[i], nodeIndex++, nodeCount);
        }

        UpdatePageControls(pageCount);
    }

    private List<StageSelectEntry> BuildEntries()
    {
        List<StageSelectEntry> entries = new List<StageSelectEntry>();
        int levelCount = levelDatabase == null ? 0 : levelDatabase.Count;

        if (levelCount > 0)
        {
            entries.Add(StageSelectEntry.Cutscene(0));
            entries.Add(StageSelectEntry.Tutorial(0, firstTutorialName));
        }

        for (int i = 0; i < levelCount; i++)
        {
            if (i == 3)
            {
                entries.Add(StageSelectEntry.Tutorial(3, secondTutorialName));
            }

            entries.Add(StageSelectEntry.Level(i));

            StageLevelConfig levelConfig = levelDatabase.GetLevel(i);
            if (levelConfig != null && levelConfig.LevelNumber == 13)
            {
                entries.Add(StageSelectEntry.PostStageCutscene(i, postStage13CutsceneName));
            }
        }

        return entries;
    }

    private void CreateEntryNode(StageSelectEntry entry, int nodeIndex, int nodeCount)
    {
        StageSelectButton stageNode = CreateNode(nodeIndex, nodeCount);

        switch (entry.Mode)
        {
            case StageSelectEntryMode.Cutscene:
                stageNode.ConfigureCutscene(stageSelectManager, entry.LevelIndex, firstCutsceneName, levelDatabase, firstCutscenePreviewSprite);
                break;
            case StageSelectEntryMode.PostStageCutscene:
                stageNode.ConfigurePostStageCutscene(stageSelectManager, entry.LevelIndex, entry.Title, levelDatabase, postStage13CutscenePreviewSprite);
                break;
            case StageSelectEntryMode.Tutorial:
                stageNode.ConfigureTutorial(stageSelectManager, entry.LevelIndex, entry.Title, levelDatabase);
                break;
            default:
                StageLevelConfig levelConfig = levelDatabase.GetLevel(entry.LevelIndex);
                stageNode.Configure(stageSelectManager, entry.LevelIndex, levelConfig);
                break;
        }
    }

    private StageSelectButton CreateNode(int nodeIndex, int nodeCount)
    {
        StageSelectButton stageNode = Instantiate(stageNodePrefab, levelListRoot);

        RectTransform stageNodeRect = stageNode.GetComponent<RectTransform>();
        if (stageNodeRect != null)
        {
            stageNodeRect.anchorMin = new Vector2(0.5f, 0.5f);
            stageNodeRect.anchorMax = new Vector2(0.5f, 0.5f);
            stageNodeRect.anchoredPosition = GetNodePosition(nodeIndex, nodeCount);
            stageNodeRect.sizeDelta = nodeSize;
        }

        return stageNode;
    }

    private Vector2 GetNodePosition(int nodeIndex, int nodeCount)
    {
        int nodesPerRow = Mathf.Max(1, maxNodesPerRow);
        int rowCount = Mathf.CeilToInt(nodeCount / (float)nodesPerRow);
        int rowIndex = nodeIndex / nodesPerRow;
        int columnIndex = nodeIndex % nodesPerRow;
        int rowNodeCount = Mathf.Min(nodesPerRow, nodeCount - rowIndex * nodesPerRow);

        float startX = -nodeSpacing * (rowNodeCount - 1) * 0.5f;
        float startY = rowSpacing * (rowCount - 1) * 0.5f;
        return new Vector2(startX + nodeSpacing * columnIndex, startY - rowSpacing * rowIndex);
    }

    private void ClearLevelList()
    {
        for (int i = levelListRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(levelListRoot.GetChild(i).gameObject);
        }
    }

    private void GoBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(backSceneName);
    }

    private void GoToStore()
    {
        if (loadingStore || string.IsNullOrWhiteSpace(storeSceneName))
        {
            return;
        }

        loadingStore = true;
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadStoreScene);
    }

    private void LoadStoreScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(storeSceneName);
    }

    private void GoToPreviousPage()
    {
        if (currentPageIndex <= 0)
        {
            return;
        }

        currentPageIndex--;
        BuildStageNodes();
    }

    private void GoToNextPage()
    {
        int entryCount = BuildEntries().Count;
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(entryCount / (float)Mathf.Max(1, levelsPerPage)));

        if (currentPageIndex >= pageCount - 1)
        {
            return;
        }

        currentPageIndex++;
        BuildStageNodes();
    }

    private void UpdatePageControls(int pageCount)
    {
        bool showPageControls = pageCount > 1;

        if (previousPageButton != null)
        {
            previousPageButton.gameObject.SetActive(showPageControls);
            previousPageButton.interactable = currentPageIndex > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.gameObject.SetActive(showPageControls);
            nextPageButton.interactable = currentPageIndex < pageCount - 1;
        }

        if (pageText != null)
        {
            pageText.gameObject.SetActive(showPageControls);
            pageText.text = $"Page {currentPageIndex + 1} / {pageCount}";
        }
    }

    private enum StageSelectEntryMode
    {
        Level,
        Tutorial,
        Cutscene,
        PostStageCutscene
    }

    private readonly struct StageSelectEntry
    {
        public StageSelectEntryMode Mode { get; }
        public int LevelIndex { get; }
        public string Title { get; }

        private StageSelectEntry(StageSelectEntryMode mode, int levelIndex, string title)
        {
            Mode = mode;
            LevelIndex = levelIndex;
            Title = title;
        }

        public static StageSelectEntry Level(int levelIndex)
        {
            return new StageSelectEntry(StageSelectEntryMode.Level, levelIndex, string.Empty);
        }

        public static StageSelectEntry Tutorial(int levelIndex, string title)
        {
            return new StageSelectEntry(StageSelectEntryMode.Tutorial, levelIndex, title);
        }

        public static StageSelectEntry Cutscene(int levelIndex)
        {
            return new StageSelectEntry(StageSelectEntryMode.Cutscene, levelIndex, string.Empty);
        }

        public static StageSelectEntry PostStageCutscene(int levelIndex, string title)
        {
            return new StageSelectEntry(StageSelectEntryMode.PostStageCutscene, levelIndex, title);
        }
    }
}
