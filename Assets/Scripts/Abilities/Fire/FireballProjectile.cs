using UnityEngine;

public class FireballProjectile : MonoBehaviour, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed    = 14f;
    [SerializeField] private float lifetime = 2f;

    [Header("Damage")]
    [SerializeField] private float impactDamage        = 10f;
    [SerializeField] private float burnDamagePerSecond = 2f;
    [SerializeField] private float burnDuration        = 3f;

    private LayerMask targetLayers;
    private int       directionX;
    private float     lifeTimer;

    // Valores reales aplicados tras escalar por efficiency
    private float actualImpactDamage;
    private float actualBurnDps;
    private float actualBurnDuration;

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño y duración de quemadura.</param>
    public void Initialize(int dirX, LayerMask layers, float efficiency = 1f)
    {
        directionX   = dirX;
        targetLayers = layers;

        actualImpactDamage = impactDamage        * efficiency;
        actualBurnDps      = burnDamagePerSecond * efficiency;
        actualBurnDuration = burnDuration        * efficiency;
    }

    private void Update()
    {
        transform.position += Vector3.right * directionX * speed * Time.deltaTime;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IAbilityTarget target = other.GetComponent<IAbilityTarget>();
        if (target != null)
        {
            target.ApplyDamage(actualImpactDamage);
            target.ApplyBurn(actualBurnDps, actualBurnDuration);
        }

        Destroy(gameObject);
    }

    public void ReverseDirection()
    {
        directionX *= -1;
    }
}