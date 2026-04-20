using UnityEngine;

public class WaterWaveProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed    = 10f;
    [SerializeField] private float lifetime = 2f;

    [Header("Effects")]
    [SerializeField] private float pushForce      = 12f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration   = 2f;
    [SerializeField] private float damage         = 10f;

    private LayerMask targetLayers;
    private int directionX;

    public void Initialize(int dirX, LayerMask layers)
    {
        directionX   = dirX;
        targetLayers = layers;
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
            target.ApplyImpulse(directionX * pushForce);
            target.ApplySlow(slowMultiplier, slowDuration);
            target.ApplyDamage(damage);
        }
    }
}