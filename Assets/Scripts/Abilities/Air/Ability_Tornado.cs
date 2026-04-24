
using UnityEngine;
using System.Collections;


// ──────────────────────────────────────────────────────────────
// Tornado
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "Tornado", menuName = "Abilities/Air/Tornado")]
public class Ability_Tornado : AbilityData
{
    [Header("Tornado")]
    [SerializeField] private TornadoArea tornadoPrefab;
    [SerializeField] private float       spawnOffsetX = 1.5f;
    [SerializeField] private float       spawnOffsetY = 0f;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        if (tornadoPrefab == null)
        {
            Debug.LogError($"[{nameof(Ability_Tornado)}] tornadoPrefab no asignado.", this);
            return;
        }

        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_Tornado)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        int     dirX     = user.FacingDirection;
        Vector3 spawnPos = owner.transform.position
                           + Vector3.right * dirX * spawnOffsetX
                           + Vector3.up    * spawnOffsetY;

        // El tornado invierte proyectiles — no tiene stats de daño que escalar por efficiency.
        Instantiate(tornadoPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
    }
}