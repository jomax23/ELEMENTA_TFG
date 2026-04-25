using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
// Golpe de Marea — proyectil que atraviesa targets (aplica efectos sin destruirse)
// pero se detiene al golpear un obstáculo físico.
// Hereda de ProjectileBase para la detección automática de obstáculos.
// ──────────────────────────────────────────────────────────────────────────────
public class WaterWaveProjectile : ProjectileBase
{
    [Header("Movement")]
    [SerializeField] private float speed    = 10f;
    [SerializeField] private float lifetime = 2f;

    [Header("Effects")]
    [SerializeField] private float pushForce      = 12f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration   = 2f;
    [SerializeField] private float damage         = 10f;

    private int   directionX;
    private float actualPushForce;
    private float actualSlowDuration;
    private float actualSlowMultiplier;
    private float actualDamage;

    // ─────────────────────────────────────────────────────────────────────────

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño, impulso y slow.</param>
    public void Initialize(int dirX, LayerMask layers, float efficiency = 1f)
    {
        directionX   = dirX;
        targetLayers = layers; // asignado en ProjectileBase

        actualDamage         = damage       * efficiency;
        actualPushForce      = pushForce    * efficiency;
        actualSlowDuration   = slowDuration * efficiency;
        // Slow multiplier funciona al revés: menor = más lento.
        // Con baja efficiency, lo acercamos a 1 (menos penalización).
        actualSlowMultiplier = Mathf.Lerp(1f, slowMultiplier, efficiency);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.right * directionX * speed * Time.deltaTime;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ProjectileBase — template methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// La ola NO se destruye al golpear a un target — aplica efectos y sigue avanzando.
    /// Esto permite que una sola ola afecte a múltiples enemigos si los hay en línea.
    /// </summary>
    protected override void OnTargetHit(Collider target)
    {
        IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
        if (abilityTarget != null)
        {
            abilityTarget.ApplyImpulse(directionX * actualPushForce);
            abilityTarget.ApplySlow(actualSlowMultiplier, actualSlowDuration);
            abilityTarget.ApplyDamage(actualDamage);
        }
        // Sin Destroy: la ola continúa avanzando.
    }

    // OnObstacleHit() usa la implementación por defecto: Destroy(gameObject).
    // La ola se detiene al chocar con una pared.
}