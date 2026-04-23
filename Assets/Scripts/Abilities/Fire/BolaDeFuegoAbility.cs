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
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(BolaDeFuegoAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = AbilityData.FindDeep(owner.transform, "LeftHandSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(BolaDeFuegoAbility)}] ProjectileSpawnPoint no encontrado en {owner.name}.", owner);
            return;
        }

        user.RunCoroutine(FireOnce(user, spawnPoint));
    }

    private IEnumerator FireOnce(IAbilityUser user, Transform spawnPoint)
    {
        int directionX = user.FacingDirection;
        SpawnProjectile(spawnPoint, directionX, user.TargetLayers);
        yield return null;
    }

    private void SpawnProjectile(Transform spawnPoint, int directionX, LayerMask layers)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, 90 * directionX);

        FireballProjectile fireball = Instantiate(fireballPrefab, spawnPoint.position, rotation);
        fireball.Initialize(directionX, layers);
    }
}