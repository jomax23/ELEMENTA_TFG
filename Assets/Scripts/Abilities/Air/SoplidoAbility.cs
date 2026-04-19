/*
 using UnityEngine;

[CreateAssetMenu(
    fileName = "Soplido",
    menuName = "Abilities/Air/Soplido"
)]
public class SoplidoAbility : AbilityData
{
    [Header("Soplido Prefab")]
    [SerializeField] private SoplidoArea soplidoPrefab;
    [SerializeField] private float spawnOffset = 1.2f;

    public override void Activate(GameObject owner)
    {
        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            Debug.LogError("El Player no tiene PlayerMovement");
            return;
        }

        int directionX = movement.FacingDirection;

        Vector3 spawnPosition =
            owner.transform.position +
            Vector3.right * directionX * spawnOffset;

        SoplidoArea soplido = Instantiate(
            soplidoPrefab,
            spawnPosition,
            Quaternion.identity
        );

        soplido.Initialize(directionX);
    }
}
*/

using UnityEngine;

[CreateAssetMenu(
    fileName = "Soplido",
    menuName = "Abilities/Air/Soplido"
)]
public class SoplidoAbility : AbilityData
{
    [Header("Soplido")]
    [SerializeField] private SoplidoArea soplidoPrefab;
    [SerializeField] private float spawnOffset = 1.2f;

    public override void Activate(GameObject owner)
    {
        if (soplidoPrefab == null)
        {
            Debug.LogError($"[{nameof(SoplidoAbility)}] soplidoPrefab no asignado.", this);
            return;
        }

        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            Debug.LogError($"[{nameof(SoplidoAbility)}] PlayerMovement no encontrado en {owner.name}.", owner);
            return;
        }

        int directionX = movement.FacingDirection;

        Vector3 spawnPos = owner.transform.position + Vector3.right * directionX * spawnOffset;

        SoplidoArea soplido = Instantiate(soplidoPrefab, spawnPos, Quaternion.identity);
        soplido.Initialize(directionX);
    }
}