using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    private const float PanelWidth = 760f;
    private const float PanelHeight = 430f;

    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteBgmToggle;
    [SerializeField] private Toggle muteSfxToggle;

    private bool isRefreshing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneBootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureSceneInstance();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSceneInstance();
    }

    private static void EnsureSceneInstance()
    {
        if (FindAnyObjectByType<SettingManager>() != null)
        {
            return;
        }

        Transform existingManager = FindSceneTransform("SettingManager");
        if (existingManager != null)
        {
            existingManager.gameObject.AddComponent<SettingManager>();
            return;
        }

        Transform settingButton = FindSceneTransform("Setting");
        if (settingButton != null && settingButton.GetComponent<Button>() != null)
        {
            new GameObject("SettingManager").AddComponent<SettingManager>();
        }
    }

    private void Awake()
    {
        ResolveOrCreateUI();
        BindUI();
        RefreshUI();
        SetPanelVisible(false);
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void ToggleSettingsPanel()
    {
        SetPanelVisible(settingsPanel == null || !settingsPanel.activeSelf);
        RefreshUI();
    }

    public void SetPanelVisible(bool visible)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(visible);
        }
    }

    public void SetBgmVolume(float volume)
    {
        if (isRefreshing || SoundManager.instance == null)
        {
            return;
        }

        SoundManager.instance.SetMusicVolume(volume);
        RefreshUI();
    }

    public void SetSfxVolume(float volume)
    {
        if (isRefreshing || SoundManager.instance == null)
        {
            return;
        }

        SoundManager.instance.SetSFXVolume(volume);
        RefreshUI();
    }

    public void SetBgmMuted(bool muted)
    {
        if (isRefreshing || SoundManager.instance == null)
        {
            return;
        }

        SoundManager.instance.SetMusicMuted(muted);
        RefreshUI();
    }

    public void SetSfxMuted(bool muted)
    {
        if (isRefreshing || SoundManager.instance == null)
        {
            return;
        }

        SoundManager.instance.SetSfxMuted(muted);
        RefreshUI();
    }

    private void ResolveOrCreateUI()
    {
        settingsButton ??= FindNamedComponent<Button>("Setting");
        settingsPanel ??= FindNamedTransform("AudioSettingsPanel")?.gameObject;

        if (settingsPanel == null)
        {
            settingsPanel = CreatePanel();
        }

        bgmVolumeSlider ??= FindNamedComponent<Slider>("BgmVolumeSlider");
        sfxVolumeSlider ??= FindNamedComponent<Slider>("SfxVolumeSlider");
        muteBgmToggle ??= FindNamedComponent<Toggle>("MuteBgmToggle");
        muteSfxToggle ??= FindNamedComponent<Toggle>("MuteSfxToggle");

        if (bgmVolumeSlider == null)
        {
            bgmVolumeSlider = CreateSliderRow(settingsPanel.transform, "Bgm", "BGM", 95f, out muteBgmToggle);
        }

        if (sfxVolumeSlider == null)
        {
            sfxVolumeSlider = CreateSliderRow(settingsPanel.transform, "Sfx", "SFX", -95f, out muteSfxToggle);
        }
    }

    private void BindUI()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(ToggleSettingsPanel);
        }

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.RemoveListener(SetBgmVolume);
            bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        if (muteBgmToggle != null)
        {
            muteBgmToggle.onValueChanged.RemoveListener(SetBgmMuted);
            muteBgmToggle.onValueChanged.AddListener(SetBgmMuted);
        }

        if (muteSfxToggle != null)
        {
            muteSfxToggle.onValueChanged.RemoveListener(SetSfxMuted);
            muteSfxToggle.onValueChanged.AddListener(SetSfxMuted);
        }
    }

    private void RefreshUI()
    {
        SoundManager soundManager = SoundManager.instance;
        if (soundManager == null)
        {
            return;
        }

        isRefreshing = true;

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.SetValueWithoutNotify(soundManager.MusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(soundManager.SfxVolume);
        }

        if (muteBgmToggle != null)
        {
            muteBgmToggle.SetIsOnWithoutNotify(soundManager.IsMusicMuted);
        }

        if (muteSfxToggle != null)
        {
            muteSfxToggle.SetIsOnWithoutNotify(soundManager.IsSfxMuted);
        }

        isRefreshing = false;
    }

    private GameObject CreatePanel()
    {
        Transform parent = FindNamedTransform("SettingCanvas") ?? FindNamedTransform("SettingButtonCanvas") ?? transform;
        GameObject panel = new GameObject("AudioSettingsPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.08f, 0.07f, 0.06f, 0.92f);

        CreateText(panel.transform, "Title", "Settings", new Vector2(0f, 160f), new Vector2(620f, 60f), 44, TextAnchor.MiddleCenter);
        return panel;
    }

    private Slider CreateSliderRow(Transform parent, string key, string label, float y, out Toggle muteToggle)
    {
        CreateText(parent, key + "Label", label, new Vector2(-250f, y + 18f), new Vector2(150f, 50f), 34, TextAnchor.MiddleRight);

        Slider slider = CreateSlider(parent, key + "VolumeSlider", new Vector2(80f, y + 20f), new Vector2(420f, 44f));
        muteToggle = CreateToggle(parent, "Mute" + key + "Toggle", new Vector2(-110f, y - 50f));
        CreateText(parent, key + "MuteLabel", "Mute", new Vector2(20f, y - 50f), new Vector2(170f, 42f), 28, TextAnchor.MiddleLeft);

        return slider;
    }

    private Slider CreateSlider(Transform parent, string objectName, Vector2 position, Vector2 size)
    {
        GameObject sliderObject = new GameObject(objectName, typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);

        RectTransform rectTransform = sliderObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        RectTransform background = CreateImage(sliderObject.transform, "Background", new Color(0.18f, 0.17f, 0.15f, 1f), Vector2.zero, size).GetComponent<RectTransform>();
        RectTransform fillArea = CreateRect(sliderObject.transform, "Fill Area", new Vector2(0f, 0f), new Vector2(-24f, 0f));
        RectTransform fill = CreateImage(fillArea, "Fill", new Color(0.72f, 0.18f, 0.13f, 1f), Vector2.zero, Vector2.zero).GetComponent<RectTransform>();
        RectTransform handleArea = CreateRect(sliderObject.transform, "Handle Slide Area", Vector2.zero, new Vector2(-24f, 0f));
        RectTransform handle = CreateImage(handleArea, "Handle", Color.white, Vector2.zero, new Vector2(34f, 58f)).GetComponent<RectTransform>();

        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        handle.anchorMin = new Vector2(0.5f, 0f);
        handle.anchorMax = new Vector2(0.5f, 1f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private Toggle CreateToggle(Transform parent, string objectName, Vector2 position)
    {
        GameObject toggleObject = new GameObject(objectName, typeof(RectTransform), typeof(Toggle));
        toggleObject.transform.SetParent(parent, false);

        RectTransform rectTransform = toggleObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(50f, 50f);

        Image background = CreateImage(toggleObject.transform, "Background", new Color(0.18f, 0.17f, 0.15f, 1f), Vector2.zero, new Vector2(50f, 50f));
        Image checkmark = CreateImage(background.transform, "Checkmark", new Color(0.72f, 0.18f, 0.13f, 1f), Vector2.zero, new Vector2(32f, 32f));

        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        toggle.isOn = false;

        return toggle;
    }

    private Text CreateText(Transform parent, string objectName, string text, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Text textComponent = textObject.GetComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = Color.white;

        return textComponent;
    }

    private Image CreateImage(Transform parent, string objectName, Color color, Vector2 position, Vector2 size)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;

        return image;
    }

    private RectTransform CreateRect(Transform parent, string objectName, Vector2 position, Vector2 sizeDelta)
    {
        GameObject rectObject = new GameObject(objectName, typeof(RectTransform));
        rectObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rectObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = sizeDelta;

        return rectTransform;
    }

    private T FindNamedComponent<T>(string objectName) where T : Component
    {
        Transform target = FindNamedTransform(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }

    private Transform FindNamedTransform(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == objectName)
            {
                return children[i];
            }
        }

        Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sceneTransforms.Length; i++)
        {
            if (sceneTransforms[i] != null && sceneTransforms[i].name == objectName)
            {
                return sceneTransforms[i];
            }
        }

        return null;
    }

    private static Transform FindSceneTransform(string objectName)
    {
        Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sceneTransforms.Length; i++)
        {
            if (sceneTransforms[i] != null && sceneTransforms[i].name == objectName)
            {
                return sceneTransforms[i];
            }
        }

        return null;
    }
}
