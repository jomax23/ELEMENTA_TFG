using UnityEngine;

// ──────────────────────────────────────────────────────────────
// Cuerpo Acorazado
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "CuerpoAcorazado", menuName = "Abilities/Earth/Cuerpo Acorazado")]
public class Ability_RockBody : AbilityData
{
    [Header("Armor Settings")]
    [SerializeField] private float absorptionAmount = 50f;
    [SerializeField] [Range(0.1f, 1f)] private float speedMultiplier = 0.5f;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        PlayerArmor armor = owner.GetComponent<PlayerArmor>();
        if (armor == null)
        {
            Debug.LogError($"[{nameof(Ability_RockBody)}] PlayerArmor no encontrado en {owner.name}.", owner);
            return;
        }

        // La absorción total se escala por efficiency
        float scaledAbsorption = absorptionAmount * efficiency;

        if (armor.IsActive)
            armor.Deactivate();
        else
            armor.Activate(scaledAbsorption, speedMultiplier);
    }
}