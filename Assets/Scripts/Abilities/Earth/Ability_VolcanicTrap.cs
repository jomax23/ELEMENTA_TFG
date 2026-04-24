using UnityEngine;

// ──────────────────────────────────────────────────────────────
// Trampa Volcánica
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "TrampaVolcanica", menuName = "Abilities/Earth/Trampa Volcánica")]
public class Ability_VolcanicTrap : AbilityData
{
    [Header("Trap Prefab")]
    [SerializeField] private TrampaVolcanicaArea trapPrefab;
    [SerializeField] private float               spawnOffsetX = 1.5f;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (trapPrefab == null)
        {
            Debug.LogError($"[{nameof(Ability_VolcanicTrap)}] trapPrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_VolcanicTrap)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Vector3 spawnPos  = owner.transform.position;
        spawnPos.x       += user.FacingDirection * spawnOffsetX;

        TrampaVolcanicaArea trap = Instantiate(trapPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
        trap.Initialize(user.TargetLayers, efficiency);
    }
}