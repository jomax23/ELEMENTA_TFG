using UnityEngine;

/// <summary>
/// Gestiona todos los efectos de sonido del jugador:
///   - Pasos (walk): llamado desde Animation Event en cada frame de apoyo del pie.
///   - Pasos (run): idem, con distinto SoundData.
///   - Puñetazo: llamado desde Animation Event en el frame de impacto.
///   - Aterrizaje: llamado desde Animation Event cuando el pie toca el suelo tras un salto.
///
/// ──────────────────────────────────────────────────────────────────────────
/// SETUP EN EL ANIMATOR:
///   1. Selecciona el clip Walk en el Animation Window.
///   2. En los frames donde el pie apoya, añade Animation Event → función: OnWalkStep()
///   3. Haz lo mismo en Run → OnRunStep()
///   4. En Punch, en el frame de impacto → OnPunch()
///   5. En Jump/Landing (si tienes clip) → OnLand()
/// ──────────────────────────────────────────────────────────────────────────
/// </summary>
public class PlayerAudioController : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private SoundData walkStepSound;
    [SerializeField] private SoundData runStepSound;

    [Header("Combat")]
    [SerializeField] private SoundData punchSound;

    [Header("Movement")]
    [SerializeField] private SoundData landSound;
    [SerializeField] private SoundData jumpSound;

    // ─────────────────────────────────────────────────────────────────────────
    // ANIMATION EVENTS
    // Estos métodos son llamados directamente por el sistema de Animation Events
    // de Unity desde los clips de animación. No es necesario llamarlos desde código.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Llamado desde Animation Event en el clip de Walk.</summary>
    public void OnWalkStep()
    {
        AudioManager.Instance?.PlaySFX(walkStepSound);
    }

    /// <summary>Llamado desde Animation Event en el clip de Run.</summary>
    public void OnRunStep()
    {
        AudioManager.Instance?.PlaySFX(runStepSound);
    }

    /// <summary>Llamado desde Animation Event en el frame de impacto del clip Punch.</summary>
    public void OnPunch()
    {
        AudioManager.Instance?.PlaySFX(punchSound);
    }

    /// <summary>Llamado desde Animation Event cuando el jugador aterriza.</summary>
    public void OnLand()
    {
        AudioManager.Instance?.PlaySFX(landSound);
    }

    /// <summary>
    /// Llamado desde código en PlayerMovement al ejecutar el salto.
    /// (Los saltos no tienen un frame claro en la animación, se dispara desde lógica.)
    /// </summary>
    public void PlayJump()
    {
        AudioManager.Instance?.PlaySFX(jumpSound);
    }
}