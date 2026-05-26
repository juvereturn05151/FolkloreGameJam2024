using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CharacterGenerationManager : MonoBehaviour
{
    private const string TokenBalancePlayerPrefsKey = "CharacterGenerationTokens";
    private const int GenerationTokenCost = 1;
    private const string GeneratedSpriteIdPrefix = "persistent://";
    private const string GeneratedSpriteRootFolder = "GeneratedCharacters";

    [Header("Backend")]
    [SerializeField] private string workerUrl = "https://slice-suffer-serve-api.juvereturn.workers.dev/";
    [SerializeField, Min(1)] private int maxPredictionPollAttempts = 60;
    [SerializeField, Min(0.1f)] private float predictionPollIntervalSeconds = 2f;

    [Header("References")]
    [SerializeField] private CharacterCustomizationManager customizationManager;
    [SerializeField] private CharacterCustomizationUIController uiController;
    [SerializeField] private Text generationStatusText;
    [SerializeField] private Image generatedWholeBodyPreviewImage;

    public int TokenBalance
    {
        get
        {
#if UNITY_EDITOR
            return int.MaxValue;
#else
            return PlayerPrefs.GetInt(TokenBalancePlayerPrefsKey, 0);
#endif
        }
    }
    public bool IsGenerating { get; private set; }

    private void Awake()
    {
        ResolveReferences();
        SetGenerationStatus("Ready to generate.");
    }

    public void GenerateSelectedCharacter()
    {
        if (IsGenerating)
        {
            SetGenerationStatus("Generation is already running...");
            return;
        }

        StartCoroutine(GenerateSelectedCharacterCoroutine());
    }

    public bool CanGenerate()
    {
#if UNITY_EDITOR
        return !IsGenerating;
#else
        return !IsGenerating && TokenBalance >= GenerationTokenCost;
#endif
    }

    public Task<CharacterGenerationResult> GenerateWholeBodyAsync(HumanType humanType, int slotIndex, string prompt)
    {
        return Task.FromResult(CharacterGenerationResult.Failed("Use GenerateSelectedCharacter so UnityWebRequest can run on the main thread."));
    }

    public Task<CharacterGenerationResult> GenerateSinglePartAsync(HumanType humanType, int slotIndex, BodyPartType part, string prompt)
    {
        return Task.FromResult(CharacterGenerationResult.Failed("Use GenerateSelectedCharacter so UnityWebRequest can run on the main thread."));
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
        SetGenerationStatus("Generation complete.");
        uiController?.OnGenerationDataChanged();
    }

    public void OnGenerationFailed(string reason)
    {
        Debug.LogWarning($"Character generation failed: {reason}");
        SetGenerationStatus($"Generation failed: {reason}");
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

    private IEnumerator GenerateSelectedCharacterCoroutine()
    {
        ResolveReferences();

        if (uiController == null || customizationManager == null)
        {
            OnGenerationFailed("Character generation is missing required scene references.");
            yield break;
        }

        if (!CanGenerate())
        {
            OnGenerationFailed("Not enough generation tokens.");
            yield break;
        }

        HumanType humanType = uiController.SelectedHumanType;
        int slotIndex = uiController.SelectedGeneratedSlotIndex;
        GenerationMode mode = uiController.SelectedGenerationMode;
        BodyPartType selectedPart = GetBodyPart(mode);
        string prompt = uiController.PromptText;

        IsGenerating = true;
        SetGenerationStatus($"Generating {GetGenerationStatusName(mode)}...");

        CharacterGenerationResult result = null;
        string failureReason = null;

        yield return SendGenerationRequest(humanType, slotIndex, mode, selectedPart, prompt, generatedResult =>
        {
            result = generatedResult;
        }, error =>
        {
            failureReason = error;
        });

        IsGenerating = false;

        if (result != null && result.success)
        {
            OnGenerationSuccess(humanType, slotIndex, mode, result);
        }
        else
        {
            OnGenerationFailed(!string.IsNullOrWhiteSpace(failureReason) ? failureReason : result?.errorMessage ?? "Generation failed.");
        }
    }

    private IEnumerator SendGenerationRequest(
        HumanType humanType,
        int slotIndex,
        GenerationMode mode,
        BodyPartType selectedPart,
        string prompt,
        Action<CharacterGenerationResult> onSuccess,
        Action<string> onFailed)
    {
        SourceImageData[] inputImages = BuildInputImages(humanType, mode, selectedPart);
        string[] inputImageDataUris = BuildInputImageDataUris(inputImages);
        PartImageDataUris partImageDataUris = BuildPartImageDataUris(inputImages);

        GenerateRequest requestData = new GenerateRequest
        {
            prompt = prompt ?? string.Empty,
            humanType = humanType.ToString(),
            generationMode = mode.ToString(),
            selectedPart = selectedPart.ToString(),
            image = inputImageDataUris.Length > 0 ? inputImageDataUris[0] : string.Empty,
            images = inputImageDataUris,
            head = partImageDataUris.head,
            neck = partImageDataUris.neck,
            stomach = partImageDataUris.stomach,
            leg = partImageDataUris.leg,
            inputImages = inputImages,
            input = new GenerateInput
            {
                prompt = prompt ?? string.Empty,
                humanType = humanType.ToString(),
                generationMode = mode.ToString(),
                selectedPart = selectedPart.ToString(),
                image = inputImageDataUris.Length > 0 ? inputImageDataUris[0] : string.Empty,
                images = inputImageDataUris,
                head = partImageDataUris.head,
                neck = partImageDataUris.neck,
                stomach = partImageDataUris.stomach,
                leg = partImageDataUris.leg
            }
        };

        string json = JsonUtility.ToJson(requestData);
        Debug.Log($"Character generation request JSON preview:\n{BuildRequestDebugPreview(json)}");

        using UnityWebRequest request = new UnityWebRequest(workerUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        SetRequestHeaders(request);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onFailed?.Invoke($"Backend Error: {request.error}");
            yield break;
        }

        Debug.Log("Character generation raw response:\n" + SanitizeGenerationLog(request.downloadHandler.text));

        GeneratedPartsResponse generatedPartsResponse;
        ReplicateResponse response;
        try
        {
            generatedPartsResponse = JsonUtility.FromJson<GeneratedPartsResponse>(request.downloadHandler.text);
            response = JsonUtility.FromJson<ReplicateResponse>(request.downloadHandler.text);
        }
        catch (Exception exception)
        {
            onFailed?.Invoke($"Could not parse backend response: {exception.Message}");
            yield break;
        }

        if (generatedPartsResponse == null && response == null)
        {
            onFailed?.Invoke("Backend response was empty.");
            yield break;
        }

        string finalResponseJson = request.downloadHandler.text;
        ReplicateResponse finalResponse = response;
        string pollingFailure = null;

        yield return PollPredictionUntilComplete(response, request.downloadHandler.text, (completedResponse, completedJson) =>
        {
            finalResponse = completedResponse;
            finalResponseJson = completedJson;
        }, error =>
        {
            pollingFailure = error;
        });

        if (!string.IsNullOrWhiteSpace(pollingFailure))
        {
            onFailed?.Invoke(pollingFailure);
            yield break;
        }

        Debug.Log("Final prediction JSON: " + SanitizeGenerationLog(finalResponseJson));

        try
        {
            generatedPartsResponse = JsonUtility.FromJson<GeneratedPartsResponse>(finalResponseJson);
            response = finalResponse ?? JsonUtility.FromJson<ReplicateResponse>(finalResponseJson);
        }
        catch (Exception exception)
        {
            onFailed?.Invoke($"Could not parse completed backend response: {exception.Message}");
            yield break;
        }

        SetGenerationStatus("Downloading generated sprite...");

        if (mode == GenerationMode.WholeBody)
        {
            yield return DownloadWholeBodyResult(generatedPartsResponse, response, finalResponseJson, partImageDataUris, humanType, slotIndex, onSuccess, onFailed);
        }
        else
        {
            yield return DownloadSinglePartResult(generatedPartsResponse, response, finalResponseJson, partImageDataUris, humanType, slotIndex, selectedPart, onSuccess, onFailed);
        }
    }

    private IEnumerator DownloadWholeBodyResult(
        GeneratedPartsResponse generatedPartsResponse,
        ReplicateResponse response,
        string responseJson,
        PartImageDataUris sourcePartImageDataUris,
        HumanType humanType,
        int slotIndex,
        Action<CharacterGenerationResult> onSuccess,
        Action<string> onFailed)
    {
        string[] outputUrls = GetOutputUrls(response, responseJson);
        if (!HasCompletePartUrls(generatedPartsResponse, response, responseJson))
        {
            if (outputUrls.Length == 1)
            {
                Debug.Log("Using single output image URL: " + outputUrls[0]);
                yield return DownloadAndSplitWholeBodyImage(outputUrls[0], humanType, slotIndex, onSuccess, onFailed);
                yield break;
            }

            if (IsSucceededStatus(GetPredictionStatus(response)) && outputUrls.Length == 0)
            {
                onFailed?.Invoke("Prediction succeeded but returned no images. Check backend/model output.");
                yield break;
            }
        }

        if (!TryGetValidatedPartUrl(generatedPartsResponse, response, responseJson, sourcePartImageDataUris, BodyPartType.Head, out string headUrl, out string error)) { onFailed?.Invoke(error); yield break; }
        if (!TryGetValidatedPartUrl(generatedPartsResponse, response, responseJson, sourcePartImageDataUris, BodyPartType.Neck, out string neckUrl, out error)) { onFailed?.Invoke(error); yield break; }
        if (!TryGetValidatedPartUrl(generatedPartsResponse, response, responseJson, sourcePartImageDataUris, BodyPartType.Stomach, out string stomachUrl, out error)) { onFailed?.Invoke(error); yield break; }
        if (!TryGetValidatedPartUrl(generatedPartsResponse, response, responseJson, sourcePartImageDataUris, BodyPartType.Leg, out string legUrl, out error)) { onFailed?.Invoke(error); yield break; }

        string headId = null;
        string neckId = null;
        string stomachId = null;
        string legId = null;
        string failure = null;

        yield return DownloadAndSaveImage(headUrl, humanType, slotIndex, BodyPartType.Head, id => headId = id, error => failure = error);
        if (!string.IsNullOrWhiteSpace(failure)) { onFailed?.Invoke(failure); yield break; }

        yield return DownloadAndSaveImage(neckUrl, humanType, slotIndex, BodyPartType.Neck, id => neckId = id, error => failure = error);
        if (!string.IsNullOrWhiteSpace(failure)) { onFailed?.Invoke(failure); yield break; }

        yield return DownloadAndSaveImage(stomachUrl, humanType, slotIndex, BodyPartType.Stomach, id => stomachId = id, error => failure = error);
        if (!string.IsNullOrWhiteSpace(failure)) { onFailed?.Invoke(failure); yield break; }

        yield return DownloadAndSaveImage(legUrl, humanType, slotIndex, BodyPartType.Leg, id => legId = id, error => failure = error);
        if (!string.IsNullOrWhiteSpace(failure)) { onFailed?.Invoke(failure); yield break; }

        onSuccess?.Invoke(CharacterGenerationResult.WholeBody(headId, neckId, stomachId, legId));
    }

    private IEnumerator PollPredictionUntilComplete(
        ReplicateResponse initialResponse,
        string initialJson,
        Action<ReplicateResponse, string> onCompleted,
        Action<string> onFailed)
    {
        string status = GetPredictionStatus(initialResponse);
        Debug.Log($"Character generation prediction status: {(!string.IsNullOrWhiteSpace(status) ? status : "none")}");

        if (string.IsNullOrWhiteSpace(status) || IsSucceededStatus(status))
        {
            onCompleted?.Invoke(initialResponse, initialJson);
            yield break;
        }

        if (IsFailedStatus(status))
        {
            onFailed?.Invoke(GetPredictionFailureMessage(initialResponse));
            yield break;
        }

        if (!IsPendingStatus(status))
        {
            onCompleted?.Invoke(initialResponse, initialJson);
            yield break;
        }

        string pollingUrl = initialResponse != null ? initialResponse.pollUrl : string.Empty;
        if (string.IsNullOrWhiteSpace(pollingUrl))
        {
            onFailed?.Invoke($"Generation is {status}, but the backend response did not include pollUrl for Worker polling.");
            yield break;
        }

        SetGenerationStatus("Generation is processing...");

        for (int attempt = 1; attempt <= maxPredictionPollAttempts; attempt++)
        {
            yield return new WaitForSeconds(predictionPollIntervalSeconds);

            Debug.Log($"Character generation polling request ({attempt}/{maxPredictionPollAttempts}): {BuildSafeUrlLogTarget(pollingUrl)}");

            using UnityWebRequest pollRequest = UnityWebRequest.Get(pollingUrl);
            SetRequestHeaders(pollRequest);
            yield return pollRequest.SendWebRequest();

            if (pollRequest.result != UnityWebRequest.Result.Success)
            {
                onFailed?.Invoke($"Prediction polling failed ({BuildSafeUrlLogTarget(pollingUrl)}): HTTP {pollRequest.responseCode} {pollRequest.error}");
                yield break;
            }

            string pollJson = pollRequest.downloadHandler.text;

            ReplicateResponse pollResponse;
            try
            {
                pollResponse = JsonUtility.FromJson<ReplicateResponse>(pollJson);
            }
            catch (Exception exception)
            {
                onFailed?.Invoke($"Could not parse prediction polling response: {exception.Message}");
                yield break;
            }

            status = GetPredictionStatus(pollResponse);
            Debug.Log($"Character generation polling status ({attempt}/{maxPredictionPollAttempts}): {(!string.IsNullOrWhiteSpace(status) ? status : "none")}");
            SetGenerationStatus($"Generation is {(!string.IsNullOrWhiteSpace(status) ? status : "processing")}...");

            if (IsSucceededStatus(status))
            {
                onCompleted?.Invoke(pollResponse, pollJson);
                yield break;
            }

            if (IsFailedStatus(status))
            {
                onFailed?.Invoke(GetPredictionFailureMessage(pollResponse));
                yield break;
            }
        }

        onFailed?.Invoke($"Generation timed out after {maxPredictionPollAttempts} polling attempt(s).");
    }

    private IEnumerator DownloadAndSplitWholeBodyImage(
        string imageUrl,
        HumanType humanType,
        int slotIndex,
        Action<CharacterGenerationResult> onSuccess,
        Action<string> onFailed)
    {
        if (!TryValidateImageUrl(imageUrl, BodyPartType.Head, out imageUrl, out string validationError))
        {
            onFailed?.Invoke($"Generated whole body image URL is invalid. {validationError}");
            yield break;
        }

        Texture2D wholeBodyTexture;
        if (imageUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            byte[] imageBytes = DecodeDataUri(imageUrl);
            if (imageBytes == null || imageBytes.Length == 0)
            {
                onFailed?.Invoke("Generated whole body image data URL was invalid.");
                yield break;
            }

            wholeBodyTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!wholeBodyTexture.LoadImage(imageBytes))
            {
                UnityEngine.Object.Destroy(wholeBodyTexture);
                onFailed?.Invoke("Generated whole body image could not be decoded.");
                yield break;
            }
        }
        else
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onFailed?.Invoke($"Whole body image download failed: {request.error}");
                yield break;
            }

            wholeBodyTexture = DownloadHandlerTexture.GetContent(request);
            if (wholeBodyTexture == null)
            {
                onFailed?.Invoke("Generated whole body image could not be decoded.");
                yield break;
            }
        }

        AssignWholeBodyPreviewSprite(wholeBodyTexture);

        string failure = null;
        string headId = SaveCroppedGeneratedImage(wholeBodyTexture, humanType, slotIndex, BodyPartType.Head, 0.74f, 1f, error => failure = error);
        string neckId = SaveCroppedGeneratedImage(wholeBodyTexture, humanType, slotIndex, BodyPartType.Neck, 0.58f, 0.74f, error => failure = error);
        string stomachId = SaveCroppedGeneratedImage(wholeBodyTexture, humanType, slotIndex, BodyPartType.Stomach, 0.30f, 0.58f, error => failure = error);
        string legId = SaveCroppedGeneratedImage(wholeBodyTexture, humanType, slotIndex, BodyPartType.Leg, 0f, 0.30f, error => failure = error);
        if (generatedWholeBodyPreviewImage == null)
        {
            UnityEngine.Object.Destroy(wholeBodyTexture);
        }

        if (!string.IsNullOrWhiteSpace(failure))
        {
            onFailed?.Invoke(failure);
            yield break;
        }

        onSuccess?.Invoke(CharacterGenerationResult.WholeBody(headId, neckId, stomachId, legId));
    }

    private IEnumerator DownloadSinglePartResult(
        GeneratedPartsResponse generatedPartsResponse,
        ReplicateResponse response,
        string responseJson,
        PartImageDataUris sourcePartImageDataUris,
        HumanType humanType,
        int slotIndex,
        BodyPartType part,
        Action<CharacterGenerationResult> onSuccess,
        Action<string> onFailed)
    {
        if (!TryGetValidatedPartUrl(generatedPartsResponse, response, responseJson, sourcePartImageDataUris, part, out string imageUrl, out string error))
        {
            string[] outputUrls = GetOutputUrls(response, responseJson);
            if (outputUrls.Length == 1
                && TryValidateImageUrl(outputUrls[0], part, out imageUrl, out error)
                && !string.Equals(imageUrl, GetSourcePartImageUrl(sourcePartImageDataUris, part), StringComparison.Ordinal))
            {
                // Single-part generation can return one Replicate output URL instead of a named part field.
            }
            else
            {
                onFailed?.Invoke(error);
                yield break;
            }
        }

        string spriteId = null;
        string failure = null;
        yield return DownloadAndSaveImage(imageUrl, humanType, slotIndex, part, id => spriteId = id, error => failure = error);

        if (!string.IsNullOrWhiteSpace(failure))
        {
            onFailed?.Invoke(failure);
            yield break;
        }

        onSuccess?.Invoke(CharacterGenerationResult.SinglePart(spriteId));
    }

    private IEnumerator DownloadAndSaveImage(
        string imageUrl,
        HumanType humanType,
        int slotIndex,
        BodyPartType part,
        Action<string> onSuccess,
        Action<string> onFailed)
    {
        if (!TryValidateImageUrl(imageUrl, part, out imageUrl, out string validationError))
        {
            onFailed?.Invoke(validationError);
            yield break;
        }

        if (imageUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            byte[] imageBytes = DecodeDataUri(imageUrl);
            if (imageBytes == null || imageBytes.Length == 0)
            {
                onFailed?.Invoke($"Generated {GetPartJsonName(part)} image data URL was invalid.");
                yield break;
            }

            SaveGeneratedImageBytes(imageBytes, humanType, slotIndex, part, onSuccess);
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Get(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onFailed?.Invoke($"Image download failed: {request.error}");
            yield break;
        }

        SaveGeneratedImageBytes(request.downloadHandler.data, humanType, slotIndex, part, onSuccess);
    }

    private SourceImageData[] BuildInputImages(HumanType humanType, GenerationMode mode, BodyPartType selectedPart)
    {
        if (mode == GenerationMode.WholeBody)
        {
            return new[]
            {
                BuildInputImage(humanType, BodyPartType.Head),
                BuildInputImage(humanType, BodyPartType.Neck),
                BuildInputImage(humanType, BodyPartType.Stomach),
                BuildInputImage(humanType, BodyPartType.Leg)
            };
        }

        return new[]
        {
            BuildInputImage(humanType, selectedPart)
        };
    }

    private SourceImageData BuildInputImage(HumanType humanType, BodyPartType part)
    {
        string resourcePath = GetSourceImageResourcePath(humanType, part);
        Texture2D texture = LoadSourceTexture(resourcePath);

        return new SourceImageData
        {
            part = part.ToString(),
            resourcePath = resourcePath,
            mimeType = "image/png",
            imageBase64 = texture != null ? Convert.ToBase64String(TextureToPng(texture)) : string.Empty
        };
    }

    private static string[] BuildInputImageDataUris(SourceImageData[] inputImages)
    {
        if (inputImages == null)
        {
            return Array.Empty<string>();
        }

        string[] dataUris = new string[inputImages.Length];
        for (int i = 0; i < inputImages.Length; i++)
        {
            SourceImageData imageData = inputImages[i];
            dataUris[i] = imageData != null && !string.IsNullOrWhiteSpace(imageData.imageBase64)
                ? $"data:{imageData.mimeType};base64,{imageData.imageBase64}"
                : string.Empty;
        }

        return dataUris;
    }

    private static PartImageDataUris BuildPartImageDataUris(SourceImageData[] inputImages)
    {
        PartImageDataUris partImageDataUris = new PartImageDataUris();
        if (inputImages == null)
        {
            return partImageDataUris;
        }

        for (int i = 0; i < inputImages.Length; i++)
        {
            SourceImageData imageData = inputImages[i];
            if (imageData == null || string.IsNullOrWhiteSpace(imageData.imageBase64))
            {
                continue;
            }

            string dataUri = $"data:{imageData.mimeType};base64,{imageData.imageBase64}";
            if (string.Equals(imageData.part, BodyPartType.Head.ToString(), StringComparison.Ordinal))
            {
                partImageDataUris.head = dataUri;
            }
            else if (string.Equals(imageData.part, BodyPartType.Neck.ToString(), StringComparison.Ordinal))
            {
                partImageDataUris.neck = dataUri;
            }
            else if (string.Equals(imageData.part, BodyPartType.Stomach.ToString(), StringComparison.Ordinal))
            {
                partImageDataUris.stomach = dataUri;
            }
            else if (string.Equals(imageData.part, BodyPartType.Leg.ToString(), StringComparison.Ordinal))
            {
                partImageDataUris.leg = dataUri;
            }
        }

        return partImageDataUris;
    }

    private static string BuildRequestDebugPreview(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return string.Empty;
        }

        string preview = SanitizeGenerationLog(json);
        int maxLength = 1400;
        if (preview.Length > maxLength)
        {
            preview = preview.Substring(0, maxLength) + "...";
        }

        return preview;
    }

    private static void SetRequestHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("Content-Type", "application/json");
    }

    private static string BuildSafeUrlLogTarget(string requestUrl)
    {
        if (string.IsNullOrWhiteSpace(requestUrl))
        {
            return "unknown URL";
        }

        try
        {
            Uri uri = new Uri(requestUrl);
            return $"{uri.Host}{uri.AbsolutePath}";
        }
        catch (UriFormatException)
        {
            return "invalid URL";
        }
    }

    private static string SanitizeGenerationLog(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        string sanitized = Regex.Replace(
            text,
            "\"replicate_api_token\"\\s*:\\s*\"[^\"]*\"",
            "\"replicate_api_token\":\"[REDACTED]\"",
            RegexOptions.IgnoreCase);

        sanitized = Regex.Replace(
            sanitized,
            "\"Authorization\"\\s*:\\s*\"Bearer [^\"]*\"",
            "\"Authorization\":\"Bearer [REDACTED]\"",
            RegexOptions.IgnoreCase);

        sanitized = Regex.Replace(
            sanitized,
            "data:image/[^;\"\\s]+;base64,[^\"]+",
            "data:image/png;base64,...",
            RegexOptions.IgnoreCase);

        return sanitized;
    }

    private void SpendTokenAfterSuccess()
    {
#if UNITY_EDITOR
        return;
#else
        PlayerPrefs.SetInt(TokenBalancePlayerPrefsKey, Mathf.Max(0, TokenBalance - GenerationTokenCost));
        PlayerPrefs.Save();
#endif
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

        if (generationStatusText == null)
        {
            generationStatusText = FindTextByName("GenerationStatusText");
        }
    }

    private void SetGenerationStatus(string status)
    {
        if (generationStatusText == null)
        {
            generationStatusText = FindTextByName("GenerationStatusText");
        }

        if (generationStatusText != null)
        {
            generationStatusText.text = status;
        }
    }

    private static Text FindTextByName(string objectName)
    {
        Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == objectName)
            {
                return texts[i];
            }
        }

        return null;
    }

    private static string GetGenerationStatusName(GenerationMode mode)
    {
        switch (mode)
        {
            case GenerationMode.WholeBody:
                return "whole body";
            case GenerationMode.HeadOnly:
                return "head";
            case GenerationMode.NeckOnly:
                return "neck";
            case GenerationMode.StomachOnly:
                return "stomach";
            case GenerationMode.LegOnly:
                return "leg";
            default:
                return "character";
        }
    }

    private static string GetPredictionStatus(ReplicateResponse response)
    {
        return response != null ? response.status : string.Empty;
    }

    private static bool IsPendingStatus(string status)
    {
        return string.Equals(status, "starting", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "processing", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSucceededStatus(string status)
    {
        return string.Equals(status, "succeeded", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFailedStatus(string status)
    {
        return string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "canceled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPredictionFailureMessage(ReplicateResponse response)
    {
        string status = GetPredictionStatus(response);
        string reason = response != null ? response.error : string.Empty;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            return $"Generation {status}: {reason}";
        }

        return !string.IsNullOrWhiteSpace(status) ? $"Generation {status}." : "Generation failed.";
    }

    private static bool TryGetValidatedPartUrl(
        GeneratedPartsResponse generatedPartsResponse,
        ReplicateResponse response,
        string responseJson,
        PartImageDataUris sourcePartImageDataUris,
        BodyPartType part,
        out string imageUrl,
        out string error)
    {
        imageUrl = GetPartUrl(generatedPartsResponse, response, responseJson, part);
        if (!TryValidateImageUrl(imageUrl, part, out imageUrl, out error))
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                error = GetMissingPartUrlError(response, responseJson, part);
            }

            return false;
        }

        string sourceImageUrl = GetSourcePartImageUrl(sourcePartImageDataUris, part);
        if (!string.IsNullOrWhiteSpace(sourceImageUrl)
            && string.Equals(imageUrl, sourceImageUrl, StringComparison.Ordinal))
        {
            string partName = GetPartJsonName(part);
            error = $"Generated {partName} image URL matches the source input. Backend appears to have echoed the request instead of returning generated output.";
            return false;
        }

        return true;
    }

    private static string GetPartUrl(
        GeneratedPartsResponse generatedPartsResponse,
        ReplicateResponse response,
        string responseJson,
        BodyPartType part)
    {
        string structuredUrl = GetStructuredPartUrl(generatedPartsResponse, part);
        if (!string.IsNullOrWhiteSpace(structuredUrl))
        {
            return NormalizeJsonString(structuredUrl);
        }

        structuredUrl = GetStructuredPartUrl(response, part);
        if (!string.IsNullOrWhiteSpace(structuredUrl))
        {
            return NormalizeJsonString(structuredUrl);
        }

        structuredUrl = GetStructuredOutputPartUrl(response, responseJson, part);
        if (!string.IsNullOrWhiteSpace(structuredUrl))
        {
            return NormalizeJsonString(structuredUrl);
        }

        return GetNamedImageUrlFromJson(responseJson, GetPartJsonName(part));
    }

    private static string GetStructuredPartUrl(GeneratedPartsResponse response, BodyPartType part)
    {
        if (response == null)
        {
            return string.Empty;
        }

        switch (part)
        {
            case BodyPartType.Head:
                return response.head;
            case BodyPartType.Neck:
                return response.neck;
            case BodyPartType.Stomach:
                return response.stomach;
            case BodyPartType.Leg:
                return response.leg;
            default:
                return string.Empty;
        }
    }

    private static string GetStructuredPartUrl(ReplicateResponse response, BodyPartType part)
    {
        if (response == null)
        {
            return string.Empty;
        }

        switch (part)
        {
            case BodyPartType.Head:
                return response.head;
            case BodyPartType.Neck:
                return response.neck;
            case BodyPartType.Stomach:
                return response.stomach;
            case BodyPartType.Leg:
                return response.leg;
            default:
                return string.Empty;
        }
    }

    private static string GetStructuredOutputPartUrl(ReplicateResponse response, string responseJson, BodyPartType part)
    {
        string[] outputUrls = GetOutputUrls(response, responseJson);
        int outputIndex = GetPartOutputIndex(part);
        if (outputIndex < 0 || outputUrls.Length < 4 || outputIndex >= outputUrls.Length)
        {
            return string.Empty;
        }

        return outputUrls[outputIndex];
    }

    private static int GetPartOutputIndex(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head:
                return 0;
            case BodyPartType.Neck:
                return 1;
            case BodyPartType.Stomach:
                return 2;
            case BodyPartType.Leg:
                return 3;
            default:
                return -1;
        }
    }

    private static bool HasCompletePartUrls(GeneratedPartsResponse generatedPartsResponse, ReplicateResponse response, string responseJson)
    {
        return !string.IsNullOrWhiteSpace(GetPartUrl(generatedPartsResponse, response, responseJson, BodyPartType.Head))
            && !string.IsNullOrWhiteSpace(GetPartUrl(generatedPartsResponse, response, responseJson, BodyPartType.Neck))
            && !string.IsNullOrWhiteSpace(GetPartUrl(generatedPartsResponse, response, responseJson, BodyPartType.Stomach))
            && !string.IsNullOrWhiteSpace(GetPartUrl(generatedPartsResponse, response, responseJson, BodyPartType.Leg));
    }

    private static string[] GetOutputUrls(ReplicateResponse response, string responseJson)
    {
        if (response != null && response.output != null && response.output.Length > 0)
        {
            string[] normalizedOutput = new string[response.output.Length];
            for (int i = 0; i < response.output.Length; i++)
            {
                normalizedOutput[i] = NormalizeJsonString(response.output[i]);
            }

            return normalizedOutput;
        }

        SingleOutputResponse singleOutputResponse = null;
        try
        {
            singleOutputResponse = JsonUtility.FromJson<SingleOutputResponse>(responseJson);
        }
        catch (Exception)
        {
            singleOutputResponse = null;
        }

        if (singleOutputResponse != null && !string.IsNullOrWhiteSpace(singleOutputResponse.output))
        {
            return new[] { NormalizeJsonString(singleOutputResponse.output) };
        }

        return GetOutputUrlsFromJson(responseJson);
    }

    private static string[] GetOutputUrlsFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        Match arrayMatch = Regex.Match(json, "\"output\"\\s*:\\s*\\[(?<items>.*?)\\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (arrayMatch.Success)
        {
            MatchCollection itemMatches = Regex.Matches(arrayMatch.Groups["items"].Value, "\"(?<url>(?:https?:\\\\?/\\\\?/|data:image/)[^\"]+)\"", RegexOptions.IgnoreCase);
            string[] urls = new string[itemMatches.Count];
            for (int i = 0; i < itemMatches.Count; i++)
            {
                urls[i] = NormalizeJsonString(itemMatches[i].Groups["url"].Value);
            }

            return urls;
        }

        Match stringMatch = Regex.Match(json, "\"output\"\\s*:\\s*\"(?<url>(?:https?:\\\\?/\\\\?/|data:image/)[^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (stringMatch.Success)
        {
            return new[] { NormalizeJsonString(stringMatch.Groups["url"].Value) };
        }

        return Array.Empty<string>();
    }

    private static string GetMissingPartUrlError(ReplicateResponse response, string responseJson, BodyPartType part)
    {
        string partName = GetPartJsonName(part);
        int outputIndex = GetPartOutputIndex(part);
        string[] outputUrls = GetOutputUrls(response, responseJson);

        if (response != null && !string.IsNullOrWhiteSpace(response.error))
        {
            return $"Generated {partName} image URL is missing because the backend returned an error: {response.error}";
        }

        if (response != null && !string.IsNullOrWhiteSpace(response.status) && !string.Equals(response.status, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return $"Generated {partName} image URL is missing because the backend response status is '{response.status}'. Expected backend field '{partName}' or output[{outputIndex}] after generation succeeds.";
        }

        if (IsSucceededStatus(GetPredictionStatus(response)) && outputUrls.Length == 0)
        {
            return "Prediction succeeded but returned no images. Check backend/model output.";
        }

        if (outputUrls.Length > 0)
        {
            return $"Generated {partName} image URL is missing. Expected backend field '{partName}' or output[{outputIndex}], but backend output had {outputUrls.Length} image item(s).";
        }

        return $"Generated {partName} image URL is missing. Expected backend field '{partName}' or output[{outputIndex}].";
    }

    private static string GetSourcePartImageUrl(PartImageDataUris sourcePartImageDataUris, BodyPartType part)
    {
        if (sourcePartImageDataUris == null)
        {
            return string.Empty;
        }

        switch (part)
        {
            case BodyPartType.Head:
                return sourcePartImageDataUris.head;
            case BodyPartType.Neck:
                return sourcePartImageDataUris.neck;
            case BodyPartType.Stomach:
                return sourcePartImageDataUris.stomach;
            case BodyPartType.Leg:
                return sourcePartImageDataUris.leg;
            default:
                return string.Empty;
        }
    }

    private static string GetNamedImageUrlFromJson(string json, string name)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        string escapedName = Regex.Escape(name);
        string pattern = $"\"{escapedName}\"\\s*:\\s*(?:\\{{[^}}]*?\"(?:url|image|image_url)\"\\s*:\\s*)?\"(?<url>(?:https?:\\\\?/\\\\?/|data:image/)[^\"]+)\"";
        Match match = Regex.Match(json, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? NormalizeJsonString(match.Groups["url"].Value) : string.Empty;
    }

    private static bool TryValidateImageUrl(string imageUrl, BodyPartType part, out string normalizedUrl, out string error)
    {
        string partName = GetPartJsonName(part);
        normalizedUrl = NormalizeJsonString(imageUrl);
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedUrl))
        {
            error = $"Generated {partName} image URL is missing. Expected backend field '{partName}'.";
            return false;
        }

        if (normalizedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || normalizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!normalizedUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            error = $"Generated {partName} image URL is invalid. Expected http(s) URL or data:image URL.";
            return false;
        }

        int commaIndex = normalizedUrl.IndexOf(',');
        if (commaIndex < 0 || commaIndex >= normalizedUrl.Length - 1)
        {
            error = $"Generated {partName} image data URL is invalid. Missing image payload.";
            return false;
        }

        string metadata = normalizedUrl.Substring(0, commaIndex);
        if (metadata.IndexOf(";base64", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            string payload = Regex.Replace(normalizedUrl.Substring(commaIndex + 1), "\\s+", string.Empty);
            try
            {
                Convert.FromBase64String(payload);
            }
            catch (FormatException)
            {
                error = $"Generated {partName} image data URL is invalid. Base64 payload could not be decoded.";
                return false;
            }
        }

        return true;
    }

    private static string NormalizeJsonString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        try
        {
            return Regex.Unescape(value).Replace("\\/", "/");
        }
        catch (ArgumentException)
        {
            return value.Replace("\\/", "/");
        }
    }

    private static string GetPartJsonName(BodyPartType part)
    {
        return part == BodyPartType.Stomach ? "stomach" : part.ToString().ToLowerInvariant();
    }

    private void AssignWholeBodyPreviewSprite(Texture2D texture)
    {
        if (generatedWholeBodyPreviewImage == null || texture == null)
        {
            return;
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);

        generatedWholeBodyPreviewImage.sprite = sprite;
        generatedWholeBodyPreviewImage.enabled = true;
        generatedWholeBodyPreviewImage.preserveAspect = true;
    }

    private static void SaveGeneratedImageBytes(
        byte[] imageBytes,
        HumanType humanType,
        int slotIndex,
        BodyPartType part,
        Action<string> onSuccess)
    {
        string relativePath = BuildGeneratedRelativePath(humanType, slotIndex, part);
        string fullPath = Path.Combine(Application.persistentDataPath, relativePath);
        string directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(fullPath, imageBytes);
        onSuccess?.Invoke(GeneratedSpriteIdPrefix + relativePath.Replace('\\', '/'));
    }

    private static string SaveCroppedGeneratedImage(
        Texture2D sourceTexture,
        HumanType humanType,
        int slotIndex,
        BodyPartType part,
        float normalizedBottom,
        float normalizedTop,
        Action<string> onFailed)
    {
        int y = Mathf.Clamp(Mathf.FloorToInt(sourceTexture.height * normalizedBottom), 0, sourceTexture.height - 1);
        int top = Mathf.Clamp(Mathf.CeilToInt(sourceTexture.height * normalizedTop), y + 1, sourceTexture.height);
        int height = Mathf.Max(1, top - y);

        try
        {
            Color[] pixels = sourceTexture.GetPixels(0, y, sourceTexture.width, height);
            Texture2D croppedTexture = new Texture2D(sourceTexture.width, height, TextureFormat.RGBA32, false);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            byte[] pngBytes = croppedTexture.EncodeToPNG();
            UnityEngine.Object.Destroy(croppedTexture);

            string spriteId = null;
            SaveGeneratedImageBytes(pngBytes, humanType, slotIndex, part, id => spriteId = id);
            return spriteId;
        }
        catch (Exception exception)
        {
            onFailed?.Invoke($"Could not crop generated whole body image into {GetPartJsonName(part)}: {exception.Message}");
            return string.Empty;
        }
    }

    private static byte[] DecodeDataUri(string dataUri)
    {
        dataUri = NormalizeJsonString(dataUri);
        int commaIndex = dataUri.IndexOf(',');
        if (commaIndex < 0 || commaIndex >= dataUri.Length - 1)
        {
            return null;
        }

        string metadata = dataUri.Substring(0, commaIndex);
        string payload = dataUri.Substring(commaIndex + 1).Trim();

        try
        {
            if (metadata.IndexOf(";base64", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                payload = Regex.Replace(payload, "\\s+", string.Empty);
                return Convert.FromBase64String(payload);
            }

            return Encoding.UTF8.GetBytes(UnityWebRequest.UnEscapeURL(payload));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string BuildGeneratedRelativePath(HumanType humanType, int slotIndex, BodyPartType part)
    {
        return Path.Combine(
            GeneratedSpriteRootFolder,
            humanType.ToString(),
            $"Slot{slotIndex + 1:00}",
            $"{part}.png");
    }

    private static string GetSourceImageResourcePath(HumanType humanType, BodyPartType part)
    {
        string folder = GetHumanResourceFolder(humanType);
        string prefix = GetHumanFilePrefix(humanType);
        string partName = part == BodyPartType.Stomach ? "stomach" : part.ToString().ToLowerInvariant();
        return $"Characters/Human/{folder}/{prefix}_{partName}";
    }

    private static string GetHumanResourceFolder(HumanType humanType)
    {
        switch (humanType)
        {
            case HumanType.RockThrowerHuman:
                return "RockThrower";
            case HumanType.ObeseHuman:
                return "Obese";
            case HumanType.RobotHuman:
                return "Robot";
            default:
                return "Normal";
        }
    }

    private static string GetHumanFilePrefix(HumanType humanType)
    {
        switch (humanType)
        {
            case HumanType.RockThrowerHuman:
                return "rock";
            case HumanType.ObeseHuman:
                return "obese";
            case HumanType.RobotHuman:
                return "robot";
            default:
                return "normal";
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

    private static Texture2D LoadSourceTexture(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite != null)
        {
            return sprite.texture;
        }

        return Resources.Load<Texture2D>(resourcePath);
    }

    private static byte[] TextureToPng(Texture2D texture)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;

        try
        {
            Graphics.Blit(texture, temporary);
            RenderTexture.active = temporary;

            Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            readableTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            readableTexture.Apply();

            byte[] pngBytes = readableTexture.EncodeToPNG();
            UnityEngine.Object.Destroy(readableTexture);
            return pngBytes;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
        }
    }

    [Serializable]
    private class GenerateRequest
    {
        public string prompt;
        public string humanType;
        public string generationMode;
        public string selectedPart;
        public string image;
        public string[] images;
        public string head;
        public string neck;
        public string stomach;
        public string leg;
        public GenerateInput input;
        public SourceImageData[] inputImages;
    }

    [Serializable]
    private class GenerateInput
    {
        public string prompt;
        public string humanType;
        public string generationMode;
        public string selectedPart;
        public string image;
        public string[] images;
        public string head;
        public string neck;
        public string stomach;
        public string leg;
    }

    private class PartImageDataUris
    {
        public string head = string.Empty;
        public string neck = string.Empty;
        public string stomach = string.Empty;
        public string leg = string.Empty;
    }

    [Serializable]
    private class SourceImageData
    {
        public string part;
        public string resourcePath;
        public string mimeType;
        public string imageBase64;
    }

    [Serializable]
    private class GeneratedPartsResponse
    {
        public string head;
        public string neck;
        public string stomach;
        public string leg;
    }

    [Serializable]
    private class ReplicateResponse
    {
        public string[] output;
        public string head;
        public string neck;
        public string stomach;
        public string leg;
        public string status;
        public string error;
        public string pollUrl;
    }

    [Serializable]
    private class SingleOutputResponse
    {
        public string output;
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
