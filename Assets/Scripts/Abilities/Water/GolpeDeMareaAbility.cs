using UnityEngine;

[CreateAssetMenu(
    fileName = "GolpeDeMarea",
    menuName = "Abilities/Water/Golpe de Marea"
)]
public class GolpeDeMareaAbility : AbilityData
{
    [Header("Wave Prefab")]
    [SerializeField] private WaterWaveProjectile wavePrefab;
    [SerializeField] private float spawnOffset = 1.5f;

    public override void Activate(GameObject owner)
    {
        if (wavePrefab == null)
        {
            Debug.LogError($"[{nameof(GolpeDeMareaAbility)}] wavePrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(GolpeDeMareaAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        int directionX = user.FacingDirection;
        Vector3 spawnPos = owner.transform.position + Vector3.right * directionX * spawnOffset;
        Quaternion rotation = Quaternion.Euler(0f, 90 * directionX, 0f);

        WaterWaveProjectile wave = Instantiate(wavePrefab, spawnPos, rotation);
        wave.Initialize(directionX, user.TargetLayers);
    }
}