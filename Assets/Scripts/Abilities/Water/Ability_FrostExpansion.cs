using UnityEngine;

// Spawns an expanding frost zone in front of the player.
// All stats are pre-scaled by efficiency before being passed to the area prefab.
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
            Debug.LogError($"[{nameof(Ability_FrostExpansion)}] areaPrefab not assigned.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_FrostExpansion)}] IAbilityUser not found on {owner.name}.", owner);
            return;
        }

        int dirX = user.FacingDirection;
        // Calculate spawn position slightly ahead of the user
        Vector3 spawnPos = owner.transform.position
                           + Vector3.right * dirX * spawnDistance
                           + Vector3.up * spawnOffset;

        float scaledDamage = damage * efficiency;

        // Pass pre-scaled values so the prefab doesn't need to know about efficiency
        ExpansionHeladaArea area = Instantiate(areaPrefab, spawnPos, Quaternion.identity);
        area.Initialize(dirX, user.TargetLayers, scaledDamage, maxLength, expandSpeed, lifetime);
    }
}