using UnityEngine;
using System.Collections.Generic;

public class PunchHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    private bool isActive = false;
    private HashSet<IAbilityTarget> hitTargets = new();

    public void SetActive(bool value)
    {
        isActive = value;

        if (value)
            hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Evitar auto-hit
        if (other.transform.root == transform.root) return;

        IAbilityTarget target = other.GetComponentInParent<IAbilityTarget>();
        if (target == null) return;

        if (hitTargets.Contains(target)) return;

        hitTargets.Add(target);

        target.ApplyDamage(damage, DamageType.Punch);
    }
}