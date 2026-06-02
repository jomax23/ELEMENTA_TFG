using UnityEngine;

/// <summary>
/// Standard water projectile that applies impact damage and physical knockback.
/// Supports direction reversal (e.g., via Tornado).
/// </summary>
public class WaterBallProjectile : ProjectileBase, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2f;

    private int directionX;
    private float actualDamage;
    private float actualPushForce;

    /// <summary>
    /// Initializes the projectile with direction, target layers, and affinity efficiency.
    /// </summary>
    public void Initialize(int dirX, LayerMask layers, float scaledDamage, float scaledPushForce)
    {
        directionX = dirX;
        targetLayers = layers;
        actualDamage = scaledDamage;
        actualPushForce = scaledPushForce;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // TryMove handles obstacle detection via Raycast
        TryMove(Vector3.right * directionX, speed);
    }

    /// <summary>
    /// Called by ProjectileBase when hitting a valid target.
    /// Applies damage, knockback, and destroys the projectile.
    /// </summary>
    protected override void OnTargetHit(Collider target)
    {
        if (target.GetComponent<IAbilityTarget>() is IAbilityTarget abilityTarget)
        {
            abilityTarget.ApplyImpulse(directionX * actualPushForce);
            abilityTarget.ApplyDamage(actualDamage);
        }
        Destroy(gameObject);
    }


    /// <summary>
    /// Reverses the horizontal movement direction.
    /// </summary>
    public void ReverseDirection()
    {
        directionX *= -1;
    }
}