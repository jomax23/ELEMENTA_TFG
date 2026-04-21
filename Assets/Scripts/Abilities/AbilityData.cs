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

    [Header("Animation")]
    [Tooltip("Nombre exacto del estado en el Animator Controller (ej: Fire1, Water3...)")]
    public string animationStateName;

    [Tooltip("Segundos desde que se pulsa el input hasta que el efecto de la habilidad ocurre. " +
             "Debe coincidir con el frame de impacto de tu animación.")]
    [Min(0f)]
    public float activationDelay = 0f;

    [Tooltip("Duración total del bloqueo de control que impone esta habilidad. " +
             "El jugador NO puede moverse, saltar ni usar otras habilidades durante este tiempo. " +
             "Debe ser >= activationDelay. Configúralo para que coincida con la duración real de tu " +
             "animación de habilidad en el Animator.")]
    [Min(0f)]
    public float totalAnimationDuration = 1f;

    [Header("Audio")]
    [Tooltip("Sonido que se reproduce en el momento exacto de activación " +
             "(cuando el efecto ocurre, es decir, tras el activationDelay).")]
    [SerializeField] private SoundData activationSound;

    // ─────────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Ejecuta el efecto de la habilidad.
    /// Llamado automáticamente por PlayerAbilities en el frame de impacto (tras activationDelay).
    /// </summary>
    public abstract void Activate(GameObject owner);

    /// <summary>
    /// Reproduce el SFX y llama a Activate().
    /// PlayerAbilities usa este método en lugar de Activate() directamente.
    /// </summary>
    public void ActivateWithAudio(GameObject owner)
    {
        if (activationSound != null)
            AudioManager.Instance?.PlaySFX(activationSound);

        Activate(owner);
    }

    /// <summary>
    /// Cancela todos los efectos en curso de esta habilidad.
    /// PlayerAbilities lo llama cuando el usuario es interrumpido (stun, muerte, etc.)
    /// antes de que la habilidad haya terminado su ciclo completo.
    ///
    /// Las subclases con efectos persistentes (haces, VFX, healing over time, bursts)
    /// DEBEN hacer override aquí para destruir objetos spawneados y abortar coroutines internas.
    /// Las habilidades instantáneas no necesitan override.
    /// </summary>
    public virtual void Cancel(GameObject owner) { }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Garantiza que la duración total nunca sea menor que el delay de activación,
        // lo que produciría un timer negativo (unlock antes de que el efecto ocurra).
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
}