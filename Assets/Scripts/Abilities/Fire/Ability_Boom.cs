using UnityEngine;
using System.Collections;


// ──────────────────────────────────────────────────────────────
// BOOM (Explosión Corpórea)
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "BOOM", menuName = "Abilities/Fire/BOOM")]
public class Ability_Boom : AbilityData
{
    [Header("Explosion Prefab")]
    [SerializeField] private ExplosionArea explosionPrefab;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_Boom)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Vector3 spawnPosition = owner.transform.position;
        spawnPosition.y = 1f;

        ExplosionArea explosion = Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
        explosion.Initialize(user.FacingDirection, user.TargetLayers, efficiency);
    }
}