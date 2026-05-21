public interface IAbilityTarget
{
    void ApplyImpulse(float force);
    void ApplySlow(float multiplier, float duration);

    void ApplyStun(float duration);
    void ApplyDamage(float damage, DamageType type = DamageType.Generic);

    void ApplyBurn(float damagePerSecond, float duration);
    
}
