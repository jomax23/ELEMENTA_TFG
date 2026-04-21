using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "EspirituLiberado",
    menuName = "Abilities/Air/Espíritu Liberado"
)]
public class EspirituLiberadoAbility : AbilityData
{
    [Header("Spirit Mode")]
    [SerializeField] private float duration = 4f;

    // ── Estado runtime ─────────────────────────────────────────────────────────
    private bool isCancelled;

    // ─────────────────────────────────────────────────────────────────────────

    public override void Activate(GameObject owner)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(EspirituLiberadoAbility)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        SceneEffectsController fx = SceneEffectsController.Instance;
        if (fx == null)
        {
            Debug.LogError($"[{nameof(EspirituLiberadoAbility)}] SceneEffectsController.Instance es null.");
            return;
        }

        isCancelled = false;
        user.RunCoroutine(SpiritRoutine(fx));
    }

    /// <summary>
    /// Deshabilita el modo espíritu inmediatamente si hay una interrupción.
    /// </summary>
    public override void Cancel(GameObject owner)
    {
        isCancelled = true;

        SceneEffectsController fx = SceneEffectsController.Instance;
        if (fx != null)
            fx.DisableSpiritMode();
    }

    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator SpiritRoutine(SceneEffectsController fx)
    {
        fx.EnableSpiritMode();

        yield return new WaitForSeconds(duration);

        // Si Cancel() fue llamado durante la espera, ya habrá llamado DisableSpiritMode().
        if (!isCancelled)
            fx.DisableSpiritMode();
    }
}