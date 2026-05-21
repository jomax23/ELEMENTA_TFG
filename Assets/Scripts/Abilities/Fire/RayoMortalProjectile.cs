using UnityEngine;
using System.Collections;

// ──────────────────────────────────────────────────────────────────────────────
// Rayo Mortal — beam con LineRenderer(s)
//
// Actualiza TODOS los LineRenderers del prefab (root + hijos) con los mismos
// dos puntos world-space: [0] = origen, [1] = destino.
// Cada LineRenderer puede tener su propio material/ancho — el script solo
// mueve sus posiciones, no toca ninguna otra propiedad.
//
// SETUP DEL PREFAB:
//   · Root: este script + BoxCollider (isTrigger = true).
//   · Añade un LineRenderer en el root y/o en cualquier hijo.
//     Todos deben tener Use World Space = TRUE y Position Count = 2.
//   · Obstacle Layers → capas de muros/suelo.
// ──────────────────────────────────────────────────────────────────────────────
[RequireComponent(typeof(BoxCollider))]
public class RayoMortalProjectile : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float maxLength    = 10f;
    [SerializeField] private float damage       = 20f;
    [SerializeField] private float stunDuration = 2f;

    [Tooltip("Tiempo que el beam permanece visible tras terminar el crecimiento.")]
    [SerializeField] private float holdDuration = 0.2f;

    [Header("Grow Animation")]
    [Tooltip("Segundos que tarda la línea en crecer desde el origen al destino.")]
    [SerializeField] private float growDuration = 0.1f;

    [Header("Collider")]
    [SerializeField] private float colliderWidth = 0.4f;
    [SerializeField] private float colliderDepth = 0.4f;

    [Header("Obstacle Detection")]
    [Tooltip("⚠ OBLIGATORIO: capas de muros, suelo y plataformas del escenario.")]
    [SerializeField] private LayerMask obstacleLayers;

    // Componentes
    private BoxCollider    hitbox;
    private LineRenderer[] allLineRenderers; // root + todos los hijos

    // Runtime
    private LayerMask targetLayers;
    private float     actualDamage;
    private float     actualStunDuration;
    private bool      hasHit;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;

        // Recoger TODOS los LineRenderers del prefab (root incluido)
        allLineRenderers = GetComponentsInChildren<LineRenderer>(includeInactive: true);

        foreach (LineRenderer lr in allLineRenderers)
        {
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position);
        }

        // Collider empieza con tamaño cero
        ApplyCollider(transform.position, transform.position);

        if (allLineRenderers.Length == 0)
            Debug.LogWarning("[RayoMortal] No se encontró ningún LineRenderer en el prefab " +
                             "ni en sus hijos. El beam no será visible.", gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MODO A — Instantáneo
    // ─────────────────────────────────────────────────────────────────────────

    public void Initialize(int directionX, LayerMask layers, float efficiency = 1f)
    {
        SetupStats(layers, efficiency);
        WarnIfEmpty();

        Vector3 origin   = transform.position;
        Vector3 dir      = Vector3.right * directionX;
        Vector3 endpoint = origin + dir * GetLength(origin, dir, maxLength);

        ApplyBeam(origin, endpoint);
        Destroy(gameObject, holdDuration);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MODO B — Animado hacia un Transform objetivo
    // ─────────────────────────────────────────────────────────────────────────

    public void InitializeToTarget(
        Transform spawnPoint,
        Transform targetPoint,
        LayerMask layers,
        float efficiency = 1f)
    {
        SetupStats(layers, efficiency);
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

            Vector3 origin    = spawnPoint.position;
            Vector3 toTarget  = targetPoint.position - origin;
            Vector3 direction = toTarget.normalized;
            float   maxDist   = Mathf.Min(toTarget.magnitude, maxLength);
            Vector3 endpoint  = origin + direction * GetLength(origin, direction, maxDist);

            ApplyBeam(origin, Vector3.Lerp(origin, endpoint, t));

            yield return null;
        }

        // Estado final exacto
        {
            Vector3 origin    = spawnPoint.position;
            Vector3 toTarget  = targetPoint.position - origin;
            Vector3 direction = toTarget.normalized;
            float   maxDist   = Mathf.Min(toTarget.magnitude, maxLength);
            Vector3 endpoint  = origin + direction * GetLength(origin, direction, maxDist);
            ApplyBeam(origin, endpoint);
        }

        Destroy(gameObject, holdDuration);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // APLICAR BEAM
    // ─────────────────────────────────────────────────────────────────────────

    private void ApplyBeam(Vector3 start, Vector3 end)
    {
        // Todos los LineRenderers reciben los mismos dos puntos
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
        float   length    = Vector3.Distance(start, end);
        Vector3 midpoint  = (start + end) * 0.5f;
        Vector3 direction = length > 0.001f ? (end - start) / length : Vector3.up;

        transform.position = midpoint;
        transform.up       = direction;

        hitbox.size   = new Vector3(colliderWidth, length, colliderDepth);
        hitbox.center = Vector3.zero;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // OBSTACLE DETECTION
    // ─────────────────────────────────────────────────────────────────────────

    private float GetLength(Vector3 origin, Vector3 direction, float maxDist)
    {
        if (obstacleLayers.value == 0)
            return maxDist;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDist,
                obstacleLayers, QueryTriggerInteraction.Ignore))
        {
#if UNITY_EDITOR
            Debug.DrawLine(origin, hit.point, Color.red, holdDuration + growDuration);
            Debug.Log($"[RayoMortal] Obstáculo: '{hit.collider.name}' " +
                      $"dist={hit.distance:F2}m");
#endif
            return hit.distance;
        }

        return maxDist;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COLISIÓN CON TARGETS
    // ─────────────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────────────

    private void SetupStats(LayerMask layers, float efficiency)
    {
        targetLayers       = layers;
        actualDamage       = damage       * efficiency;
        actualStunDuration = stunDuration * efficiency;
        hasHit             = false;
    }

    private void WarnIfEmpty()
    {
        if (obstacleLayers.value == 0)
            Debug.LogWarning("[RayoMortal] ⚠ 'Obstacle Layers' vacío — " +
                             "el rayo atravesará muros.", gameObject);
    }
}