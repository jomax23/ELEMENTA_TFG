using UnityEngine;

// Syncs the hitbox's vertical position with the VFX animation curve.
// We move the transform instead of scaling the collider to keep the collision area accurate.
public class MatchHitboxToVFX : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Transform of the hitbox object (e.g., a Cube with a MeshCollider).")]
    [SerializeField] private Transform hitboxObject;

    [Header("Curve")]
    [Tooltip("Animation curve defining the height progression over time.")]
    [SerializeField] private AnimationCurve heightCurve;

    [Header("Config")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float maxHeight = 3f;

    private float timer;
    private Vector3 startPos;

    private void Start()
    {
        // Cache the starting local position so we only modify the Y axis
        startPos = hitboxObject.localPosition;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        // Evaluate the curve and apply the max height multiplier
        float height = heightCurve.Evaluate(t) * maxHeight;

        // Move ONLY on the Y axis, preserving X and Z
        hitboxObject.localPosition = new Vector3(startPos.x, startPos.y + height, startPos.z);

        // Clean up once the animation finishes
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}