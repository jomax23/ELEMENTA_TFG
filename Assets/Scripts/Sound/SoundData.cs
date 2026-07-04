using UnityEngine;
using UnityEngine.Audio;

// Data container for audio clips.
// Supports randomized clip selection and pitch/volume variance to prevent the 
// "machine gun" effect from repetitive sounds (like footsteps or punches).
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
    [Tooltip("Optional: Target AudioMixerGroup for global volume control.")]
    public AudioMixerGroup mixerGroup;

    public AudioClip GetClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetVolume() 
    {
        return Mathf.Clamp01(volume + Random.Range(-volumeVariance, volumeVariance));
    }

    public float GetPitch()  
    {
        return Mathf.Clamp(pitch + Random.Range(-pitchVariance, pitchVariance), 0.1f, 3f);
    }
}