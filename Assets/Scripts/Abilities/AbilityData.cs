using UnityEngine;

public abstract class AbilityData : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;

    [TextArea]
    public string description;

    public ElementType element;

    [Header("UI")]
    public Sprite icon;

    [Header("Cooldown")]
    public float cooldown = 1f;

    [Header("Combat Range")]
    [Tooltip("Minimum distance to the target for this ability to make sense.\n" +
             "Example: a melee slam → 0. A fireball → 3.")]
    public float minRange = 0f;

    [Tooltip("Maximum distance at which this ability is effective.\n" +
             "Example: a close-range shockwave → 2. A long-range projectile → 12.")]
    public float maxRange = 8f;

    [Tooltip("How much the AI prefers this ability over others when several are valid.\n" +
             "Higher = chosen more often. Think of it as 'combo weight'.")]
    [Range(0.1f, 3f)]
    public float aiPriority = 1f;

    public abstract void Activate(GameObject owner);
}