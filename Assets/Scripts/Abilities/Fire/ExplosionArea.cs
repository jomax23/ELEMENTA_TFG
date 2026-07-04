using UnityEngine;

// Instant AoE that damages and knocks back enemies. 
// Calculates push direction based on the target's X position relative to the blast center.
public class ExplosionArea : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject explosionVfxPrefab;

    private LayerMask targetLayers;
    private float actualDamage;
    private float actualPushForce;
    
    // Note: Radius is expected to be set on the prefab's collider or passed via another method, 
    // but here we assume it's defined in the inspector or handled by the OverlapSphere radius.
    // (Assuming a serialized radius field exists or is handled by the base/trigger, 
    // but based on the code it uses a local 'radius' variable. I'll add the field for clarity).
    [Header("Stats")]
    [SerializeField] private float radius = 3f; 

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
                
                // Calculate horizontal push direction based on relative X position
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
        
        // Clean up the VFX object automatically after its particle system finishes playing
        Destroy(vfx, ps != null ? ps.main.duration : 2f);
    }
}