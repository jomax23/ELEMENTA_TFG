using UnityEngine;

// Fires a targeted lightning beam. 
// Uses a SphereCast to find enemies, then tries to lock onto a specific bone (like the spine) 
// so the beam visually connects to the target's center of mass.
[CreateAssetMenu(fileName = "RayoMortal", menuName = "Abilities/Fire/Rayo Mortal")]
public class Ability_MortalThunder : AbilityData
{
    [Header("Beam Prefab")]
    [SerializeField] private RayoMortalProjectile beamPrefab;

    [Header("Target Search")]
    [SerializeField] private float maxSearchDistance = 12f;
    [SerializeField] private float searchRadius = 0.5f;
    [Tooltip("Name of the Transform INSIDE the enemy to target (e.g., 'mixamorig1:Spine2'). Falls back to root if not found.")]
    [SerializeField] private string targetTransformName = "mixamorig1:Spine2";

    [Header("Stats")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float stunDuration = 2f;

    public override string GetFormattedDescription(float efficiency)
    {
        return string.Format(descriptionTemplate, damage * efficiency, stunDuration * efficiency);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_MortalThunder)}] IAbilityUser not found on {owner.name}.", owner);
            return;
        }

        Transform spawnPoint = FindDeep(owner.transform, "RightHandSpawn");
        if (spawnPoint == null)
        {
            Debug.LogError($"[{nameof(Ability_MortalThunder)}] RightHandSpawn not found on {owner.name}.", owner);
            return;
        }

        RayoMortalProjectile beam = Instantiate(beamPrefab, spawnPoint.position, Quaternion.identity);
        
        // Try to find a specific bone on an enemy in range
        Transform targetPoint = FindEnemyTargetTransform(spawnPoint.position, user);

        if (targetPoint != null)
        {
            // Aim directly at the found bone/target
            beam.InitializeToTarget(spawnPoint, targetPoint, user.TargetLayers, damage * efficiency, stunDuration * efficiency);
        }
        else
        {
            // Fallback: fire straight ahead if no target is found
            beam.Initialize(user.FacingDirection, user.TargetLayers, damage * efficiency, stunDuration * efficiency);
        }
    }

    // Uses a SphereCast to find an enemy, then digs into their hierarchy to find a specific bone
    private Transform FindEnemyTargetTransform(Vector3 origin, IAbilityUser user)
    {
        Vector3 direction = Vector3.right * user.FacingDirection;
        
        bool found = Physics.SphereCast(
            origin, searchRadius, direction, out RaycastHit hit, 
            maxSearchDistance, user.TargetLayers, QueryTriggerInteraction.Collide
        );

        if (!found) return null;

        // Climb up the hierarchy to find the actual enemy root (in case we hit a child collider)
        Transform enemyRoot = FindRootInLayer(hit.transform, user.TargetLayers);
        if (enemyRoot == null)
        {
            Debug.LogWarning($"[{nameof(Ability_MortalThunder)}] Hit '{hit.collider.name}' has no ancestor in targetLayers.", this);
            enemyRoot = hit.transform;
        }

        // Attempt to find the specific bone within the enemy for precise visual targeting
        if (!string.IsNullOrEmpty(targetTransformName))
        {
            Transform bone = FindDeep(enemyRoot, targetTransformName);
            if (bone != null) return bone;
        }

        // Fallback to the enemy's root transform
        return enemyRoot;
    }

    // Climbs up the hierarchy to find the first Transform whose GameObject.layer is in the mask
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