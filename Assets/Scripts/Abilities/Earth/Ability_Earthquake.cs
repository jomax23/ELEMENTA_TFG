using UnityEngine;


// ──────────────────────────────────────────────────────────────
// Terremoto
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "Terremoto", menuName = "Abilities/Earth/Terremoto")]
public class Ability_Earthquake : AbilityData
{
    [Header("Area Prefab")]
    [SerializeField] private EarthquakeArea areaPrefab;
    [SerializeField] private float          spawnOffset = 0f;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (areaPrefab == null)
        {
            Debug.LogError($"[{nameof(Ability_Earthquake)}] areaPrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_Earthquake)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Vector3 spawnPos = owner.transform.position;
        spawnPos.y += spawnOffset;

        EarthquakeArea area = Instantiate(areaPrefab, spawnPos, Quaternion.identity);
        area.Initialize(user.TargetLayers, efficiency);
    }
}