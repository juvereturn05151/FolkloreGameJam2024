#if Steamworks
using Steamworks;
#endif


using UnityEngine;

public class SteamLeaderboardManager : MonoBehaviour
{
    private const string s_leaderboardName = "Slice, Suffer, Serve! High-Score Demo";

    public static SteamLeaderboardManager instance;
    public static bool s_initialized = false;

#if Steamworks

    public static SteamLeaderboard_t s_currentLeaderboard;
    
    private static readonly CallResult<LeaderboardFindResult_t> m_findResult = new CallResult<LeaderboardFindResult_t>();
    private static readonly CallResult<LeaderboardScoreUploaded_t> m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();

    private const ELeaderboardUploadScoreMethod s_leaderboardMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;
#endif
    public bool getScores = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        Init();
    }

    public static void UpdateScore(int score)
    {
#if Steamworks

        if (!SteamManager.Initialized)
            return;

        if (!s_initialized)
        {
            Init();
            Debug.Log("Can't upload to the leaderboard because it isn't loaded yet");
        }
        else
        {
            SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(s_currentLeaderboard, s_leaderboardMethod, score, null, 0);
            m_uploadResult.Set(hSteamAPICall, OnLeaderboardUploadResult);
            SteamAPI.RunCallbacks();
        }

#endif
    }

    public static void Init()
    {
#if Steamworks


        if (!SteamManager.Initialized)
            return;

        SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(s_leaderboardName);
        m_findResult.Set(hSteamAPICall, OnLeaderboardFindResult);

#endif
    }

#if Steamworks

    private static void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure)
    {
        if (failure) 
        {
            Debug.Log("dont' find");
        }

        s_currentLeaderboard = pCallback.m_hSteamLeaderboard;
        s_initialized = true;

        /*if(instance.getScores)
            SteamLeaderboardDisplay.GetScores();*/
    }



    private static void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
    {
        Debug.Log("STEAM LEADERBOARDS: failure - " + failure + " Completed - " + pCallback.m_bSuccess + " NewScore: " + pCallback.m_nGlobalRankNew + " Score " + pCallback.m_nScore + " HasChanged - " + pCallback.m_bScoreChanged);
    }

#endif
}
