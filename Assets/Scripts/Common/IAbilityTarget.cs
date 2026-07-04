// Implemented by anything that can receive abilities (Player, Enemies). 
// Centralizes all status effects and damage application so abilities don't need to know 
// if they're hitting a player or an AI.
public interface IAbilityTarget
{
    // God mode toggle. Blocks all incoming damage and effects.
    bool IsIntangible { get; set; }

    // Physical knockback.
    void ApplyImpulse(float force);

    // Movement speed reduction.
    void ApplySlow(float multiplier, float duration);

    // Locks out actions/movement.
    void ApplyStun(float duration);

    // Direct HP reduction.
    void ApplyDamage(float damage, DamageType type = DamageType.Generic);

    // Damage over time (DoT).
    void ApplyBurn(float damagePerSecond, float duration);
}