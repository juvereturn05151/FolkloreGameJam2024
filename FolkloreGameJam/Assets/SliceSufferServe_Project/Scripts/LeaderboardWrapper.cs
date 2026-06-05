#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using Steamworks;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using GooglePlayGames.BasicApi;

#if UNITY_ANDROID
using System.Collections.Generic;
using System.Text;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
#endif

public class LeaderboardWrapper : MonoBehaviour
{
    private const int MaxGooglePlayScores = 10;

    [SerializeField]
    private TextMeshProUGUI info;

    [SerializeField]
    private TextMeshProUGUI scores;

    [SerializeField]
    private TextMeshProUGUI yourRankNumber;

    [SerializeField]
    private SteamLeaderboardDisplay steamLeaderboardDisplay;

    private bool isLoadingGoogleLeaderboard;

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

    public void ShowGooglePlayClassicLeaderboardUI()
    {
#if UNITY_ANDROID
        SetText(info, "Opening Google Play Classic Leaderboard...");
        GooglePlayManager.ShowClassicLeaderboard((success, status) =>
        {
            SetText(info, success ? string.Empty : "Google Play leaderboard failed: " + status);
        });
#else
        SetText(info, "Google Play leaderboard is available on Android.");
#endif
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
#if UNITY_ANDROID
        if (isLoadingGoogleLeaderboard)
        {
            return;
        }

        isLoadingGoogleLeaderboard = true;

        SetText(info, "Connecting to Google Play Games...");
        SetText(scores, string.Empty);
        SetText(yourRankNumber, string.Empty);

        Debug.Log("[GPGS] Starting leaderboard scene.");
        Debug.Log("[GPGS] Already authenticated: " + PlayGamesPlatform.Instance.localUser.authenticated);

        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            LoadGooglePlayClassicLeaderboard();
            return;
        }

        PlayGamesPlatform.Instance.Authenticate(OnGooglePlayAuthenticated);
#endif
    }

#if UNITY_ANDROID
    private void OnGooglePlayAuthenticated(SignInStatus status)
    {
        Debug.Log("[GPGS] Auth status: " + status);
        Debug.Log("[GPGS] Authenticated after callback: " + PlayGamesPlatform.Instance.localUser.authenticated);

        if (status != SignInStatus.Success)
        {
            isLoadingGoogleLeaderboard = false;
            SetText(info, "Google Play Games sign-in failed: " + status);
            return;
        }

        LoadGooglePlayClassicLeaderboard();
    }

    private void LoadGooglePlayClassicLeaderboard()
    {
        Debug.Log("[GPGS] Loading Classic Mode leaderboard scores...");
        Debug.Log("[GPGS] Authenticated before leaderboard load: " + PlayGamesPlatform.Instance.localUser.authenticated);

        SetText(info, "Loading Classic Mode Leaderboard...");
        SetText(scores, string.Empty);
        SetText(yourRankNumber, string.Empty);

        GooglePlayManager.LoadClassicLeaderboardScores(
            MaxGooglePlayScores,
            OnGooglePlayClassicScoresLoaded
        );
    }

    private void OnGooglePlayClassicScoresLoaded(bool success, LeaderboardScoreData data)
    {
        isLoadingGoogleLeaderboard = false;

        ResponseStatus status = data != null ? data.Status : ResponseStatus.NotAuthorized;

        Debug.Log("[GPGS] Leaderboard load success: " + success);
        Debug.Log("[GPGS] Leaderboard response status: " + status);
        Debug.Log("[GPGS] Authenticated when leaderboard returned: " + PlayGamesPlatform.Instance.localUser.authenticated);

        if (!success || data == null)
        {
            SetText(info, "Leaderboard failed: " + status);
            return;
        }

        IScore[] loadedScores = data.Scores ?? new IScore[0];

        List<string> userIds = new List<string>();
        AddScoreUserId(loadedScores, userIds);
        AddScoreUserId(data.PlayerScore, userIds);

        if (userIds.Count == 0)
        {
            DisplayGooglePlayScores(data, null);
            return;
        }

        PlayGamesPlatform.Instance.LoadUsers(userIds.ToArray(), profiles =>
        {
            DisplayGooglePlayScores(data, BuildUserNameLookup(profiles));
        });
    }

    private void DisplayGooglePlayScores(LeaderboardScoreData data, Dictionary<string, string> userNamesById)
    {
        IScore[] loadedScores = data.Scores ?? new IScore[0];

        bool hasPlayerScore = data.PlayerScore != null && data.PlayerScore.value > 0;
        bool hasLeaderboardScores = loadedScores.Length > 0;

        SetText(info, hasLeaderboardScores || hasPlayerScore ? string.Empty : "No Classic Mode scores yet.");

        StringBuilder scoreBuilder = new StringBuilder();

        for (int i = 0; i < loadedScores.Length; i++)
        {
            IScore score = loadedScores[i];

            scoreBuilder.Append('#')
                .Append(score.rank > 0 ? score.rank : i + 1)
                .Append(". ")
                .Append(GetDisplayName(score, userNamesById))
                .Append("  :  ")
                .Append(score.value.ToString("n0"))
                .AppendLine();
        }

        SetText(scores, scoreBuilder.ToString());

        IScore playerScore = data.PlayerScore;

        if (playerScore != null && playerScore.rank > 0)
        {
            string rankText =
                "#" + playerScore.rank +
                ". " + GetDisplayName(playerScore, userNamesById) +
                "  :  " + playerScore.value.ToString("n0");

            SetText(yourRankNumber, rankText);
        }
        else
        {
            SetText(yourRankNumber, string.Empty);
        }
    }

    private static void AddScoreUserId(IScore[] scoreList, List<string> userIds)
    {
        for (int i = 0; i < scoreList.Length; i++)
        {
            AddScoreUserId(scoreList[i], userIds);
        }
    }

    private static void AddScoreUserId(IScore score, List<string> userIds)
    {
        if (score == null || string.IsNullOrEmpty(score.userID) || userIds.Contains(score.userID))
        {
            return;
        }

        userIds.Add(score.userID);
    }

    private static Dictionary<string, string> BuildUserNameLookup(IUserProfile[] profiles)
    {
        Dictionary<string, string> userNamesById = new Dictionary<string, string>();

        if (profiles == null)
        {
            return userNamesById;
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            IUserProfile profile = profiles[i];

            if (profile == null || string.IsNullOrEmpty(profile.id) || string.IsNullOrEmpty(profile.userName))
            {
                continue;
            }

            userNamesById[profile.id] = profile.userName;
        }

        return userNamesById;
    }

    private static string GetDisplayName(IScore score, Dictionary<string, string> userNamesById)
    {
        if (score != null &&
            userNamesById != null &&
            userNamesById.TryGetValue(score.userID, out string userName))
        {
            return userName.ToUpper();
        }

        return "PLAYER";
    }
#endif

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
