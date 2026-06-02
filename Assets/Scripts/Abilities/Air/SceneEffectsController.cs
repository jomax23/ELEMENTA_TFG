using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Controls global post-processing effects during specific abilities (e.g., Spirit Mode).
/// Acts as a scene singleton. Clones the Volume profile on Awake to prevent mutating the shared asset.
/// </summary>
public class SceneEffectsController : MonoBehaviour
{
    public static SceneEffectsController Instance { get; private set; }

    [SerializeField] private Volume globalVolume;
    [SerializeField] private float transitionDuration = 0.6f;

    private ColorAdjustments color;
    private DepthOfField dof;

    private float originalExposure;
    private float originalSaturation;
    private float originalContrast;
    private bool originalDOFActive;
    private float originalDOFRadius;

    private Coroutine transitionRoutine;

    private void Awake()
    {
        // Scene singleton: does not persist between scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (globalVolume == null)
        {
            Debug.LogError($"[{nameof(SceneEffectsController)}] Global Volume not assigned.", this);
            return;
        }

        // CRITICAL: Clone the profile to avoid mutating the shared asset on disk.
        globalVolume.profile = Instantiate(globalVolume.profile);

        VolumeProfile profile = globalVolume.profile;
        profile.TryGet(out color);
        profile.TryGet(out dof);

        if (color == null)
        {
            Debug.LogError($"[{nameof(SceneEffectsController)}] ColorAdjustments not found in the Volume Profile.", this);
            return;
        }

        CacheOriginalValues();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void CacheOriginalValues()
    {
        originalExposure = color.postExposure.value;
        originalSaturation = color.saturation.value;
        originalContrast = color.contrast.value;

        if (dof != null)
        {
            originalDOFActive = dof.active;
            originalDOFRadius = dof.gaussianMaxRadius.value;
        }
    }

    public void EnableSpiritMode() =>
        StartTransition(-1.8f, -35f, 25f, enableDOF: true, targetBlur: 0.35f);

    public void DisableSpiritMode() =>
        StartTransition(originalExposure, originalSaturation, originalContrast,
                        originalDOFActive, originalDOFRadius);

    private void StartTransition(
        float targetExposure, float targetSaturation, float targetContrast,
        bool enableDOF, float targetBlur)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(
            TransitionRoutine(targetExposure, targetSaturation, targetContrast, enableDOF, targetBlur));
    }

    private IEnumerator TransitionRoutine(
        float targetExposure, float targetSaturation, float targetContrast,
        bool enableDOF, float targetBlur)
    {
        float startExposure = color.postExposure.value;
        float startSaturation = color.saturation.value;
        float startContrast = color.contrast.value;
        float startBlur = dof != null ? dof.gaussianMaxRadius.value : 0f;

        if (dof != null)
        {
            dof.active = enableDOF;
            dof.mode.value = DepthOfFieldMode.Gaussian;
        }

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            // SmoothStep for a more natural transition than linear lerp
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            color.postExposure.value = Mathf.Lerp(startExposure, targetExposure, t);
            color.saturation.value = Mathf.Lerp(startSaturation, targetSaturation, t);
            color.contrast.value = Mathf.Lerp(startContrast, targetContrast, t);

            if (dof != null)
                dof.gaussianMaxRadius.value = Mathf.Lerp(startBlur, targetBlur, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure exact final values
        color.postExposure.value = targetExposure;
        color.saturation.value = targetSaturation;
        color.contrast.value = targetContrast;

        if (dof != null)
        {
            dof.gaussianMaxRadius.value = targetBlur;
            dof.active = enableDOF;
        }
    }
}