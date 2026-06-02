using UnityEngine;
using System.Collections;

/// <summary>
/// Creates a "Hit Stop" effect by briefly pausing all Animators in the scene.
/// This provides visual impact without freezing physics or Time.timeScale.
/// </summary>
public class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private Coroutine currentRoutine;

    private void Awake() => instance = this;

    /// <summary>
    /// Triggers the hit stop effect for the specified duration.
    /// Restarts the timer if triggered again while already active.
    /// </summary>
    public static void Trigger(float duration = 0.05f)
    {
        if (instance == null) return;

        if (instance.currentRoutine != null)
            instance.StopCoroutine(instance.currentRoutine);

        instance.currentRoutine = instance.StartCoroutine(instance.HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        // Find all animators and pause them
        Animator[] anims = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        
        for (int i = 0; i < anims.Length; i++)
            anims[i].speed = 0f;

        // Wait using realtime to ignore Time.timeScale (though we aren't changing it here, it's safer)
        yield return new WaitForSecondsRealtime(duration);

        // Resume all animators
        for (int i = 0; i < anims.Length; i++)
            anims[i].speed = 1f;

        currentRoutine = null;
    }
}