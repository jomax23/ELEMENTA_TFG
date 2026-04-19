using UnityEngine;

[CreateAssetMenu(
    fileName = "ExpansionHelada",
    menuName = "Abilities/Water/Expansión Helada"
)]
public class ExpansionHeladaAbility : AbilityData
{
    [Header("Prefab")]
    [SerializeField] private ExpansionHeladaArea areaPrefab;
    [SerializeField] private float spawnDistance;
    [SerializeField] private float spawnOffset = 0f;

    public override void Activate(GameObject owner)
    {
        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
            return;
        
        Transform t = owner.transform;

        Vector3 spawnPosition =
            t.position +
            t.forward * spawnDistance;

        spawnPosition.y += spawnOffset; // suelo
        
        int directionX = movement.FacingDirection;

        ExpansionHeladaArea area = Instantiate(
            areaPrefab,
            spawnPosition,
            Quaternion.identity
        );

        area.Initialize(directionX);
    }
}