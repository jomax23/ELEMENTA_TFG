using UnityEngine;

/// <summary>
/// Fire ability that casts a targeted lightning beam.
/// Uses a SphereCast to find an enemy, then attempts to lock onto a specific bone 
/// (e.g., spine) for precise visual targeting.
/// </summary>
[CreateAssetMenu(fileName = "RayoMortal", menuName = "Abilities/Fire/Rayo Mortal")]
public class Ability_MortalThunder : AbilityData
{
    [Header("Beam Prefab")]
    [SerializeField] private RayoMortalProjectile beamPrefab;

    [Header("Target Search")]
    [Tooltip("Maximum distance to search for an enemy.")]
    [SerializeField] private float maxSearchDistance = 12f;

    [Tooltip("Radius of the SphereCast to find the enemy.")]
    [SerializeField] private float searchRadius = 0.5f;

    [Tooltip("Name of the Transform INSIDE the enemy to target (e.g., 'mixamorig1:Spine2'). Falls back to root if not found.")]
    [SerializeField] private string targetTransformName = "mixamorig1:Spine2";

    [Header("Stats")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float stunDuration = 2f;
    
    public override string GetFormattedDescription(float efficiency)
    {
        return string.Format(descriptionTemplate, damage*efficiency, stunDuration*efficiency);

    }
    
    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_MortalThunder)}] IAbilityUser not found on {owner.name}.", owner);
        }

        Transform spawnPoint = FindDeep(owner.transform, "RightHandSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(Ability_MortalThunder)}] RightHandSpawn not found on {owner.name}.", owner);
            return;
        }

        RayoMortalProjectile beam = Instantiate(beamPrefab, spawnPoint.position, Quaternion.identity);
        Transform targetPoint = FindEnemyTargetTransform(spawnPoint.position, user);

        if (targetPoint != null)
        {
            beam.InitializeToTarget(spawnPoint, targetPoint, user.TargetLayers, damage * efficiency, stunDuration * efficiency);
        }
        else
        {
            beam.Initialize(user.FacingDirection, user.TargetLayers, damage * efficiency, stunDuration * efficiency);
        }
    }

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
            QueryTriggerInteraction.Collide
        );

        if (!found)
        {
            Debug.Log($"[{nameof(Ability_MortalThunder)}] No enemy in range. Using static beam.");
            return null;
        }

        // Find the root of the enemy by climbing up the hierarchy to ensure we hit the correct layer
        Transform enemyRoot = FindRootInLayer(hit.transform, user.TargetLayers);

        if (enemyRoot == null)
        {
            Debug.LogWarning($"[{nameof(Ability_MortalThunder)}] Hit '{hit.collider.name}' has no ancestor in targetLayers. Using hit transform.");
            enemyRoot = hit.transform;
        }

        // Attempt to find the specific bone within the enemy
        if (!string.IsNullOrEmpty(targetTransformName))
        {
            Transform bone = FindDeep(enemyRoot, targetTransformName);
            if (bone != null)
            {
                return bone;
            }
            Debug.LogWarning($"[{nameof(Ability_MortalThunder)}] Bone '{targetTransformName}' not found in '{enemyRoot.name}'. Using root.");
        }

        return enemyRoot;
    }

    /// <summary>
    /// Climbs up the hierarchy to find the first Transform whose GameObject.layer is in the mask.
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