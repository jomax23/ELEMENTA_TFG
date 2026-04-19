using UnityEngine;

[CreateAssetMenu(
    fileName = "ElMuro",
    menuName = "Abilities/Earth/El Muro"
)]
public class MuroAbility : AbilityData
{
    [Header("Wall Prefab")]
    [SerializeField] private StoneWall wallPrefab;

    [SerializeField] private float spawnDistance = 1.5f;
    [SerializeField] private float spawnOffset = 0f;

    public override void Activate(GameObject owner)
    {
        Transform t = owner.transform;

        Vector3 spawnPosition =
            t.position +
            t.forward * spawnDistance;

        spawnPosition.y += spawnOffset; // suelo

        Instantiate(
            wallPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}