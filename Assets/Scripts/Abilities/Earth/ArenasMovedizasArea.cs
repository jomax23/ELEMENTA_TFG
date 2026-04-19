using UnityEngine;

public class ArenasMovedizasArea : MonoBehaviour
{
    [Header("Slow Settings")]
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private float slowRefreshTime = 0.2f;
    [SerializeField] private float lifetime = 5f;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask targetLayers;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerStay(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target == null)
            return;

        // 🔁 Renovamos constantemente el slow
        target.ApplySlow(slowMultiplier, slowRefreshTime);
    }
}