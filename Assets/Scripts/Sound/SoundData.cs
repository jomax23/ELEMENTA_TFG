using UnityEngine;

/// <summary>
/// Asset configurable que representa un sonido del juego.
/// Soporta variación de pitch/volumen para evitar la monotonía de efectos repetitivos
/// (pasos, puñetazos, etc.).
/// </summary>
[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("Clip(s)")]
    [Tooltip("Si hay más de un clip se elige uno al azar en cada reproducción.")]
    public AudioClip[] clips;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume     = 1f;
    [Range(0f, 0.3f)] public float volumeVariance = 0f;

    [Header("Pitch")]
    [Range(0.1f, 3f)] public float pitch         = 1f;
    [Range(0f, 0.3f)] public float pitchVariance  = 0f;

    [Header("Mixing")]
    [Tooltip("Opcional: AudioMixerGroup destino.")]
    public UnityEngine.Audio.AudioMixerGroup mixerGroup;

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Devuelve un clip aleatorio del array (o null si está vacío).</summary>
    public AudioClip GetClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetVolume() => Mathf.Clamp01(volume + Random.Range(-volumeVariance, volumeVariance));
    public float GetPitch()  => Mathf.Clamp(pitch  + Random.Range(-pitchVariance,  pitchVariance),  0.1f, 3f);
}