using DG.Tweening;
using UnityEngine;

public class CustomerFeedbackController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform heartLocation;
    [SerializeField] private Transform feedbackParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject heart;
    [SerializeField] private GameObject brokenHeart;
    [SerializeField] private GameObject satisfyFeedback;
    [SerializeField] private GameObject unsatisfyFeedback;
    [SerializeField] private ScoreFeedback scoreFeedback;

    public void PlaySatisfiedFeedback()
    {
        if (animator != null)
        {
            animator.SetBool("Happy", true);
        }

        if (heart != null && heartLocation != null)
        {
            Object.Instantiate(heart, heartLocation.position, Quaternion.identity, heartLocation);
        }

        if (satisfyFeedback != null && feedbackParent != null)
        {
            Object.Instantiate(satisfyFeedback, feedbackParent);
        }
    }

    public void PlayWrongFoodFeedback()
    {
        if (animator != null)
        {
            animator.SetTrigger("Anger");
        }

        if (unsatisfyFeedback != null && feedbackParent != null)
        {
            Object.Instantiate(unsatisfyFeedback, feedbackParent);
        }

        if (brokenHeart != null && heartLocation != null)
        {
            Object.Instantiate(brokenHeart, heartLocation.position, Quaternion.identity, heartLocation);
        }

        transform.DOShakePosition(1f, 0.5f);

        if (GameUtility.FeedbackManagerExists())
        {
            FeedbackManager.Instance.ShakeCameraFeedback(0.5f, 1f);
        }
    }

    public void PlayTimeoutFeedback()
    {
        if (animator != null)
        {
            animator.SetBool("Anger", true);
        }
    }

    public void SetEatingAnimation(bool isEating)
    {
        if (animator != null)
        {
            animator.SetBool("Pick", isEating);
        }
    }

    public void SetHappyAnimation(bool isHappy) 
    {
        if (animator != null)
        {
            animator.SetBool("Happy", isHappy);
        }
    }

    public void SpawnScoreFeedback(int score)
    {
        if (scoreFeedback == null)
            return;

        GameObject scoreFeedbackObject = Object.Instantiate(
            scoreFeedback.gameObject,
            transform.position,
            transform.rotation
        );

        if (scoreFeedbackObject.TryGetComponent(out ScoreFeedback feedback))
        {
            feedback.SetScore(score);
        }
    }
}
