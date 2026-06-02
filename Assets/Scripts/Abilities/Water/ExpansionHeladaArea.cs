using UnityEngine;

/// <summary>
/// Area effect that expands laterally from the caster. 
/// Uses Raycast for obstacle detection to stop growth at walls/platforms.
/// </summary>
public class ExpansionHeladaArea : MonoBehaviour
{
    [Header("Obstacle Detection")]
    [Tooltip("Layers that stop the ice expansion (walls, ground, etc.).")]
    [SerializeField] private LayerMask obstacleLayers;

    private BoxCollider hitbox;
    private int directionX;
    private float currentLength;
    private LayerMask targetLayers;
    
    // Runtime values (injected by AbilityData)
    private float actualDamage;
    private float maxLength;
    private float expandSpeed;
    private float lifetime;
    
    private bool isBlocked;
    
    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;
    }

    /// <summary>
    /// Initializes the expansion direction, target layers, and scaled damage.
    /// </summary>
    public void Initialize(int facingDirection, LayerMask layers, float scaledDamage, float areaMaxLength, float areaExpandSpeed, float areaLifetime)
    {
        directionX = facingDirection;
        targetLayers = layers;
        
        actualDamage = scaledDamage;
        maxLength = areaMaxLength;
        expandSpeed = areaExpandSpeed;
        lifetime = areaLifetime;

        // Orient the transform to face the correct direction
        transform.right = Vector3.right * directionX;

        currentLength = 0f;
        UpdateHitbox();

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        Expand();
    }

    /// <summary>
    /// Grows the area step-by-step, checking for obstacles via Raycast.
    /// </summary>
    private void Expand()
    {
        if (isBlocked) return;

        float nextStep = expandSpeed * Time.deltaTime;
        float checkDist = currentLength + nextStep + 0.1f; // Small margin to prevent tunneling

        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, checkDist, obstacleLayers))
        {
            // Cap the ice exactly at the obstacle
            currentLength = Mathf.Max(0f, hit.distance - 0.05f);
            isBlocked = true;
            UpdateHitbox();
            return;
        }

        currentLength += nextStep;
        currentLength = Mathf.Min(currentLength, maxLength);
        UpdateHitbox();
    }

    private void UpdateHitbox()
    {
        hitbox.size = new Vector3(currentLength, 1f, 1f);
        hitbox.center = new Vector3(currentLength * 0.5f, 0f, 0f);
    }

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

        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(box.center, box.size);
    }
#endif
}