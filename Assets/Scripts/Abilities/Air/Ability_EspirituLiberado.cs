using UnityEngine;
using System.Collections;

// "Spirit Mode": makes the user intangible (invulnerable) for a scaled duration.
// Triggers a visual effect and uses a coroutine to track the timer.
[CreateAssetMenu(fileName = "EspirituLiberado", menuName = "Abilities/Air/Espíritu Liberado")]
public class Ability_EspirituLiberado : AbilityData
{
    [Header("Spirit Mode Settings")]
    [SerializeField] private float duration = 4f;
    
    private bool isCancelled;

    public override string GetFormattedDescription(float efficiency)
    {
        return string.Format(descriptionTemplate, duration * efficiency);
    }

    public override void Activate(GameObject owner, float efficiency = 1f)
    {
        IAbilityUser user = owner.GetComponent<IAbilityUser>();
        IAbilityTarget target = owner.GetComponent<IAbilityTarget>();
        
        if (user == null || target == null)
        {
            Debug.LogError($"[{nameof(Ability_EspirituLiberado)}] Required components not found on {owner.name}.", owner);
            return;
        }

        SceneEffectsController fx = SceneEffectsController.Instance;
        if (fx == null)
        {
            Debug.LogError($"[{nameof(Ability_EspirituLiberado)}] SceneEffectsController instance is null.", owner);
            return;
        }

        isCancelled = false;
        
        // Apply invulnerability and visual effects
        target.IsIntangible = true;
        fx.EnableSpiritMode();
        
        // Start the duration timer
        user.RunCoroutine(SpiritRoutine(fx, target, duration * efficiency));
    }

    // Clean up flags and effects if interrupted before the timer finishes
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