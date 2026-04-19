using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "BolaDeFuego",
    menuName = "Abilities/Fire/Bola de Fuego"
)]
public class BolaDeFuegoAbility : AbilityData
{
    [Header("Fireball Prefab")]
    [SerializeField] private FireballProjectile fireballPrefab;

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
            FireOnce(owner.transform, spawnPoint)
        );
    }

    private IEnumerator FireOnce(Transform ownerTransform, Transform spawnPoint)
    {
        PlayerMovement movement =
            ownerTransform.GetComponent<PlayerMovement>();

        if (movement == null)
            yield break;

        int directionX = movement.FacingDirection;

        SpawnProjectile(spawnPoint, directionX);

        yield return null; // mantiene el patrón de coroutine
    }

    private void SpawnProjectile(Transform spawnPoint, int directionX)
    {
        
        Quaternion rotation = Quaternion.Euler(0f, 0f, 90 * directionX);

        FireballProjectile fireball = Instantiate(
            fireballPrefab,
            spawnPoint.position,
            rotation
        );

        fireball.Initialize(directionX);
    }

}