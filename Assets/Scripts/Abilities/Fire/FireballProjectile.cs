using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
// Bola de Fuego — proyectil que aplica daño de impacto + quemadura.
// Hereda de ProjectileBase para la detección automática de obstáculos.
// ──────────────────────────────────────────────────────────────────────────────
public class FireballProjectile : ProjectileBase, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed    = 14f;
    [SerializeField] private float lifetime = 2f;

    [Header("Damage")]
    [SerializeField] private float impactDamage        = 10f;
    [SerializeField] private float burnDamagePerSecond = 2f;
    [SerializeField] private float burnDuration        = 3f;

    private int   directionX;
    private float lifeTimer;

    private float actualImpactDamage;
    private float actualBurnDps;
    private float actualBurnDuration;

    // ─────────────────────────────────────────────────────────────────────────

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño y duración de quemadura.</param>
    public void Initialize(int dirX, LayerMask layers, float efficiency = 1f)
    {
        directionX   = dirX;
        targetLayers = layers;  // asignado en ProjectileBase

        actualImpactDamage = impactDamage        * efficiency;
        actualBurnDps      = burnDamagePerSecond * efficiency;
        actualBurnDuration = burnDuration        * efficiency;
    }

    private void Update()
    {
        transform.position += Vector3.right * directionX * speed * Time.deltaTime;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
            Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ProjectileBase — template methods
    // ─────────────────────────────────────────────────────────────────────────

    protected override void OnTargetHit(Collider target)
    {
        IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
        if (abilityTarget != null)
        {
            abilityTarget.ApplyDamage(actualImpactDamage);
            abilityTarget.ApplyBurn(actualBurnDps, actualBurnDuration);
        }

        Destroy(gameObject);
    }

    // OnObstacleHit() usa la implementación por defecto: Destroy(gameObject).
    // Si quieres añadir VFX de impacto en pared, haz override aquí.

    // ─────────────────────────────────────────────────────────────────────────
    // IReversible
    // ─────────────────────────────────────────────────────────────────────────

    public void ReverseDirection()
    {
        directionX *= -1;
    }
}