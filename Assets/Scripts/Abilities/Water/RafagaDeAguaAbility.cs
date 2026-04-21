using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "RafagaDeAgua",
    menuName = "Abilities/Water/Ráfaga de Agua"
)]
public class RafagaDeAguaAbility : AbilityData
{
    [Header("Projectile")]
    [SerializeField] private WaterBallProjectile projectilePrefab;

    [Header("Burst Settings")]
    [SerializeField] private int   projectileCount  = 3;
    [SerializeField] private float timeBetweenShots = 0.5f;

    // ── Estado runtime ─────────────────────────────────────────────────────────
    private bool isCancelled;

    // ─────────────────────────────────────────────────────────────────────────

    public override void Activate(GameObject owner)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"[{nameof(RafagaDeAguaAbility)}] projectilePrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(RafagaDeAguaAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = owner.transform.Find("ProjectileSpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(RafagaDeAguaAbility)}] ProjectileSpawnPoint no encontrado en {owner.name}.", owner);
            return;
        }

        isCancelled = false;
        user.RunCoroutine(FireBurst(user, spawnPoint));
    }

    /// <summary>
    /// Aborta el burst en curso. Los proyectiles ya instanciados siguen su trayectoria
    /// (son auto-contenidos), pero no se dispararán más del burst.
    /// </summary>
    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
    }

    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator FireBurst(IAbilityUser user, Transform spawnPoint)
    {
        int       directionX = user.FacingDirection;
        LayerMask layers     = user.TargetLayers;

        for (int i = 0; i < projectileCount; i++)
        {
            // Comprobar antes de cada disparo del burst.
            if (isCancelled) yield break;

            SpawnProjectile(spawnPoint, directionX, layers);

            // No esperar después del último disparo (optimización menor).
            if (i < projectileCount - 1)
                yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    private void SpawnProjectile(Transform spawnPoint, float directionX, LayerMask layers)
    {
        WaterBallProjectile proj = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        proj.Initialize(directionX, layers);
    }
}