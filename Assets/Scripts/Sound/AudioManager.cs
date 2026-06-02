using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent Singleton managing background music and SFX.
/// - Music: Dedicated AudioSource with crossfade support.
/// - SFX: Object pool of AudioSources for simultaneous overlapping effects.
/// - Scene Logic: Automatically switches music based on the loaded scene.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private SoundData menuAmbientSound;
    [SerializeField] private SoundData gameMusic;
    [SerializeField] private float musicFadeDuration = 1f;

    [Header("Scene Names")]
    [Tooltip("Exact name of the main gameplay scene (e.g., 'Map1').")]
    [SerializeField] private string gameSceneName = "Map1";

    [Header("SFX Pool Size")]
    [SerializeField] private int sfxPoolSize = 8;

    private AudioSource musicSource;
    private AudioSource[] sfxPool;
    private int sfxPoolIndex;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Singleton initialization
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildAudioSources();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    /// <summary>
    /// Initializes the music and SFX AudioSources at runtime.
    /// </summary>
    private void BuildAudioSources()
    {
        // Music Source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // SFX Pool
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.loop = false;
            src.playOnAwake = false;
            sfxPool[i] = src;
        }
    }

    /// <summary>
    /// Handles scene load events to switch music automatically.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameScene = scene.name == gameSceneName;

        if (isGameScene)
        {
            PlayMusic(gameMusic);
        }
        else
        {
            // Play menu ambient only if it's not already playing
            if (!IsMusicPlaying(menuAmbientSound))
                PlayMusic(menuAmbientSound);
        }
    }

    #region Music API

    /// <summary>
    /// Plays a music track with a crossfade from the current track.
    /// </summary>
    public void PlayMusic(SoundData data)
    {
        if (data == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CrossfadeMusic(data));
    }

    /// <summary>
    /// Stops the music with a fade-out effect.
    /// </summary>
    public void StopMusic(float fadeDuration = -1f)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        float duration = fadeDuration >= 0f ? fadeDuration : musicFadeDuration;
        fadeCoroutine = StartCoroutine(FadeOutMusic(duration));
    }

    private IEnumerator CrossfadeMusic(SoundData data)
    {
        AudioClip clip = data.GetClip();
        if (clip == null) yield break;

        float targetVolume = data.GetVolume();

        // Fade out current track
        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / musicFadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        // Configure and fade in new track
        musicSource.clip = clip;
        musicSource.pitch = data.GetPitch();
        musicSource.volume = 0f;

        if (data.mixerGroup != null)
            musicSource.outputAudioMixerGroup = data.mixerGroup;

        musicSource.Play();

        float elapsedIn = 0f;
        while (elapsedIn < musicFadeDuration)
        {
            elapsedIn += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedIn / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVol = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 0f;
    }

    private bool IsMusicPlaying(SoundData data)
    {
        return data != null && musicSource.isPlaying && musicSource.clip == data.GetClip();
    }

    #endregion

    #region SFX API

    /// <summary>
    /// Plays a 2D SFX using the object pool (round-robin).
    /// </summary>
    public void PlaySFX(SoundData data)
    {
        if (data == null) return;

        AudioClip clip = data.GetClip();
        if (clip == null) return;

        AudioSource source = GetNextSFXSource();
        source.clip = clip;
        source.volume = data.GetVolume();
        source.pitch = data.GetPitch();
        source.outputAudioMixerGroup = data.mixerGroup; // Null assignment is safe and clears previous mixer if needed

        source.Play();
    }

    /// <summary>
    /// Plays a 3D SFX at a specific world position without using the pool.
    /// </summary>
    public void PlaySFXAtPoint(SoundData data, Vector3 worldPosition)
    {
        if (data == null) return;

        AudioClip clip = data.GetClip();
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, worldPosition, data.GetVolume());
    }

    /// <summary>
    /// Retrieves the next available AudioSource from the pool.
    /// Falls back to round-robin if all sources are currently playing.
    /// </summary>
    private AudioSource GetNextSFXSource()
    {
        // Look for an idle source
        for (int i = 0; i < sfxPool.Length; i++)
        {
            int idx = (sfxPoolIndex + i) % sfxPool.Length;
            if (!sfxPool[idx].isPlaying)
            {
                sfxPoolIndex = (idx + 1) % sfxPool.Length;
                return sfxPool[idx];
            }
        }

        // Fallback: reuse the next source in round-robin order
        AudioSource fallback = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;
        return fallback;
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// Updates the global music volume (useful for settings menu).
    /// </summary>
    public void SetMusicVolume(float volume) 
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    #endregion
}