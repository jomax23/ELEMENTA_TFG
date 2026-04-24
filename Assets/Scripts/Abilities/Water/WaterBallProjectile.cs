using UnityEngine;

public class WaterBallProjectile : MonoBehaviour, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed    = 12f;
    [SerializeField] private float lifetime = 2f;

    [Header("Effects")]
    [SerializeField] private float pushForce = 6f;
    [SerializeField] private float damage    = 10f;

    private LayerMask targetLayers;
    private float     directionX;
    private float     actualPushForce;
    private float     actualDamage;

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño e impulso.</param>
    public void Initialize(float dirX, LayerMask layers, float efficiency = 1f)
    {
        directionX     = Mathf.Sign(dirX);
        targetLayers   = layers;
        actualPushForce = pushForce * efficiency;
        actualDamage    = damage    * efficiency;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.right * directionX * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target != null)
        {
            target.ApplyImpulse(directionX * actualPushForce);
            target.ApplyDamage(actualDamage);
        }

        Destroy(gameObject);
    }

    public void ReverseDirection()
    {
        directionX = Mathf.Sign(directionX) * -1f;
    }
}