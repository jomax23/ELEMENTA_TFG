using UnityEngine;

// Spawns a persistent tornado hazard that can reflect enemy projectiles.
// Spawns slightly in front of the user based on their facing direction.
[CreateAssetMenu(fileName = "Tornado", menuName = "Abilities/Air/Tornado")]
public class Ability_Tornado : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float spawnOffsetX = 1.5f;
    [SerializeField] private float spawnOffsetY = 0f;

    [Header("Prefab")]
    [SerializeField] private TornadoArea tornadoPrefab;

    public override string GetFormattedDescription(float efficiency)
    {
        return string.Format(descriptionTemplate, lifetime * efficiency);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (tornadoPrefab == null)
        {
            Debug.LogError($"[{nameof(Ability_Tornado)}] tornadoPrefab is not assigned.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_Tornado)}] IAbilityUser component not found on {owner.name}.", owner);
            return;
        }

        // Calculate spawn position in front of the player
        int dirX = user.FacingDirection;
        Vector3 spawnPos = owner.transform.position 
                           + Vector3.right * dirX * spawnOffsetX 
                           + Vector3.up * spawnOffsetY;

        TornadoArea tornado = Instantiate(tornadoPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
        tornado.Initialize(lifetime * efficiency);
    }
}