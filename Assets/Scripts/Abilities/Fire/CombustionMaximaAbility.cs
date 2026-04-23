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

    // ── Estado runtime ─────────────────────────────────────────────────────────
    // ScriptableObject compartido: solo un jugador lo usa a la vez en un juego 1v1,
    // por lo que guardar estado de instancia aquí es seguro.
    private bool       isCancelled;
    private GameObject activeBeam;

    // ─────────────────────────────────────────────────────────────────────────

    public override void Activate(GameObject owner)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(CombustionMaximaAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = AbilityData.FindDeep(owner.transform, "HeadSpawn");

        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(CombustionMaximaAbility)}] ProjectileSpawnPoint no encontrado en {owner.name}.", owner);
            return;
        }

        // Reset de la bandera antes de cada uso.
        isCancelled = false;
        activeBeam  = null;

        user.RunCoroutine(Execute(user, spawnPoint));
    }

    /// <summary>
    /// Cancela el haz activo y marca la coroutine interna para que aborte.
    /// Llamado por PlayerAbilities cuando el jugador es interrumpido.
    /// </summary>
    public override void Cancel(GameObject owner)
    {
        isCancelled = true;

        if (activeBeam != null)
        {
            Object.Destroy(activeBeam);
            activeBeam = null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator Execute(IAbilityUser user, Transform spawnPoint)
    {
        int     directionX = user.FacingDirection;
        Vector3 dir        = Vector3.right * directionX;

        // ── Fase 1: mostrar haz de aire ───────────────────────────────────────
        activeBeam = Instantiate(airBeamPrefab, Vector3.zero, Quaternion.identity);

        CombustionBeam beam = activeBeam.GetComponent<CombustionBeam>();
        beam.Initialize(spawnPoint, dir, maxDistance, obstacleLayers, airBeamDuration);

        yield return new WaitForSeconds(airBeamDuration);

        // Si fuimos interrumpidos durante la espera, Cancel() ya destruyó el haz.
        // Abortamos aquí para no disparar el proyectil.
        if (isCancelled) yield break;

        // ── Fase 2: destruir haz y disparar fireball ──────────────────────────
        if (activeBeam != null)
        {
            Object.Destroy(activeBeam);
            activeBeam = null;
        }

        FireballProjectile fireball = Instantiate(
            fireballPrefab,
            spawnPoint.position,
            Quaternion.Euler(0f, 0f, 90 * directionX)
        );

        fireball.Initialize(directionX, user.TargetLayers);
    }
}