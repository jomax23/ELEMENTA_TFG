using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private Coroutine currentRoutine;

    private void Awake()
    {
        instance = this;
    }

    public static void Trigger(float duration)
    {
        if (instance == null) return;

        if (instance.currentRoutine != null)
            instance.StopCoroutine(instance.currentRoutine);

        instance.currentRoutine = instance.StartCoroutine(instance.HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float originalTimeScale = Time.timeScale;

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
    }
}