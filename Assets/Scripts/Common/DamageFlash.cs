using UnityEngine;
using System.Collections;

/// <summary>
/// Triggers a visual flash effect on the entity's renderers when taking damage.
/// Uses MaterialPropertyBlock to avoid creating material instances at runtime.
/// </summary>
public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashIntensity = 1.5f;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;

    // Cached Shader Property ID to avoid string lookups
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Starts the flash effect. Restarts the timer if called multiple times rapidly.
    /// </summary>
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
        flashRoutine = null;
    }

    private void SetFlash(bool enabled)
    {
        Color finalColor = enabled ? flashColor * flashIntensity : Color.black;

        foreach (var r in renderers)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColorID, finalColor);
            r.SetPropertyBlock(mpb);
        }
    }
}