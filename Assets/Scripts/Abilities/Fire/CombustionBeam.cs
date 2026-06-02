using UnityEngine;

/// <summary>
/// Visual beam effect that tracks a starting point, raycasts for obstacles, 
/// and fades out over a set duration.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CombustionBeam : MonoBehaviour
{
    [Header("References")]
    private LineRenderer lineRenderer;
    private Renderer beamRenderer;
    private MaterialPropertyBlock mpb;

    [Header("Configuration")]
    private Transform startPoint;
    private Vector3 direction;
    private float maxDistance;
    private LayerMask obstacleLayers;
    private float duration;
    private float timer;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        beamRenderer = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Initializes the beam with its trajectory and lifespan.
    /// </summary>
    public void Initialize(Transform start, Vector3 dir, float distance, LayerMask obstacles, float beamDuration)
    {
        startPoint = start;
        direction = dir;
        maxDistance = distance;
        obstacleLayers = obstacles;
        duration = beamDuration;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Auto-cleanup when duration expires or origin is destroyed
        if (timer >= duration || startPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 start = startPoint.position;
        Vector3 end = start + direction * maxDistance;

        // Shorten beam if it hits an obstacle
        if (Physics.Raycast(start, direction, out RaycastHit hit, maxDistance, obstacleLayers))
        {
            end = hit.point;
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Fade out effect based on remaining time
        float alpha = 1f - Mathf.Clamp01(timer / duration);
        beamRenderer.GetPropertyBlock(mpb);
        Color baseColor = Color.white;
        baseColor.a = alpha;
        mpb.SetColor(BaseColorID, baseColor);
        beamRenderer.SetPropertyBlock(mpb);
    }
}