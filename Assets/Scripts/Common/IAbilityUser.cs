using System.Collections;
using UnityEngine;

/// <summary>
/// Interface for any entity that can cast/execute abilities 
/// (e.g., PlayerMovement, EnemyAI). Provides context to the ability system.
/// </summary>
public interface IAbilityUser
{
    /// <summary>+1 = facing right, -1 = facing left. Used to flip projectiles/areas.</summary>
    int FacingDirection { get; }

    /// <summary>
    /// The layer mask of valid targets for this caster. 
    /// Injected into spawned projectiles/areas to ensure they only hit the correct faction.
    /// </summary>
    LayerMask TargetLayers { get; }

    /// <summary>Runs a coroutine in the context of the caster's MonoBehaviour.</summary>
    void RunCoroutine(IEnumerator routine);
}