using UnityEngine;

[RequireComponent(typeof(HumanBody))]
public class RobotHuman : MonoBehaviour
{
    public const string AppearSoundName = "HumanRobotAppear";
    public const string ExplodeSoundName = "HumanRobotExplode";
    private const string CoinCollectingSound = "CoinCollecting";

    [SerializeField] private float missedRewardPatiencePercent = 0.1f;
    [SerializeField] private int missedRewardScore = 15;

    private bool hasGrantedMissedReward;

    private void Start()
    {
        PlaySound(AppearSoundName);
    }

    public void PlayExplodeSound()
    {
        PlaySound(ExplodeSoundName);
    }

    public void GrantMissedDestroyerReward()
    {
        if (hasGrantedMissedReward)
        {
            return;
        }

        hasGrantedMissedReward = true;

        Customer[] customers = FindObjectsByType<Customer>();
        for (int i = 0; i < customers.Length; i++)
        {
            if (customers[i] != null)
            {
                customers[i].RewardPatiencePercent(missedRewardPatiencePercent);
            }
        }

        if (GameUtility.GameManagerExists())
        {
            GameManager.Instance.IncreaseScore(missedRewardScore);
        }

        GameplayHUDUI.Instance?.PlayScoreCoinEffect(transform.position);

        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlaySFX(CoinCollectingSound);
        }
    }

    private static void PlaySound(string soundName)
    {
        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlaySFX(soundName);
        }
    }
}
