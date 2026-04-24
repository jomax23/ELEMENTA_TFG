using UnityEngine;

/// <summary>
/// ScriptableObject que define la tabla completa de afinidades entre elementos.
///
/// Configura las 4 tablas (una por posible elemento principal) en el Inspector.
/// La eficiencia escala:
///   - Daño / curación          → multiplicado por efficiency
///   - Duración de efectos      → multiplicado por efficiency
///   - Intensidad de efectos    → multiplicado por efficiency
///   - Cooldown                 → multiplicado por cooldownMultiplier = 1 + (1 - efficiency)
///     · 100% efficiency → 1.0x cooldown (sin penalización)
///     · 60%  efficiency → 1.4x cooldown
///     · 30%  efficiency → 1.7x cooldown
/// </summary>
[CreateAssetMenu(
    fileName = "AffinityData",
    menuName  = "Elementa/Affinity Data"
)]
public class AffinityData : ScriptableObject
{
    // ──────────────────────────────────────────────────────────────────────────
    // DATA STRUCTURES
    // ──────────────────────────────────────────────────────────────────────────

    [System.Serializable]
    public class ElementAffinityRow
    {
        [Tooltip("Elemento al que aplica esta fila.")]
        public ElementType element;

        [Tooltip("Número de habilidades disponibles de este elemento (0–4).")]
        [Range(0, 4)]
        public int availableAbilities = 4;

        [Tooltip("Multiplicador de potencia (daño, duración, intensidad). " +
                 "1.0 = sin penalización. 0.0 = sin efecto (elemento bloqueado).")]
        [Range(0f, 1f)]
        public float efficiency = 1f;
    }

    [System.Serializable]
    public class AffinityEntry
    {
        [Tooltip("Elemento principal elegido por el jugador.")]
        public ElementType mainElement;

        [Tooltip("Relación de afinidad con cada uno de los 4 elementos.")]
        public ElementAffinityRow[] rows = new ElementAffinityRow[4];
    }

    // ──────────────────────────────────────────────────────────────────────────
    // INSPECTOR
    // ──────────────────────────────────────────────────────────────────────────

    [Tooltip("Una entrada por elemento principal (Fire, Water, Earth, Air).")]
    [SerializeField] private AffinityEntry[] affinities;

    // ──────────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve cuántas habilidades están disponibles del <paramref name="targetElement"/>
    /// cuando el jugador ha elegido <paramref name="mainElement"/> como principal.
    /// </summary>
    public int GetAvailableAbilityCount(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        return row?.availableAbilities ?? 4;
    }

    /// <summary>
    /// Devuelve el multiplicador de eficiencia del <paramref name="targetElement"/>
    /// cuando el jugador ha elegido <paramref name="mainElement"/> como principal.
    /// Escala daño, curación, duración e intensidad de efectos.
    /// </summary>
    public float GetEfficiency(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        return row?.efficiency ?? 1f;
    }

    /// <summary>
    /// Devuelve el multiplicador de cooldown del <paramref name="targetElement"/>
    /// cuando el jugador ha elegido <paramref name="mainElement"/> como principal.
    /// Fórmula: 1 + (1 - efficiency), i.e. a menor eficiencia mayor penalización.
    /// </summary>
    public float GetCooldownMultiplier(ElementType mainElement, ElementType targetElement)
    {
        float eff = GetEfficiency(mainElement, targetElement);
        if (eff <= 0f) return 1f; // elemento bloqueado: el cooldown es irrelevante
        return 1f + (1f - eff);   // 100% → 1.0x  |  60% → 1.4x  |  30% → 1.7x
    }

    /// <summary>
    /// Devuelve el resumen completo de afinidad para un par main/target de elementos.
    /// Útil para mostrar en UI de preview.
    /// </summary>
    public AffinityInfo GetAffinityInfo(ElementType mainElement, ElementType targetElement)
    {
        ElementAffinityRow row = FindRow(mainElement, targetElement);
        if (row == null)
            return new AffinityInfo(4, 1f, 1f);

        float cooldownMult = row.efficiency > 0f ? 1f + (1f - row.efficiency) : 1f;

        return new AffinityInfo(row.availableAbilities, row.efficiency, cooldownMult);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ──────────────────────────────────────────────────────────────────────────

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

        Debug.LogWarning(
            $"[AffinityData] No se encontró fila para main={mainElement}, target={targetElement}. " +
            $"Usando valores por defecto (4 habilidades, 100% eficiencia)."
        );
        return null;
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// VALUE TYPE — resultado de una consulta de afinidad
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Resumen compacto de la afinidad entre un elemento principal y uno secundario.
/// Devuelto por <see cref="AffinityData.GetAffinityInfo"/>.
/// </summary>
public readonly struct AffinityInfo
{
    /// <summary>Número de habilidades disponibles (0–4).</summary>
    public readonly int availableAbilities;

    /// <summary>Multiplicador de potencia (daño, curación, duración, intensidad). 0–1.</summary>
    public readonly float efficiency;

    /// <summary>Multiplicador de cooldown. ≥ 1 (1.0 = sin penalización).</summary>
    public readonly float cooldownMultiplier;

    public AffinityInfo(int availableAbilities, float efficiency, float cooldownMultiplier)
    {
        this.availableAbilities = availableAbilities;
        this.efficiency         = efficiency;
        this.cooldownMultiplier = cooldownMultiplier;
    }
}