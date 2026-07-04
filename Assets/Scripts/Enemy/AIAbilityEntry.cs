using UnityEngine;

// Data container for the AI's ability configuration. 
// Tracks distance thresholds, priority, and runtime cooldowns.
[System.Serializable]
public class AIAbilityEntry
{
    [Tooltip("The ability to use")]
    public AbilityData ability;

    [Tooltip("Minimum distance to the player required to use this ability")]
    public float minRange = 0f;

    [Tooltip("Maximum distance to the player at which this ability can be used")]
    public float maxRange = 8f;

    [Tooltip("How strongly this ability is preferred when multiple are valid. Higher values = used more often.")]
    [Range(0.1f, 3f)]
    public float priority = 1f;

    // Runtime cooldown tracking (reset every play, not serialized)
    [HideInInspector]
    public float cooldownTimer = 0f;
}