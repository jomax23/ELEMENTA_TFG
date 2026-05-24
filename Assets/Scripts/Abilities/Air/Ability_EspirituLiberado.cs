using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "EspirituLiberado", menuName = "Abilities/Air/Espíritu Liberado")]
public class Ability_EspirituLiberado : AbilityData
{
    [Header("Spirit Mode")]
    [SerializeField] private float duration = 4f;

    private bool isCancelled;

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        IAbilityTarget target = owner.GetComponent<IAbilityTarget>(); // ← NUEVO
        
        if (user == null || target == null)
        {
            Debug.LogError($"[{nameof(Ability_EspirituLiberado)}] Componentes requeridos no encontrados.", owner);
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
        
        target.IsIntangible = true; // ← Activa intangibilidad
        fx.EnableSpiritMode();
        
        user.RunCoroutine(SpiritRoutine(fx, target, scaledDuration));
    }

    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
        var target = owner.GetComponent<IAbilityTarget>();
        if (target != null) target.IsIntangible = false; // ← Limpieza segura
        SceneEffectsController.Instance?.DisableSpiritMode();
    }

    private IEnumerator SpiritRoutine(SceneEffectsController fx, IAbilityTarget target, float scaledDuration)
    {
        yield return new WaitForSeconds(scaledDuration);

        if (!isCancelled)
        {
            target.IsIntangible = false; // ← Desactiva intangibilidad
            fx.DisableSpiritMode();
        }
    }
}