using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CombustionBeam : MonoBehaviour
{
    private LineRenderer line;

    private Transform startPoint;
    private Vector3 direction;

    private float maxDistance;
    private LayerMask obstacleLayers;

    private float duration;
    private float timer;

    MaterialPropertyBlock mpb;
    Renderer rend;
    
    public void Initialize(Transform start, Vector3 dir, float distance, LayerMask obstacles, float beamDuration)
    {
        startPoint = start;
        direction = dir;
        maxDistance = distance;
        obstacleLayers = obstacles;
        duration = beamDuration;
    }

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (startPoint == null)
            return;

        timer += Time.deltaTime;

        // ======================
        // Raycast dinámico
        // ======================
        Vector3 start = startPoint.position;
        Vector3 end = start + direction * maxDistance;

        RaycastHit hit;

        if (Physics.Raycast(start, direction, out hit, maxDistance, obstacleLayers))
        {
            end = hit.point;
        }

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        // ======================
        // Animar opacidad
        // ======================
        float alpha = Mathf.Clamp01(timer / duration);

        rend.GetPropertyBlock(mpb);

        Color baseColor = Color.white;
        baseColor.a = alpha;

        mpb.SetColor("_BaseColor", baseColor);

        rend.SetPropertyBlock(mpb);
    }
}