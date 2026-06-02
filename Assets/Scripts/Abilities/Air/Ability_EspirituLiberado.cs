using UnityEngine;
using System.Collections;

/// <summary>
/// Air ability that makes the user intangible (invulnerable to damage/effects) 
/// and triggers a visual "Spirit Mode" effect for a scaled duration.
/// </summary>
[CreateAssetMenu(fileName = "EspirituLiberado", menuName = "Abilities/Air/Espíritu Liberado")]
public class Ability_EspirituLiberado : AbilityData
{
    [Header("Spirit Mode Settings")]
    [Tooltip("Base duration of the spirit mode. Scaled by affinity efficiency.")]
    [SerializeField] private float duration = 4f;

    private bool isCancelled;

    public override string GetFormattedDescription(float efficiency)
    {
        float scaledDuration = duration * efficiency;
        return string.Format(descriptionTemplate, scaledDuration);

    }
    
    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        IAbilityTarget target = owner.GetComponent<IAbilityTarget>();
        
        if (user == null || target == null)
        {
            Debug.LogError($"[{nameof(Ability_EspirituLiberado)}] Required IAbilityUser/IAbilityTarget components not found on {owner.name}.", owner);
            return;
        }

        SceneEffectsController fx = SceneEffectsController.Instance;
        if (fx == null)
        {
            Debug.LogError($"[{nameof(Ability_EspirituLiberado)}] SceneEffectsController instance is null.", owner);
            return;
        }

        isCancelled = false;
        float scaledDuration = duration * efficiency;
        
        target.IsIntangible = true;
        fx.EnableSpiritMode();
        
        user.RunCoroutine(SpiritRoutine(fx, target, scaledDuration));
    }

    public override void Cancel(GameObject owner)
    {
        isCancelled = true;
        
        var target = owner.GetComponent<IAbilityTarget>();
        if (target != null) target.IsIntangible = false;
        
        SceneEffectsController.Instance?.DisableSpiritMode();
    }

    private IEnumerator SpiritRoutine(SceneEffectsController fx, IAbilityTarget target, float scaledDuration)
    {
        yield return new WaitForSeconds(scaledDuration);

        if (!isCancelled)
        {
            target.IsIntangible = false;
            fx.DisableSpiritMode();
        }
    }
}