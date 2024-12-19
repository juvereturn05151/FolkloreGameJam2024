using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    [SerializeField]
    private HumanPart humanPart;

    private void Start()
    {
        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);
        }

        SteamLeaderboardManager.Init();

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

    public void ClickToGoToCredit()
    {
        FadingUI.Instance.StartFadeIn();
    }
}
