using UnityEngine;

public class FoodScoringOnExpire : MonoBehaviour
{
    [SerializeField] 
    private int decreaseScoreOnBurnt = 10;
    [SerializeField] 
    private ScoreFeedback scoreFeedbackPrefab;

    public void ApplyPenalty(Vector3 worldPos)
    {
        // Only apply if game exists and isn't over
        if (!GameUtility.GameManagerExists()) return;
        if (GameManager.Instance.IsGameOver) return;

        // Floating score feedback (optional)
        if (scoreFeedbackPrefab != null)
        {
            GameObject obj = Instantiate(scoreFeedbackPrefab.gameObject, worldPos, Quaternion.identity);
            if (obj.GetComponent<ScoreFeedback>() is ScoreFeedback scoreFeedback)
            {
                scoreFeedback.SetScore(-1 * decreaseScoreOnBurnt);
            }
        }

        // Score subtract
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SubtractScore(decreaseScoreOnBurnt);
        }

        // Play feedback (optional)
        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.DecreaseScoreFeedback.PlayFeedbacks();
        }

        // Tutorial hook (kept here so Food stays clean)
        if (GameUtility.SSSAdvancedTutorialManagerExists())
        {
            if (GameManager.Instance.IsTutorial &&
                SSSAdvancedTutorialManager.Instance.CurrentTutorial.Type == TutorialType.WaitForRotten &&
                SSSAdvancedTutorialManager.Instance.IsOperating)
            {
                SSSAdvancedTutorialManager.Instance.rottenCount++;
            }
        }
    }
}