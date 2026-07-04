using UnityEngine;

// Expanding frost zone that grows horizontally until it hits a wall or max length.
// Dynamically resizes its BoxCollider to match the visual ice expansion.
public class ExpansionHeladaArea : MonoBehaviour
{
    [Header("Obstacle Detection")]
    [Tooltip("Layers that stop the ice expansion (walls, ground, etc.).")]
    [SerializeField] private LayerMask obstacleLayers;

    private BoxCollider hitbox;
    private int directionX;
    private float currentLength;
    
    private LayerMask targetLayers;
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

    // Grows the area step-by-step, checking for obstacles via Raycast.
    private void Expand()
    {
        if (isBlocked) return;

        float nextStep = expandSpeed * Time.deltaTime;
        // Add a small margin to the raycast to prevent physics tunneling at high speeds
        float checkDist = currentLength + nextStep + 0.1f; 

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

    // Stretches the collider to match the current length of the ice.
    // We offset the center by half the length so it expands outward from the origin.
    private void UpdateHitbox()
    {
        hitbox.size = new Vector3(currentLength, 1f, 1f);
        hitbox.center = new Vector3(currentLength * 0.5f, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        
        if (other.GetComponent<IAbilityTarget>() is IAbilityTarget target)
        {
            target.ApplyDamage(actualDamage);
        }
    }

#if UNITY_EDITOR
    // Visualize the expanding hitbox in the editor for easier tuning.
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