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
    private float directionX;

    public void Initialize(float dirX, LayerMask layers)
    {
        directionX   = Mathf.Sign(dirX);
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
            target.ApplyDamage(damage);
        }

        Destroy(gameObject);
    }

    public void ReverseDirection()
    {
        directionX = Mathf.Sign(directionX) * -1f;
    }
}