using UnityEngine;

[CreateAssetMenu(
    fileName = "VueloDeIcaro",
    menuName = "Abilities/Air/Vuelo de Ícaro"
)]
public class VueloDeIcaroAbility : AbilityData
{
    [Header("Flight Settings")]
    [SerializeField] private float flightDuration = 4f;

    public override void Activate(GameObject owner)
    {
        PlayerFlight flight = owner.GetComponent<PlayerFlight>();
        if (flight == null)
        {
            Debug.LogError($"[{nameof(VueloDeIcaroAbility)}] PlayerFlight no encontrado en {owner.name}.", owner);
            return;
        }

        flight.StartFlight(flightDuration);
    }
}