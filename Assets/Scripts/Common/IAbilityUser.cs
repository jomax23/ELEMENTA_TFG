using System.Collections;
using UnityEngine;

// Implemented by anything that can cast abilities (Player, AI). 
// Provides the ability system with necessary context so ScriptableObjects don't have to guess.
public interface IAbilityUser
{
    // +1 for right, -1 for left. Used to flip projectiles and area effects.
    int FacingDirection { get; }

    // Layer mask of valid targets. Injected into spawned effects so they only hit the correct faction.
    LayerMask TargetLayers { get; }

    // ScriptableObjects can't run coroutines themselves, so they delegate async logic to the caster.
    void RunCoroutine(IEnumerator routine);
}