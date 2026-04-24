using UnityEngine;
using System.Collections;


// ──────────────────────────────────────────────────────────────
// Golpe de Marea
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "GolpeDeMarea", menuName = "Abilities/Water/Golpe de Marea")]
public class Ability_SeaHit : AbilityData
{
    [Header("Wave Prefab")]
    [SerializeField] private WaterWaveProjectile wavePrefab;
    [SerializeField] private float spawnOffset = 1.5f;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (wavePrefab == null)
        {
            Debug.LogError($"[{nameof(Ability_SeaHit)}] wavePrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_SeaHit)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        int       dirX     = user.FacingDirection;
        Vector3   spawnPos = owner.transform.position + Vector3.right * dirX * spawnOffset;
        Quaternion rot     = Quaternion.Euler(0f, 90 * dirX, 0f);

        WaterWaveProjectile wave = Instantiate(wavePrefab, spawnPos, rot);
        wave.Initialize(dirX, user.TargetLayers, efficiency);
    }
}