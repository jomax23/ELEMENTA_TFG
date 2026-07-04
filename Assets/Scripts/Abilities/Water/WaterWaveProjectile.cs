using UnityEngine;

// Continuous water wave that pushes, slows, and damages enemies.
// Unlike standard projectiles, it passes through targets and only stops at physical walls.
public class WaterWaveProjectile : ProjectileBase
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f;

    private int directionX;
    private float actualDamage, actualPushForce, actualSlowDuration, actualSlowMultiplier;

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
        // TryMove handles obstacle detection via Raycast. 
        // It will destroy the wave if it hits a wall, but ignores enemies.
        TryMove(Vector3.right * directionX, speed);
    }

    // Apply effects but DO NOT destroy the wave, allowing it to hit multiple targets in its path.
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