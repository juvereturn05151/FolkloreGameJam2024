#if STEAMWORKS_NET
using Steamworks;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LeaderboardWrapper : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI info;
    [SerializeField]
    private TextMeshProUGUI scores;
    [SerializeField]
    private TextMeshProUGUI yourRankNumber;

    [SerializeField]
    private SteamLeaderboardDisplay steamLeaderboardDisplay;

    void Start()
    {
        steamLeaderboardDisplay.Activate(info, scores, yourRankNumber);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //EventClass
    public void GoToMainMenu() 
    {
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadMainMenu);
    }

    public void GoToGameplay()
    {
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadGameplayScene);
    }

    private void LoadGameplayScene()
    {
        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlayGameplayBGM();
        }

        SceneManager.LoadScene("GameplayScene");
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
