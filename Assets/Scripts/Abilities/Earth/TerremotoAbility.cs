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
        Vector3 spawnPos = owner.transform.position;
        spawnPos.y += spawnOffset;

        Instantiate(areaPrefab, spawnPos, Quaternion.identity);
    }
}