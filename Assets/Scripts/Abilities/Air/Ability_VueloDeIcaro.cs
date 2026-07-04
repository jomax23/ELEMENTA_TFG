using UnityEngine;

// Grants temporary flight. Requires the PlayerFlight component on the owner.
[CreateAssetMenu(fileName = "VueloDeIcaro", menuName = "Abilities/Air/Vuelo de Ícaro")]
public class Ability_VueloDeIcaro : AbilityData
{
    [Header("Flight Settings")]
    // Base duration, scaled by affinity efficiency
    [SerializeField] private float flightDuration = 4f;

    public override string GetFormattedDescription(float efficiency)
    {
        float scaledDuration = flightDuration * efficiency;
        return string.Format(descriptionTemplate, scaledDuration);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        PlayerFlight flight = owner.GetComponent<PlayerFlight>();
        if (flight == null)
        {
            Debug.LogError($"[{nameof(Ability_VueloDeIcaro)}] PlayerFlight component not found on {owner.name}.", owner);
            return;
        }
        
        flight.StartFlight(flightDuration * efficiency);
    }

    // Immediately end flight if the player is interrupted
    public override void Cancel(GameObject owner)
    {
        PlayerFlight flight = owner.GetComponent<PlayerFlight>();
        flight?.EndFlight();
    }
}