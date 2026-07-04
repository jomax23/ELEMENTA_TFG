using UnityEngine;

// Air dash ability. Delegates the actual movement physics to the PlayerAirDash component.
[CreateAssetMenu(fileName = "BolaAirosa", menuName = "Abilities/Air/Bola Airosa")]
public class Ability_BolaAirosa : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.6f;

    public override string GetFormattedDescription(float efficiency)
    {
        // Efficiency could scale duration in the future
        return string.Format(descriptionTemplate, dashDuration, dashSpeed);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (owner.GetComponent<PlayerAirDash>() is PlayerAirDash dash)
        {
            dash.OnDashEnded += OnDashFinished;
            // Pass SO values, scaled by efficiency
            dash.StartDash(dashSpeed, dashDuration * efficiency); 
        }
    }

    private void OnDashFinished() { }
}