using UnityEngine;

/// <summary>
/// Area effect that instantly damages and applies knockback to all targets within a radius.
/// </summary>
public class ExplosionArea : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject explosionVfxPrefab;

    
    private LayerMask targetLayers;
    private float actualDamage;
    private float actualPushForce;
    private float radius;
    
    /// <summary>
    /// Initializes the explosion with target layers and scales effects based on affinity efficiency.
    /// </summary>
    /// <param name="facingDirection">Unused in this specific implementation, but kept for interface consistency.</param>
    /// <param name="layers">The layer mask of valid targets.</param>
    /// <param name="efficiency">Affinity multiplier (0–1). Scales damage and push force.</param>
    public void Initialize(int facingDirection, LayerMask layers, float scaledDamage, float scaledPushForce)
    {
        targetLayers = layers;
        actualDamage = scaledDamage;
        actualPushForce = scaledPushForce;
        SpawnVFX();
        Explode();
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayers);
        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<IAbilityTarget>() is IAbilityTarget target)
            {
                target.ApplyDamage(actualDamage);
                float deltaX = hit.transform.position.x - transform.position.x;
                int pushDir = deltaX >= 0f ? 1 : -1;
                target.ApplyImpulse(pushDir * actualPushForce);
            }
        }
        Destroy(gameObject);
    }

    private void SpawnVFX()
    {
        if (explosionVfxPrefab == null) return;

        GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        
        // Destroy VFX after its main duration, or fallback to 2 seconds
        Destroy(vfx, ps != null ? ps.main.duration : 2f);
    }
}