using UnityEngine;
using System.Collections;

// Creates a "hit stop" effect for impactful hits. 
// Pauses all Animators briefly without touching Time.timeScale, 
// so physics, cooldowns, and gameplay timers keep ticking normally.
public class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private Coroutine currentRoutine;

    private void Awake() => instance = this;

    // Static entry point. Restarts the timer if hit again during an active hitstop to keep it snappy.
    public static void Trigger(float duration = 0.05f)
    {
        if (instance == null) return;
        
        if (instance.currentRoutine != null)
            instance.StopCoroutine(instance.currentRoutine);
            
        instance.currentRoutine = instance.StartCoroutine(instance.HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        // Find all animators and freeze them
        Animator[] anims = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        for (int i = 0; i < anims.Length; i++)
            anims[i].speed = 0f;

        // Wait using realtime so we aren't affected by Time.timeScale (even though we don't change it here, it's safer)
        yield return new WaitForSecondsRealtime(duration);

        // Resume all animators
        for (int i = 0; i < anims.Length; i++)
            anims[i].speed = 1f;
            
        currentRoutine = null;
    }
}