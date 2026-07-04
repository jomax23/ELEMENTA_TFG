// Categorizes incoming damage so we can trigger specific reactions 
// (e.g., a different hit flash or sound for a punch vs an ability).
public enum DamageType
{
    Generic,
    Punch,
    Ability,
    Burn
}