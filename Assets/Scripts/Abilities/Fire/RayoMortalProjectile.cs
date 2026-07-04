using UnityEngine;
using System.Collections;

// Mortal Thunder beam. Can fire instantly or animate towards a specific target bone.
// We stretch a BoxCollider between the start and end points to perfectly match the LineRenderer visuals.
[RequireComponent(typeof(BoxCollider))]
public class RayoMortalProjectile : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float maxLength = 10f;
    [Tooltip("Time the beam remains visible after growing.")]
    [SerializeField] private float holdDuration = 0.2f;

    [Header("Grow Animation")]
    [Tooltip("Seconds it takes for the line to grow from origin to destination.")]
    [SerializeField] private float growDuration = 0.1f;

    [Header("Collider")]
    [SerializeField] private float colliderWidth = 0.4f;
    [SerializeField] private float colliderDepth = 0.4f;

    [Header("Obstacle Detection")]
    [Tooltip("Layers for walls, ground, and platforms.")]
    [SerializeField] private LayerMask obstacleLayers;

    private BoxCollider hitbox;
    private LineRenderer[] allLineRenderers;

    private LayerMask targetLayers;
    private float actualDamage;
    private float actualStunDuration;
    private bool hasHit;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;

        // Gather all LineRenderers in the prefab (useful if the beam has multiple layered visual effects)
        allLineRenderers = GetComponentsInChildren<LineRenderer>(includeInactive: true);
        foreach (LineRenderer lr in allLineRenderers)
        {
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position);
        }

        ApplyCollider(transform.position, transform.position);
    }

    // =========================================================================
    // MODE A: Instantaneous (Fires straight if no target is found)
    // =========================================================================
    public void Initialize(int directionX, LayerMask layers, float scaledDamage, float scaledStunDuration)
    {
        SetupStats(layers, scaledDamage, scaledStunDuration);
        
        Vector3 origin = transform.position;
        Vector3 dir = Vector3.right * directionX;
        Vector3 endpoint = origin + dir * GetLength(origin, dir, maxLength);
        
        ApplyBeam(origin, endpoint);
        Destroy(gameObject, holdDuration);
    }

    // =========================================================================
    // MODE B: Animated towards a specific Transform (e.g., enemy spine)
    // =========================================================================
    public void InitializeToTarget(Transform spawnPoint, Transform targetPoint, LayerMask layers, float scaledDamage, float scaledStunDuration)
    {
        SetupStats(layers, scaledDamage, scaledStunDuration);
        StartCoroutine(GrowRoutine(spawnPoint, targetPoint));
    }

    private IEnumerator GrowRoutine(Transform spawnPoint, Transform targetPoint)
    {
        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / growDuration);

            Vector3 origin = spawnPoint.position;
            Vector3 toTarget = targetPoint.position - origin;
            Vector3 direction = toTarget.normalized;
            
            float maxDist = Mathf.Min(toTarget.magnitude, maxLength);
            Vector3 endpoint = origin + direction * GetLength(origin, direction, maxDist);

            // Lerp the end position to create the "growing" effect
            ApplyBeam(origin, Vector3.Lerp(origin, endpoint, t));
            yield return null;
        }

        // Snap to exact final state to avoid floating point drift
        Vector3 finalOrigin = spawnPoint.position;
        Vector3 finalToTarget = targetPoint.position - finalOrigin;
        Vector3 finalDirection = finalToTarget.normalized;
        float finalMaxDist = Mathf.Min(finalToTarget.magnitude, maxLength);
        Vector3 finalEndpoint = finalOrigin + finalDirection * GetLength(finalOrigin, finalDirection, finalMaxDist);
        
        ApplyBeam(finalOrigin, finalEndpoint);
        Destroy(gameObject, holdDuration);
    }

    // =========================================================================
    // BEAM & COLLIDER APPLICATION
    // =========================================================================
    private void ApplyBeam(Vector3 start, Vector3 end)
    {
        foreach (LineRenderer lr in allLineRenderers)
        {
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
        ApplyCollider(start, end);
    }

    // Stretches the BoxCollider to match the exact length and rotation of the visual beam
    private void ApplyCollider(Vector3 start, Vector3 end)
    {
        float length = Vector3.Distance(start, end);
        Vector3 midpoint = (start + end) * 0.5f;
        Vector3 direction = length > 0.001f ? (end - start) / length : Vector3.up;

        transform.position = midpoint;
        transform.up = direction; // Aligns the local Y axis with the beam direction
        
        hitbox.size = new Vector3(colliderWidth, length, colliderDepth);
        hitbox.center = Vector3.zero;
    }

    // =========================================================================
    // OBSTACLE DETECTION
    // =========================================================================
    // Raycasts to see if a wall is in the way, shortening the beam if necessary
    private float GetLength(Vector3 origin, Vector3 direction, float maxDist)
    {
        if (obstacleLayers.value == 0) return maxDist;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDist, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.distance;
        }
        return maxDist;
    }

    // =========================================================================
    // TARGET COLLISION
    // =========================================================================
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.GetComponent<IAbilityTarget>() is IAbilityTarget target)
        {
            hasHit = true;
            target.ApplyDamage(actualDamage);
            target.ApplyStun(actualStunDuration);
        }
    }

    private void SetupStats(LayerMask layers, float scaledDamage, float scaledStunDuration)
    {
        targetLayers = layers;
        actualDamage = scaledDamage;
        actualStunDuration = scaledStunDuration;
        hasHit = false;
    }
}