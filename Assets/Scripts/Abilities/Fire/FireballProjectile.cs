using UnityEngine;

public class FireballProjectile : MonoBehaviour, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifetime = 2f;

    [Header("Damage")]
    [SerializeField] private float impactDamage = 10f;
    [SerializeField] private float burnDamagePerSecond = 2f;
    [SerializeField] private float burnDuration = 3f;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask targetLayers;

    private int directionX;
    private float lifeTimer;

    public void Initialize(int dirX)
    {
        directionX = dirX;
    }

    private void Update()
    {
        // Movimiento
        transform.position += Vector3.right * directionX * speed * Time.deltaTime;

        // Temporizador de vida
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target != null)
        {
            target.ApplyDamage(impactDamage);
            target.ApplyBurn(burnDamagePerSecond, burnDuration);
        }

        Destroy(gameObject);
    }

    public void ReverseDirection()
    {
        directionX *= -1;
    }
}