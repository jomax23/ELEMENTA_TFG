using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "SanacionMarina",
    menuName = "Abilities/Water/Sanación Marina"
)]
public class SanacionMarinaAbility : AbilityData
{
    [Header("Healing")]
    [SerializeField] private float totalHeal = 30f;
    [SerializeField] private float duration  = 3f;

    [Header("VFX - Heal")]
    [SerializeField] private GameObject healVfxPrefab;
    [SerializeField] private Vector3    healVfxOffset = Vector3.zero;

    [Header("VFX - Dome")]
    [SerializeField] private GameObject domeVfxPrefab;
    [SerializeField] private Vector3    domeVfxOffset = Vector3.zero;

    // ── Estado runtime ─────────────────────────────────────────────────────────
    private bool       isCancelled;
    private GameObject activeHealVfx;
    private GameObject activeDomeVfx;

    // ─────────────────────────────────────────────────────────────────────────

    public override void Activate(GameObject owner)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(SanacionMarinaAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        Health health = owner.GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError($"[{nameof(SanacionMarinaAbility)}] Health no encontrado en {owner.name}.", owner);
            return;
        }

        isCancelled  = false;
        activeHealVfx = null;
        activeDomeVfx = null;

        user.RunCoroutine(HealWithVFX(owner.transform, health));
    }

    /// <summary>
    /// Detiene la curación y destruye los VFX activos inmediatamente.
    /// </summary>
    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
        DestroyVFX();
    }

    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator HealWithVFX(Transform owner, Health health)
    {
        // Spawn VFX
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

        float healed        = 0f;
        float healPerSecond = totalHeal / duration;

        while (healed < totalHeal)
        {
            // Abortar si fuimos interrumpidos (Cancel ya habrá destruido los VFX).
            if (isCancelled) yield break;

            float amount = healPerSecond * Time.deltaTime;
            health.Heal(amount);
            healed += amount;

            yield return null;
        }

        // Curación completada con normalidad → destruir VFX.
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