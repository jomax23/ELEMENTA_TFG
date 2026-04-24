using UnityEngine;
using System.Collections;


// ──────────────────────────────────────────────────────────────
// Rayo Mortal
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "RayoMortal", menuName = "Abilities/Fire/Rayo Mortal")]
public class Ability_MortalThunder : AbilityData
{
    [Header("Beam Prefab")]
    [SerializeField] private RayoMortalProjectile beamPrefab;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_MortalThunder)}] IAbilityUser no encontrado.", owner);
            return;
        }

        Transform spawnPoint = FindDeep(owner.transform, "RightHandSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(Ability_MortalThunder)}] RightHandSpawn no encontrado.", owner);
            return;
        }

        RayoMortalProjectile beam = Instantiate(beamPrefab, spawnPoint.position, Quaternion.identity);
        beam.Initialize(user.FacingDirection, user.TargetLayers, efficiency);
    }
}
