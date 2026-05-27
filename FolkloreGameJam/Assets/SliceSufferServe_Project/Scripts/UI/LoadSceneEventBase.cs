using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneEventBase : MonoBehaviour
{


    [SerializeField]
    private string targetSceneName;
    [SerializeField]
    private StageLevelConfig selectedLevelConfig;

    protected void OnLoadSceneEvent()
    {
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
}
