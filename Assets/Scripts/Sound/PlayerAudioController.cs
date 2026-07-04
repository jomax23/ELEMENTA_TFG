using UnityEngine;

// Acts as a bridge between Animation Events and the global AudioManager.
// Keeps the Animator completely decoupled from our audio singleton.
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

    // These are triggered directly via Animation Events in the Animator window
    public void OnWalkStep() => AudioManager.Instance?.PlaySFX(walkStepSound);
    public void OnRunStep() => AudioManager.Instance?.PlaySFX(runStepSound);
    public void OnPunch() => AudioManager.Instance?.PlaySFX(punchSound);
    public void OnLand() => AudioManager.Instance?.PlaySFX(landSound);

    // Called directly from code (e.g., PlayerMovement) when jumping
    public void PlayJump() => AudioManager.Instance?.PlaySFX(jumpSound);
}