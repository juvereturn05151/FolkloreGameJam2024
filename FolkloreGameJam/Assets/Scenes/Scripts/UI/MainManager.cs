using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnClickStart()
    {
        //Call start game
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadTutorialScene);
    }

    private void LoadTutorialScene()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
