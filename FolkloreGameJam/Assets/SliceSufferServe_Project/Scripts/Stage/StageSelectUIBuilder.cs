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
    [SerializeField] private string firstTutorialName = "First Tutorial";
    [SerializeField] private string secondTutorialName = "Second Tutorial";

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

        int levelCount = levelDatabase == null ? 0 : levelDatabase.Count;
        int safeLevelsPerPage = Mathf.Max(1, levelsPerPage);
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(levelCount / (float)safeLevelsPerPage));
        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pageCount - 1);

        int firstLevelIndex = currentPageIndex * safeLevelsPerPage;
        int lastLevelIndexExclusive = Mathf.Min(levelCount, firstLevelIndex + safeLevelsPerPage);
        int tutorialCount = GetTutorialNodeCount(firstLevelIndex, lastLevelIndexExclusive);
        int nodeCount = lastLevelIndexExclusive - firstLevelIndex + tutorialCount;
        int nodeIndex = 0;

        if (IsLevelOnCurrentPage(0, firstLevelIndex, lastLevelIndexExclusive))
        {
            CreateTutorialNode(nodeIndex++, 0, firstTutorialName, nodeCount);
        }

        for (int i = firstLevelIndex; i < lastLevelIndexExclusive; i++)
        {
            if (IsLevelOnCurrentPage(3, firstLevelIndex, lastLevelIndexExclusive) && i == 3)
            {
                CreateTutorialNode(nodeIndex++, 3, secondTutorialName, nodeCount);
            }

            StageLevelConfig levelConfig = levelDatabase.GetLevel(i);
            StageSelectButton stageNode = CreateNode(nodeIndex++, nodeCount);
            stageNode.Configure(stageSelectManager, i, levelConfig);
        }

        UpdatePageControls(pageCount);
    }

    private int GetTutorialNodeCount(int firstLevelIndex, int lastLevelIndexExclusive)
    {
        int tutorialCount = IsLevelOnCurrentPage(0, firstLevelIndex, lastLevelIndexExclusive) ? 1 : 0;
        return IsLevelOnCurrentPage(3, firstLevelIndex, lastLevelIndexExclusive) ? tutorialCount + 1 : tutorialCount;
    }

    private bool IsLevelOnCurrentPage(int levelIndex, int firstLevelIndex, int lastLevelIndexExclusive)
    {
        return levelIndex >= firstLevelIndex && levelIndex < lastLevelIndexExclusive;
    }

    private void CreateTutorialNode(int nodeIndex, int targetLevelIndex, string tutorialName, int nodeCount)
    {
        StageSelectButton stageNode = CreateNode(nodeIndex, nodeCount);
        stageNode.ConfigureTutorial(stageSelectManager, targetLevelIndex, tutorialName);
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
        int levelCount = levelDatabase == null ? 0 : levelDatabase.Count;
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(levelCount / (float)Mathf.Max(1, levelsPerPage)));

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
}
