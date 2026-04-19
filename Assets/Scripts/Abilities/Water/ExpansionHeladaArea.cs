using UnityEngine;

public class ExpansionHeladaArea : MonoBehaviour
{
    [Header("Expansion Settings")]
    [SerializeField] private float maxLength = 8f;
    [SerializeField] private float expandSpeed = 12f;
    [SerializeField] private float lifetime = 1.5f;

    [Header("Damage")]
    [SerializeField] private float damage = 15f;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask targetLayers;

    private BoxCollider hitbox;
    private int directionX;
    private float currentLength;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;
    }

    public void Initialize(int facingDirection)
    {
        directionX = facingDirection;

        // Orientación: solo eje X
        transform.right = Vector3.right * directionX;

        // Reset
        currentLength = 0f;
        UpdateHitbox();

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        Expand();
    }

    private void Expand()
    {
        currentLength += expandSpeed * Time.deltaTime;
        currentLength = Mathf.Min(currentLength, maxLength);

        UpdateHitbox();
    }

    private void UpdateHitbox()
    {
        // El BoxCollider crece en X (forward)
        hitbox.size = new Vector3(currentLength, 1f, 1f);

        // Lo desplazamos para que empiece desde el jugador
        hitbox.center = new Vector3(currentLength * 0.5f, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target == null)
            return;

        target.ApplyDamage(damage);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
            return;

        Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Azul hielo semitransparente

        // Matriz para respetar posición y rotación
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawCube(box.center, box.size);
    }
#endif

}