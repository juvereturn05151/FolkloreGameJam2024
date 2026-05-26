using UnityEngine;

public class HumanSpawnCustomizationResolver : MonoBehaviour
{
    [SerializeField] private CharacterCustomizationManager customizationManager;

    public CharacterSpriteSet GetSpriteSetForSpawn(HumanType type)
    {
        ResolveManager();

        if (customizationManager == null)
        {
            return new CharacterSpriteSet();
        }

        CharacterSpriteSlot slot = customizationManager.GetRandomEnabledSlotForSpawn(type);
        return customizationManager.ResolveSpriteSet(type, slot);
    }

    private void ResolveManager()
    {
        if (customizationManager == null)
        {
            customizationManager = CharacterCustomizationManager.Instance ?? FindAnyObjectByType<CharacterCustomizationManager>();
        }
    }
}
