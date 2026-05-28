using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHUDUI : MonoBehaviour
{
    public static GameplayHUDUI Instance { get; private set; }

    [Header("Gameplay UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image heartImage;
    [SerializeField] private Image clockTimerImage;
    [SerializeField] private Image clockHand;
    [SerializeField] private Image scoreCoinImage;

    [Header("Score Coin Effect")]
    [SerializeField] private Sprite scoreCoinSprite;
    [SerializeField] private int scoreCoinEffectCount = 5;
    [SerializeField] private Vector2 scoreCoinEffectSize = new Vector2(42f, 42f);
    [SerializeField] private float scoreCoinEffectDuration = 0.65f;
    [SerializeField] private float scoreCoinEffectStagger = 0.05f;
    [SerializeField] private float scoreCoinEffectSpread = 80f;

    private Canvas parentCanvas;
    private RectTransform canvasRectTransform;
    private Vector3 scoreTextInitialScale = Vector3.one;

    private void Awake()
    {
        Instance = this;
        ResolveCoinEffectReferences();
        CacheScoreTextInitialScale();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

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
        ResolveCoinEffectReferences();
        CacheScoreTextInitialScale();

        if (ScoreManager.Instance != null)
        {
            UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
        }

        if (heartImage != null)
        {
            heartImage.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void PlayScoreCoinEffect(Vector3 sourceWorldPosition)
    {
        ResolveCoinEffectReferences();

        Sprite coinSprite = scoreCoinSprite != null ? scoreCoinSprite : scoreCoinImage != null ? scoreCoinImage.sprite : null;
        if (coinSprite == null || canvasRectTransform == null || scoreText == null)
            return;

        Camera uiCamera = GetCanvasCamera();
        Camera worldCamera = Camera.main;
        if (worldCamera == null)
            return;

        Vector2 startPosition = WorldToCanvasPosition(worldCamera.WorldToScreenPoint(sourceWorldPosition), uiCamera);
        Vector2 endPosition = GetTargetCanvasPosition(uiCamera);
        int coinCount = Mathf.Max(1, scoreCoinEffectCount);

        for (int i = 0; i < coinCount; i++)
        {
            Image coin = CreateScoreCoin(coinSprite, startPosition);
            RectTransform coinTransform = coin.rectTransform;
            Vector2 burstPosition = startPosition + Random.insideUnitCircle * Mathf.Max(0f, scoreCoinEffectSpread);
            float delay = i * Mathf.Max(0f, scoreCoinEffectStagger);

            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(delay);
            sequence.Append(coinTransform.DOAnchorPos(burstPosition, 0.18f).SetEase(Ease.OutQuad));
            sequence.Append(coinTransform.DOAnchorPos(endPosition, Mathf.Max(0.05f, scoreCoinEffectDuration)).SetEase(Ease.InQuad));
            sequence.Join(coinTransform.DOScale(0.45f, Mathf.Max(0.05f, scoreCoinEffectDuration)).SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                if (coin != null)
                {
                    Destroy(coin.gameObject);
                }

                PulseScoreText();
            });
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

    private void ResolveCoinEffectReferences()
    {
        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        if (canvasRectTransform == null && parentCanvas != null)
        {
            canvasRectTransform = parentCanvas.transform as RectTransform;
        }
    }

    private void CacheScoreTextInitialScale()
    {
        if (scoreText != null)
        {
            scoreTextInitialScale = scoreText.transform.localScale;
        }
    }

    private void PulseScoreText()
    {
        if (scoreText == null)
            return;

        Transform scoreTransform = scoreText.transform;
        scoreTransform.DOKill();
        scoreTransform.localScale = scoreTextInitialScale;
        scoreTransform
            .DOPunchScale(Vector3.one * 0.12f, 0.18f, 8, 0.5f)
            .OnComplete(() =>
            {
                if (scoreTransform != null)
                {
                    scoreTransform.localScale = scoreTextInitialScale;
                }
            });
    }

    private Camera GetCanvasCamera()
    {
        if (parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return parentCanvas.worldCamera != null ? parentCanvas.worldCamera : Camera.main;
    }

    private Vector2 GetTargetCanvasPosition(Camera uiCamera)
    {
        RectTransform target = scoreCoinImage != null ? scoreCoinImage.rectTransform : scoreText.rectTransform;
        return WorldToCanvasPosition(RectTransformUtility.WorldToScreenPoint(uiCamera, target.position), uiCamera);
    }

    private Vector2 WorldToCanvasPosition(Vector2 screenPosition, Camera uiCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, uiCamera, out Vector2 canvasPosition);
        return canvasPosition;
    }

    private Image CreateScoreCoin(Sprite coinSprite, Vector2 anchoredPosition)
    {
        GameObject coinObject = new GameObject("Score Coin Effect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        coinObject.transform.SetParent(canvasRectTransform, false);
        coinObject.transform.SetAsLastSibling();

        Image coin = coinObject.GetComponent<Image>();
        coin.sprite = coinSprite;
        coin.preserveAspect = true;
        coin.raycastTarget = false;

        RectTransform coinTransform = coin.rectTransform;
        coinTransform.anchorMin = new Vector2(0.5f, 0.5f);
        coinTransform.anchorMax = new Vector2(0.5f, 0.5f);
        coinTransform.pivot = new Vector2(0.5f, 0.5f);
        coinTransform.anchoredPosition = anchoredPosition;
        coinTransform.sizeDelta = scoreCoinEffectSize;

        return coin;
    }
}
