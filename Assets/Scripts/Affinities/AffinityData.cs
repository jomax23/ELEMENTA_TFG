using UnityEngine;

/// <summary>
/// ScriptableObject that defines the complete affinity table between elements.
/// Configures 4 tables (one for each possible main element) in the Inspector.
/// 
/// Efficiency scales:
///   - Damage / Healing          → multiplied by efficiency
///   - Effect duration           → multiplied by efficiency
///   - Effect intensity          → multiplied by efficiency
///   - Cooldown                  → multiplied by cooldownMultiplier = 1 + (1 - efficiency)
///     · 100% efficiency → 1.0x cooldown (no penalty)
///     · 60%  efficiency → 1.4x cooldown
///     · 30%  efficiency → 1.7x cooldown
/// </summary>
[CreateAssetMenu(fileName = "AffinityData", menuName = "Elementa/Affinity Data")]
public class AffinityData : ScriptableObject
{
    [System.Serializable]
    public class ElementAffinityRow
    {
        [Tooltip("The element this row applies to.")]
        public ElementType element;

        [Tooltip("Number of available abilities for this element (0–4).")]
        [Range(0, 4)]
        public int availableAbilities = 4;

        [Tooltip("Power multiplier (damage, healing, duration, intensity). " +
                 "1.0 = no penalty. 0.0 = no effect (element locked).")]
        [Range(0f, 1f)]
        public float efficiency = 1f;
    }

    [System.Serializable]
    public class AffinityEntry
    {
        [Tooltip("The main element chosen by the player.")]
        public ElementType mainElement;

        [Tooltip("Affinity relationship with each of the 4 elements.")]
        public ElementAffinityRow[] rows = new ElementAffinityRow[4];
    }

    [Tooltip("One entry per main element (Fire, Water, Earth, Air).")]
    [SerializeField] private AffinityEntry[] affinities;

    /// <summary>
    /// Returns how many abilities are available for the <paramref name="targetElement"/>
    /// when the player has chosen <paramref name="mainElement"/> as their main.
    /// </summary>
    public int GetAvailableAbilityCount(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        return row?.availableAbilities ?? 4;
    }

    /// <summary>
    /// Returns the efficiency multiplier for the <paramref name="targetElement"/>
    /// when the player has chosen <paramref name="mainElement"/> as their main.
    /// Scales damage, healing, duration, and effect intensity.
    /// </summary>
    public float GetEfficiency(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        return row?.efficiency ?? 1f;
    }

    /// <summary>
    /// Returns the cooldown multiplier for the <paramref name="targetElement"/>
    /// when the player has chosen <paramref name="mainElement"/> as their main.
    /// Formula: 1 + (1 - efficiency), i.e., lower efficiency means higher penalty.
    /// </summary>
    public float GetCooldownMultiplier(ElementType mainElement, ElementType targetElement)
    {
        float eff = GetEfficiency(mainElement, targetElement);
        if (eff <= 0f) return 1f; // Locked element: cooldown is irrelevant
        return 1f + (1f - eff);   // 100% → 1.0x | 60% → 1.4x | 30% → 1.7x
    }

    /// <summary>
    /// Returns the complete affinity summary for a main/target element pair.
    /// Useful for displaying in a preview UI.
    /// </summary>
    public AffinityInfo GetAffinityInfo(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        if (row == null)
            return new AffinityInfo(4, 1f, 1f);

        float cooldownMult = row.efficiency > 0f ? 1f + (1f - row.efficiency) : 1f;
        return new AffinityInfo(row.availableAbilities, row.efficiency, cooldownMult);
    }

    private ElementAffinityRow FindRow(ElementType mainElement, ElementType targetElement)
    {
        if (affinities == null) return null;

        foreach (AffinityEntry entry in affinities)
        {
            if (entry.mainElement != mainElement) continue;
            if (entry.rows == null) continue;

            foreach (ElementAffinityRow row in entry.rows)
            {
                if (row.element == targetElement)
                    return row;
            }
        }

        Debug.LogWarning($"[AffinityData] No row found for main={mainElement}, target={targetElement}. Using default values (4 abilities, 100% efficiency).");
        return null;
    }
}

/// <summary>
/// Compact summary of the affinity between a main element and a secondary element.
/// Returned by <see cref="AffinityData.GetAffinityInfo"/>.
/// </summary>
public readonly struct AffinityInfo
{
    /// <summary>Number of available abilities (0–4).</summary>
    public readonly int availableAbilities;
    
    /// <summary>Power multiplier (damage, healing, duration, intensity). 0–1.</summary>
    public readonly float efficiency;

    /// <summary>Cooldown multiplier. ≥ 1 (1.0 = no penalty).</summary>
    public readonly float cooldownMultiplier;

    public AffinityInfo(int availableAbilities, float efficiency, float cooldownMultiplier)
    {
        this.availableAbilities = availableAbilities;
        this.efficiency = efficiency;
        this.cooldownMultiplier = cooldownMultiplier;
    }
}