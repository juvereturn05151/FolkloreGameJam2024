using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

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
        }
        else
        {
            Destroy(gameObject);
        }

        soundEffects = new Dictionary<string, AudioClip>();
        foreach (var soundEffect in soundEffectsList)
        {
            if (!soundEffects.ContainsKey(soundEffect.name))
            {
                soundEffects[soundEffect.name] = soundEffect.clip;
            }
        }
    }

    private void Start()
    {
        PlayMusic(backgroundMusic);
    }

    public void PlayGameplayBGM() 
    {
        PlayMusic(backgroundGameplayMusic);
    }

    public void PlayMenuBGM()
    {
        PlayMusic(backgroundMenu);
    }

    public void PlayMusic(AudioClip clip)
    {
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
    }
}