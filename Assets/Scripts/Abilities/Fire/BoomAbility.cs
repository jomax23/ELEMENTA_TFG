using UnityEngine;

[CreateAssetMenu(
    fileName = "BOOM",
    menuName = "Abilities/Fire/BOOM"
)]
public class BoomAbility : AbilityData
{
    [Header("Explosion Prefab")]
    [SerializeField] private ExplosionArea explosionPrefab;

    public override void Activate(GameObject owner)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(BoomAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Vector3 spawnPosition   = owner.transform.position;
        spawnPosition.y         = 1f;

        ExplosionArea explosion = Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
        explosion.Initialize(user.FacingDirection, user.TargetLayers);
    }
}