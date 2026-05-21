using UnityEngine;

public class FireballProjectile : ProjectileBase, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed    = 14f;
    [SerializeField] private float lifetime = 2f;

    [Header("Damage")]
    [SerializeField] private float impactDamage        = 10f;
    [SerializeField] private float burnDamagePerSecond = 2f;
    [SerializeField] private float burnDuration        = 3f;

    private int   directionX;
    private float lifeTimer;

    private float actualImpactDamage;
    private float actualBurnDps;
    private float actualBurnDuration;

    // ─────────────────────────────────────────────────────────────────────────

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
        // TryMove hace el Raycast antes de mover — si hay obstáculo, se destruye
        // y el Update no continúa ejecutándose ese fotograma.
        if (!TryMove(Vector3.right * directionX, speed)) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
            Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ProjectileBase
    // ─────────────────────────────────────────────────────────────────────────

    protected override void OnTargetHit(Collider target)
    {
        IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
        if (abilityTarget != null)
        {
            abilityTarget.ApplyDamage(actualImpactDamage);
            abilityTarget.ApplyBurn(actualBurnDps, actualBurnDuration);
        }
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IReversible
    // ─────────────────────────────────────────────────────────────────────────

    public void ReverseDirection()
    {
        directionX *= -1;
    }
}