using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFadeOut : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayBeforeFade = 0f;
    [SerializeField] private bool destroyAfterFade = true;

    private SpriteRenderer sr;
    private float timer;
    private float startAlpha;
    private bool fading = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startAlpha = sr.color.a;
    }

    private void OnEnable()
    {
        timer = 0f;
        fading = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (!fading)
        {
            if (timer >= delayBeforeFade)
            {
                fading = true;
                timer = 0f;
            }
            return;
        }

        float t = timer / fadeDuration;
        float newAlpha = Mathf.Lerp(startAlpha, 0f, t);

        Color c = sr.color;
        c.a = newAlpha;
        sr.color = c;

        if (t >= 1f)
        {
            if (destroyAfterFade)
                Destroy(gameObject);
            else
                enabled = false;
        }
    }
}