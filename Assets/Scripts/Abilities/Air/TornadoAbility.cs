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

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(TornadoAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        int directionX = user.FacingDirection;

        Vector3 spawnPos = owner.transform.position
                           + Vector3.right * directionX * spawnOffsetX
                           + Vector3.up    * spawnOffsetY;

        Instantiate(tornadoPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
    }
}