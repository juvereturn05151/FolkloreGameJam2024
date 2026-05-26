using UnityEngine;
using UnityEngine.UI;

public class CharacterPreviewView : MonoBehaviour
{
    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer headSpriteRenderer;
    [SerializeField] private SpriteRenderer neckSpriteRenderer;
    [SerializeField] private SpriteRenderer stomachSpriteRenderer;
    [SerializeField] private SpriteRenderer legSpriteRenderer;

    [Header("UI Images")]
    [SerializeField] private Image headImage;
    [SerializeField] private Image neckImage;
    [SerializeField] private Image stomachImage;
    [SerializeField] private Image legImage;

    public void ApplySpriteSet(CharacterSpriteSet set)
    {
        if (set == null)
        {
            SetPart(BodyPartType.Head, null);
            SetPart(BodyPartType.Neck, null);
            SetPart(BodyPartType.Stomach, null);
            SetPart(BodyPartType.Leg, null);
            return;
        }

        SetPart(BodyPartType.Head, set.head);
        SetPart(BodyPartType.Neck, set.neck);
        SetPart(BodyPartType.Stomach, set.stomach);
        SetPart(BodyPartType.Leg, set.leg);
    }

    private void SetPart(BodyPartType part, Sprite sprite)
    {
        switch (part)
        {
            case BodyPartType.Head:
                SetSprite(headSpriteRenderer, headImage, sprite);
                break;
            case BodyPartType.Neck:
                SetSprite(neckSpriteRenderer, neckImage, sprite);
                break;
            case BodyPartType.Stomach:
                SetSprite(stomachSpriteRenderer, stomachImage, sprite);
                break;
            case BodyPartType.Leg:
                SetSprite(legSpriteRenderer, legImage, sprite);
                break;
        }
    }

    private static void SetSprite(SpriteRenderer spriteRenderer, Image image, Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }

        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
        }
    }
}
