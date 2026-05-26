using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationApplier : MonoBehaviour
{
    private const string HeadsResourcePath = "CharacterParts/Heads";
    private const string NecksResourcePath = "CharacterParts/Necks";
    private const string BodiesResourcePath = "CharacterParts/Bodies";
    private const string LegsResourcePath = "CharacterParts/Legs";

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
        Sprite[] heads = HasSprites(headOverrides) ? headOverrides : Resources.LoadAll<Sprite>(HeadsResourcePath);
        Sprite[] necks = HasSprites(neckOverrides) ? neckOverrides : Resources.LoadAll<Sprite>(NecksResourcePath);
        Sprite[] bodies = HasSprites(bodyOverrides) ? bodyOverrides : Resources.LoadAll<Sprite>(BodiesResourcePath);
        Sprite[] legs = HasSprites(legsOverrides) ? legsOverrides : Resources.LoadAll<Sprite>(LegsResourcePath);

        Debug.Log($"Manual customization applier loaded sprites: heads={heads.Length}, necks={necks.Length}, bodies={bodies.Length}, legs={legs.Length}");

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
