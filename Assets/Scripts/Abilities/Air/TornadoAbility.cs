using UnityEngine;

[CreateAssetMenu(
    fileName = "Tornado",
    menuName = "Abilities/Air/Tornado"
)]
public class TornadoAbility : AbilityData
{
    [Header("Tornado")]
    [SerializeField] private TornadoArea tornadoPrefab;
    [SerializeField] private float spawnOffsetX = 1.5f;
    [SerializeField] private float spawnOffsetY = 0f;

    public override void Activate(GameObject owner)
    {
        if (tornadoPrefab == null)
        {
            Debug.LogError($"[{nameof(TornadoAbility)}] tornadoPrefab no asignado.", this);
            return;
        }

        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            Debug.LogError($"[{nameof(TornadoAbility)}] PlayerMovement no encontrado en {owner.name}.", owner);
            return;
        }

        int directionX = movement.FacingDirection;

        Vector3 spawnPos = owner.transform.position
            + Vector3.right * directionX * spawnOffsetX
            + Vector3.up    * spawnOffsetY;

        Instantiate(tornadoPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
    }
}