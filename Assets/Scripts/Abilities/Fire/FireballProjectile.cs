using UnityEngine;

// Standard fireball. Moves forward, handles impact/burn damage, and can be reflected by the Tornado.
// Inherits basic movement and collision logic from ProjectileBase.
public class FireballProjectile : ProjectileBase, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifetime = 2f;

    private int directionX;
    private float lifeTimer;

    // Scaled stats injected by the AbilityData
    private float actualImpactDamage;
    private float actualBurnDps;
    private float actualBurnDuration;

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
        // TryMove (from base class) raycasts ahead. If it hits a wall, it destroys the projectile and returns false.
        if (!TryMove(Vector3.right * directionX, speed)) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    // Triggered by the base class when the hitbox overlaps a valid target
    protected override void OnTargetHit(Collider target)
    {
        if (target.GetComponent<IAbilityTarget>() is IAbilityTarget abilityTarget)
        {
            abilityTarget.ApplyDamage(actualImpactDamage);
            abilityTarget.ApplyBurn(actualBurnDps, actualBurnDuration);
        }
        Destroy(gameObject);
    }

    // Required by IReversible. Flips the direction so the Tornado can bounce it back.
    public void ReverseDirection()
    {
        directionX *= -1;
    }
}