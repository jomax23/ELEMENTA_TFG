using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
// Expansión Helada — área que crece lateralmente hasta alcanzar su longitud
// máxima O chocar con un obstáculo (pared, plataforma, etc.).
//
// Detección de obstáculos:
//   - OnTriggerEnter captura impactos directos (misma lógica que ProjectileBase).
//   - Raycast adelantado en Expand() como seguridad para geometría sólida.
//   Ambos mecanismos se complementan según la configuración del collider del obstáculo.
// ──────────────────────────────────────────────────────────────────────────────
public class ExpansionHeladaArea : MonoBehaviour
{
    [Header("Expansion Settings")]
    [SerializeField] private float maxLength   = 8f;
    [SerializeField] private float expandSpeed = 12f;
    [SerializeField] private float lifetime    = 1.5f;

    [Header("Damage")]
    [SerializeField] private float damage = 15f;

    [Header("Obstacle Detection")]
    [Tooltip("Capas que bloquean el avance del hielo (paredes, suelo, plataformas, etc.)")]
    [SerializeField] private LayerMask obstacleLayers;

    private BoxCollider hitbox;
    private int         directionX;
    private float       currentLength;
    private LayerMask   targetLayers;
    private float       actualDamage;

    private bool        isBlocked; // true cuando el hielo ha chocado con un obstáculo

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        hitbox           = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;
    }

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño.</param>
    public void Initialize(int facingDirection, LayerMask layers, float efficiency = 1f)
    {
        directionX   = facingDirection;
        targetLayers = layers;
        actualDamage = damage * efficiency;

        transform.right = Vector3.right * directionX;

        currentLength = 0f;
        UpdateHitbox();

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        Expand();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EXPANSIÓN CON RAYCAST DE OBSTÁCULO
    // ─────────────────────────────────────────────────────────────────────────

    private void Expand()
    {
        if (isBlocked) return;

        // Raycast desde el extremo actual en la dirección de expansión.
        // Añadimos un pequeño margen (0.05f) para detectar la pared antes de penetrar.
        Vector3 rayOrigin    = transform.position;
        Vector3 rayDirection = transform.right;
        float   rayDistance  = currentLength + expandSpeed * Time.deltaTime + 0.05f;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance, obstacleLayers))
        {
            // Cortar la expansión exactamente donde está el obstáculo
            currentLength = Mathf.Max(0f, hit.distance - 0.05f);
            isBlocked     = true;
            UpdateHitbox();
            return;
        }

        currentLength += expandSpeed * Time.deltaTime;
        currentLength  = Mathf.Min(currentLength, maxLength);
        UpdateHitbox();
    }

    private void UpdateHitbox()
    {
        hitbox.size   = new Vector3(currentLength, 1f, 1f);
        hitbox.center = new Vector3(currentLength * 0.5f, 0f, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COLISIÓN
    // ─────────────────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;

        // Obstáculo: detener la expansión por contacto directo
        // (cubre casos donde el raycast no adelantó la detección).
        if ((obstacleLayers.value & (1 << layer)) != 0)
        {
            isBlocked     = true;
            currentLength = Mathf.Max(0f, currentLength - 0.1f);
            UpdateHitbox();
            return;
        }

        // Target: aplicar daño
        if ((targetLayers.value & (1 << layer)) == 0) return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        target?.ApplyDamage(actualDamage);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color  = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(box.center, box.size);
    }
#endif
}