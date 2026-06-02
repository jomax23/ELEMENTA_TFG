using UnityEngine;

/// <summary>
/// Base ScriptableObject for all ability definitions.
/// Handles core metadata, animation locking, audio, and AI targeting ranges.
/// Subclasses must implement <see cref="Activate"/> to define the specific effect.
/// </summary>
public abstract class AbilityData : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;
    public ElementType element;

    [Header("UI")]
    public Sprite icon;

    [Header("Description Template")]
    [TextArea(3, 5)]
    [Tooltip("Usa {0}, {1}, {2}, etc., como marcadores. Las subclases inyectarán los valores reales aquí.")]
    public string descriptionTemplate;
    
    [Header("Cooldown & Ranges")]
    public float cooldown = 1f;

    [Tooltip("Minimum distance to the target for the AI to consider using this ability.")]
    public float minRange = 0f;

    [Tooltip("Maximum effective distance for the AI to use this ability.")]
    public float maxRange = 8f;

    [Tooltip("AI preference weight. Higher values make the AI choose this ability more often.")]
    [Range(0.1f, 3f)]
    public float aiPriority = 1f;

    [Header("Animation & Timing")]
    [Tooltip("Exact name of the state in the Animator Controller (e.g., Fire1, Water3).")]
    public string animationStateName;

    [Tooltip("Seconds from input press until the ability effect actually triggers.")]
    [Min(0f)]
    public float activationDelay = 0f;

    [Tooltip("Total duration the player is locked in this ability animation. Must be >= activationDelay.")]
    [Min(0f)]
    public float totalAnimationDuration = 1f;

    [Header("Audio")]
    [Tooltip("Sound played at the exact moment of activation.")]
    [SerializeField] private SoundData activationSound;

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    /// <summary>
    /// Executes the ability effect, applying the affinity efficiency multiplier.
    /// </summary>
    /// <param name="owner">The GameObject casting the ability.</param>
    /// <param name="efficiency">Scales damage/healing/duration (1.0 = no penalty, 0.0 = locked).</param>
    public abstract void Activate(GameObject owner, float efficiency = 1f);

    /// <summary>
    /// Plays the activation sound and calls Activate().
    /// Used by PlayerAbilities to ensure audio is always synchronized.
    /// </summary>
    public void ActivateWithAudio(GameObject owner, float efficiency = 1f)
    {
        if (activationSound != null)
            AudioManager.Instance?.PlaySFX(activationSound);

        Activate(owner, efficiency);
    }

    /// <summary>
    /// Cancels all ongoing effects of this ability.
    /// Called when the user is interrupted before the ability finishes its full cycle.
    /// Subclasses with persistent effects MUST override this.
    /// </summary>
    public virtual void Cancel(GameObject owner) { }

    // =========================================================================
    // EDITOR & HELPERS
    // =========================================================================

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (totalAnimationDuration < activationDelay)
        {
            totalAnimationDuration = activationDelay;
            Debug.LogWarning($"[{name}] totalAnimationDuration was lower than activationDelay. Adjusted to {totalAnimationDuration:F2}s.", this);
        }
    }
#endif

    /// <summary>
    /// Recursively searches for a Transform by name in the entire hierarchy.
    /// </summary>
    public static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        
        foreach (Transform child in root)
        {
            Transform result = FindDeep(child, name);
            if (result != null) return result;
        }
        
        return null;
    }
    
    public virtual string GetFormattedDescription(float efficiency)
    {
        // Por defecto, devuelve el texto base sin modificar (para habilidades sin stats escalables)
        return descriptionTemplate;
    }
}