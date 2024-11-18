using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    [SerializeField]
    private HumanPart humanPart;

    private void Start()
    {
        humanPart.OnPartDestroyed.AddListener(OnClickStart);
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

    public void ClickToExit()
    {
        //Call start game
        Application.Quit();
    }
}
