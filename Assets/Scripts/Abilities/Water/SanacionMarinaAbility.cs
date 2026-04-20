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
    [SerializeField] private float duration = 3f;

    [Header("VFX - Heal")]
    [SerializeField] private GameObject healVfxPrefab;
    [SerializeField] private Vector3 healVfxOffset = Vector3.zero;

    [Header("VFX - Dome")]
    [SerializeField] private GameObject domeVfxPrefab;
    [SerializeField] private Vector3 domeVfxOffset = Vector3.zero;

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

        user.RunCoroutine(HealWithVFX(owner.transform, health));
    }

    private IEnumerator HealWithVFX(Transform owner, Health health)
    {
        GameObject healVfxInstance = null;
        GameObject domeVfxInstance = null;

        if (healVfxPrefab != null)
        {
            healVfxInstance = Instantiate(healVfxPrefab, owner);
            healVfxInstance.transform.localPosition = healVfxOffset;
        }

        if (domeVfxPrefab != null)
        {
            domeVfxInstance = Instantiate(domeVfxPrefab, owner);
            domeVfxInstance.transform.localPosition = domeVfxOffset;
        }

        float healed = 0f;
        float healPerSecond = totalHeal / duration;

        while (healed < totalHeal)
        {
            float amount = healPerSecond * Time.deltaTime;
            health.Heal(amount);
            healed += amount;

            yield return null;
        }

        if (healVfxInstance != null)
            Destroy(healVfxInstance);

        if (domeVfxInstance != null)
            Destroy(domeVfxInstance);
    }
}