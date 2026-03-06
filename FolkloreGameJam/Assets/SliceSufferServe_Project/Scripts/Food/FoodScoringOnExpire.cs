using UnityEngine;

public class FoodScoringOnExpire : MonoBehaviour
{
    [SerializeField] 
    private int decreaseScoreOnBurnt = 10;
    [SerializeField] 
    private ScoreFeedback scoreFeedbackPrefab;

    public void ApplyPenalty(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (!GameUtility.GameManagerExists())
            return;

        if (GameManager.Instance.IsGameOver)
            return;

        if (scoreFeedbackPrefab != null)
        {
            GameObject scoreFeedbackObj = Instantiate(scoreFeedbackPrefab.gameObject, worldPosition, worldRotation);

            if (scoreFeedbackObj.TryGetComponent(out ScoreFeedback scoreFeedback))
            {
                scoreFeedback.SetScore(-decreaseScoreOnBurnt);
            }
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SubtractScore(decreaseScoreOnBurnt);
        }

        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.DecreaseScoreFeedback.PlayFeedbacks();
        }

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