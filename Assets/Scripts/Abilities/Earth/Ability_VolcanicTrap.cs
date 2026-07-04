using UnityEngine;

// Spawns a lingering hazard that deals damage over time.
// All stats scale with the player's elemental affinity.
[CreateAssetMenu(fileName = "TrampaVolcanica", menuName = "Abilities/Earth/Trampa Volcánica")]
public class Ability_VolcanicTrap : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float spawnOffsetX = 1.5f;

    [Header("Prefab")]
    [SerializeField] private TrampaVolcanicaArea trapPrefab;

    public override string GetFormattedDescription(float efficiency)
    {
        float actualDps = damagePerSecond * efficiency;
        float actualLifetime = lifetime * efficiency;
        return string.Format(descriptionTemplate, actualDps, actualLifetime);
    }

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

        // Calculate the exact spawn position based on player facing
        Vector3 spawnPos = owner.transform.position;
        spawnPos.x += user.FacingDirection * spawnOffsetX;

        // Pre-scale the values so the prefab doesn't need to know about efficiency
        float scaledDps = damagePerSecond * efficiency;
        float scaledLifetime = lifetime * efficiency;

        TrampaVolcanicaArea trap = Instantiate(trapPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
        trap.Initialize(user.TargetLayers, scaledDps, scaledLifetime);
    }
}