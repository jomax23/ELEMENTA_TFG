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
        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            Debug.LogError("El Player no tiene PlayerMovement");
            return;
        }

        Transform spawnPoint = owner.transform.Find("ProjectileSpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogError("No existe ProjectileSpawnPoint en el Player");
            return;
        }

        int directionX = movement.FacingDirection;

        RayoMortalProjectile beam = Instantiate(
            beamPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        beam.Initialize(directionX);
    }
}