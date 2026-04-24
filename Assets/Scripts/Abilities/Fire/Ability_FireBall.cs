using UnityEngine;
using System.Collections;


// ──────────────────────────────────────────────────────────────
// Bola de Fuego
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "BolaDeFuego", menuName = "Abilities/Fire/Bola de Fuego")]
public class Ability_FireBall : AbilityData
{
    [Header("Fireball Prefab")]
    [SerializeField] private FireballProjectile fireballPrefab;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null) { LogError(owner, "IAbilityUser"); return; }

        Transform spawnPoint = FindDeep(owner.transform, "LeftHandSpawn");
        if (spawnPoint == null) { LogError(owner, "LeftHandSpawn"); return; }

        user.RunCoroutine(FireOnce(user, spawnPoint, efficiency));
    }

    private IEnumerator FireOnce(IAbilityUser user, Transform spawnPoint, float efficiency)
    {
        SpawnProjectile(spawnPoint, user.FacingDirection, user.TargetLayers, efficiency);
        yield return null;
    }

    private void SpawnProjectile(Transform spawnPoint, int dirX, LayerMask layers, float efficiency)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, 90 * dirX);
        FireballProjectile fireball = Instantiate(fireballPrefab, spawnPoint.position, rotation);
        fireball.Initialize(dirX, layers, efficiency);
    }

    private void LogError(GameObject owner, string missing) =>
        Debug.LogError($"[{nameof(Ability_FireBall)}] {missing} no encontrado en {owner.name}.", owner);
}