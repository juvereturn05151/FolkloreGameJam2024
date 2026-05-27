using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeSelect : MonoBehaviour
{
    [SerializeField]
    private string storeSceneName = "Store";

    [SerializeField]
    private string menuSceneName = "MainMenu";

    private string targetSceneName;
    public void ClickToGoToStore()
    {
        // Set target scene to the credits scene
        targetSceneName = storeSceneName;

        // Call start game
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadTargetScene);
    }

    public void ClickToMainMenu()
    {
        // Set target scene to the main menu scene
        targetSceneName = menuSceneName;

        // Call start game
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadTargetScene);
    }

    private void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}
