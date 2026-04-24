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
    private int       directionX;
    private float     actualPushForce;
    private float     actualSlowDuration;
    private float     actualSlowMultiplier;
    private float     actualDamage;

    /// <param name="efficiency">Multiplicador de afinidad (0–1). Escala daño, impulso y slow.</param>
    public void Initialize(int dirX, LayerMask layers, float efficiency = 1f)
    {
        directionX          = dirX;
        targetLayers        = layers;

        actualDamage        = damage       * efficiency;
        actualPushForce     = pushForce    * efficiency;
        actualSlowDuration  = slowDuration * efficiency;
        // El multiplier de slow funciona al revés (menor = más lento);
        // con baja efficiency lo hacemos menos efectivo: acercamos a 1.
        actualSlowMultiplier = Mathf.Lerp(1f, slowMultiplier, efficiency);

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
            target.ApplySlow(actualSlowMultiplier, actualSlowDuration);
            target.ApplyDamage(actualDamage);
        }
    }
}