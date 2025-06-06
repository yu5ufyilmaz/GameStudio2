using System.Collections;
using UnityEngine;

// BackgroundMusicManager: Singleton that manages background music playback without fade transitions.
public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance;

    private AudioSource audioSource;

    [Tooltip("Optional default background music played when not inside any building.")]
    public AudioClip defaultMusic;

    void Awake()
    {
        // Singleton pattern to keep only one music manager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
        }

        // Start playing default music if assigned
        if (defaultMusic != null)
        {
            PlayMusic(defaultMusic, true);
        }
    }

    // Call this to play new background music immediately
    public void PlayMusic(AudioClip musicClip, bool immediate = false)
    {
        if (musicClip == null || PlayerPrefs.GetFloat("MusicVolume") <= 0f)
            return; // Don't play if volume is zero

        if (audioSource.clip == musicClip)
            return; // same music already playing

        audioSource.clip = musicClip;
        audioSource.volume = PlayerPrefs.GetFloat("MusicVolume");
        audioSource.Play();
    }

    // Optional: stop music immediately
    public void StopMusic()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    // Play default music immediately
    public void PlayDefaultMusic()
    {
        PlayMusic(defaultMusic);
    }
}
