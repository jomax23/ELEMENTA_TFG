using UnityEngine;

// Spawns a water wave that pushes and slows enemies.
// The slow multiplier is lerped so efficiency scales the *strength* of the slow effect.
[CreateAssetMenu(fileName = "GolpeDeMarea", menuName = "Abilities/Water/Golpe de Marea")]
public class Ability_SeaHit : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float pushForce = 12f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 2f;
    [SerializeField] private float spawnOffset = 1.5f;

    [Header("Wave Prefab")]
    [SerializeField] private WaterWaveProjectile wavePrefab;

    public override string GetFormattedDescription(float efficiency)
    {
        float actualDamage = damage * efficiency;
        float actualSlowDur = slowDuration * efficiency;
        // Lerp from 1.0 (no slow) to the base slowMultiplier based on efficiency
        float actualSlowMult = Mathf.Lerp(1f, slowMultiplier, efficiency);
        
        return string.Format(descriptionTemplate, actualDamage, pushForce, actualSlowMult * 100f, actualSlowDur);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null) return;

        int dirX = user.FacingDirection;
        Vector3 spawnPos = owner.transform.position + Vector3.right * dirX * spawnOffset;
        
        // Rotate the wave prefab to face the correct direction
        Quaternion rot = Quaternion.Euler(0f, 90f * dirX, 0f);
        
        float scaledSlowMult = Mathf.Lerp(1f, slowMultiplier, efficiency);
        
        WaterWaveProjectile wave = Instantiate(wavePrefab, spawnPos, rot);
        wave.Initialize(dirX, user.TargetLayers, damage * efficiency, pushForce * efficiency, scaledSlowMult, slowDuration * efficiency);
    }
}