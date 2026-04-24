using UnityEngine;
using System.Collections;



// ──────────────────────────────────────────────────────────────
// Vuelo de Ícaro
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "VueloDeIcaro", menuName = "Abilities/Air/Vuelo de Ícaro")]
public class VueloDeIcaroAbility : AbilityData
{
    [Header("Flight Settings")]
    [SerializeField] private float flightDuration = 4f;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        PlayerFlight flight = owner.GetComponent<PlayerFlight>();
        if (flight == null)
        {
            Debug.LogError($"[{nameof(VueloDeIcaroAbility)}] PlayerFlight no encontrado en {owner.name}.", owner);
            return;
        }

        // La duración del vuelo se escala por efficiency
        float scaledDuration = flightDuration * efficiency;
        flight.StartFlight(scaledDuration);
    }

    public override void Cancel(GameObject owner)
    {
        PlayerFlight flight = owner.GetComponent<PlayerFlight>();
        flight?.EndFlight();
    }
}