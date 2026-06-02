using UnityEngine;

/// <summary>
/// Spawns a TornadoArea prefab in front of the user.
/// The tornado acts as a persistent hazard that can reverse enemy projectiles.
/// </summary>
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
        float scaledLifetime = lifetime * efficiency;
        return string.Format(descriptionTemplate, scaledLifetime);

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

        int dirX = user.FacingDirection;
        Vector3 spawnPos = owner.transform.position 
                           + Vector3.right * dirX * spawnOffsetX 
                           + Vector3.up * spawnOffsetY;

        float scaledLifetime = lifetime * efficiency;

        TornadoArea tornado = Instantiate(tornadoPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
        tornado.Initialize(scaledLifetime);
    }
}