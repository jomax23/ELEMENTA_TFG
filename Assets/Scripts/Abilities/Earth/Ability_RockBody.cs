using UnityEngine;

// Grants a damage-absorbing shield but slows the player down.
// Acts as a toggle: using it while active will remove the armor.
[CreateAssetMenu(fileName = "CuerpoAcorazado", menuName = "Abilities/Earth/Cuerpo Acorazado")]
public class Ability_RockBody : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float absorptionAmount = 50f;
    [SerializeField, Range(0.1f, 1f)] private float speedMultiplier = 0.5f;

    public override string GetFormattedDescription(float efficiency)
    {
        float actualAbsorption = absorptionAmount * efficiency;
        // Convert the multiplier (e.g., 0.5) into a percentage penalty (e.g., 50%) for the UI
        float speedPenalty = (1f - speedMultiplier) * 100f;
        return string.Format(descriptionTemplate, actualAbsorption, speedPenalty);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        PlayerArmor armor = owner.GetComponent<PlayerArmor>();
        if (armor == null)
        {
            Debug.LogError($"[{nameof(Ability_RockBody)}] PlayerArmor no encontrado en {owner.name}.", owner);
            return;
        }

        float scaledAbsorption = absorptionAmount * efficiency;

        // Toggle behavior: if it's already on, turn it off. Otherwise, activate it.
        if (armor.IsActive)
        {
            armor.Deactivate();
        }
        else
        {
            armor.Activate(scaledAbsorption, speedMultiplier);
        }
    }
}