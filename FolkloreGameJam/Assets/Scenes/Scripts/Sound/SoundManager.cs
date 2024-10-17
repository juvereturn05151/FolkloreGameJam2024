using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Singleton instance
    public static SoundManager instance;

    // Audio sources
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    // Volume controls
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // Dictionary to hold audio clips
    private Dictionary<string, AudioClip> soundEffects;

    // Background music clip
    public AudioClip backgroundMusic;

    private void Awake()
    {
        // Ensure only one SoundManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize sound effects dictionary
        soundEffects = new Dictionary<string, AudioClip>();
    }

    private void Start()
    {
        PlayMusic(backgroundMusic);
    }

    // Play background music
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Play a one-shot sound effect
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

    // Add a sound effect to the dictionary
    public void AddSoundEffect(string sfxName, AudioClip clip)
    {
        if (!soundEffects.ContainsKey(sfxName))
        {
            soundEffects[sfxName] = clip;
        }
    }

    // Update volumes dynamically
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