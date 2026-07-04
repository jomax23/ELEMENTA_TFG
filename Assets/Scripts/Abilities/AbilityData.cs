using UnityEngine;

// Base ScriptableObject for all abilities. 
// Handles metadata, animation locks, audio, and AI targeting.
// Subclasses MUST implement Activate() to define the actual effect.
public abstract class AbilityData : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;
    public ElementType element;

    [Header("UI")]
    public Sprite icon;

    [Header("Description")]
    [TextArea(3, 5)]
    [Tooltip("Use {0}, {1}, etc. as placeholders. Subclasses inject the real values.")]
    public string descriptionTemplate;

    [Header("Cooldown & Ranges")]
    public float cooldown = 1f;
    [Tooltip("Min distance for AI to consider using this.")]
    public float minRange = 0f;
    [Tooltip("Max effective distance for AI.")]
    public float maxRange = 8f;
    [Tooltip("AI preference weight. Higher = AI uses it more often.")]
    [Range(0.1f, 3f)]
    public float aiPriority = 1f;

    [Header("Animation & Timing")]
    [Tooltip("Exact Animator state name (e.g., Fire1, Water3).")]
    public string animationStateName;
    [Tooltip("Seconds from input press until the effect actually triggers.")]
    [Min(0f)]
    public float activationDelay = 0f;
    [Tooltip("Total time the player is locked in the animation. Must be >= activationDelay.")]
    [Min(0f)]
    public float totalAnimationDuration = 1f;

    [Header("Audio")]
    [Tooltip("SFX played at the exact moment of activation.")]
    [SerializeField] private SoundData activationSound;

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    // Executes the ability effect. 'efficiency' scales damage/healing (1.0 = normal, 0.0 = locked).
    public abstract void Activate(GameObject owner, float efficiency = 1f);

    // Plays the SFX and triggers Activate(). Keeps audio synced across all player abilities.
    public void ActivateWithAudio(GameObject owner, float efficiency = 1f)
    {
        if (activationSound != null)
            AudioManager.Instance?.PlaySFX(activationSound);
            
        Activate(owner, efficiency);
    }

    // Cleans up persistent effects if the user gets interrupted.
    // Subclasses with ongoing effects MUST override this.
    public virtual void Cancel(GameObject owner) { }

    // =========================================================================
    // EDITOR & HELPERS
    // =========================================================================

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Prevents the animation lock from ending before the ability actually fires.
        if (totalAnimationDuration < activationDelay)
        {
            totalAnimationDuration = activationDelay;
            Debug.LogWarning($"[{name}] totalAnimationDuration was lower than activationDelay. Adjusted to {totalAnimationDuration:F2}s.", this);
        }
    }
#endif

    // Digs through the hierarchy to find a specific Transform by name.
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

    // Formats the description string with actual stats.
    // Fallback for abilities without scalable stats; subclasses override this.
    public virtual string GetFormattedDescription(float efficiency)
    {
        return descriptionTemplate;
    }
}