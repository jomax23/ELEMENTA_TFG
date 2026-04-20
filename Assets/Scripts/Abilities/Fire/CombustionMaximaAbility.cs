using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "CombustionMaxima",
    menuName = "Abilities/Fire/Combustión Máxima"
)]
public class CombustionMaximaAbility : AbilityData
{
    [Header("Air Beam Phase")]
    [SerializeField] private GameObject airBeamPrefab;
    [SerializeField] private float airBeamDuration = 1f;
    [SerializeField] private float maxDistance     = 12f;

    [Header("Fireball")]
    [SerializeField] private FireballProjectile fireballPrefab;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayers;

    public override void Activate(GameObject owner)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(CombustionMaximaAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = owner.transform.Find("ProjectileSpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(CombustionMaximaAbility)}] ProjectileSpawnPoint no encontrado en {owner.name}.", owner);
            return;
        }

        user.RunCoroutine(Execute(user, spawnPoint));
    }

    private IEnumerator Execute(IAbilityUser user, Transform spawnPoint)
    {
        int     directionX = user.FacingDirection;
        Vector3 dir        = Vector3.right * directionX;

        // ── Mostrar haz de aire ──────────────────────────────────────────────
        GameObject beamObj = Instantiate(airBeamPrefab, Vector3.zero, Quaternion.identity);

        CombustionBeam beam = beamObj.GetComponent<CombustionBeam>();
        beam.Initialize(spawnPoint, dir, maxDistance, obstacleLayers, airBeamDuration);

        yield return new WaitForSeconds(airBeamDuration);

        Destroy(beamObj);

        // ── Crear bola de fuego con la capa correcta ─────────────────────────
        FireballProjectile fireball = Instantiate(
            fireballPrefab,
            spawnPoint.position,
            Quaternion.Euler(0f, 0f, 90 * directionX)
        );

        fireball.Initialize(directionX, user.TargetLayers);
    }
}