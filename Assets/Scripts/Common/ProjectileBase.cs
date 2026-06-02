using UnityEngine;

/// <summary>
/// Abstract base class for all projectiles.
/// Uses Raycast for obstacle collision (to avoid physics trigger issues with static colliders)
/// and OnTriggerEnter for target detection (since targets use CharacterControllers).
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Obstacle Collision")]
    [Tooltip("Layers that block the projectile (walls, ground, etc.).")]
    [SerializeField] private LayerMask obstacleLayers;

    /// <summary>Target layers to be assigned by the subclass during initialization.</summary>
    protected LayerMask targetLayers;

    /// <summary>Read-only access to obstacle layers for subclass custom raycasts.</summary>
    protected LayerMask ObstacleLayers => obstacleLayers;

    /// <summary>
    /// Moves the projectile and checks for obstacles via Raycast before applying the movement.
    /// </summary>
    /// <returns>True if movement was successful, false if an obstacle was hit.</returns>
    protected bool TryMove(Vector3 direction, float speed)
    {
        float step = speed * Time.deltaTime;
        // Look-ahead margin prevents fast projectiles from tunneling into geometry
        float lookAhead = step + 0.15f;

        if (Physics.Raycast(transform.position, direction, lookAhead, obstacleLayers))
        {
            OnObstacleHit();
            return false;
        }

        transform.position += direction * step;
        return true;
    }

    /// <summary>
    /// Detects targets overlapping with the projectile's trigger collider.
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (IsInMask(other.gameObject.layer, targetLayers))
            OnTargetHit(other);
    }

    /// <summary>Called when the raycast detects an obstacle. Override to add VFX/SFX.</summary>
    protected virtual void OnObstacleHit()
    {
        Destroy(gameObject);
    }

    /// <summary>Called when the trigger overlaps with a valid target layer.</summary>
    protected abstract void OnTargetHit(Collider target);

    /// <summary>Helper to check if a layer is included in a LayerMask.</summary>
    protected static bool IsInMask(int layer, LayerMask mask) =>
        (mask.value & (1 << layer)) != 0;
}