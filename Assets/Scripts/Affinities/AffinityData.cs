using UnityEngine;

// Defines the 4x4 affinity matrix. 
// Each "Main Element" has a specific set of rules for how it interacts with all 4 elements.
[CreateAssetMenu(fileName = "AffinityData", menuName = "Elementa/Affinity Data")]
public class AffinityData : ScriptableObject
{
    [System.Serializable]
    public class ElementAffinityRow
    {
        public ElementType element;
        
        [Range(0, 4)]
        public int availableAbilities = 4;
        
        // 1.0 = full power, 0.0 = completely locked
        [Range(0f, 1f)]
        public float efficiency = 1f;
    }

    [System.Serializable]
    public class AffinityEntry
    {
        public ElementType mainElement;
        public ElementAffinityRow[] rows = new ElementAffinityRow[4];
    }

    [SerializeField] private AffinityEntry[] affinities;

    public int GetAvailableAbilityCount(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        return row?.availableAbilities ?? 4; // Fallback to 4 if data is missing
    }

    public float GetEfficiency(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        return row?.efficiency ?? 1f;
    }

    // Cooldown penalty formula: lower efficiency = higher cooldown.
    // 100% efficiency = 1.0x cooldown (no penalty)
    // 60% efficiency  = 1.4x cooldown
    // 0% efficiency   = 2.0x cooldown (locked, though cooldown doesn't matter here)
    public float GetCooldownMultiplier(ElementType mainElement, ElementType targetElement)
    {
        float eff = GetEfficiency(mainElement, targetElement);
        if (eff <= 0f) return 1f; 
        return 1f + (1f - eff);   
    }

    // Bundles all affinity stats into a single struct for easy UI consumption
    public AffinityInfo GetAffinityInfo(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        if (row == null) return new AffinityInfo(4, 1f, 1f);
        
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
                if (row.element == targetElement) return row;
            }
        }
        
        Debug.LogWarning($"[AffinityData] Missing data for main={mainElement}, target={targetElement}. Using defaults.");
        return null;
    }
}

// Compact data container returned to the UI
public readonly struct AffinityInfo
{
    public readonly int availableAbilities;
    public readonly float efficiency;
    public readonly float cooldownMultiplier;

    public AffinityInfo(int availableAbilities, float efficiency, float cooldownMultiplier)
    {
        this.availableAbilities = availableAbilities;
        this.efficiency = efficiency;
        this.cooldownMultiplier = cooldownMultiplier;
    }
}