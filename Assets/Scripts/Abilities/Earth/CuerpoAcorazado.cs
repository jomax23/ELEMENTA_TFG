using UnityEngine;

[CreateAssetMenu(
    fileName = "Cuerpo Acorazado",
    menuName = "Abilities/Earth/Cuerpo Acorazado"
)]
public class CuerpoAcorazado : AbilityData
{
    [Header("Armor Settings")]
    [SerializeField] private float absorptionAmount = 50f;
    [SerializeField] [Range(0.1f, 1f)] private float speedMultiplier = 0.5f;

    public override void Activate(GameObject owner)
    {
        PlayerArmor armor = owner.GetComponent<PlayerArmor>();
        if (armor == null)
        {
            Debug.LogError($"[{nameof(CuerpoAcorazado)}] PlayerArmor no encontrado en {owner.name}.", owner);
            return;
        }

        // Toggle
        if (armor.IsActive)
        {
            armor.Deactivate(); // NUEVO
        }
        else
        {
            armor.Activate(absorptionAmount, speedMultiplier);
        }
    }
}