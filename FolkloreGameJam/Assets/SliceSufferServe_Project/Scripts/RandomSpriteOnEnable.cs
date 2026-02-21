using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomSpriteOnEnable : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        RandomizeSprite();
    }

    public void RandomizeSprite()
    {
        if (sprites == null || sprites.Length == 0)
            return;

        int index = Random.Range(0, sprites.Length);
        sr.sprite = sprites[index];
    }
}