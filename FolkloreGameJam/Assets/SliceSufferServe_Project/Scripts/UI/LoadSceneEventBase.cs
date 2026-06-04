using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadSceneEventBase : MonoBehaviour
{
    [SerializeField]
    private string targetSceneName;
    [SerializeField]
    private StageLevelConfig selectedLevelConfig;
    [SerializeField]
    private StageLevelConfig requiredCompletedLevelConfig;
    [SerializeField]
    private GameObject lockedVisualRoot;
    [SerializeField]
    private GameObject lockIconRoot;
    [SerializeField]
    [Range(0.1f, 1f)]
    private float lockedAlpha = 0.45f;
    [SerializeField]
    private TMP_Text lockStateText;
    [SerializeField]
    private string lockedText = "Score Challenge Mode\nComplete Level 6";

    private string unlockedText;

    protected void OnLoadSceneEvent()
    {
        if (!IsLoadUnlocked())
        {
            Debug.LogWarning($"{name} is locked until {GetRequiredLevelName()} is complete.");
            return;
        }

        if (selectedLevelConfig != null)
        {
            StageSelection.SelectLevel(selectedLevelConfig);
        }

        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadTargetScene);
    }

    protected void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }

    protected bool IsLoadUnlocked()
    {
        return requiredCompletedLevelConfig == null
            || StageUnlockSystem.IsLevelCompleted(requiredCompletedLevelConfig);
    }

    protected void RefreshLockedVisualState()
    {
        Transform root = lockedVisualRoot == null ? transform : lockedVisualRoot.transform;
        bool unlocked = IsLoadUnlocked();
        float alpha = unlocked ? 1f : lockedAlpha;

        if (lockIconRoot != null)
        {
            lockIconRoot.SetActive(!unlocked);
        }

        RefreshLockStateText(root, unlocked);

        Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Color color = graphics[i].color;
            color.a = alpha;
            graphics[i].color = color;
        }

        SpriteRenderer[] spriteRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color color = spriteRenderers[i].color;
            color.a = alpha;
            spriteRenderers[i].color = color;
        }
    }

    private string GetRequiredLevelName()
    {
        return requiredCompletedLevelConfig == null ? "the required level" : requiredCompletedLevelConfig.DisplayName;
    }

    private void RefreshLockStateText(Transform root, bool unlocked)
    {
        if (lockStateText == null)
        {
            lockStateText = root.GetComponentInChildren<TMP_Text>(true);
        }

        if (lockStateText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(unlockedText))
        {
            unlockedText = lockStateText.text;
        }

        lockStateText.text = unlocked ? unlockedText : lockedText;
    }
}
