using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player's flight mechanics, including the initial lift-off phase 
/// and the sustained vertical movement controlled by player input.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFlight : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Lift-Off")]
    [SerializeField] private float liftDuration = 0.5f;
    [SerializeField] private float liftForce = 3f;
    [SerializeField] private AnimationCurve liftCurve;

    [Header("Input")]
    [SerializeField] private InputActionReference verticalMoveAction;

    private CharacterController controller;
    private PlayerMovement movement;

    private bool isFlying;
    private bool isLifting;
    private float flightTimer;
    private float liftTimer;
    private float startY;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();

        if (verticalMoveAction == null)
            Debug.LogError($"[{nameof(PlayerFlight)}] verticalMoveAction not assigned.", this);
    }

    /// <summary>
    /// Begins the flight sequence, disabling gravity and starting the lift-off phase.
    /// </summary>
    public void StartFlight(float duration)
    {
        if (isFlying) return;

        isFlying = true;
        startY = transform.position.y;
        isLifting = true;
        liftTimer = liftDuration;
        flightTimer = duration;

        movement.SetGravityEnabled(false);
        movement.SetFlying(true); // Locks jump and forces the Flying animation
    }

    /// <summary>
    /// Ends the flight sequence and restores normal gravity and animator states.
    /// </summary>
    public void EndFlight()
    {
        if (!isFlying) return;

        isFlying = false;
        isLifting = false;

        movement.SetGravityEnabled(true);
        movement.SetFlying(false); // Returns normal control to the animator
    }

    private void Update()
    {
        if (!isFlying) return;

        // ── PHASE 1: LIFT-OFF ──────────────────────────────────────────────
        if (isLifting)
        {
            liftTimer -= Time.deltaTime;

            float t = 1f - (liftTimer / liftDuration);
            float curveValue = liftCurve.Evaluate(t);

            float targetY = startY + (curveValue * liftForce);
            float deltaY = targetY - transform.position.y;

            controller.Move(Vector3.up * deltaY);

            // Interrupt lift-off early if the player provides input or time runs out
            if (HasInput() || liftTimer <= 0f)
                isLifting = false;

            return;
        }

        // ── PHASE 2: SUSTAINED FLIGHT ──────────────────────────────────────
        flightTimer -= Time.deltaTime;
        ApplyVerticalInput();

        if (flightTimer <= 0f)
            EndFlight();
    }

    private void ApplyVerticalInput()
    {
        if (verticalMoveAction == null) return;
        float input = verticalMoveAction.action.ReadValue<float>();
        controller.Move(Vector3.up * input * verticalSpeed * Time.deltaTime);
    }

    private bool HasInput()
    {
        if (verticalMoveAction == null) return false;
        return Mathf.Abs(verticalMoveAction.action.ReadValue<float>()) > 0.1f;
    }
}