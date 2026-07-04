using UnityEngine;
using System.Collections.Generic;

// Persistent hazard area that detects and reverses incoming projectiles.
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TornadoArea : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private ParticleSystem tornadoParticles;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask projectileLayers;

    // Tracks projectiles we've already bounced. 
    // Prevents the tornado from reversing the same projectile multiple times if it lingers in the trigger.
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

    public void Initialize(float scaledLifetime)
    {
        if (tornadoParticles != null)
            tornadoParticles.Play();
            
        Destroy(gameObject, scaledLifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore layers not specified in the mask
        if ((projectileLayers.value & (1 << other.gameObject.layer)) == 0) return;

        IReversible projectile = other.GetComponent<IReversible>();
        
        // Skip if it's not reversible or if we already bounced it
        if (projectile == null || reversedProjectiles.Contains(projectile)) return;

        reversedProjectiles.Add(projectile);
        projectile.ReverseDirection();
    }

    private void OnTriggerExit(Collider other)
    {
        IReversible projectile = other.GetComponent<IReversible>();
        if (projectile != null)
            reversedProjectiles.Remove(projectile);
    }

    private void OnDestroy()
    {
        reversedProjectiles.Clear();
    }
}