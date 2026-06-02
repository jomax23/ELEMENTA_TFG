using UnityEngine;

/// <summary>
/// Water ability that spawns a frost expansion area in front of the user.
/// The area's effects are scaled by elemental affinity efficiency.
/// </summary>
[CreateAssetMenu(fileName = "ExpansionHelada", menuName = "Abilities/Water/Expansión Helada")]
public class Ability_FrostExpansion : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float maxLength = 8f;
    [SerializeField] private float expandSpeed = 12f;
    [SerializeField] private float lifetime = 1.5f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float spawnOffset = 0f;

    [Header("Prefab")]
    [SerializeField] private ExpansionHeladaArea areaPrefab;

    public override string GetFormattedDescription(float efficiency)
    {
        float actualDamage = damage * efficiency;

        return string.Format(descriptionTemplate, maxLength, actualDamage);

    }
    
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

        int dirX = user.FacingDirection;
        Vector3 spawnPos = owner.transform.position
                           + Vector3.right * dirX * spawnDistance
                           + Vector3.up * spawnOffset;

        // Calculate scaled stats
        float scaledDamage = damage * efficiency;

        // Pass pre-scaled values to the area prefab
        ExpansionHeladaArea area = Instantiate(areaPrefab, spawnPos, Quaternion.identity);
        area.Initialize(dirX, user.TargetLayers, scaledDamage, maxLength, expandSpeed, lifetime);
    }
}