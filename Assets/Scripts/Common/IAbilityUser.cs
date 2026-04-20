using System.Collections;
using UnityEngine;

public interface IAbilityUser
{
    /// <summary>+1 = facing right, -1 = facing left.</summary>
    int FacingDirection { get; }

    /// <summary>
    /// La capa de los objetivos que este caster debe afectar.
    /// Player → capa del enemigo. Enemy → capa del player.
    /// Se inyecta en cada área/proyectil al instanciarlo.
    /// </summary>
    LayerMask TargetLayers { get; }

    /// <summary>Runs a coroutine on the caster's MonoBehaviour context.</summary>
    void RunCoroutine(IEnumerator routine);
}