using UnityEngine;

public class WaterBallProjectile : ProjectileBase, IReversible
{
    [Header("Movement")]
    [SerializeField] private float speed    = 12f;
    [SerializeField] private float lifetime = 2f;

    [Header("Effects")]
    [SerializeField] private float pushForce = 6f;
    [SerializeField] private float damage    = 10f;

    private float directionX;
    private float actualPushForce;
    private float actualDamage;

    // ─────────────────────────────────────────────────────────────────────────

    public void Initialize(float dirX, LayerMask layers, float efficiency = 1f)
    {
        directionX    = Mathf.Sign(dirX);
        targetLayers  = layers;

        actualPushForce = pushForce * efficiency;
        actualDamage    = damage    * efficiency;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        TryMove(Vector3.right * directionX, speed);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ProjectileBase
    // ─────────────────────────────────────────────────────────────────────────

    protected override void OnTargetHit(Collider target)
    {
        IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
        if (abilityTarget != null)
        {
            abilityTarget.ApplyImpulse(directionX * actualPushForce);
            abilityTarget.ApplyDamage(actualDamage);
        }
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IReversible
    // ─────────────────────────────────────────────────────────────────────────

    public void ReverseDirection()
    {
        directionX = Mathf.Sign(directionX) * -1f;
    }
}