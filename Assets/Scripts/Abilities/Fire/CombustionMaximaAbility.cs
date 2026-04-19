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
    [SerializeField] private float maxDistance = 12f;

    [Header("Fireball")]
    [SerializeField] private FireballProjectile fireballPrefab;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayers;

    public override void Activate(GameObject owner)
    {
        AbilityCoroutineRunner runner =
            owner.GetComponent<AbilityCoroutineRunner>();

        if (runner == null)
        {
            Debug.LogError("Falta AbilityCoroutineRunner en el Player");
            return;
        }

        runner.RunCoroutine(Execute(owner));
    }

    private IEnumerator Execute(GameObject owner)
    {
        PlayerMovement movement = owner.GetComponent<PlayerMovement>();
        if (movement == null)
            yield break;

        int directionX = movement.FacingDirection;
        Vector3 dir = Vector3.right * directionX;

        Transform spawnPoint = owner.transform.Find("ProjectileSpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogError("No existe ProjectileSpawnPoint");
            yield break;
        }

        // =========================
        // 🔎 Detectar obstáculo
        // =========================
        float beamDistance = maxDistance;
        Vector3 endPoint = spawnPoint.position + dir * maxDistance;

        RaycastHit hit;

        if (Physics.Raycast(
                spawnPoint.position,
                dir,
                out hit,
                maxDistance,
                obstacleLayers
            ))
        {
            endPoint = hit.point;
        }
        
        
        // =========================
        // Mostrar haz de aire
        // =========================
        /*
        GameObject airBeam = Instantiate(
            airBeamPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        airBeam.transform.up = dir;
        float halfLength = airBeam.transform.localScale.y;
        airBeam.transform.position += dir * halfLength;
        */
        GameObject beamObj = Instantiate(
            airBeamPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        CombustionBeam beam = beamObj.GetComponent<CombustionBeam>();

        beam.Initialize(
            spawnPoint,
            dir,
            maxDistance,
            obstacleLayers,
            airBeamDuration
        );

        yield return new WaitForSeconds(airBeamDuration);

        Destroy(beamObj);
        
        // Crear bola de fuego
        // =========================
        FireballProjectile fireball = Instantiate(
            fireballPrefab,
            spawnPoint.position,
            Quaternion.Euler(0f, 0f, 90 * directionX)
        );

        fireball.Initialize(directionX);
    }
}