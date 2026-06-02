using UnityEngine;

/// <summary>
/// Air dash ability. 
/// Requires the PlayerAirDash component on the owner to handle the actual movement physics.
/// </summary>
[CreateAssetMenu(fileName = "BolaAirosa", menuName = "Abilities/Air/Bola Airosa")]
public class Ability_BolaAirosa : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.6f;
    
    public override string GetFormattedDescription(float efficiency)
    {
        // La eficiencia podría escalar la duración en el futuro
        return string.Format(descriptionTemplate, dashDuration, dashSpeed);
    }
    
    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (owner.GetComponent<PlayerAirDash>() is PlayerAirDash dash)
        {
            dash.OnDashEnded += OnDashFinished;
            dash.StartDash(dashSpeed, dashDuration * efficiency); // Pasamos los valores desde el SO
        }
    }
    private void OnDashFinished() { }
}