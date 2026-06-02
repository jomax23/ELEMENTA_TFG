using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Configurable ScriptableObject representing a game sound.
/// Supports pitch and volume variance to prevent repetition fatigue.
/// </summary>
[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("Clips")]
    [Tooltip("If multiple clips are provided, one is chosen at random on playback.")]
    public AudioClip[] clips;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 0.3f)] public float volumeVariance = 0f;

    [Header("Pitch")]
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(0f, 0.3f)] public float pitchVariance = 0f;

    [Header("Mixing")]
    [Tooltip("Optional: Target AudioMixerGroup.")]
    public AudioMixerGroup mixerGroup;

    /// <summary>
    /// Returns a random clip from the array, or null if empty.
    /// </summary>
    public AudioClip GetClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    /// <summary>
    /// Returns the volume with random variance applied, clamped between 0 and 1.
    /// </summary>
    public float GetVolume() 
    {
        return Mathf.Clamp01(volume + Random.Range(-volumeVariance, volumeVariance));
    }

    /// <summary>
    /// Returns the pitch with random variance applied, clamped between 0.1 and 3.
    /// </summary>
    public float GetPitch()  
    {
        return Mathf.Clamp(pitch + Random.Range(-pitchVariance, pitchVariance), 0.1f, 3f);
    }
}