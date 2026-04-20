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
        if (areaPrefab == null)
        {
            Debug.LogError($"[{nameof(ExpansionHeladaAbility)}] areaPrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(ExpansionHeladaAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        int directionX = user.FacingDirection;

        Vector3 spawnPos = owner.transform.position
                           + Vector3.right * directionX * spawnDistance
                           + Vector3.up    * spawnOffset;

        ExpansionHeladaArea area = Instantiate(areaPrefab, spawnPos, Quaternion.identity);
        area.Initialize(directionX, user.TargetLayers);
    }
}