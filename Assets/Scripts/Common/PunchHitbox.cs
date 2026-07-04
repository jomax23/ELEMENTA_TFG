using UnityEngine;
using System.Collections.Generic;

// Manages the active window and collision detection for melee punches.
// Uses a HashSet to ensure each target is only hit once per punch activation,
// preventing multi-hits if the enemy stays inside the trigger zone.
public class PunchHitbox : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float damage = 10f;

    private bool isActive;
    private readonly HashSet<IAbilityTarget> hitTargets = new();

    // Called by animation events to turn the hitbox on/off.
    // Clears the registry when activated so the next punch can hit the same enemies again.
    public void SetActive(bool value)
    {
        isActive = value;
        if (value) hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        
        // Prevent self-damage if the hitbox clips the player's own model
        if (other.transform.root == transform.root) return;

        IAbilityTarget target = other.GetComponentInParent<IAbilityTarget>();
        if (target == null) return;

        // HashSet.Add() returns false if the item is already in the set.
        // This elegantly prevents hitting the same target multiple times in one swing.
        if (!hitTargets.Add(target)) return;

        target.ApplyDamage(damage, DamageType.Punch);
    }
}