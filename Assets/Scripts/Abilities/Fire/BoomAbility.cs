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
        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            Debug.LogError("El Player no tiene PlayerMovement");
            return;
        }

        int directionX = movement.FacingDirection;

        
        Transform t = owner.transform;

        Vector3 spawnPosition = owner.transform.position;

        spawnPosition.y = 1;
        
        ExplosionArea explosion = Instantiate(
            explosionPrefab,
            spawnPosition,
            Quaternion.identity
        );

        explosion.Initialize(directionX);
    }
}