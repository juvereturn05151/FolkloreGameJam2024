using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private const string DefaultGameplaySceneName = "GameplayScene";
    private const string ArcadeGameplaySceneName = "ArcadeMode";

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopingSfxSource;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [SerializeField] private List<SoundEffect> soundEffectsList;

    private Dictionary<string, AudioClip> soundEffects;

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

        EnsureLoopingSfxSource();
    }

    private void Start()
    {
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

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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
        musicSource.volume = musicVolume;
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
        if (soundEffects.ContainsKey(sfxName))
        {
            sfxSource.PlayOneShot(soundEffects[sfxName], sfxVolume);
        }
        else
        {
            Debug.LogWarning("Sound effect not found: " + sfxName);
        }
    }

    public bool PlayRandomSFXByPrefix(string sfxNamePrefix)
    {
        if (string.IsNullOrWhiteSpace(sfxNamePrefix) || soundEffects == null || sfxSource == null)
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

        sfxSource.PlayOneShot(matchingClips[Random.Range(0, matchingClips.Count)], sfxVolume);
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
        loopingSfxSource.volume = sfxVolume;
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
        musicVolume = volume;
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;

        if (loopingSfxSource != null)
        {
            loopingSfxSource.volume = sfxVolume;
        }
    }

    private bool TryGetSoundEffect(string sfxName, out AudioClip clip)
    {
        clip = null;
        return !string.IsNullOrWhiteSpace(sfxName)
            && soundEffects != null
            && soundEffects.TryGetValue(sfxName, out clip)
            && clip != null;
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
        loopingSfxSource.volume = sfxVolume;
    }
}
