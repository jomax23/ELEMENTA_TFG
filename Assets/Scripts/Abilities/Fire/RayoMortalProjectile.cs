using UnityEngine;

public class RayoMortalProjectile : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float damage       = 20f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float lifetime     = 0.15f;

    private LayerMask targetLayers;

    public void Initialize(int directionX, LayerMask layers)
    {
        targetLayers = layers;

        Vector3 dir = Vector3.right * directionX;

        // Prefab con Z = 90 → eje largo = UP
        transform.up = dir;

        // Mover para que empiece desde el jugador
        float halfLength = transform.localScale.y * 0.5f;
        transform.position += dir * halfLength;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target == null) return;

        target.ApplyDamage(damage);
        target.ApplyStun(stunDuration);
    }
}