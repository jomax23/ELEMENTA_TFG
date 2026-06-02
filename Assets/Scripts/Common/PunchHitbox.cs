using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the active window and collision detection for melee punches.
/// Uses a HashSet to ensure each target is only hit once per punch activation.
/// </summary>
public class PunchHitbox : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float damage = 10f;

    private bool isActive;
    private readonly HashSet<IAbilityTarget> hitTargets = new();

    /// <summary>
    /// Enables or disables the hitbox logic. 
    /// Clears the hit registry when activated to allow fresh hits.
    /// </summary>
    public void SetActive(bool value)
    {
        isActive = value;
        if (value) hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Prevent self-damage
        if (other.transform.root == transform.root) return;

        IAbilityTarget target = other.GetComponentInParent<IAbilityTarget>();
        if (target == null) return;

        // Prevent multiple hits on the same target during a single punch
        if (!hitTargets.Add(target)) return;

        target.ApplyDamage(damage, DamageType.Punch);
    }
}