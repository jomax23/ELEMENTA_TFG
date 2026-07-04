using UnityEngine;

// Spawns a physical barrier to block paths or projectiles.
// Currently only scales lifetime, but structured to easily add more scaled stats later.
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
        // Spawn slightly in front of the player
        Vector3 spawnPosition = t.position + t.forward * spawnDistance;
        spawnPosition.y += spawnOffset;

        float scaledLifetime = lifetime * efficiency;
        
        StoneWall wall = Instantiate(wallPrefab, spawnPosition, Quaternion.identity);
        wall.Initialize(scaledLifetime);
    }
}