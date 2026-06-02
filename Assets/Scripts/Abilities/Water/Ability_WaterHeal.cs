using UnityEngine;
using System.Collections;

/// <summary>
/// Water ability that heals the user over time while displaying VFX.
/// The total healing amount is scaled by elemental affinity efficiency.
/// </summary>
[CreateAssetMenu(fileName = "SanacionMarina", menuName = "Abilities/Water/Sanación Marina")]
public class Ability_WaterHeal : AbilityData
{
    [Header("Healing")]
    [SerializeField] private float totalHeal = 30f;
    [SerializeField] private float duration = 3f;

    [Header("VFX - Heal")]
    [SerializeField] private GameObject healVfxPrefab;
    [SerializeField] private Vector3 healVfxOffset = Vector3.zero;

    [Header("VFX - Dome")]
    [SerializeField] private GameObject domeVfxPrefab;
    [SerializeField] private Vector3 domeVfxOffset = Vector3.zero;

    private bool isCancelled;
    private GameObject activeHealVfx;
    private GameObject activeDomeVfx;

    public override string GetFormattedDescription(float efficiency)
    {
        float scaledHeal = totalHeal * efficiency;
        float scaledDuration = duration * efficiency;
        float healPerSecond = scaledHeal / scaledDuration;

        return string.Format(descriptionTemplate, scaledHeal, scaledDuration, healPerSecond);
    }
    
    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_WaterHeal)}] IAbilityUser component not found on {owner.name}.", owner);
            return;
        }

        Health health = owner.GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError($"[{nameof(Ability_WaterHeal)}] Health component not found on {owner.name}.", owner);
            return;
        }

        isCancelled = false;
        activeHealVfx = null;
        activeDomeVfx = null;

        float scaledHeal = totalHeal * efficiency;
        float scaledDuration = duration * efficiency;
        
        user.RunCoroutine(HealWithVFX(owner.transform, health, scaledHeal, scaledDuration));
    }

    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
        DestroyVFX();
    }

    private IEnumerator HealWithVFX(Transform owner, Health health, float scaledTotalHeal, float scaledDuration)
    {
        if (healVfxPrefab != null)
        {
            activeHealVfx = Instantiate(healVfxPrefab, owner);
            activeHealVfx.transform.localPosition = healVfxOffset;
        }

        if (domeVfxPrefab != null)
        {
            activeDomeVfx = Instantiate(domeVfxPrefab, owner);
            activeDomeVfx.transform.localPosition = domeVfxOffset;
        }

        float healed = 0f;
        float healPerSecond = scaledTotalHeal / scaledDuration;

        while (healed < scaledTotalHeal)
        {
            if (isCancelled) yield break;

            float amount = healPerSecond * Time.deltaTime;
            health.Heal(amount);
            healed += amount;

            yield return null;
        }

        DestroyVFX();
    }

    private void DestroyVFX()
    {
        if (activeHealVfx != null)
        {
            Object.Destroy(activeHealVfx);
            activeHealVfx = null;
        }

        if (activeDomeVfx != null)
        {
            Object.Destroy(activeDomeVfx);
            activeDomeVfx = null;
        }
    }
}