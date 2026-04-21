using UnityEngine;

[CreateAssetMenu(
    fileName = "BolaAirosa",
    menuName = "Abilities/Air/Bola Airosa"
)]
public class BolaAirosaAbility : AbilityData
{
    public override void Activate(GameObject owner)
    {
        PlayerAirDash dash = owner.GetComponent<PlayerAirDash>();
        if (dash == null)
        {
            Debug.LogError(
                $"[{nameof(BolaAirosaAbility)}] PlayerAirDash no encontrado en {owner.name}.",
                owner
            );
            return;
        }

        dash.OnDashEnded += OnDashFinished;
        dash.StartDash();
    }

    /// <summary>
    /// Termina el dash inmediatamente si el jugador es interrumpido.
    /// </summary>
    public override void Cancel(GameObject owner)
    {
        PlayerAirDash dash = owner.GetComponent<PlayerAirDash>();
        dash?.ForceEndDash();
    }

    private void OnDashFinished()
    {
        // Punto de extensión: VFX de aterrizaje, audio, etc.
    }
}