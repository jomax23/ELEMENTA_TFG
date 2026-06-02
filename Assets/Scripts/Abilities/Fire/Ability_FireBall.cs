using UnityEngine;
using System.Collections;

/// <summary>
/// Fire ability that spawns a standard fireball projectile from a designated spawn point.
/// </summary>
[CreateAssetMenu(fileName = "BolaDeFuego", menuName = "Abilities/Fire/Bola de Fuego")]
public class Ability_FireBall : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float impactDamage = 10f;
    [SerializeField] private float burnDamagePerSecond = 2f;
    [SerializeField] private float burnDuration = 3f;
    
    [Header("Fireball Prefab")]
    [SerializeField] private FireballProjectile fireballPrefab;
    
    public override string GetFormattedDescription(float efficiency)
    {
        float actualImpact = impactDamage * efficiency;
        float actualBurnDps = burnDamagePerSecond * efficiency;
        float actualBurnDur = burnDuration * efficiency;
        return string.Format(descriptionTemplate, actualImpact, actualBurnDps, actualBurnDur);
    }
    
    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_FireBall)}] IAbilityUser not found on {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = FindDeep(owner.transform, "LeftHandSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(Ability_FireBall)}] LeftHandSpawn not found on {owner.name}.", owner);
            return;
        }

        user.RunCoroutine(FireOnce(user, spawnPoint, efficiency));
    }

    private IEnumerator FireOnce(IAbilityUser user, Transform spawnPoint, float efficiency)
    {
        float scaledDamage = impactDamage * efficiency;
        float scaledBurnDps = burnDamagePerSecond * efficiency;
        float scaledBurnDur = burnDuration * efficiency;

        Quaternion rotation = Quaternion.Euler(0f, 0f, 90f * user.FacingDirection);
        FireballProjectile fireball = Instantiate(fireballPrefab, spawnPoint.position, rotation);
        fireball.Initialize(user.FacingDirection, user.TargetLayers, scaledDamage, scaledBurnDps, scaledBurnDur);
        yield return null;
    }
}