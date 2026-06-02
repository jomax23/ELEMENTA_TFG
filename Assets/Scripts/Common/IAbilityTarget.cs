/// <summary>
/// Interface for any entity that can be targeted and affected by abilities 
/// (e.g., Player, EnemyDummy). Handles status effects and damage application.
/// </summary>
public interface IAbilityTarget
{
    /// <summary>Gets or sets whether the target is currently immune to damage/effects.</summary>
    bool IsIntangible { get; set; }

    /// <summary>Applies a physical knockback force to the target.</summary>
    void ApplyImpulse(float force);

    /// <summary>Applies a movement speed reduction for a specific duration.</summary>
    void ApplySlow(float multiplier, float duration);

    /// <summary>Applies a stun effect, preventing actions for a specific duration.</summary>
    void ApplyStun(float duration);

    /// <summary>Applies direct damage to the target's health.</summary>
    void ApplyDamage(float damage, DamageType type = DamageType.Generic);

    /// <summary>Applies a Damage Over Time (DoT) burn effect.</summary>
    void ApplyBurn(float damagePerSecond, float duration);
}