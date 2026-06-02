using UnityEngine;

/// <summary>
/// Earth ability that spawns a stone wall in front of the user.
/// Acts as a spatial control tool. Currently, its stats are not scaled by efficiency,
/// but this can be extended in the future (e.g., scaling its lifetime).
/// </summary>
[CreateAssetMenu(fileName = "ElMuro", menuName = "Abilities/Earth/El Muro")]
public class Ability_TheWall : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float spawnDistance = 1.5f;
    [SerializeField] private float spawnOffset = 0f;

    [Header("Prefab")]
    [SerializeField] private StoneWall wallPrefab;

    public override string GetFormattedDescription(float efficiency)
    {
        float actualLifetime = lifetime * efficiency;
        return string.Format(descriptionTemplate, actualLifetime);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (wallPrefab == null)
        {
            Debug.LogError($"[{nameof(Ability_TheWall)}] wallPrefab no asignado.", this);
            return;
        }

        Transform t = owner.transform;
        Vector3 spawnPosition = t.position + t.forward * spawnDistance;
        spawnPosition.y += spawnOffset;

        // Calculate scaled lifetime
        float scaledLifetime = lifetime * efficiency;

        // Pass pre-scaled value to the wall prefab
        StoneWall wall = Instantiate(wallPrefab, spawnPosition, Quaternion.identity);
        wall.Initialize(scaledLifetime);
    }
}