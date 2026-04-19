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

        Quaternion rotation = Quaternion.Euler(0f, 90 * directionX, 0f);
        WaterWaveProjectile wave = Instantiate(
            wavePrefab,
            spawnPosition,
            rotation
        );

        wave.Initialize(directionX);
    }
}