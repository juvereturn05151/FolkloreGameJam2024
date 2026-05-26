using UnityEngine;

public class HumanSpawnCustomizationResolver : MonoBehaviour
{
    [SerializeField] private CharacterCustomizationManager customizationManager;

    public CharacterSpriteSet GetSpriteSetForSpawn(HumanType type)
    {
        CharacterSpriteSet manualSpriteSet = CharacterCustomizationApplier.LoadSavedSpriteSet();
        if (manualSpriteSet != null && manualSpriteSet.HasAnySprite())
        {
            return manualSpriteSet;
        }

        ResolveManager();

        if (customizationManager == null)
        {
            return new CharacterSpriteSet();
        }

        return customizationManager.ResolveSpriteSet(type, customizationManager.GetHumanData(type).defaultSlot);
    }

    private void ResolveManager()
    {
        if (customizationManager == null)
        {
            customizationManager = CharacterCustomizationManager.Instance ?? FindAnyObjectByType<CharacterCustomizationManager>();
        }
    }
}
