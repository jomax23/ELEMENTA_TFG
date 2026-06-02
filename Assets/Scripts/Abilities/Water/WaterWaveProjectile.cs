using UnityEngine;

/// <summary>
/// Continuous water wave that applies damage, slow, and knockback to targets it passes through.
/// Unlike standard projectiles, it does NOT destroy upon hitting a target.
/// It only stops when hitting a physical obstacle (handled by ProjectileBase).
/// </summary>
public class WaterWaveProjectile : ProjectileBase
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f;

    private int directionX;
    private float actualDamage, actualPushForce, actualSlowDuration, actualSlowMultiplier;

    /// <summary>
    /// Initializes the wave with direction, target layers, and affinity efficiency.
    /// </summary>
    public void Initialize(int dirX, LayerMask layers, float scaledDamage, float scaledPush, float scaledSlowMult, float scaledSlowDur)
    {
        directionX = dirX;
        targetLayers = layers;
        actualDamage = scaledDamage;
        actualPushForce = scaledPush;
        actualSlowMultiplier = scaledSlowMult;
        actualSlowDuration = scaledSlowDur;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // TryMove handles obstacle detection via Raycast
        TryMove(Vector3.right * directionX, speed);
    }

    /// <summary>
    /// Called by ProjectileBase when overlapping a valid target.
    /// Applies effects but DOES NOT destroy the wave, allowing it to pass through multiple targets.
    /// </summary>
    protected override void OnTargetHit(Collider target)
    {
        if (target.GetComponent<IAbilityTarget>() is IAbilityTarget abilityTarget)
        {
            abilityTarget.ApplyImpulse(directionX * actualPushForce);
            abilityTarget.ApplySlow(actualSlowMultiplier, actualSlowDuration);
            abilityTarget.ApplyDamage(actualDamage);
        }
    }
}