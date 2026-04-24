using UnityEngine;

/// <summary>
/// Base class para todos los datos de habilidad.
///
/// CAMBIO vs versión anterior:
///   Activate() y ActivateWithAudio() ahora aceptan un parámetro <c>efficiency</c> (0–1).
///   Las subclases deben pasar este valor a sus proyectiles/áreas para que apliquen
///   las penalizaciones de afinidad (daño reducido, duraciones más cortas, etc.).
///
///   Compatibilidad con llamadas antiguas: el parámetro tiene valor por defecto 1f,
///   por lo que el código del enemigo (EnemyAI) no requiere modificación.
/// </summary>
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

    [Header("Animation")]
    [Tooltip("Nombre exacto del estado en el Animator Controller (ej: Fire1, Water3...)")]
    public string animationStateName;

    [Tooltip("Segundos desde que se pulsa el input hasta que el efecto de la habilidad ocurre. " +
             "Debe coincidir con el frame de impacto de tu animación.")]
    [Min(0f)]
    public float activationDelay = 0f;

    [Tooltip("Duración total del bloqueo de control que impone esta habilidad. " +
             "El jugador NO puede moverse, saltar ni usar otras habilidades durante este tiempo. " +
             "Debe ser >= activationDelay.")]
    [Min(0f)]
    public float totalAnimationDuration = 1f;

    [Header("Audio")]
    [Tooltip("Sonido que se reproduce en el momento exacto de activación.")]
    [SerializeField] private SoundData activationSound;

    // ─────────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Ejecuta el efecto de la habilidad aplicando el multiplicador de eficiencia de afinidad.
    ///
    /// <paramref name="efficiency"/> escala daño, curación, duración e intensidad de efectos.
    ///   · 1.0 = elemento principal (sin penalización).
    ///   · 0.6 = elemento afín secundario.
    ///   · 0.3 = elemento lejano.
    ///   · 0.0 = elemento bloqueado (nunca debe llegar a llamarse).
    /// </summary>
    public abstract void Activate(GameObject owner, float efficiency = 1f);

    /// <summary>
    /// Reproduce el SFX y llama a Activate().
    /// PlayerAbilities usa este método en lugar de Activate() directamente.
    /// </summary>
    public void ActivateWithAudio(GameObject owner, float efficiency = 1f)
    {
        if (activationSound != null)
            AudioManager.Instance?.PlaySFX(activationSound);

        Activate(owner, efficiency);
    }

    /// <summary>
    /// Cancela todos los efectos en curso de esta habilidad.
    /// PlayerAbilities lo llama cuando el usuario es interrumpido antes de que
    /// la habilidad haya terminado su ciclo completo.
    ///
    /// Las subclases con efectos persistentes DEBEN hacer override aquí.
    /// Las habilidades instantáneas no necesitan override.
    /// </summary>
    public virtual void Cancel(GameObject owner) { }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (totalAnimationDuration < activationDelay)
        {
            totalAnimationDuration = activationDelay;
            Debug.LogWarning(
                $"[{name}] totalAnimationDuration era menor que activationDelay. " +
                $"Se ha ajustado a {totalAnimationDuration:F2}s.",
                this
            );
        }
    }
#endif

    /// <summary>Busca un Transform por nombre en toda la jerarquía (no solo hijos directos).</summary>
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
}