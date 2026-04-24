using UnityEngine;
using System.Collections;


// ──────────────────────────────────────────────────────────────
// Ráfaga de Agua
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "RafagaDeAgua", menuName = "Abilities/Water/Ráfaga de Agua")]
public class Ability_WaterBall : AbilityData
{
    [Header("Projectile")]
    [SerializeField] private WaterBallProjectile projectilePrefab;

    [Header("Burst Settings")]
    [SerializeField] private int   projectileCount  = 3;
    [SerializeField] private float timeBetweenShots = 0.5f;

    private bool isCancelled;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"[{nameof(Ability_WaterBall)}] projectilePrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_WaterBall)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = AbilityData.FindDeep(owner.transform, "RightHandSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(Ability_WaterBall)}] RightHandSpawn no encontrado.", owner);
            return;
        }

        isCancelled = false;
        user.RunCoroutine(FireBurst(user, spawnPoint, efficiency));
    }

    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
    }

    private IEnumerator FireBurst(IAbilityUser user, Transform spawnPoint, float efficiency)
    {
        int       dirX   = user.FacingDirection;
        LayerMask layers = user.TargetLayers;

        for (int i = 0; i < projectileCount; i++)
        {
            if (isCancelled) yield break;

            WaterBallProjectile proj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            proj.Initialize(dirX, layers, efficiency);

            if (i < projectileCount - 1)
                yield return new WaitForSeconds(timeBetweenShots);
        }
    }
}