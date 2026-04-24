using UnityEngine;
using System.Collections;


// ──────────────────────────────────────────────────────────────
// Bola Airosa (Air Dash)
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "BolaAirosa", menuName = "Abilities/Air/Bola Airosa")]
public class Ability_BolaAirosa : AbilityData
{
    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        PlayerAirDash dash = owner.GetComponent<PlayerAirDash>();
        if (dash == null)
        {
            Debug.LogError($"[{nameof(Ability_BolaAirosa)}] PlayerAirDash no encontrado en {owner.name}.", owner);
            return;
        }

        // El dash es un efecto de movilidad; efficiency podría escalar la velocidad/duración
        // en el futuro si PlayerAirDash lo expone. Por ahora lo activamos como estaba.
        dash.OnDashEnded += OnDashFinished;
        dash.StartDash();
    }

    public override void Cancel(GameObject owner)
    {
        PlayerAirDash dash = owner.GetComponent<PlayerAirDash>();
        dash?.ForceEndDash();
    }

    private void OnDashFinished() { /* VFX de aterrizaje, audio, etc. */ }
}