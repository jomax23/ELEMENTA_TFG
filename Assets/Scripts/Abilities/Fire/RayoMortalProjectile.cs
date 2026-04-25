using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
// Rayo Mortal — beam de corta duración que aplica daño + stun en área.
// Hereda de ProjectileBase para la detección automática de obstáculos.
// ──────────────────────────────────────────────────────────────────────────────
public class RayoMortalProjectile : ProjectileBase
{
    [Header("Beam Settings")]
    [SerializeField] private float damage       = 20f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float lifetime     = 0.15f;

    private float actualDamage;
    private float actualStunDuration;

    // ─────────────────────────────────────────────────────────────────────────

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño y duración del stun.</param>
    public void Initialize(int directionX, LayerMask layers, float efficiency = 1f)
    {
        targetLayers       = layers; // asignado en ProjectileBase
        actualDamage       = damage       * efficiency;
        actualStunDuration = stunDuration * efficiency;

        Vector3 dir = Vector3.right * directionX;

        transform.up = dir;

        float halfLength = transform.localScale.y * 0.5f;
        transform.position += dir * halfLength;

        Destroy(gameObject, lifetime);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ProjectileBase — template methods
    // ─────────────────────────────────────────────────────────────────────────

    protected override void OnTargetHit(Collider target)
    {
        IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
        if (abilityTarget == null) return;

        abilityTarget.ApplyDamage(actualDamage);
        abilityTarget.ApplyStun(actualStunDuration);
    }

    /// <summary>
    /// El rayo se destruye al tocar un obstáculo, impidiendo que atraviese paredes.
    /// Al ser un beam de muy corta duración (0.15s), si la pared está en medio del
    /// recorrido, los enemigos tras ella quedan protegidos.
    /// </summary>
    protected override void OnObstacleHit(Collider obstacle)
    {
        Destroy(gameObject);
    }
}