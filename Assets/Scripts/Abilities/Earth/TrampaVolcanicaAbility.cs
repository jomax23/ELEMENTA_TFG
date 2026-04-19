using UnityEngine;

[CreateAssetMenu(
    fileName = "TrampaVolcanica",
    menuName = "Abilities/Earth/Trampa Volcánica"
)]
public class TrampaVolcanicaAbility : AbilityData
{
    [Header("Trap Prefab")]
    [SerializeField] private TrampaVolcanicaArea trapPrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnOffsetX = 1.5f;

    public override void Activate(GameObject owner)
    {
        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
            return;

        Vector3 spawnPos = owner.transform.position;
        spawnPos.x += movement.FacingDirection * spawnOffsetX;

        Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);
        
        Instantiate(
            trapPrefab,
            spawnPos,
            rotation
        );
    }
}