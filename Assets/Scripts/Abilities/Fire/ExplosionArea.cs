using UnityEngine;

public class ExplosionArea : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float pushForce = 10f;

    [Header("VFX")]
    [SerializeField] private GameObject explosionVfxPrefab;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask targetLayers;

    public void Initialize(int facingDirection)
    {
        SpawnVFX();
        Explode();
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius,
            targetLayers
        );

        foreach (Collider hit in hits)
        {
            IAbilityTarget target = hit.GetComponent<IAbilityTarget>();
            if (target == null)
                continue;

            target.ApplyDamage(damage);

            // 🔥 DIRECCIÓN CORRECTA: alejándose del centro
            float deltaX = hit.transform.position.x - transform.position.x;
            int pushDirection = deltaX >= 0f ? 1 : -1;

            target.ApplyImpulse(pushDirection * pushForce);
        }

        Destroy(gameObject);
    }

    private void SpawnVFX()
    {
        if (explosionVfxPrefab == null)
            return;

        GameObject vfx = Instantiate(
            explosionVfxPrefab,
            transform.position,
            Quaternion.identity
        );

        var ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(vfx, ps.main.duration);
        else
            Destroy(vfx, 2f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}