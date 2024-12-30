using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreFeedback : MonoBehaviour
{
    [SerializeField] float duration = 3.0f;  // Duration in seconds to move up
    [SerializeField] float speed = 2.0f;     // Speed of the upward movement
    [SerializeField] TextMeshProUGUI scoreText;

    private float elapsedTime = 0.0f;

    private void Update()
    {
        // Update the elapsed time
        elapsedTime += Time.deltaTime;

        // Move the object upward as long as the duration hasn't passed
        if (elapsedTime < duration)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
        else
        {
            // Destroy the object after the duration has passed
            Destroy(gameObject);
        }
    }

    // Public method to set the score from other scripts
    public void SetScore(int newScore)
    {
        // Update the text color based on the score value
        if (scoreText != null)
        {
            if (newScore >= 0)
            {
                scoreText.color = Color.green;  // Positive score: green color
            }
            else
            {
                scoreText.color = Color.red;    // Negative score: red color
            }

            // Update the text to show the score
            scoreText.text = newScore.ToString();
        }
    }
}
