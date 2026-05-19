using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHUDUI : MonoBehaviour
{
    [Header("Gameplay UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image heartImage;
    [SerializeField] private Image clockTimerImage;
    [SerializeField] private Image clockHand;

    public void Bind()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged += UpdateTimeUI;
            TimeManager.Instance.OnClockChanged += UpdateClockUI;
        }

        if (HPManager.Instance != null)
        {
            HPManager.Instance.OnHealthChanged += UpdateHP;
        }
    }

    public void Unbind()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= UpdateTimeUI;
            TimeManager.Instance.OnClockChanged -= UpdateClockUI;
        }

        if (HPManager.Instance != null)
        {
            HPManager.Instance.OnHealthChanged -= UpdateHP;
        }
    }

    public void Initialize()
    {
        if (ScoreManager.Instance != null)
        {
            UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
        }

        if (heartImage != null)
        {
            heartImage.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore;
        }
    }

    private void UpdateTimeUI(string formattedTime)
    {
        // Reserved for a text timer if one is restored to the gameplay HUD.
    }

    private void UpdateClockUI(float currentTime, float maxTime)
    {
        float denominator = maxTime - 18.0f;
        float normalizedTime = denominator <= 0f ? 0f : (currentTime - 18.0f) / denominator;

        if (clockTimerImage != null)
        {
            clockTimerImage.fillAmount = normalizedTime;
        }

        if (clockHand != null)
        {
            float angle = normalizedTime * 360.0f;
            clockHand.transform.rotation = Quaternion.Euler(0, 0, -angle);
        }
    }

    private void UpdateHP(int hp)
    {
        if (hpText != null)
        {
            hpText.text = "HP: " + hp;
        }
    }
}
