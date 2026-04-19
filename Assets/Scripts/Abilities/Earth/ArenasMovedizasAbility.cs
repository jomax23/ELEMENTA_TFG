using UnityEngine;

[CreateAssetMenu(
    fileName = "ArenasMovedizas",
    menuName = "Abilities/Earth/Arenas Movedizas"
)]
public class ArenasMovedizasAbility : AbilityData
{
    [Header("Area Prefab")]
    [SerializeField] private ArenasMovedizasArea areaPrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnOffsetX = 1.5f;

    public override void Activate(GameObject owner)
    {
        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
            return;

        Vector3 spawnPos = owner.transform.position;
        spawnPos.x += movement.FacingDirection * spawnOffsetX;

        Instantiate(
            areaPrefab,
            spawnPos,
            Quaternion.identity
        );
    }
}