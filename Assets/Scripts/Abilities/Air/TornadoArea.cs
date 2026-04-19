using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TornadoArea : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 4f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem tornadoParticles;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask projectileLayers;

    private readonly HashSet<IReversible> reversedProjectiles = new();

    private CapsuleCollider capsule;

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        capsule.isTrigger = true;
    }

    private void Start()
    {
        if (tornadoParticles != null)
            tornadoParticles.Play();

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer, projectileLayers))
            return;

        IReversible projectile = other.GetComponent<IReversible>();
        if (projectile == null)
            return;

        if (reversedProjectiles.Contains(projectile))
            return;

        reversedProjectiles.Add(projectile);
        projectile.ReverseDirection();
    }

    private void OnTriggerExit(Collider other)
    {
        IReversible projectile = other.GetComponent<IReversible>();
        if (projectile != null)
            reversedProjectiles.Remove(projectile);
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void OnDestroy()
    {
        reversedProjectiles.Clear();
    }
/*
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col == null) return;

        Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.3f);

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            Vector3.one
        );

        Gizmos.matrix = rotationMatrix;

        float radius = col.radius;
        float height = Mathf.Max(col.height, radius * 2f);

        Vector3 center = col.center;

        float cylinderHeight = height - (radius * 2f);

        Vector3 top = center + Vector3.up * (cylinderHeight / 2f);
        Vector3 bottom = center - Vector3.up * (cylinderHeight / 2f);

        Gizmos.DrawWireSphere(top, radius);
        Gizmos.DrawWireSphere(bottom, radius);
        Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
        Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
        Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
        Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
    }
#endif
*/
}