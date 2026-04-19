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
        AbilityCoroutineRunner runner =
            owner.GetComponent<AbilityCoroutineRunner>();

        if (runner == null)
        {
            Debug.LogError("El Player no tiene AbilityCoroutineRunner");
            return;
        }

        Health health = owner.GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError("El Player no tiene PlayerHealth");
            return;
        }

        runner.RunCoroutine(
            HealWithVFX(owner.transform, health)
        );
    }

    private IEnumerator HealWithVFX(
        Transform owner,
        Health health
    )
    {
        GameObject healVfxInstance = null;
        GameObject domeVfxInstance = null;

        // 🌊 VFX de curación
        if (healVfxPrefab != null)
        {
            healVfxInstance = Instantiate(healVfxPrefab, owner);
            healVfxInstance.transform.localPosition = healVfxOffset;
        }

        // 🫧 VFX de cúpula
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

        // 🧹 Limpiar VFX
        if (healVfxInstance != null)
            Destroy(healVfxInstance);

        if (domeVfxInstance != null)
            Destroy(domeVfxInstance);
    }
}