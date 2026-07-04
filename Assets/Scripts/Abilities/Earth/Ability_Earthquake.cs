using UnityEngine;

// Creates a lingering AoE that stuns and damages enemies.
// Spawns slightly ahead of the player based on their facing direction.
[CreateAssetMenu(fileName = "Terremoto", menuName = "Abilities/Earth/Terremoto")]
public class Ability_Earthquake : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float spawnOffset = 0f;

    [Header("Area Settings")]
    [SerializeField] private EarthquakeArea areaPrefab;

    public override string GetFormattedDescription(float efficiency)
    {
        float actualStun = stunDuration * efficiency;
        float actualDps = damagePerSecond * efficiency;
        return string.Format(descriptionTemplate, duration, actualStun, actualDps);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null) return;

        int dirX = user.FacingDirection;
        Vector3 spawnPos = owner.transform.position + Vector3.right * dirX * spawnDistance + Vector3.up * spawnOffset;

        EarthquakeArea area = Instantiate(areaPrefab, spawnPos, Quaternion.identity);
        // Pass the pre-scaled stun and DPS values to the area
        area.Initialize(user.TargetLayers, duration, stunDuration * efficiency, damagePerSecond * efficiency);
    }
}