using UnityEngine;
using System.Collections;

/// <summary>
/// Mortal Thunder projectile: a beam that grows towards a target or instantly strikes forward.
/// Uses LineRenderer(s) for visuals and a dynamically scaled BoxCollider for hit detection.
/// </summary>
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
    [Tooltip("⚠ MANDATORY: Layers for walls, ground, and platforms.")]
    [SerializeField] private LayerMask obstacleLayers;

    // Components
    private BoxCollider hitbox;
    private LineRenderer[] allLineRenderers;

    // Runtime state
    private LayerMask targetLayers;
    private float actualDamage;
    private float actualStunDuration;
    private bool hasHit;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;

        // Gather ALL LineRenderers in the prefab (root and children)
        allLineRenderers = GetComponentsInChildren<LineRenderer>(includeInactive: true);

        foreach (LineRenderer lr in allLineRenderers)
        {
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position);
        }

        // Collider starts with zero size
        ApplyCollider(transform.position, transform.position);

        if (allLineRenderers.Length == 0)
        {
            Debug.LogWarning("[RayoMortal] No LineRenderer found in the prefab or its children. The beam will not be visible.", gameObject);
        }
    }

    // =========================================================================
    // MODE A — Instantaneous
    // =========================================================================

    public void Initialize(int directionX, LayerMask layers, float scaledDamage, float scaledStunDuration)
    {
        SetupStats(layers, scaledDamage, scaledStunDuration);
        WarnIfEmpty();

        Vector3 origin = transform.position;
        Vector3 dir = Vector3.right * directionX;
        Vector3 endpoint = origin + dir * GetLength(origin, dir, maxLength);

        ApplyBeam(origin, endpoint);
        Destroy(gameObject, holdDuration);
    }

    // =========================================================================
    // MODE B — Animated towards a Transform target
    // =========================================================================

    public void InitializeToTarget(Transform spawnPoint, Transform targetPoint, LayerMask layers, float scaledDamage, float scaledStunDuration)
    {
        SetupStats(layers, scaledDamage, scaledStunDuration);
        WarnIfEmpty();
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

            ApplyBeam(origin, Vector3.Lerp(origin, endpoint, t));
            yield return null;
        }

        // Ensure exact final state
        Vector3 finalOrigin = spawnPoint.position;
        Vector3 finalToTarget = targetPoint.position - finalOrigin;
        Vector3 finalDirection = finalToTarget.normalized;
        float finalMaxDist = Mathf.Min(finalToTarget.magnitude, maxLength);
        Vector3 finalEndpoint = finalOrigin + finalDirection * GetLength(finalOrigin, finalDirection, finalMaxDist);
        
        ApplyBeam(finalOrigin, finalEndpoint);
        Destroy(gameObject, holdDuration);
    }

    // =========================================================================
    // BEAM APPLICATION
    // =========================================================================

    private void ApplyBeam(Vector3 start, Vector3 end)
    {
        // All LineRenderers receive the same two points
        foreach (LineRenderer lr in allLineRenderers)
        {
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        ApplyCollider(start, end);

#if UNITY_EDITOR
        Debug.DrawLine(start, end, Color.yellow, holdDuration + growDuration);
#endif
    }

    private void ApplyCollider(Vector3 start, Vector3 end)
    {
        float length = Vector3.Distance(start, end);
        Vector3 midpoint = (start + end) * 0.5f;
        Vector3 direction = length > 0.001f ? (end - start) / length : Vector3.up;

        transform.position = midpoint;
        transform.up = direction;

        hitbox.size = new Vector3(colliderWidth, length, colliderDepth);
        hitbox.center = Vector3.zero;
    }

    // =========================================================================
    // OBSTACLE DETECTION
    // =========================================================================

    private float GetLength(Vector3 origin, Vector3 direction, float maxDist)
    {
        if (obstacleLayers.value == 0)
            return maxDist;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDist, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
#if UNITY_EDITOR
            Debug.DrawLine(origin, hit.point, Color.red, holdDuration + growDuration);
            Debug.Log($"[RayoMortal] Obstacle hit: '{hit.collider.name}' at dist={hit.distance:F2}m");
#endif
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

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target == null) return;

        hasHit = true;
        target.ApplyDamage(actualDamage);
        target.ApplyStun(actualStunDuration);
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private void SetupStats(LayerMask layers, float scaledDamage, float scaledStunDuration)
    {
        targetLayers = layers;
        actualDamage = scaledDamage;
        actualStunDuration = scaledStunDuration;
        hasHit = false;
    }


    private void WarnIfEmpty()
    {
        if (obstacleLayers.value == 0)
        {
            Debug.LogWarning("[RayoMortal] ⚠ 'Obstacle Layers' is empty — the beam will pass through walls.", gameObject);
        }
    }
}