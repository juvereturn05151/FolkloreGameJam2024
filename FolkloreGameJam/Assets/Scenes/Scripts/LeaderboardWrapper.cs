using UnityEngine;
using Steamworks;
using TMPro;

public class LeaderboardWrapper : MonoBehaviour
{
    [HideInInspector]
    public TextMeshProUGUI info;
    [HideInInspector]
    public TextMeshProUGUI scores;

    [SerializeField]
    private SteamLeaderboardDisplay _steamLeaderboardDisplay;
    void Start()
    {
        _steamLeaderboardDisplay.info = info;
        _steamLeaderboardDisplay.scores = scores;
        _steamLeaderboardDisplay.Activate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
