using UnityEngine;

/// <summary>
/// Air ability that grants the player temporary flight.
/// Requires the PlayerFlight component on the owner.
/// </summary>
[CreateAssetMenu(fileName = "VueloDeIcaro", menuName = "Abilities/Air/Vuelo de Ícaro")]
public class Ability_VueloDeIcaro : AbilityData
{
    [Header("Flight Settings")]
    [Tooltip("Base duration of the flight. Scaled by affinity efficiency.")]
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

        float scaledDuration = flightDuration * efficiency;
        flight.StartFlight(scaledDuration);
    }

    public override void Cancel(GameObject owner)
    {
        PlayerFlight flight = owner.GetComponent<PlayerFlight>();
        flight?.EndFlight();
    }
}