using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
// Ráfaga de Agua — proyectil que aplica impulso + daño.
// Hereda de ProjectileBase para la detección automática de obstáculos.
// ──────────────────────────────────────────────────────────────────────────────
public class WaterBallProjectile : ProjectileBase, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed    = 12f;
    [SerializeField] private float lifetime = 2f;

    [Header("Effects")]
    [SerializeField] private float pushForce = 6f;
    [SerializeField] private float damage    = 10f;

    private float directionX;
    private float actualPushForce;
    private float actualDamage;

    // ─────────────────────────────────────────────────────────────────────────

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño e impulso.</param>
    public void Initialize(float dirX, LayerMask layers, float efficiency = 1f)
    {
        directionX    = Mathf.Sign(dirX);
        targetLayers  = layers; // asignado en ProjectileBase

        actualPushForce = pushForce * efficiency;
        actualDamage    = damage    * efficiency;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.right * directionX * speed * Time.deltaTime;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ProjectileBase — template methods
    // ─────────────────────────────────────────────────────────────────────────

    protected override void OnTargetHit(Collider target)
    {
        IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
        if (abilityTarget != null)
        {
            abilityTarget.ApplyImpulse(directionX * actualPushForce);
            abilityTarget.ApplyDamage(actualDamage);
        }

        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IReversible
    // ─────────────────────────────────────────────────────────────────────────

    public void ReverseDirection()
    {
        directionX = Mathf.Sign(directionX) * -1f;
    }
}