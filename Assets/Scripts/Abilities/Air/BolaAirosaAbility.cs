/*
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
            Debug.LogError("El Player no tiene PlayerAirDash");
            return;
        }

        dash.StartDash();
    }
}
*/

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

        // Suscripción one-shot: se desuscribe sola al dispararse
        // para evitar acumulación de listeners entre activaciones.
        dash.OnDashEnded += OnDashFinished;
        dash.StartDash();
    }

    private void OnDashFinished()
    {
        // Punto de extensión: disparar cooldown, VFX de aterrizaje, audio, etc.
        // La desuscripción aquí garantiza que nunca se llame más de una vez
        // aunque StartDash sea invocado de nuevo antes de que acabe el anterior.
    }
}