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
        if (trapPrefab == null)
        {
            Debug.LogError($"[{nameof(TrampaVolcanicaAbility)}] trapPrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(TrampaVolcanicaAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Vector3 spawnPos  = owner.transform.position;
        spawnPos.x       += user.FacingDirection * spawnOffsetX;

        TrampaVolcanicaArea trap = Instantiate(trapPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
        trap.Initialize(user.TargetLayers);
    }
}