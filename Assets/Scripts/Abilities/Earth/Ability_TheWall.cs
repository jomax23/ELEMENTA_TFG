using UnityEngine;

// ──────────────────────────────────────────────────────────────
// Muro de Tierra (El Muro)
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "ElMuro", menuName = "Abilities/Earth/El Muro")]
public class Ability_TheWall : AbilityData
{
    [Header("Wall Prefab")]
    [SerializeField] private StoneWall wallPrefab;
    [SerializeField] private float     spawnDistance = 1.5f;
    [SerializeField] private float     spawnOffset   = 0f;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        // El muro no tiene stats escalables por efficiency (es una estructura de control
        // de espacio, no hace daño directamente). Sí podría escalarse su lifetime,
        // pero lo dejamos como decisión de diseño futura vía override en subclase.
        Transform t = owner.transform;

        Vector3 spawnPosition = t.position + t.forward * spawnDistance;
        spawnPosition.y += spawnOffset;

        Instantiate(wallPrefab, spawnPosition, Quaternion.identity);
    }
}