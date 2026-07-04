using UnityEngine;

// Visual beam for the Max Combustion ability. 
// Dynamically shortens if it hits a wall, and fades out using a MaterialPropertyBlock 
// to avoid breaking Unity's draw call batching.
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

        // Auto-cleanup when duration expires or if the caster is destroyed
        if (timer >= duration || startPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 start = startPoint.position;
        Vector3 end = start + direction * maxDistance;

        // Shorten the beam if it hits a wall or obstacle
        if (Physics.Raycast(start, direction, out RaycastHit hit, maxDistance, obstacleLayers))
        {
            end = hit.point;
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Fade out the alpha over time. 
        // We use MaterialPropertyBlock so we don't create unique material instances at runtime.
        float alpha = 1f - Mathf.Clamp01(timer / duration);
        beamRenderer.GetPropertyBlock(mpb);
        
        Color baseColor = Color.white;
        baseColor.a = alpha;
        mpb.SetColor(BaseColorID, baseColor);
        
        beamRenderer.SetPropertyBlock(mpb);
    }
}