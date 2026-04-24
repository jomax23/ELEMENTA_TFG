using UnityEngine;
using System.Collections;

// ──────────────────────────────────────────────────────────────
// Expansión Helada
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "ExpansionHelada", menuName = "Abilities/Water/Expansión Helada")]
public class Ability_FrostExpansion : AbilityData
{
    [Header("Prefab")]
    [SerializeField] private ExpansionHeladaArea areaPrefab;
    [SerializeField] private float spawnDistance;
    [SerializeField] private float spawnOffset = 0f;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (areaPrefab == null)
        {
            Debug.LogError($"[{nameof(Ability_FrostExpansion)}] areaPrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_FrostExpansion)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        int     dirX     = user.FacingDirection;
        Vector3 spawnPos = owner.transform.position
                           + Vector3.right * dirX * spawnDistance
                           + Vector3.up    * spawnOffset;

        ExpansionHeladaArea area = Instantiate(areaPrefab, spawnPos, Quaternion.identity);
        area.Initialize(dirX, user.TargetLayers, efficiency);
    }
}