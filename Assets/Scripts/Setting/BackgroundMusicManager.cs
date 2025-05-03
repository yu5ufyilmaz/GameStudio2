using System.Collections;
using UnityEngine;

// BackgroundMusicManager: Singleton that manages background music playback with smooth fade transitions.
public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    [Tooltip("Optional default background music played when not inside any building.")]
    public AudioClip defaultMusic;

    [Tooltip("Fade duration in seconds when switching music.")]
    public float fadeDuration = 1.0f;

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

    // Call this to play new background music with fade transition
    public void PlayMusic(AudioClip musicClip, bool immediate = false)
    {
        if (musicClip == null || PlayerPrefs.GetFloat("MusicVolume") <= 0f)
            return; // Don't play if volume is zero

        if (audioSource.clip == musicClip)
            return; // same music already playing

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (immediate || fadeDuration <= 0f)
        {
            audioSource.clip = musicClip;
            audioSource.volume = PlayerPrefs.GetFloat("MusicVolume");
            audioSource.Play();
        }
        else
        {
            fadeCoroutine = StartCoroutine(FadeMusicRoutine(musicClip));
        }
    }

    // Fade out current music, then fade in new music
    private IEnumerator FadeMusicRoutine(AudioClip newClip)
    {
        // Fade out
        float startVolume = audioSource.volume;
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(
                0f,
                PlayerPrefs.GetFloat("MusicVolume"),
                time / fadeDuration
            );
            yield return null;
        }

        audioSource.volume = PlayerPrefs.GetFloat("MusicVolume");

        fadeCoroutine = null;
    }

    // Optional: stop music or revert to default music with fade
    public void StopMusic()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float startVolume = audioSource.volume;
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }
        audioSource.Stop();
        audioSource.clip = null;
        fadeCoroutine = null;
    }

    // Play default music with fade transition
    public void PlayDefaultMusic()
    {
        PlayMusic(defaultMusic);
    }
}
