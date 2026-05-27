using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationApplier : MonoBehaviour
{
    private const string HeadsResourcePath = "Characters/Human/Normal/Heads";
    private const string NecksResourcePath = "Characters/Human/Normal/Necks";
    private const string StomachsResourcePath = "Characters/Human/Normal/Stomach";
    private const string LegsResourcePath = "Characters/Human/Normal/Legs";

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
        Sprite[] resourceHeads = Resources.LoadAll<Sprite>(HeadsResourcePath);
        Sprite[] resourceNecks = Resources.LoadAll<Sprite>(NecksResourcePath);
        Sprite[] resourceStomachs = Resources.LoadAll<Sprite>(StomachsResourcePath);
        Sprite[] resourceLegs = Resources.LoadAll<Sprite>(LegsResourcePath);

        Sprite[] heads = HasSprites(resourceHeads) ? resourceHeads : headOverrides;
        Sprite[] necks = HasSprites(resourceNecks) ? resourceNecks : neckOverrides;
        Sprite[] bodies = HasSprites(resourceStomachs) ? resourceStomachs : bodyOverrides;
        Sprite[] legs = HasSprites(resourceLegs) ? resourceLegs : legsOverrides;

        heads ??= System.Array.Empty<Sprite>();
        necks ??= System.Array.Empty<Sprite>();
        bodies ??= System.Array.Empty<Sprite>();
        legs ??= System.Array.Empty<Sprite>();

        SortSpritesByDefaultFirst(heads);
        SortSpritesByDefaultFirst(necks);
        SortSpritesByDefaultFirst(bodies);
        SortSpritesByDefaultFirst(legs);

        Debug.Log($"Manual customization applier loaded normal human sprites: heads={heads.Length}, necks={necks.Length}, stomachs={bodies.Length}, legs={legs.Length}");

        CharacterCustomizationData data = CharacterCustomizationData.LoadFromPlayerPrefs();
        Debug.Log($"Manual customization applying selected indices: head={data.headIndex}, neck={data.neckIndex}, body={data.bodyIndex}, legs={data.legsIndex}");

        return new CharacterSpriteSet
        {
            head = GetSprite(heads, data.headIndex),
            neck = GetSprite(necks, data.neckIndex),
            stomach = GetSprite(bodies, data.bodyIndex),
            leg = GetSprite(legs, data.legsIndex)
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

    private static void SortSpritesByDefaultFirst(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length <= 1)
        {
            return;
        }

        System.Array.Sort(sprites, CompareSpritesByDefaultFirst);
    }

    private static int CompareSpritesByDefaultFirst(Sprite left, Sprite right)
    {
        if (left == right)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        bool leftDefault = IsDefaultSpriteName(left.name);
        bool rightDefault = IsDefaultSpriteName(right.name);

        if (leftDefault != rightDefault)
        {
            return leftDefault ? -1 : 1;
        }

        return string.CompareOrdinal(left.name, right.name);
    }

    private static bool IsDefaultSpriteName(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
        {
            return false;
        }

        return !System.Text.RegularExpressions.Regex.IsMatch(spriteName, @"\d+$");
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
