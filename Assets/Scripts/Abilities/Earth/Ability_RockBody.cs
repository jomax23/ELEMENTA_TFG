using UnityEngine;

/// <summary>
/// Earth ability that grants the player temporary armor.
/// The armor absorbs a specific amount of damage and applies a movement speed penalty.
/// The absorption amount is scaled by elemental affinity efficiency.
/// </summary>
[CreateAssetMenu(fileName = "CuerpoAcorazado", menuName = "Abilities/Earth/Cuerpo Acorazado")]
public class Ability_RockBody : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float absorptionAmount = 50f;
    [SerializeField, Range(0.1f, 1f)] private float speedMultiplier = 0.5f;

    public override string GetFormattedDescription(float efficiency)
    {
        float actualAbsorption = absorptionAmount * efficiency;
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

        // Scale the total absorption based on elemental affinity
        float scaledAbsorption = absorptionAmount * efficiency;

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