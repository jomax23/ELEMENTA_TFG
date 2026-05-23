using UnityEngine;
using System.Collections;


// ──────────────────────────────────────────────────────────────
// Combustión Máxima
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "CombustionMaxima", menuName = "Abilities/Fire/Combustión Máxima")]
public class Ability_MaxCombustion : AbilityData
{
    [Header("Air Beam Phase")]
    [SerializeField] private GameObject airBeamPrefab;
    [SerializeField] private float airBeamDuration = 1f;
    [SerializeField] private float maxDistance     = 12f;

    [Header("Fireball")]
    [SerializeField] private FireballProjectile fireballPrefab;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayers;

    private bool       isCancelled;
    private GameObject activeBeam;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_MaxCombustion)}] IAbilityUser no encontrado.", owner);
            return;
        }

        Transform spawnPoint = FindDeep(owner.transform, "HeadSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(Ability_MaxCombustion)}] HeadSpawn no encontrado.", owner);
            return;
        }

        isCancelled = false;
        activeBeam  = null;

        user.RunCoroutine(Execute(user, spawnPoint, efficiency));
    }

    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
        // Destrucción inmediata por si la coroutine sigue viva un frame más
        if (activeBeam != null) Object.Destroy(activeBeam);
        activeBeam = null;
    }

    private IEnumerator Execute(IAbilityUser user, Transform spawnPoint, float efficiency)
    {
        int     dirX = user.FacingDirection;
        Vector3 dir  = Vector3.right * dirX;

        activeBeam = Instantiate(airBeamPrefab, spawnPoint.position, Quaternion.identity);
        CombustionBeam beam = activeBeam.GetComponent<CombustionBeam>();
        beam.Initialize(spawnPoint, dir, maxDistance, obstacleLayers, airBeamDuration);

        yield return new WaitForSeconds(airBeamDuration);

        // Si hubo stun/interrupción, abortamos antes de lanzar la fireball
        if (isCancelled) yield break;

        // Limpieza extra (el haz ya se autodestruirá, pero liberamos referencia)
        if (activeBeam != null) Object.Destroy(activeBeam);
        activeBeam = null;

        FireballProjectile fireball = Instantiate(
            fireballPrefab,
            spawnPoint.position,
            Quaternion.Euler(0f, 0f, 90 * dirX)
        );
        fireball.Initialize(dirX, user.TargetLayers, efficiency);
    }
}