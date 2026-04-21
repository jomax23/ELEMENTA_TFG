using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton persistente entre escenas.
///
/// Responsabilidades:
///   - Música / sonido ambiente: un AudioSource dedicado con fade-in/out.
///   - SFX: pool de AudioSources para reproducir efectos solapados simultáneamente.
///   - Lógica de escena: cambia la música automáticamente según la escena cargada.
///
/// Uso:
///   AudioManager.Instance.PlaySFX(soundData);
///   AudioManager.Instance.PlayMusic(soundData);
///   AudioManager.Instance.StopMusic(fadeDuration: 0.5f);
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Music")]
    [SerializeField] private SoundData menuAmbientSound;
    [SerializeField] private SoundData gameMusic;
    [SerializeField] private float     musicFadeDuration = 1f;

    [Header("Scene Names")]
    [Tooltip("Nombre exacto de la escena principal de combate (ej: 'Map1').")]
    [SerializeField] private string gameSceneName = "Map1";

    [Header("SFX Pool Size")]
    [SerializeField] private int sfxPoolSize = 8;

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE
    // ─────────────────────────────────────────────────────────────────────────

    private AudioSource   musicSource;
    private AudioSource[] sfxPool;
    private int           sfxPoolIndex;

    private Coroutine fadeCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildAudioSources();
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void BuildAudioSources()
    {
        // ── Fuente de música ──────────────────────────────────────────────────
        musicSource          = gameObject.AddComponent<AudioSource>();
        musicSource.loop     = true;
        musicSource.playOnAwake = false;

        // ── Pool de SFX ───────────────────────────────────────────────────────
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src    = gameObject.AddComponent<AudioSource>();
            src.loop           = false;
            src.playOnAwake    = false;
            sfxPool[i]         = src;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SCENE CHANGE
    // ─────────────────────────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameScene = scene.name == gameSceneName;

        if (isGameScene)
        {
            // Escena de juego: cruzar fundido al tema de combate.
            PlayMusic(gameMusic);
        }
        else
        {
            // Menú principal, Info, Configuración…
            // Solo cambia si la música de ambiente no está ya sonando
            // (evita cortar entre menús de la misma "zona").
            if (!IsMusicPlaying(menuAmbientSound))
                PlayMusic(menuAmbientSound);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MUSIC API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Reproduce una música con fade-out del tema anterior y fade-in del nuevo.</summary>
    public void PlayMusic(SoundData data)
    {
        if (data == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CrossfadeMusic(data));
    }

    /// <summary>Para la música con fade.</summary>
    public void StopMusic(float fadeDuration = -1f)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        float dur = fadeDuration >= 0f ? fadeDuration : musicFadeDuration;
        fadeCoroutine = StartCoroutine(FadeOutMusic(dur));
    }

    private IEnumerator CrossfadeMusic(SoundData data)
    {
        AudioClip clip = data.GetClip();
        if (clip == null) yield break;

        float targetVolume = data.GetVolume();

        // ── Fade-out del tema actual ──────────────────────────────────────────
        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            float elapsed  = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed           += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / musicFadeDuration);
                yield return null;
            }

            musicSource.Stop();
        }

        // ── Fade-in del tema nuevo ────────────────────────────────────────────
        musicSource.clip   = clip;
        musicSource.pitch  = data.GetPitch();
        musicSource.volume = 0f;

        if (data.mixerGroup != null)
            musicSource.outputAudioMixerGroup = data.mixerGroup;

        musicSource.Play();

        float elapsed2 = 0f;
        while (elapsed2 < musicFadeDuration)
        {
            elapsed2          += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed2 / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVol = musicSource.volume;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed           += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 0f;
    }

    private bool IsMusicPlaying(SoundData data)
    {
        return data != null
            && musicSource.isPlaying
            && musicSource.clip == data.GetClip();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SFX API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reproduce un SFX desde el pool (round-robin).
    /// Si todos los sources están ocupados, reutiliza el más antiguo.
    /// </summary>
    public void PlaySFX(SoundData data)
    {
        if (data == null) return;

        AudioClip clip = data.GetClip();
        if (clip == null) return;

        AudioSource source = GetNextSFXSource();

        source.clip   = clip;
        source.volume = data.GetVolume();
        source.pitch  = data.GetPitch();

        if (data.mixerGroup != null)
            source.outputAudioMixerGroup = data.mixerGroup;

        source.Play();
    }

    /// <summary>
    /// Reproduce un SFX en una posición 3D del mundo.
    /// Usa AudioSource.PlayClipAtPoint para no consumir el pool.
    /// </summary>
    public void PlaySFXAtPoint(SoundData data, Vector3 worldPosition)
    {
        if (data == null) return;

        AudioClip clip = data.GetClip();
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, worldPosition, data.GetVolume());
    }

    private AudioSource GetNextSFXSource()
    {
        // Preferir sources que no estén sonando.
        for (int i = 0; i < sfxPool.Length; i++)
        {
            int idx = (sfxPoolIndex + i) % sfxPool.Length;
            if (!sfxPool[idx].isPlaying)
            {
                sfxPoolIndex = (idx + 1) % sfxPool.Length;
                return sfxPool[idx];
            }
        }

        // Todos ocupados: reutilizar el siguiente en round-robin.
        AudioSource fallback = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;
        return fallback;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VOLUME CONTROL (para settings)
    // ─────────────────────────────────────────────────────────────────────────

    public void SetMusicVolume(float volume) => musicSource.volume = Mathf.Clamp01(volume);
}