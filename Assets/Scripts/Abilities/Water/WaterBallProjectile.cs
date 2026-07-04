using UnityEngine;

// Standard single-target water projectile. 
// Destroys on impact and can be reflected back by the Tornado.
public class WaterBallProjectile : ProjectileBase, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2f;

    private int directionX;
    private float actualDamage;
    private float actualPushForce;

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
        TryMove(Vector3.right * directionX, speed);
    }

    // Standard impact logic: apply damage/knockback and destroy the projectile.
    protected override void OnTargetHit(Collider target)
    {
        if (target.GetComponent<IAbilityTarget>() is IAbilityTarget abilityTarget)
        {
            abilityTarget.ApplyImpulse(directionX * actualPushForce);
            abilityTarget.ApplyDamage(actualDamage);
        }
        Destroy(gameObject);
    }

    // Flips the movement direction so the Tornado can bounce it back at enemies.
    public void ReverseDirection()
    {
        directionX *= -1;
    }
}