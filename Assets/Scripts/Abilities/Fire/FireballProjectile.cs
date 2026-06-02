using UnityEngine;

/// <summary>
/// Standard fireball projectile that moves forward, detects obstacles via Raycast,
/// and applies impact damage plus a burn effect on hit.
/// </summary>
public class FireballProjectile : ProjectileBase, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifetime = 2f;
    
    private int directionX;
    private float lifeTimer;
    private float actualImpactDamage;
    private float actualBurnDps;
    private float actualBurnDuration;

    /// <summary>
    /// Initializes the fireball with direction, target layers, and affinity efficiency.
    /// </summary>
    public void Initialize(int dirX, LayerMask layers, float scaledDamage, float scaledBurnDps, float scaledBurnDur)
    {
        directionX = dirX;
        targetLayers = layers;
        actualImpactDamage = scaledDamage;
        actualBurnDps = scaledBurnDps;
        actualBurnDuration = scaledBurnDur;
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // TryMove performs a Raycast before moving. If an obstacle is hit, 
        // it destroys the projectile and returns false, halting further Update execution.
        if (!TryMove(Vector3.right * directionX, speed)) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called by ProjectileBase when the trigger overlaps with a valid target.
    /// </summary>
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

    /// <summary>
    /// Reverses the horizontal movement direction (used by abilities like Tornado).
    /// </summary>
    public void ReverseDirection()
    {
        directionX *= -1;
    }
}