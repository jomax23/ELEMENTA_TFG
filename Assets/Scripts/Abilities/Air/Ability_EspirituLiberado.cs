using UnityEngine;
using System.Collections;

// ──────────────────────────────────────────────────────────────
// Espíritu Liberado
// ──────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "EspirituLiberado", menuName = "Abilities/Air/Espíritu Liberado")]
public class Ability_EspirituLiberado : AbilityData
{
    [Header("Spirit Mode")]
    [SerializeField] private float duration = 4f;

    private bool isCancelled;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        if (user == null)
        {
            Debug.LogError($"[{nameof(Ability_EspirituLiberado)}] IAbilityUser no encontrado en {owner.name}.", owner);
            return;
        }

        SceneEffectsController fx = SceneEffectsController.Instance;
        if (fx == null)
        {
            Debug.LogError($"[{nameof(Ability_EspirituLiberado)}] SceneEffectsController.Instance es null.");
            return;
        }

        isCancelled = false;
        float scaledDuration = duration * efficiency;
        user.RunCoroutine(SpiritRoutine(fx, scaledDuration));
    }

    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
        SceneEffectsController.Instance?.DisableSpiritMode();
    }

    private System.Collections.IEnumerator SpiritRoutine(SceneEffectsController fx, float scaledDuration)
    {
        fx.EnableSpiritMode();

        yield return new WaitForSeconds(scaledDuration);

        if (!isCancelled)
            fx.DisableSpiritMode();
    }
}