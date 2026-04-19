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
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float timeBetweenShots = 0.5f;

    public override void Activate(GameObject owner)
    {
        AbilityCoroutineRunner runner =
            owner.GetComponent<AbilityCoroutineRunner>();

        if (runner == null)
        {
            Debug.LogError("El jugador no tiene AbilityCoroutineRunner");
            return;
        }

        Transform spawnPoint = owner.transform.Find("ProjectileSpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogError("No existe ProjectileSpawnPoint en el Player");
            return;
        }

        runner.RunCoroutine(
            FireBurst(owner.transform, spawnPoint)
        );
    }

    private IEnumerator FireBurst(Transform ownerTransform, Transform spawnPoint)
    {
        PlayerMovement movement =
            ownerTransform.GetComponent<PlayerMovement>();

        if (movement == null)
            yield break;

        int directionX = movement.FacingDirection;

        for (int i = 0; i < projectileCount; i++)
        {
            SpawnProjectile(spawnPoint, directionX);
            yield return new WaitForSeconds(timeBetweenShots);
        }
    }



    private void SpawnProjectile(Transform spawnPoint, float directionX)
    {
        WaterBallProjectile proj = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        proj.Initialize(directionX);
    }
}