using UnityEngine;

[CreateAssetMenu(
    fileName = "RayoMortal",
    menuName = "Abilities/Fire/Rayo Mortal"
)]
public class RayoMortalAbility : AbilityData
{
    [Header("Beam Prefab")]
    [SerializeField] private RayoMortalProjectile beamPrefab;

    public override void Activate(GameObject owner)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(RayoMortalAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = owner.transform.Find("ProjectileSpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(RayoMortalAbility)}] ProjectileSpawnPoint no encontrado en {owner.name}.", owner);
            return;
        }

        RayoMortalProjectile beam = Instantiate(beamPrefab, spawnPoint.position, Quaternion.identity);
        beam.Initialize(user.FacingDirection, user.TargetLayers);
    }
}