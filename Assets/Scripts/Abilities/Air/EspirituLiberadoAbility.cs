/*
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
        AbilityCoroutineRunner runner =
            owner.GetComponent<AbilityCoroutineRunner>();

        if (runner == null)
        {
            Debug.LogError("El Player no tiene AbilityCoroutineRunner");
            return;
        }

        SceneEffectsController fx =
            Object.FindObjectOfType<SceneEffectsController>();

        if (fx == null)
        {
            Debug.LogError("No existe ScenePostFXController en la escena");
            return;
        }

        runner.RunCoroutine(SpiritRoutine(fx));
    }

    private IEnumerator SpiritRoutine(SceneEffectsController fx)
    {
        fx.EnableSpiritMode();

        yield return new WaitForSeconds(duration);

        fx.DisableSpiritMode();

        // AVISAMOS: la habilidad ha terminado
    }
}
*/

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
        AbilityCoroutineRunner runner = owner.GetComponent<AbilityCoroutineRunner>();
        if (runner == null)
        {
            Debug.LogError($"[{nameof(EspirituLiberadoAbility)}] AbilityCoroutineRunner no encontrado en {owner.name}.", owner);
            return;
        }

        // Acceso via singleton: elimina FindObjectOfType en hot path.
        // SceneEffectsController.Instance se registra en Awake.
        SceneEffectsController fx = SceneEffectsController.Instance;
        if (fx == null)
        {
            Debug.LogError($"[{nameof(EspirituLiberadoAbility)}] SceneEffectsController.Instance es null. ¿Está en escena?");
            return;
        }

        runner.RunCoroutine(SpiritRoutine(fx));
    }

    private IEnumerator SpiritRoutine(SceneEffectsController fx)
    {
        fx.EnableSpiritMode();

        yield return new WaitForSeconds(duration);

        fx.DisableSpiritMode();

        // Punto de extensión: aquí se dispararía OnAbilityFinished
        // si AbilityData expusiera un evento de ciclo de vida.
    }
}