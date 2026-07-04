using UnityEngine;

// Abstract base class for all projectiles.
// Uses a hybrid collision approach: 
// 1. Raycasts for physical obstacles (walls/ground) to prevent fast projectiles from tunneling.
// 2. Trigger colliders for targets (since targets use CharacterControllers).
[RequireComponent(typeof(Collider))]
public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Obstacle Collision")]
    [Tooltip("Layers that block the projectile (walls, ground, etc.).")]
    [SerializeField] private LayerMask obstacleLayers;

    // Target layers are assigned by the subclass during initialization
    protected LayerMask targetLayers;
    
    // Read-only access to obstacle layers for subclass custom raycasts
    protected LayerMask ObstacleLayers => obstacleLayers;

    // Moves the projectile and checks for obstacles via Raycast before applying the movement.
    protected bool TryMove(Vector3 direction, float speed)
    {
        float step = speed * Time.deltaTime;
        
        // Look-ahead margin prevents fast projectiles from phasing through thin geometry
        float lookAhead = step + 0.15f;
        
        if (Physics.Raycast(transform.position, direction, lookAhead, obstacleLayers))
        {
            OnObstacleHit();
            return false;
        }
        
        transform.position += direction * step;
        return true;
    }

    // Detects targets overlapping with the projectile's trigger collider.
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (IsInMask(other.gameObject.layer, targetLayers))
            OnTargetHit(other);
    }

    // Override to add impact VFX/SFX when hitting a wall
    protected virtual void OnObstacleHit()
    {
        Destroy(gameObject);
    }

    // Subclasses define what happens when a valid target is hit
    protected abstract void OnTargetHit(Collider target);

    // Helper to check if a layer is included in a LayerMask
    protected static bool IsInMask(int layer, LayerMask mask) =>
        (mask.value & (1 << layer)) != 0;
}