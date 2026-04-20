using UnityEngine;

[CreateAssetMenu(
    fileName = "Terremoto",
    menuName = "Abilities/Earth/Terremoto"
)]
public class TerremotoAbility : AbilityData
{
    [Header("Area Prefab")]
    [SerializeField] private EarthquakeArea areaPrefab;
    [SerializeField] private float spawnOffset = 0f;

    public override void Activate(GameObject owner)
    {
        if (areaPrefab == null)
        {
            Debug.LogError($"[{nameof(TerremotoAbility)}] areaPrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(TerremotoAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Vector3 spawnPos  = owner.transform.position;
        spawnPos.y       += spawnOffset;

        EarthquakeArea area = Instantiate(areaPrefab, spawnPos, Quaternion.identity);
        area.Initialize(user.TargetLayers);
    }
}