using UnityEngine;
using UnityEngine.UI;

public class StageSelectUIBuilder : MonoBehaviour
{
    [SerializeField] private StageSelectManager stageSelectManager;
    [SerializeField] private StageLevelConfig[] levels;
    [SerializeField] private RectTransform levelListRoot;
    [SerializeField] private StageSelectButton stageNodePrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private string backSceneName = "GameModeSelect";
    [SerializeField] private Vector2 nodeSize = new Vector2(320, 300);
    [SerializeField] private float nodeSpacing = 360f;

    private void Awake()
    {
        if (stageSelectManager == null)
        {
            stageSelectManager = GetComponent<StageSelectManager>();
        }

        if (stageSelectManager != null)
        {
            stageSelectManager.SetLevels(levels);
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

        int levelCount = levels == null || levels.Length == 0 ? 0 : levels.Length;
        float startX = -nodeSpacing * (levelCount - 1) * 0.5f;

        for (int i = 0; i < levelCount; i++)
        {
            StageLevelConfig levelConfig = levels[i];
            StageSelectButton stageNode = Instantiate(stageNodePrefab, levelListRoot);

            RectTransform stageNodeRect = stageNode.GetComponent<RectTransform>();
            if (stageNodeRect != null)
            {
                stageNodeRect.anchorMin = new Vector2(0.5f, 0.5f);
                stageNodeRect.anchorMax = new Vector2(0.5f, 0.5f);
                stageNodeRect.anchoredPosition = new Vector2(startX + nodeSpacing * i, 0);
                stageNodeRect.sizeDelta = nodeSize;
            }

            stageNode.Configure(stageSelectManager, i, levelConfig);
        }
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
