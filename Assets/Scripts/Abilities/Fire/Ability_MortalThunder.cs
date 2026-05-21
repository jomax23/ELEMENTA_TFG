using UnityEngine;

[CreateAssetMenu(fileName = "RayoMortal", menuName = "Abilities/Fire/Rayo Mortal")]
public class Ability_MortalThunder : AbilityData
{
    [Header("Beam Prefab")]
    [SerializeField] private RayoMortalProjectile beamPrefab;

    [Header("Target Search")]
    [Tooltip("Distancia máxima de búsqueda del enemigo.")]
    [SerializeField] private float maxSearchDistance = 12f;

    [Tooltip("Radio del SphereCast para encontrar al enemigo.")]
    [SerializeField] private float searchRadius = 0.5f;

    [Tooltip("Nombre del Transform DENTRO DEL ENEMIGO al que apuntará el rayo.\n" +
             "Ejemplo: 'mixamorig1:Spine2'. Si no se encuentra, se usa el root del enemigo.")]
    [SerializeField] private string targetTransformName = "mixamorig1:Spine2";

    // ─────────────────────────────────────────────────────────────────────────

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_MortalThunder)}] IAbilityUser no encontrado.", owner);
            return;
        }

        Transform spawnPoint = FindDeep(owner.transform, "RightHandSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(Ability_MortalThunder)}] RightHandSpawn no encontrado.", owner);
            return;
        }

        RayoMortalProjectile beam = Instantiate(beamPrefab, spawnPoint.position, Quaternion.identity);

        Transform targetPoint = FindEnemyTargetTransform(spawnPoint.position, user);

        if (targetPoint != null)
        {
            beam.InitializeToTarget(spawnPoint, targetPoint, user.TargetLayers, efficiency);
        }
        else
        {
            beam.Initialize(user.FacingDirection, user.TargetLayers, efficiency);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BÚSQUEDA DEL ENEMIGO
    // ─────────────────────────────────────────────────────────────────────────

    private Transform FindEnemyTargetTransform(Vector3 origin, IAbilityUser user)
    {
        Vector3 direction = Vector3.right * user.FacingDirection;

        bool found = Physics.SphereCast(
            origin,
            searchRadius,
            direction,
            out RaycastHit hit,
            maxSearchDistance,
            user.TargetLayers,
            QueryTriggerInteraction.Collide);

        if (!found)
        {
            Debug.Log($"[{nameof(Ability_MortalThunder)}] Sin enemigo en rango. " +
                      $"(layers={user.TargetLayers.value}) Usando beam estático.");
            return null;
        }

        // ── Encontrar el root del enemigo subiendo por la jerarquía ──────────
        // Buscamos el primer ancestro (o el propio objeto) cuya layer esté
        // incluida en targetLayers. Esto garantiza que estamos dentro del
        // enemigo y no del jugador ni de otro objeto colateral.
        Transform enemyRoot = FindRootInLayer(hit.transform, user.TargetLayers);

        if (enemyRoot == null)
        {
            Debug.LogWarning($"[{nameof(Ability_MortalThunder)}] " +
                             $"El hit '{hit.collider.name}' no tiene ningún ancestro " +
                             $"en targetLayers ({user.TargetLayers.value}). " +
                             $"Usando transform del hit.");
            enemyRoot = hit.transform;
        }

        Debug.Log($"[{nameof(Ability_MortalThunder)}] Enemigo encontrado: '{enemyRoot.name}'.");

        // ── Buscar el hueso dentro del enemigo ───────────────────────────────
        if (!string.IsNullOrEmpty(targetTransformName))
        {
            Transform bone = FindDeep(enemyRoot, targetTransformName);

            if (bone != null)
            {
                Debug.Log($"[{nameof(Ability_MortalThunder)}] Hueso '{targetTransformName}' " +
                          $"encontrado en '{enemyRoot.name}'.");
                return bone;
            }

            Debug.LogWarning($"[{nameof(Ability_MortalThunder)}] " +
                             $"Hueso '{targetTransformName}' no encontrado en '{enemyRoot.name}'. " +
                             $"Usando root del enemigo.");
        }

        return enemyRoot;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPER: subir la jerarquía buscando el primer nodo en la layer correcta
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sube por la jerarquía desde <paramref name="start"/> y devuelve el primer
    /// Transform cuyo GameObject.layer esté incluido en <paramref name="layerMask"/>.
    ///
    /// Esto resuelve el caso en que el SphereCast golpea un hueso hijo del enemigo
    /// (que puede estar en layer Default) — subimos hasta encontrar el GameObject
    /// raíz que sí tiene la layer "Enemy".
    /// </summary>
    private static Transform FindRootInLayer(Transform start, LayerMask layerMask)
    {
        Transform current = start;

        while (current != null)
        {
            if ((layerMask.value & (1 << current.gameObject.layer)) != 0)
                return current;

            current = current.parent;
        }

        return null;
    }
}