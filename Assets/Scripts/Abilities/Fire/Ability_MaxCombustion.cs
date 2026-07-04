using UnityEngine;
using System.Collections;

// Two-phase ultimate: fires a continuous beam, then launches a heavy fireball.
// Supports interruption; if cancelled during the beam phase, the fireball won't fire.
[CreateAssetMenu(fileName = "CombustionMaxima", menuName = "Abilities/Fire/Combustión Máxima")]
public class Ability_MaxCombustion : AbilityData
{
    [Header("Air Beam Phase")]
    [SerializeField] private GameObject airBeamPrefab;
    [SerializeField] private float airBeamDuration = 1f;
    [SerializeField] private float maxDistance = 12f;

    [Header("Fireball Stats")]
    [SerializeField] private float fireballImpactDamage = 15f;
    [SerializeField] private float fireballBurnDps = 3f;
    [SerializeField] private float fireballBurnDuration = 3f;

    [Header("Fireball")]
    [SerializeField] private FireballProjectile fireballPrefab;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayers;

    private bool isCancelled;
    private GameObject activeBeam;

    public override string GetFormattedDescription(float efficiency)
    {
        float scaledBeamDur = airBeamDuration * efficiency;
        float scaledImpact = fireballImpactDamage * efficiency;
        float scaledBurnDps = fireballBurnDps * efficiency;
        float scaledBurnDur = fireballBurnDuration * efficiency;
        return string.Format(descriptionTemplate, scaledBeamDur, scaledImpact, scaledBurnDps, scaledBurnDur);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_MaxCombustion)}] IAbilityUser not found on {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = FindDeep(owner.transform, "HeadSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(Ability_MaxCombustion)}] HeadSpawn not found on {owner.name}.", owner);
            return;
        }

        isCancelled = false;
        activeBeam = null;
        user.RunCoroutine(Execute(user, spawnPoint, efficiency));
    }

    // Cleans up the beam if the player gets interrupted (e.g., stunned)
    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
        if (activeBeam != null)
        {
            Object.Destroy(activeBeam);
            activeBeam = null;
        }
    }

    private IEnumerator Execute(IAbilityUser user, Transform spawnPoint, float efficiency)
    {
        int dirX = user.FacingDirection;
        Vector3 dir = Vector3.right * dirX;

        // 1. Spawn and initialize the visual beam
        activeBeam = Instantiate(airBeamPrefab, spawnPoint.position, Quaternion.identity);
        CombustionBeam beam = activeBeam.GetComponent<CombustionBeam>();
        beam.Initialize(spawnPoint, dir, maxDistance, obstacleLayers, airBeamDuration * efficiency);

        // 2. Wait for the beam phase to finish
        yield return new WaitForSeconds(airBeamDuration * efficiency);

        // 3. Abort if the player was interrupted during the beam
        if (isCancelled) yield break;

        // 4. Clean up beam reference (the beam component handles its own visual destruction)
        if (activeBeam != null)
        {
            Object.Destroy(activeBeam);
            activeBeam = null;
        }

        // 5. Calculate scaled fireball stats
        float scaledImpact = fireballImpactDamage * efficiency;
        float scaledBurnDps = fireballBurnDps * efficiency;
        float scaledBurnDur = fireballBurnDuration * efficiency;

        // 6. Spawn the follow-up fireball
        FireballProjectile fireball = Instantiate(
            fireballPrefab,
            spawnPoint.position,
            Quaternion.Euler(0f, 0f, 90f * dirX)
        );
        fireball.Initialize(dirX, user.TargetLayers, scaledImpact, scaledBurnDps, scaledBurnDur);
    }
}