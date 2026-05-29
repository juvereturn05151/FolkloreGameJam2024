#if !DISABLESTEAMWORKS && STEAMWORKS_NET
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
        if (ShouldUseGooglePlayClassicLeaderboard())
        {
            ShowGooglePlayClassicLeaderboard();
            return;
        }

        if (steamLeaderboardDisplay != null)
        {
            steamLeaderboardDisplay.Activate(info, scores, yourRankNumber);
        }
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

    private bool ShouldUseGooglePlayClassicLeaderboard()
    {
#if UNITY_ANDROID
        return StageSelection.IsClassicMode;
#else
        return false;
#endif
    }

    private void ShowGooglePlayClassicLeaderboard()
    {
        SetText(info, "Opening Classic Mode Leaderboard...");
        SetText(scores, string.Empty);
        SetText(yourRankNumber, string.Empty);

        GooglePlayManager.ShowClassicLeaderboard();
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text == null)
        {
            return;
        }

        text.gameObject.SetActive(!string.IsNullOrEmpty(value));
        text.text = value;
    }
}
