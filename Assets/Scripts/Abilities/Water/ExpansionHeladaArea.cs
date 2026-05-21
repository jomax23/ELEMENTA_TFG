using UnityEngine;

// La Expansión Helada no es un proyectil que se mueve — crece lateralmente.
// No puede heredar de ProjectileBase, así que tiene su propia detección de
// obstáculos con Raycast en el método Expand().
public class ExpansionHeladaArea : MonoBehaviour
{
    [Header("Expansion Settings")]
    [SerializeField] private float maxLength   = 8f;
    [SerializeField] private float expandSpeed = 12f;
    [SerializeField] private float lifetime    = 1.5f;

    [Header("Damage")]
    [SerializeField] private float damage = 15f;

    [Header("Obstacle Detection")]
    [Tooltip("Capas que detienen la expansión del hielo (paredes, suelo, plataformas...).\n" +
             "No requieren Rigidbody ni isTrigger — el Raycast los detecta igual.")]
    [SerializeField] private LayerMask obstacleLayers;

    private BoxCollider hitbox;
    private int         directionX;
    private float       currentLength;
    private LayerMask   targetLayers;
    private float       actualDamage;
    private bool        isBlocked;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        hitbox           = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;
    }

    public void Initialize(int facingDirection, LayerMask layers, float efficiency = 1f)
    {
        directionX    = facingDirection;
        targetLayers  = layers;
        actualDamage  = damage * efficiency;

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
    // EXPANSIÓN CON RAYCAST
    // ─────────────────────────────────────────────────────────────────────────

    private void Expand()
    {
        if (isBlocked) return;

        float nextStep   = expandSpeed * Time.deltaTime;
        // Raycast desde el origen en la dirección de expansión,
        // a la distancia que llegaríamos este fotograma + un margen.
        float checkDist  = currentLength + nextStep + 0.1f;

        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, checkDist, obstacleLayers))
        {
            // Recortar el hielo exactamente hasta el obstáculo
            currentLength = Mathf.Max(0f, hit.distance - 0.05f);
            isBlocked     = true;
            UpdateHitbox();
            return;
        }

        currentLength += nextStep;
        currentLength  = Mathf.Min(currentLength, maxLength);
        UpdateHitbox();
    }

    private void UpdateHitbox()
    {
        hitbox.size   = new Vector3(currentLength, 1f, 1f);
        hitbox.center = new Vector3(currentLength * 0.5f, 0f, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COLISIÓN CON TARGETS
    // ─────────────────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

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