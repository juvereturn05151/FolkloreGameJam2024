using UnityEngine;
using UnityEngine.UI;

public class StageSelectUIBuilder : MonoBehaviour
{
    [SerializeField] private StageSelectManager stageSelectManager;
    [SerializeField] private StageLevelDatabase levelDatabase;
    [SerializeField] private RectTransform levelListRoot;
    [SerializeField] private StageSelectButton stageNodePrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private string backSceneName = "GameModeSelect";
    [SerializeField] private Vector2 nodeSize = new Vector2(320, 300);
    [SerializeField] private float nodeSpacing = 360f;
    [SerializeField] private float rowSpacing = 320f;
    [SerializeField] private int maxNodesPerRow = 4;
    [SerializeField] private string firstTutorialName = "First Tutorial";
    [SerializeField] private string secondTutorialName = "Second Tutorial";

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
    }

    private void OnDisable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
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
        int tutorialCount = GetTutorialNodeCount(levelCount);
        int nodeCount = levelCount + tutorialCount;
        int nodeIndex = 0;

        if (levelCount > 0)
        {
            CreateTutorialNode(nodeIndex++, 0, firstTutorialName, nodeCount);
        }

        for (int i = 0; i < levelCount; i++)
        {
            if (i == 3)
            {
                CreateTutorialNode(nodeIndex++, 3, secondTutorialName, nodeCount);
            }

            StageLevelConfig levelConfig = levelDatabase.GetLevel(i);
            StageSelectButton stageNode = CreateNode(nodeIndex++, nodeCount);
            stageNode.Configure(stageSelectManager, i, levelConfig);
        }
    }

    private int GetTutorialNodeCount(int levelCount)
    {
        int tutorialCount = levelCount > 0 ? 1 : 0;
        return levelCount > 3 ? tutorialCount + 1 : tutorialCount;
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
}
