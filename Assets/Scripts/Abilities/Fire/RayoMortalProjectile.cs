using UnityEngine;

public class RayoMortalProjectile : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float damage       = 20f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float lifetime     = 0.15f;

    private LayerMask targetLayers;
    private float     actualDamage;
    private float     actualStunDuration;

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño y duración del stun.</param>
    public void Initialize(int directionX, LayerMask layers, float efficiency = 1f)
    {
        targetLayers      = layers;
        actualDamage      = damage       * efficiency;
        actualStunDuration = stunDuration * efficiency;

        Vector3 dir = Vector3.right * directionX;

        transform.up = dir;

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

        target.ApplyDamage(actualDamage);
        target.ApplyStun(actualStunDuration);
    }
}