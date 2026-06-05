using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationApplier : MonoBehaviour
{
    [Header("Optional Target")]
    [SerializeField] private HumanBody humanBody;

    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private SpriteRenderer neckRenderer;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer legsRenderer;

    [Header("UI Images")]
    [SerializeField] private Image headImage;
    [SerializeField] private Image neckImage;
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image legsImage;

    [Header("Optional Sprite Overrides")]
    [SerializeField] private Sprite[] headOptions;
    [SerializeField] private Sprite[] neckOptions;
    [SerializeField] private Sprite[] bodyOptions;
    [SerializeField] private Sprite[] legsOptions;

    private void Start()
    {
        ApplySavedCustomization();
    }

    public void ApplySavedCustomization()
    {
        CharacterSpriteSet spriteSet = LoadSavedSpriteSet(headOptions, neckOptions, bodyOptions, legsOptions);
        ApplySpriteSet(spriteSet);
    }

    public void ApplySpriteSet(CharacterSpriteSet spriteSet)
    {
        if (spriteSet == null)
        {
            return;
        }

        if (humanBody == null)
        {
            humanBody = GetComponent<HumanBody>();
        }

        if (humanBody != null)
        {
            humanBody.ApplyCustomizationSpriteSet(spriteSet);
        }

        SetSprite(headRenderer, headImage, spriteSet.head);
        SetSprite(neckRenderer, neckImage, spriteSet.neck);
        SetSprite(bodyRenderer, bodyImage, spriteSet.stomach);
        SetSprite(legsRenderer, legsImage, spriteSet.leg);
    }

    public static CharacterSpriteSet LoadSavedSpriteSet(
        Sprite[] headOverrides = null,
        Sprite[] neckOverrides = null,
        Sprite[] bodyOverrides = null,
        Sprite[] legsOverrides = null)
    {
        CharacterCustomizationData data = CharacterCustomizationData.LoadFromPlayerPrefs();
        HumanType humanType = data.selectedHumanType;
        Sprite[] resourceHeads = CharacterCustomizer.LoadHumanPartSprites(humanType, BodyPartType.Head);
        Sprite[] resourceNecks = CharacterCustomizer.LoadHumanPartSprites(humanType, BodyPartType.Neck);
        Sprite[] resourceStomachs = CharacterCustomizer.LoadHumanPartSprites(humanType, BodyPartType.Stomach);
        Sprite[] resourceLegs = CharacterCustomizer.LoadHumanPartSprites(humanType, BodyPartType.Leg);

        Sprite[] heads = HasSprites(resourceHeads) ? resourceHeads : headOverrides;
        Sprite[] necks = HasSprites(resourceNecks) ? resourceNecks : neckOverrides;
        Sprite[] bodies = HasSprites(resourceStomachs) ? resourceStomachs : bodyOverrides;
        Sprite[] legs = HasSprites(resourceLegs) ? resourceLegs : legsOverrides;

        heads ??= System.Array.Empty<Sprite>();
        necks ??= System.Array.Empty<Sprite>();
        bodies ??= System.Array.Empty<Sprite>();
        legs ??= System.Array.Empty<Sprite>();

        Debug.Log($"Manual customization applier loaded {humanType} sprites: heads={heads.Length}, necks={necks.Length}, stomachs={bodies.Length}, legs={legs.Length}");
        Debug.Log($"Manual customization applying selected indices: type={humanType}, head={data.headIndex}, neck={data.neckIndex}, body={data.bodyIndex}, legs={data.legsIndex}");

        int headIndex = ResolveSavedIndex(data, BodyPartType.Head, heads, data.headIndex);
        int neckIndex = ResolveSavedIndex(data, BodyPartType.Neck, necks, data.neckIndex);
        int bodyIndex = ResolveSavedIndex(data, BodyPartType.Stomach, bodies, data.bodyIndex);
        int legsIndex = ResolveSavedIndex(data, BodyPartType.Leg, legs, data.legsIndex);

        return new CharacterSpriteSet
        {
            head = GetUnlockedSprite(humanType, BodyPartType.Head, heads, headIndex),
            neck = GetUnlockedSprite(humanType, BodyPartType.Neck, necks, neckIndex),
            stomach = GetUnlockedSprite(humanType, BodyPartType.Stomach, bodies, bodyIndex),
            leg = GetUnlockedSprite(humanType, BodyPartType.Leg, legs, legsIndex)
        };
    }

    private static bool HasSprites(Sprite[] sprites)
    {
        return sprites != null && sprites.Length > 0;
    }

    private static Sprite GetSprite(Sprite[] options, int index)
    {
        if (options == null || options.Length == 0)
        {
            return null;
        }

        return options[Mathf.Clamp(index, 0, options.Length - 1)];
    }

    private static Sprite GetUnlockedSprite(HumanType humanType, BodyPartType part, Sprite[] options, int index)
    {
        if (!CharacterCustomizer.IsHumanPartUnlocked(humanType, part, index))
        {
            return null;
        }

        return GetSprite(options, index);
    }

    private static int ResolveSavedIndex(CharacterCustomizationData data, BodyPartType part, Sprite[] options, int fallbackIndex)
    {
        int resolvedIndex = CharacterCustomizer.FindSpriteIndex(options, data.GetSpriteId(part));
        if (resolvedIndex >= 0)
        {
            return resolvedIndex;
        }

        return options == null || options.Length == 0 ? 0 : Mathf.Clamp(fallbackIndex, 0, options.Length - 1);
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
