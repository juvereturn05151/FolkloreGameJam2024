using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneEventBase : MonoBehaviour
{


    [SerializeField]
    private string targetSceneName;

    protected void OnLoadSceneEvent()
    {
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadTargetScene);
    }

    protected void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}
