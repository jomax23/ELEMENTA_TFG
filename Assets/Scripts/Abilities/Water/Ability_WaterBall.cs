using UnityEngine;
using System.Collections;

/// <summary>
/// Water ability that fires a burst of multiple water projectiles over time.
/// Supports interruption via the Cancel method.
/// </summary>
[CreateAssetMenu(fileName = "RafagaDeAgua", menuName = "Abilities/Water/Ráfaga de Agua")]
public class Ability_WaterBall : AbilityData
{
    [Header("Stats")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float pushForce = 6f;
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float timeBetweenShots = 0.5f;
    
    [Header("Projectile")]
    [SerializeField] private WaterBallProjectile projectilePrefab;
    
    public override string GetFormattedDescription(float efficiency)
    {
        float actualDamage = damage * efficiency;
        float actualPush = pushForce * efficiency;

        return string.Format(descriptionTemplate, projectileCount, actualDamage, actualPush);

    }
    
    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null) return;

        Transform spawnPoint = FindDeep(owner.transform, "RightHandSpawn");
        if (spawnPoint == null) return;

        user.RunCoroutine(FireBurst(user, spawnPoint, efficiency));
    }

    private IEnumerator FireBurst(IAbilityUser user, Transform spawnPoint, float efficiency)
    {
        float scaledDamage = damage * efficiency;
        float scaledPush = pushForce * efficiency;

        for (int i = 0; i < projectileCount; i++)
        {
            WaterBallProjectile proj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            proj.Initialize(user.FacingDirection, user.TargetLayers, scaledDamage, scaledPush);
            if (i < projectileCount - 1) yield return new WaitForSeconds(timeBetweenShots);
        }
    }
}