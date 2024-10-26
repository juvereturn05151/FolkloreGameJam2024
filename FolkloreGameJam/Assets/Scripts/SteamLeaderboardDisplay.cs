#if STEAMWORKS_NET
using Steamworks;
#endif
using UnityEngine;
using TMPro;

public class SteamLeaderboardDisplay : MonoBehaviour
{
    [SerializeField]
    private int _maxPlayer = 13;

    private TextMeshProUGUI info;
    private TextMeshProUGUI scores;
    private TextMeshProUGUI yourRankNumber;

#if STEAMWORKS_NET

    [HideInInspector]
    public SteamLeaderboardEntries_t m_SteamLeaderboardEntries;
    private static readonly CallResult<LeaderboardScoresDownloaded_t> m_scoresDownloadedResult = new CallResult<LeaderboardScoresDownloaded_t>();
    private static readonly CallResult<LeaderboardScoresDownloaded_t> m_playerRankDownloadedResult = new CallResult<LeaderboardScoresDownloaded_t>();

#endif

    public void Activate(TextMeshProUGUI info, TextMeshProUGUI scores, TextMeshProUGUI yourRankNumber)
    {
        this.info = info;
        this.scores = scores;
        this.yourRankNumber = yourRankNumber;

        if (!SteamLeaderboardManager.s_initialized) 
        {
            SteamLeaderboardManager.Init();
        }

        if (info)
        {
            info.gameObject.SetActive(true);
            info.text = "LOADING SCORES...";
        }

        GetScores();
    }

    public static void GetScores()
    {
#if STEAMWORKS_NET

        if (!SteamLeaderboardManager.s_initialized)
        {
            Debug.Log("Can't fetch leaderboard because it isn't loaded yet");
        }
        else
        {
            // Download leaderboard entries
            SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(SteamLeaderboardManager.s_currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, 10); // Maximum of 10 entries
            m_scoresDownloadedResult.Set(handle, OnLeaderboardScoresDownloaded);
        }

#endif
    }
#if STEAMWORKS_NET
    private static void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
    {
        SteamLeaderboardDisplay instance = FindObjectOfType<SteamLeaderboardDisplay>();
        instance.ProcessDownloadedScores(pCallback);
    }

    private void ProcessDownloadedScores(LeaderboardScoresDownloaded_t pCallback)
    {
        info.gameObject.SetActive(false);

        scores.text = "";
        scores.gameObject.SetActive(true);
        
        int numEntries = Mathf.Min(pCallback.m_cEntryCount, _maxPlayer);;
        m_SteamLeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;

        int rank = 1;

        // Process each leaderboard entry
        for (int index = 0; index < numEntries; index++)
        {
            SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, index, out LeaderboardEntry_t leaderboardEntry, null, 0);
            string username = SteamFriends.GetFriendPersonaName(leaderboardEntry.m_steamIDUser);

            // Handle rare unknown username issue and reset leaderboard
            if (username.ToUpper() == "[UNKNOWN]")
            {
                if (info)
                {
                    // Display loading message
                    info.gameObject.SetActive(true);
                    info.text = "LOADING SCORES...";

                    scores.gameObject.SetActive(false);
                }

                SteamLeaderboardManager.Init();
                return;
            }

            scores.text += "#" + rank.ToString() + ". " + username.ToUpper() + "  :  " + leaderboardEntry.m_nScore.ToString("n0") + "\n";

            rank++;
        }

        GetCurrentPlayerRank();
    }

#endif
    private static void GetCurrentPlayerRank()
    {
#if STEAMWORKS_NET
        if (SteamLeaderboardManager.s_initialized)
        {
            CSteamID[] users = { SteamUser.GetSteamID() };
            SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntriesForUsers(SteamLeaderboardManager.s_currentLeaderboard, users, users.Length);
            m_playerRankDownloadedResult.Set(handle, OnLeaderboardScoresDownloadedForCurrentPlayer);
        }
        else
        {
            Debug.Log("Leaderboard is not initialized.");
        }
#endif
    }

#if STEAMWORKS_NET
    private void DisplayCurrentPlayerRank(LeaderboardScoresDownloaded_t pCallback)
    {
        // Retrieve only the current player's entry (should only be one entry here)
        SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, 0, out LeaderboardEntry_t leaderboardEntry, null, 0);

        int playerRank = leaderboardEntry.m_nGlobalRank;
        CSteamID playerSteamID = leaderboardEntry.m_steamIDUser;
        int highScore = leaderboardEntry.m_nScore;     
        string playerName = SteamFriends.GetFriendPersonaName(playerSteamID);

        if (yourRankNumber != null)
        {
            yourRankNumber.gameObject.SetActive(true);
            yourRankNumber.text = "#" + playerRank + ". " + playerName + "  :  " + highScore + "\n";
        }
    }
#endif
#if STEAMWORKS_NET
    private static void OnLeaderboardScoresDownloadedForCurrentPlayer(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
    {
        if (bIOFailure || pCallback.m_cEntryCount == 0)
        {
            Debug.Log("Failed to retrieve current player rank.");
            return;
        }

        SteamLeaderboardDisplay instance = FindObjectOfType<SteamLeaderboardDisplay>();
        instance.DisplayCurrentPlayerRank(pCallback);
    }
#endif
}