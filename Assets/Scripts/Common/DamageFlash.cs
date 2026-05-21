using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashIntensity = 1.5f;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private Coroutine flashRoutine;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void TriggerFlash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetFlash(true);
        yield return new WaitForSeconds(flashDuration);
        SetFlash(false);
    }

    private void SetFlash(bool enabled)
    {
        Color finalColor = enabled ? flashColor * flashIntensity : Color.black;

        foreach (var r in renderers)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColor, finalColor);
            r.SetPropertyBlock(mpb);
        }
    }
}