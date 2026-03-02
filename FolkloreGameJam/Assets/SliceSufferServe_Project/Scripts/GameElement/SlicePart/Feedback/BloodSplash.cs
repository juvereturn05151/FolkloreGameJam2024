using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BloodSplash : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField]
    private float stayDuration = 2f;     
    [SerializeField] 
    private float fadeDuration = 1.5f;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private Color _originalColor;

    private void Awake()
    {
        if (spriteRenderer == null) 
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        _originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // Stay fully visible
        yield return new WaitForSeconds(stayDuration);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            Color newColor = _originalColor;
            newColor.a = alpha;
            spriteRenderer.color = newColor;

            yield return null;
        }

        Destroy(gameObject);
    }
}