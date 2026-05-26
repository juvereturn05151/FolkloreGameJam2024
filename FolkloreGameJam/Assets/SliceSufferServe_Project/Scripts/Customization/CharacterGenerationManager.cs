using System;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterGenerationManager : MonoBehaviour
{
    private const string TokenBalancePlayerPrefsKey = "CharacterGenerationTokens";
    private const int GenerationTokenCost = 1;

    [SerializeField] private CharacterCustomizationManager customizationManager;
    [SerializeField] private CharacterCustomizationUIController uiController;

    public int TokenBalance => PlayerPrefs.GetInt(TokenBalancePlayerPrefsKey, 0);
    public bool IsGenerating { get; private set; }

    public async void GenerateSelectedCharacter()
    {
        ResolveReferences();

        if (uiController == null || customizationManager == null)
        {
            OnGenerationFailed("Character generation is missing required scene references.");
            return;
        }

        if (!CanGenerate())
        {
            OnGenerationFailed("Not enough generation tokens.");
            return;
        }

        IsGenerating = true;

        try
        {
            HumanType humanType = uiController.SelectedHumanType;
            int slotIndex = uiController.SelectedGeneratedSlotIndex;
            string prompt = uiController.PromptText;

            CharacterGenerationResult result;
            if (uiController.SelectedGenerationMode == GenerationMode.WholeBody)
            {
                result = await GenerateWholeBodyAsync(humanType, slotIndex, prompt);
            }
            else
            {
                result = await GenerateSinglePartAsync(humanType, slotIndex, GetBodyPart(uiController.SelectedGenerationMode), prompt);
            }

            if (result != null && result.success)
            {
                OnGenerationSuccess(humanType, slotIndex, uiController.SelectedGenerationMode, result);
            }
            else
            {
                OnGenerationFailed(result != null ? result.errorMessage : "Generation failed.");
            }
        }
        catch (Exception exception)
        {
            OnGenerationFailed(exception.Message);
        }
        finally
        {
            IsGenerating = false;
        }
    }

    public bool CanGenerate()
    {
        return !IsGenerating && TokenBalance >= GenerationTokenCost;
    }

    public async Task<CharacterGenerationResult> GenerateWholeBodyAsync(HumanType humanType, int slotIndex, string prompt)
    {
        await Task.Yield();

        // Placeholder only:
        // The final version should call your backend server, and that backend should call Nano Banana.
        // Do not put a Nano Banana API key directly inside the Unity Android client.
        return CharacterGenerationResult.Failed("Nano Banana generation backend is not connected yet.");
    }

    public async Task<CharacterGenerationResult> GenerateSinglePartAsync(HumanType humanType, int slotIndex, BodyPartType part, string prompt)
    {
        await Task.Yield();

        // Placeholder only:
        // The final version should call your backend server, and that backend should call Nano Banana.
        // Do not put a Nano Banana API key directly inside the Unity Android client.
        return CharacterGenerationResult.Failed("Nano Banana generation backend is not connected yet.");
    }

    public void OnGenerationSuccess(HumanType humanType, int slotIndex, GenerationMode mode, CharacterGenerationResult result)
    {
        ResolveReferences();

        if (customizationManager == null || result == null)
        {
            return;
        }

        if (mode == GenerationMode.WholeBody)
        {
            customizationManager.SaveGeneratedWholeBody(humanType, slotIndex, result.headSpriteId, result.neckSpriteId, result.stomachSpriteId, result.legSpriteId);
        }
        else
        {
            customizationManager.SaveGeneratedPart(humanType, slotIndex, GetBodyPart(mode), result.singlePartSpriteId);
        }

        SpendTokenAfterSuccess();
        uiController?.OnGenerationDataChanged();
    }

    public void OnGenerationFailed(string reason)
    {
        Debug.LogWarning($"Character generation failed: {reason}");
    }

    public void AddTokens(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        PlayerPrefs.SetInt(TokenBalancePlayerPrefsKey, TokenBalance + amount);
        PlayerPrefs.Save();
    }

    private void SpendTokenAfterSuccess()
    {
        PlayerPrefs.SetInt(TokenBalancePlayerPrefsKey, Mathf.Max(0, TokenBalance - GenerationTokenCost));
        PlayerPrefs.Save();
    }

    private void ResolveReferences()
    {
        if (customizationManager == null)
        {
            customizationManager = CharacterCustomizationManager.Instance ?? FindAnyObjectByType<CharacterCustomizationManager>();
        }

        if (uiController == null)
        {
            uiController = FindAnyObjectByType<CharacterCustomizationUIController>();
        }
    }

    private static BodyPartType GetBodyPart(GenerationMode mode)
    {
        switch (mode)
        {
            case GenerationMode.HeadOnly:
                return BodyPartType.Head;
            case GenerationMode.NeckOnly:
                return BodyPartType.Neck;
            case GenerationMode.StomachOnly:
                return BodyPartType.Stomach;
            case GenerationMode.LegOnly:
                return BodyPartType.Leg;
            default:
                return BodyPartType.Head;
        }
    }
}

[Serializable]
public class CharacterGenerationResult
{
    public bool success;
    public string errorMessage;
    public string headSpriteId;
    public string neckSpriteId;
    public string stomachSpriteId;
    public string legSpriteId;
    public string singlePartSpriteId;

    public static CharacterGenerationResult Failed(string message)
    {
        return new CharacterGenerationResult
        {
            success = false,
            errorMessage = message
        };
    }

    public static CharacterGenerationResult WholeBody(string headId, string neckId, string stomachId, string legId)
    {
        return new CharacterGenerationResult
        {
            success = true,
            headSpriteId = headId,
            neckSpriteId = neckId,
            stomachSpriteId = stomachId,
            legSpriteId = legId
        };
    }

    public static CharacterGenerationResult SinglePart(string spriteId)
    {
        return new CharacterGenerationResult
        {
            success = true,
            singlePartSpriteId = spriteId
        };
    }
}
