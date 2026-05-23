using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private Coroutine currentRoutine;

    private void Awake() => instance = this;

    public static void Trigger(float duration = 0.05f)
    {
        if (instance == null) return;
        if (instance.currentRoutine != null) instance.StopCoroutine(instance.currentRoutine);
        instance.currentRoutine = instance.StartCoroutine(instance.HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        // EFECTO SOLO VISUAL: Pausa animaciones sin tocar Time.timeScale ni físicas
        Animator[] anims = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (var a in anims) a.speed = 0f;

        yield return new WaitForSecondsRealtime(duration);

        foreach (var a in anims) a.speed = 1f;
    }
}