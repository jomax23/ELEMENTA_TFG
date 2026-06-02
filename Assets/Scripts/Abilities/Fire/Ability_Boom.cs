using UnityEngine;

/// <summary>
/// Fire ability that spawns an explosion area centered on the user.
/// </summary>
[CreateAssetMenu(fileName = "BOOM", menuName = "Abilities/Fire/BOOM")]
public class Ability_Boom : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float pushForce = 10f;
    
    [Header("Explosion Prefab")]
    [SerializeField] private ExplosionArea explosionPrefab;

    public override string GetFormattedDescription(float efficiency)
    {
        float actualDamage = damage * efficiency;
        float actualPush = pushForce * efficiency;

        return string.Format(descriptionTemplate, radius, actualDamage, actualPush);

    }
    
    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_Boom)}] IAbilityUser not found on {owner.name}.", owner);
            return;
        }

        Vector3 spawnPosition = owner.transform.position;
        spawnPosition.y = 1f; // Elevate slightly to ensure proper trigger overlap

        ExplosionArea explosion = Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
        explosion.Initialize(user.FacingDirection, user.TargetLayers, damage * efficiency, pushForce * efficiency);
    }
}