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
            Debug.LogError($"[{nameof(EspirituLiberadoAbility)}] SceneEffectsController.Instance es null. ¿Está en escena?");
            return;
        }

        user.RunCoroutine(SpiritRoutine(fx));
    }

    private IEnumerator SpiritRoutine(SceneEffectsController fx)
    {
        fx.EnableSpiritMode();

        yield return new WaitForSeconds(duration);

        fx.DisableSpiritMode();
    }
}