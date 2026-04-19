using UnityEngine;

public class MatchHitboxToVFX : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform hitboxObject; // Cube con MeshCollider

    [Header("Curva")]
    [SerializeField] private AnimationCurve heightCurve;

    [Header("Config")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float maxHeight = 3f;

    private float timer;
    private Vector3 startPos;

    private void Start()
    {
        // Guardamos posición inicial (suelo)
        startPos = hitboxObject.localPosition;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;

        float curveValue = heightCurve.Evaluate(t);

        float height = curveValue * maxHeight;

        // SOLO mover en Y (sin escalar)
        hitboxObject.localPosition = new Vector3(
            startPos.x,
            startPos.y + height,
            startPos.z
        );

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}