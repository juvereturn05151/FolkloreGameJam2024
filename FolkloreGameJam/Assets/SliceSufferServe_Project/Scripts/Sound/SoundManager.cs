using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private const string MusicVolumePrefsKey = "Settings.MusicVolume";
    private const string SfxVolumePrefsKey = "Settings.SfxVolume";
    private const string MusicMutedPrefsKey = "Settings.MusicMuted";
    private const string SfxMutedPrefsKey = "Settings.SfxMuted";
    private const string DefaultGameplaySceneName = "GameplayScene";
    private const string ArcadeGameplaySceneName = "ArcadeMode";
    private const string DefaultClickSoundName = "EvilClickSoundEffect";

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopingSfxSource;
    [SerializeField] private AudioClip clickSoundClip;
    [SerializeField] private string clickSoundName = DefaultClickSoundName;
    [SerializeField] private float clickButtonScanInterval = 0.5f;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public bool IsMusicMuted { get; private set; }
    public bool IsSfxMuted { get; private set; }
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    [SerializeField] private List<SoundEffect> soundEffectsList;

    private Dictionary<string, AudioClip> soundEffects;
    private readonly HashSet<Selectable> clickSoundSelectables = new();
    private float nextClickButtonScanTime;

    public AudioClip backgroundMusic;
    public AudioClip backgroundGameplayMusic;
    public AudioClip backgroundMenu;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        soundEffects = new Dictionary<string, AudioClip>();
        foreach (var soundEffect in soundEffectsList)
        {
            if (!soundEffects.ContainsKey(soundEffect.name))
            {
                soundEffects[soundEffect.name] = soundEffect.clip;
            }
        }

        if (clickSoundClip != null)
        {
            soundEffects[GetClickSoundName()] = clickSoundClip;
        }

        EnsureLoopingSfxSource();
        LoadAudioSettings();
        ApplyAudioSettings();
    }

    private void Start()
    {
        BindClickSoundsToSelectables();

        Scene activeScene = SceneManager.GetActiveScene();
        if (IsGameplayScene(activeScene.name))
        {
            PlayGameplayBGM();
        }
        else
        {
            PlayMusic(backgroundMusic);
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < nextClickButtonScanTime)
        {
            return;
        }

        nextClickButtonScanTime = Time.unscaledTime + Mathf.Max(0.1f, clickButtonScanInterval);
        BindClickSoundsToSelectables();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        clickSoundSelectables.Clear();
        BindClickSoundsToSelectables();

        if (IsGameplayScene(scene.name))
        {
            PlayGameplayBGM();
        }
    }

    public void PlayGameplayBGM() 
    {
        PlayMusic(backgroundGameplayMusic);
    }

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName == DefaultGameplaySceneName
            || sceneName == ArcadeGameplaySceneName
            || sceneName == StageSelection.GetSelectedGameplaySceneName();
    }

    public void PlayMenuBGM()
    {
        PlayMusic(backgroundMenu);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
        {
            return;
        }

        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        musicSource.clip = clip;
        musicSource.volume = GetEffectiveMusicVolume();
        musicSource.mute = IsMusicMuted;
        musicSource.loop = true;
        musicSource.pitch = 1.0f;
        musicSource.Play();
    }

    public void SpeedUpMusic() 
    {
        musicSource.pitch = 1.5f;
    }

    public void PlaySFX(string sfxName)
    {
        if (IsSfxMuted || sfxSource == null)
        {
            return;
        }

        if (soundEffects.ContainsKey(sfxName))
        {
            sfxSource.PlayOneShot(soundEffects[sfxName], 1f);
        }
        else
        {
            Debug.LogWarning("Sound effect not found: " + sfxName);
        }
    }

    public void PlayClickSFX()
    {
        PlaySFX(GetClickSoundName());
    }

    public bool PlayRandomSFXByPrefix(string sfxNamePrefix)
    {
        if (IsSfxMuted || string.IsNullOrWhiteSpace(sfxNamePrefix) || soundEffects == null || sfxSource == null)
        {
            return false;
        }

        List<AudioClip> matchingClips = new();
        foreach (KeyValuePair<string, AudioClip> soundEffect in soundEffects)
        {
            if (soundEffect.Value != null && soundEffect.Key.StartsWith(sfxNamePrefix))
            {
                matchingClips.Add(soundEffect.Value);
            }
        }

        if (matchingClips.Count == 0)
        {
            Debug.LogWarning("Sound effect prefix not found: " + sfxNamePrefix);
            return false;
        }

        sfxSource.PlayOneShot(matchingClips[Random.Range(0, matchingClips.Count)], 1f);
        return true;
    }

    public void PlayLoopingSFX(string sfxName)
    {
        if (!TryGetSoundEffect(sfxName, out AudioClip clip))
        {
            Debug.LogWarning("Sound effect not found: " + sfxName);
            return;
        }

        EnsureLoopingSfxSource();
        if (loopingSfxSource == null)
        {
            return;
        }

        if (loopingSfxSource.isPlaying && loopingSfxSource.clip == clip)
        {
            return;
        }

        loopingSfxSource.Stop();
        loopingSfxSource.clip = clip;
        loopingSfxSource.volume = GetEffectiveSfxVolume();
        loopingSfxSource.mute = IsSfxMuted;
        loopingSfxSource.loop = true;
        loopingSfxSource.Play();
    }

    public void StopLoopingSFX(string sfxName)
    {
        if (loopingSfxSource == null || !loopingSfxSource.isPlaying)
        {
            return;
        }

        if (TryGetSoundEffect(sfxName, out AudioClip clip) && loopingSfxSource.clip != clip)
        {
            return;
        }

        loopingSfxSource.Stop();
        loopingSfxSource.clip = null;
    }

    public void AddSoundEffect(string sfxName, AudioClip clip)
    {
        if (!soundEffects.ContainsKey(sfxName))
        {
            soundEffects[sfxName] = clip;
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumePrefsKey, musicVolume);
        PlayerPrefs.Save();
        ApplyMusicSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumePrefsKey, sfxVolume);
        PlayerPrefs.Save();
        ApplySfxSettings();
    }

    public void SetMusicMuted(bool muted)
    {
        IsMusicMuted = muted;
        PlayerPrefs.SetInt(MusicMutedPrefsKey, IsMusicMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicSettings();
    }

    public void SetSfxMuted(bool muted)
    {
        IsSfxMuted = muted;
        PlayerPrefs.SetInt(SfxMutedPrefsKey, IsSfxMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplySfxSettings();
    }

    public void ToggleMusicMuted()
    {
        SetMusicMuted(!IsMusicMuted);
    }

    public void ToggleSfxMuted()
    {
        SetSfxMuted(!IsSfxMuted);
    }

    private bool TryGetSoundEffect(string sfxName, out AudioClip clip)
    {
        clip = null;
        return !string.IsNullOrWhiteSpace(sfxName)
            && soundEffects != null
            && soundEffects.TryGetValue(sfxName, out clip)
            && clip != null;
    }

    private void BindClickSoundsToSelectables()
    {
        Selectable[] selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include);
        for (int i = 0; i < selectables.Length; i++)
        {
            Selectable selectable = selectables[i];
            if (selectable == null || clickSoundSelectables.Contains(selectable))
            {
                continue;
            }

            if (selectable.GetComponent<UIClickSoundPlayer>() == null)
            {
                selectable.gameObject.AddComponent<UIClickSoundPlayer>();
            }

            clickSoundSelectables.Add(selectable);
        }
    }

    private string GetClickSoundName()
    {
        return string.IsNullOrWhiteSpace(clickSoundName) ? DefaultClickSoundName : clickSoundName;
    }

    private void EnsureLoopingSfxSource()
    {
        if (loopingSfxSource != null)
        {
            return;
        }

        loopingSfxSource = gameObject.AddComponent<AudioSource>();
        loopingSfxSource.playOnAwake = false;
        loopingSfxSource.loop = true;
        loopingSfxSource.volume = GetEffectiveSfxVolume();
        loopingSfxSource.mute = IsSfxMuted;
    }

    private void LoadAudioSettings()
    {
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumePrefsKey, musicVolume));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumePrefsKey, sfxVolume));
        IsMusicMuted = PlayerPrefs.GetInt(MusicMutedPrefsKey, 0) == 1;
        IsSfxMuted = PlayerPrefs.GetInt(SfxMutedPrefsKey, 0) == 1;
    }

    private void ApplyAudioSettings()
    {
        ApplyMusicSettings();
        ApplySfxSettings();
    }

    private void ApplyMusicSettings()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.volume = GetEffectiveMusicVolume();
        musicSource.mute = IsMusicMuted;
    }

    private void ApplySfxSettings()
    {
        if (sfxSource != null)
        {
            sfxSource.volume = GetEffectiveSfxVolume();
            sfxSource.mute = IsSfxMuted;
        }

        if (loopingSfxSource != null)
        {
            loopingSfxSource.volume = GetEffectiveSfxVolume();
            loopingSfxSource.mute = IsSfxMuted;

        }
    }

    private float GetEffectiveMusicVolume()
    {
        return IsMusicMuted ? 0f : musicVolume;
    }

    private float GetEffectiveSfxVolume()
    {
        return IsSfxMuted ? 0f : sfxVolume;
    }
}
