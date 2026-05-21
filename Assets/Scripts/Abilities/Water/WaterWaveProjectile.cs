using UnityEngine;

// La ola NO se destruye al golpear targets — aplica efectos y continúa avanzando.
// SÍ se detiene al golpear un obstáculo físico (pared, plataforma).
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

    public void Initialize(int dirX, LayerMask layers, float efficiency = 1f)
    {
        directionX   = dirX;
        targetLayers = layers;

        actualDamage         = damage       * efficiency;
        actualPushForce      = pushForce    * efficiency;
        actualSlowDuration   = slowDuration * efficiency;
        actualSlowMultiplier = Mathf.Lerp(1f, slowMultiplier, efficiency);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        TryMove(Vector3.right * directionX, speed);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ProjectileBase
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>La ola NO se destruye al golpear un target — sigue avanzando.</summary>
    protected override void OnTargetHit(Collider target)
    {
        IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
        if (abilityTarget != null)
        {
            abilityTarget.ApplyImpulse(directionX * actualPushForce);
            abilityTarget.ApplySlow(actualSlowMultiplier, actualSlowDuration);
            abilityTarget.ApplyDamage(actualDamage);
        }
        // Sin Destroy: la ola continúa.
    }

    // OnObstacleHit() usa el default de ProjectileBase: Destroy(gameObject).
}