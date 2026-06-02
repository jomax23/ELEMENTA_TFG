using UnityEngine;

/// <summary>
/// Manages all player sound effects (footsteps, combat, movement).
/// Methods are triggered via Animation Events or direct code calls.
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

    /// <summary>Called via Animation Event during the Walk clip.</summary>
    public void OnWalkStep() => AudioManager.Instance?.PlaySFX(walkStepSound);

    /// <summary>Called via Animation Event during the Run clip.</summary>
    public void OnRunStep() => AudioManager.Instance?.PlaySFX(runStepSound);

    /// <summary>Called via Animation Event on the impact frame of the Punch clip.</summary>
    public void OnPunch() => AudioManager.Instance?.PlaySFX(punchSound);

    /// <summary>Called via Animation Event when the player lands on the ground.</summary>
    public void OnLand() => AudioManager.Instance?.PlaySFX(landSound);

    /// <summary>Called directly from code (e.g., PlayerMovement) when jumping.</summary>
    public void PlayJump() => AudioManager.Instance?.PlaySFX(jumpSound);
}