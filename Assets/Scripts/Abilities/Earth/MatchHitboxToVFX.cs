using UnityEngine;

/// <summary>
/// Synchronizes a physics hitbox's vertical position with a visual effect's animation curve.
/// This ensures the collision area matches the visual height of the VFX over time without scaling the hitbox itself.
/// </summary>
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
        // Store the initial local position (ground level)
        startPos = hitboxObject.localPosition;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        
        // Evaluate curve and apply max height multiplier
        float height = heightCurve.Evaluate(t) * maxHeight;

        // Move ONLY on the Y axis, preserving X and Z
        hitboxObject.localPosition = new Vector3(startPos.x, startPos.y + height, startPos.z);

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}