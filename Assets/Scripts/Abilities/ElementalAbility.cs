using UnityEngine;

[CreateAssetMenu(
    fileName = "New Elemental Ability",
    menuName = "Abilities/Elemental Ability"
)]
public class ElementalAbility : AbilityData
{
    public override void Activate(GameObject owner)
    {
        Debug.Log(
            $"{owner.name} usa {abilityName} ({element})"
        );

        // Aquí más adelante:
        // - Instanciar proyectil
        // - Aplicar impulso
        // - Reproducir animación
        // - Consumir recursos
    }
}