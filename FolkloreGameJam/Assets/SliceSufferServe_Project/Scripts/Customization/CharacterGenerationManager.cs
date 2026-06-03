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

    [Header("Mode")]
    public bool useAIGeneration = false;

    private bool isGenerating;

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
    public bool IsGenerating => isGenerating;

    private void Awake()
    {
        ResolveReferences();
        SetGenerationStatus(useAIGeneration ? "Ready to generate." : "Manual customization is active.");
    }

    public void GenerateSelectedCharacter()
    {
        if (!useAIGeneration)
        {
            Debug.Log("AI character generation is disabled. Manual customization is active.");
            SetGenerationStatus("Manual customization is active.");
            return;
        }

        if (isGenerating)
        {
            SetGenerationStatus("Generation is already running...");
            return;
        }

        if (!HasGenerationTokens())
        {
            OnGenerationFailed("Not enough generation tokens.");
            return;
        }

        isGenerating = true;
        StartCoroutine(GenerateSelectedCharacterCoroutine());
    }

    public bool CanGenerate()
    {
        return useAIGeneration && !isGenerating && HasGenerationTokens();
    }

    private bool HasGenerationTokens()
    {
#if UNITY_EDITOR
        return true;
#else
        return TokenBalance >= GenerationTokenCost;
#endif
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

        customizationManager.SaveGeneratedPart(humanType, slotIndex, GetBodyPart(mode), result.singlePartSpriteId);

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
        try
        {
            yield return GenerateSelectedCharacterCoroutineBody();
        }
        finally
        {
            isGenerating = false;
        }
    }

    private IEnumerator GenerateSelectedCharacterCoroutineBody()
    {
        ResolveReferences();

        if (uiController == null || customizationManager == null)
        {
            OnGenerationFailed("Character generation is missing required scene references.");
            yield break;
        }

        if (!HasGenerationTokens())
        {
            OnGenerationFailed("Not enough generation tokens.");
            yield break;
        }

        HumanType humanType = uiController.SelectedHumanType;
        int slotIndex = uiController.SelectedGeneratedSlotIndex;
        GenerationMode mode = uiController.SelectedGenerationMode;
        BodyPartType selectedPart = GetBodyPart(mode);
        string prompt = BuildStrictPartPrompt(uiController.PromptText);

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
        SourceImageData[] inputImages = BuildInputImages(humanType, selectedPart);
        string[] inputImageDataUris = BuildInputImageDataUris(inputImages);
        PartImageDataUris partImageDataUris = BuildPartImageDataUris(inputImages);

        GenerateRequest requestData = new GenerateRequest
        {
            prompt = prompt ?? string.Empty,
            image = inputImageDataUris.Length > 0 ? inputImageDataUris[0] : string.Empty,
            humanType = humanType.ToString(),
            generationMode = mode.ToString(),
            selectedPart = selectedPart.ToString()
        };

        string json = JsonUtility.ToJson(requestData);
        Debug.Log($"Character generation request JSON preview:\n{BuildRequestDebugPreview(json)}");
        Debug.Log("Sending ONE generation request for part: " + selectedPart);

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

        ReplicateResponse response;
        try
        {
            response = JsonUtility.FromJson<ReplicateResponse>(request.downloadHandler.text);
        }
        catch (Exception exception)
        {
            onFailed?.Invoke($"Could not parse backend response: {exception.Message}");
            yield break;
        }

        if (response == null)
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
            response = finalResponse ?? JsonUtility.FromJson<ReplicateResponse>(finalResponseJson);
        }
        catch (Exception exception)
        {
            onFailed?.Invoke($"Could not parse completed backend response: {exception.Message}");
            yield break;
        }

        SetGenerationStatus("Downloading generated sprite...");

        yield return DownloadSinglePartResult(response, finalResponseJson, partImageDataUris, humanType, slotIndex, selectedPart, onSuccess, onFailed);
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

    private IEnumerator DownloadSinglePartResult(
        ReplicateResponse response,
        string responseJson,
        PartImageDataUris sourcePartImageDataUris,
        HumanType humanType,
        int slotIndex,
        BodyPartType part,
        Action<CharacterGenerationResult> onSuccess,
        Action<string> onFailed)
    {
        if (!TryGetValidatedPartUrl(response, responseJson, sourcePartImageDataUris, part, out string imageUrl, out string error))
        {
            onFailed?.Invoke(error);
            yield break;
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

            SaveAlignedGeneratedImageBytes(imageBytes, humanType, slotIndex, part, onSuccess, onFailed);
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Get(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onFailed?.Invoke($"Image download failed: {request.error}");
            yield break;
        }

        SaveAlignedGeneratedImageBytes(request.downloadHandler.data, humanType, slotIndex, part, onSuccess, onFailed);
    }

    private SourceImageData[] BuildInputImages(HumanType humanType, BodyPartType selectedPart)
    {
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

    private static string BuildStrictPartPrompt(string userPrompt)
    {
        return "Use the reference image as a strict template.\n"
            + "Keep the exact same transparent background, canvas size, framing, pose, silhouette, object position, and scale.\n"
            + "Do not zoom, crop, rotate, or redraw the composition.\n"
            + "Only modify this part according to the request: "
            + (string.IsNullOrWhiteSpace(userPrompt) ? "preserve the original part design with minimal changes." : userPrompt.Trim());
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
        ReplicateResponse response,
        string responseJson,
        PartImageDataUris sourcePartImageDataUris,
        BodyPartType part,
        out string imageUrl,
        out string error)
    {
        imageUrl = GetPartUrl(response, responseJson);
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

    private static string GetPartUrl(ReplicateResponse response, string responseJson)
    {
        string outputUrl = GetFirstOutputUrl(response, responseJson);
        if (!string.IsNullOrWhiteSpace(outputUrl))
        {
            Debug.Log("Using single output image URL: " + outputUrl);
            return outputUrl;
        }

        return string.Empty;
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

    private static string GetFirstOutputUrl(ReplicateResponse response, string responseJson)
    {
        string[] outputUrls = GetOutputUrls(response, responseJson);
        return outputUrls.Length > 0 ? outputUrls[0] : string.Empty;
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
        string[] outputUrls = GetOutputUrls(response, responseJson);

        if (response != null && !string.IsNullOrWhiteSpace(response.error))
        {
            return $"Generated {partName} image URL is missing because the backend returned an error: {response.error}";
        }

        if (response != null && !string.IsNullOrWhiteSpace(response.status) && !string.Equals(response.status, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return $"Generated {partName} image URL is missing because the backend response status is '{response.status}'. Expected output after generation succeeds.";
        }

        if (IsSucceededStatus(GetPredictionStatus(response)) && outputUrls.Length == 0)
        {
            return "Prediction succeeded but returned no images. Check backend/model output.";
        }

        return $"Generated {partName} image URL is missing. Expected backend output.";
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

    private static bool TryValidateImageUrl(string imageUrl, BodyPartType part, out string normalizedUrl, out string error)
    {
        string partName = GetPartJsonName(part);
        normalizedUrl = NormalizeJsonString(imageUrl);
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedUrl))
        {
            error = $"Generated {partName} image URL is missing. Expected backend output.";
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

    private static void SaveAlignedGeneratedImageBytes(
        byte[] imageBytes,
        HumanType humanType,
        int slotIndex,
        BodyPartType part,
        Action<string> onSuccess,
        Action<string> onFailed)
    {
        if (!TryAlignGeneratedImageToReference(imageBytes, humanType, part, out byte[] alignedBytes, out string error))
        {
            onFailed?.Invoke(error);
            return;
        }

        SaveGeneratedImageBytes(alignedBytes, humanType, slotIndex, part, onSuccess);
    }

    private static bool TryAlignGeneratedImageToReference(
        byte[] generatedImageBytes,
        HumanType humanType,
        BodyPartType part,
        out byte[] alignedImageBytes,
        out string error)
    {
        alignedImageBytes = null;
        error = string.Empty;

        if (generatedImageBytes == null || generatedImageBytes.Length == 0)
        {
            error = $"Generated {GetPartJsonName(part)} image was empty.";
            return false;
        }

        Texture2D generatedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Texture2D referenceReadableTexture = null;
        Texture2D generatedReadableTexture = null;
        Texture2D alignedTexture = null;

        try
        {
            if (!generatedTexture.LoadImage(generatedImageBytes))
            {
                error = $"Generated {GetPartJsonName(part)} image could not be decoded.";
                return false;
            }

            Texture2D referenceTexture = LoadSourceTexture(GetSourceImageResourcePath(humanType, part));
            if (referenceTexture == null)
            {
                error = $"Reference {GetPartJsonName(part)} image could not be loaded.";
                return false;
            }

            referenceReadableTexture = CreateReadableCopy(referenceTexture);
            generatedReadableTexture = CreateReadableCopy(generatedTexture);
            RemoveNearBlackEdgeBackground(generatedReadableTexture);

            const byte visibleAlphaThreshold = 10;
            bool referenceHasVisiblePixels = TryAnalyzeAlpha(referenceReadableTexture, visibleAlphaThreshold, out int referenceVisiblePixelCount, out RectInt referenceBounds);
            bool generatedHasVisiblePixels = TryAnalyzeAlpha(generatedReadableTexture, visibleAlphaThreshold, out int generatedVisiblePixelCount, out RectInt generatedBounds);
            LogAlphaAnalysis($"Reference {part}", referenceReadableTexture, referenceVisiblePixelCount, referenceBounds);
            LogAlphaAnalysis($"Generated {part}", generatedReadableTexture, generatedVisiblePixelCount, generatedBounds);

            if (!referenceHasVisiblePixels)
            {
                error = $"Reference {GetPartJsonName(part)} image has no visible opaque pixels.";
                return false;
            }

            if (!generatedHasVisiblePixels)
            {
                error = $"Generated {GetPartJsonName(part)} image has no visible opaque pixels.";
                return false;
            }

            int minimumVisiblePixels = GetMinimumVisiblePixelCount(part);
            if (generatedVisiblePixelCount < minimumVisiblePixels)
            {
                error = $"Generated {GetPartJsonName(part)} image is too small. Visible pixels: {generatedVisiblePixelCount}.";
                return false;
            }

            alignedTexture = AlignVisibleBounds(generatedReadableTexture, generatedBounds, referenceReadableTexture.width, referenceReadableTexture.height, referenceBounds);
            alignedImageBytes = alignedTexture.EncodeToPNG();
            if (alignedImageBytes == null || alignedImageBytes.Length == 0)
            {
                error = $"Aligned {GetPartJsonName(part)} image could not be encoded.";
                return false;
            }

            Debug.Log($"Aligned generated {part} alpha bounds from {FormatBounds(generatedBounds)} to reference {FormatBounds(referenceBounds)}.");
            return true;
        }
        finally
        {
            UnityEngine.Object.Destroy(generatedTexture);
            if (referenceReadableTexture != null)
            {
                UnityEngine.Object.Destroy(referenceReadableTexture);
            }

            if (generatedReadableTexture != null)
            {
                UnityEngine.Object.Destroy(generatedReadableTexture);
            }

            if (alignedTexture != null)
            {
                UnityEngine.Object.Destroy(alignedTexture);
            }
        }
    }

    private static Texture2D AlignVisibleBounds(
        Texture2D generatedTexture,
        RectInt generatedBounds,
        int outputWidth,
        int outputHeight,
        RectInt targetBounds)
    {
        Texture2D outputTexture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false);
        Color32[] outputPixels = new Color32[outputWidth * outputHeight];
        Color32 transparent = new Color32(0, 0, 0, 0);
        for (int i = 0; i < outputPixels.Length; i++)
        {
            outputPixels[i] = transparent;
        }

        Color32[] generatedPixels = generatedTexture.GetPixels32();
        for (int y = targetBounds.yMin; y < targetBounds.yMax; y++)
        {
            float v = targetBounds.height <= 1 ? 0.5f : (y - targetBounds.yMin + 0.5f) / targetBounds.height;
            int sourceY = Mathf.Clamp(generatedBounds.yMin + Mathf.FloorToInt(v * generatedBounds.height), generatedBounds.yMin, generatedBounds.yMax - 1);

            for (int x = targetBounds.xMin; x < targetBounds.xMax; x++)
            {
                float u = targetBounds.width <= 1 ? 0.5f : (x - targetBounds.xMin + 0.5f) / targetBounds.width;
                int sourceX = Mathf.Clamp(generatedBounds.xMin + Mathf.FloorToInt(u * generatedBounds.width), generatedBounds.xMin, generatedBounds.xMax - 1);
                outputPixels[y * outputWidth + x] = generatedPixels[sourceY * generatedTexture.width + sourceX];
            }
        }

        outputTexture.SetPixels32(outputPixels);
        outputTexture.Apply();
        return outputTexture;
    }

    private static void RemoveNearBlackEdgeBackground(Texture2D texture)
    {
        Color32[] pixels = texture.GetPixels32();
        bool[] visited = new bool[pixels.Length];
        int[] queue = new int[pixels.Length];
        int readIndex = 0;
        int writeIndex = 0;

        EnqueueBlackEdgePixels(texture, pixels, visited, queue, ref writeIndex);

        while (readIndex < writeIndex)
        {
            int pixelIndex = queue[readIndex++];
            pixels[pixelIndex].a = 0;

            int x = pixelIndex % texture.width;
            int y = pixelIndex / texture.width;
            TryQueueNearBlackPixel(texture, pixels, visited, queue, ref writeIndex, x - 1, y);
            TryQueueNearBlackPixel(texture, pixels, visited, queue, ref writeIndex, x + 1, y);
            TryQueueNearBlackPixel(texture, pixels, visited, queue, ref writeIndex, x, y - 1);
            TryQueueNearBlackPixel(texture, pixels, visited, queue, ref writeIndex, x, y + 1);
        }

        if (writeIndex > 0)
        {
            texture.SetPixels32(pixels);
            texture.Apply();
            Debug.Log($"Removed near-black edge background pixels: {writeIndex}");
        }
    }

    private static void EnqueueBlackEdgePixels(Texture2D texture, Color32[] pixels, bool[] visited, int[] queue, ref int writeIndex)
    {
        for (int x = 0; x < texture.width; x++)
        {
            TryQueueNearBlackPixel(texture, pixels, visited, queue, ref writeIndex, x, 0);
            TryQueueNearBlackPixel(texture, pixels, visited, queue, ref writeIndex, x, texture.height - 1);
        }

        for (int y = 1; y < texture.height - 1; y++)
        {
            TryQueueNearBlackPixel(texture, pixels, visited, queue, ref writeIndex, 0, y);
            TryQueueNearBlackPixel(texture, pixels, visited, queue, ref writeIndex, texture.width - 1, y);
        }
    }

    private static void TryQueueNearBlackPixel(Texture2D texture, Color32[] pixels, bool[] visited, int[] queue, ref int writeIndex, int x, int y)
    {
        if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
        {
            return;
        }

        int pixelIndex = y * texture.width + x;
        if (visited[pixelIndex] || !IsNearBlackBackgroundPixel(pixels[pixelIndex]))
        {
            return;
        }

        visited[pixelIndex] = true;
        queue[writeIndex++] = pixelIndex;
    }

    private static bool IsNearBlackBackgroundPixel(Color32 pixel)
    {
        const byte blackThreshold = 24;
        return pixel.a > 0
            && pixel.r <= blackThreshold
            && pixel.g <= blackThreshold
            && pixel.b <= blackThreshold;
    }

    private static bool TryAnalyzeAlpha(
        Texture2D tex,
        byte alphaThreshold,
        out int visiblePixelCount,
        out RectInt visibleBounds)
    {
        visiblePixelCount = 0;
        visibleBounds = default;

        if (tex == null)
        {
            return false;
        }

        Color32[] pixels = tex.GetPixels32();
        int minX = tex.width;
        int minY = tex.height;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < tex.height; y++)
        {
            int rowOffset = y * tex.width;
            for (int x = 0; x < tex.width; x++)
            {
                if (pixels[rowOffset + x].a <= alphaThreshold)
                {
                    continue;
                }

                visiblePixelCount++;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
        {
            return false;
        }

        visibleBounds = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        return true;
    }

    private static int GetMinimumVisiblePixelCount(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Neck:
                return 20;
            case BodyPartType.Head:
                return 100;
            case BodyPartType.Stomach:
                return 100;
            case BodyPartType.Leg:
                return 100;
            default:
                return 50;
        }
    }

    private static void LogAlphaAnalysis(string label, Texture2D texture, int visiblePixelCount, RectInt visibleBounds)
    {
        if (texture == null)
        {
            Debug.Log($"{label} alpha analysis: texture is null.");
            return;
        }

        int totalPixels = texture.width * texture.height;
        int transparentPixelCount = Mathf.Max(0, totalPixels - visiblePixelCount);
        float visiblePercentage = totalPixels > 0 ? visiblePixelCount * 100f / totalPixels : 0f;
        string boundsText = visiblePixelCount > 0 ? FormatBounds(visibleBounds) : "none";

        Debug.Log(
            $"{label} alpha analysis: size={texture.width}x{texture.height}, "
            + $"visible pixels={visiblePixelCount}, transparent pixels={transparentPixelCount}, "
            + $"visible={visiblePercentage:F4}%, bounds={boundsText}");
    }

    private static Texture2D CreateReadableCopy(Texture2D sourceTexture)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;

        try
        {
            Graphics.Blit(sourceTexture, temporary);
            RenderTexture.active = temporary;

            Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
            readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
            readableTexture.Apply();
            return readableTexture;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
        }
    }

    private static string FormatBounds(RectInt bounds)
    {
        return $"x:{bounds.xMin}-{bounds.xMax - 1}, y:{bounds.yMin}-{bounds.yMax - 1}, size:{bounds.width}x{bounds.height}";
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
            case HumanType.BigHuman:
                return "Big";
            case HumanType.KnightHuman:
                return "Knight";
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
            case HumanType.BigHuman:
                return "big";
            case HumanType.KnightHuman:
                return "knight";
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
        public string image;
        public string humanType;
        public string generationMode;
        public string selectedPart;
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
    private class ReplicateResponse
    {
        public string[] output;
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
    public string singlePartSpriteId;

    public static CharacterGenerationResult Failed(string message)
    {
        return new CharacterGenerationResult
        {
            success = false,
            errorMessage = message
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
