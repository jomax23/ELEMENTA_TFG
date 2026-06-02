using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines a persistent hazard area (Tornado) that detects and reverses 
/// any incoming projectiles implementing the IReversible interface.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TornadoArea : MonoBehaviour
{
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
    
    public void Initialize(float scaledLifetime)
    {
        if (tornadoParticles != null)
            tornadoParticles.Play();

        Destroy(gameObject, scaledLifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((projectileLayers.value & (1 << other.gameObject.layer)) == 0) return;

        IReversible projectile = other.GetComponent<IReversible>();
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

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

}