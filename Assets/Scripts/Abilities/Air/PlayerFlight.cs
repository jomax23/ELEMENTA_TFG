using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFlight : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Lift-Off")]
    [SerializeField] private float liftDuration = 0.5f;
    [SerializeField] private float liftForce    = 3f;
    [SerializeField] private AnimationCurve liftCurve;

    [Header("Input")]
    [SerializeField] private InputActionReference verticalMoveAction;

    private CharacterController controller;
    private PlayerMovement      movement;

    private bool  isFlying;
    private bool  isLifting;
    private float flightTimer;
    private float liftTimer;
    private float startY;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement   = GetComponent<PlayerMovement>();

        if (verticalMoveAction == null)
            Debug.LogError($"[{nameof(PlayerFlight)}] verticalMoveAction no asignado.", this);
    }

    public void StartFlight(float duration)
    {
        if (isFlying) return;

        isFlying    = true;
        startY      = transform.position.y;
        isLifting   = true;
        liftTimer   = liftDuration;
        flightTimer = duration;

        movement.SetGravityEnabled(false);
    }

    /// <summary>
    /// Termina el vuelo inmediatamente.
    /// Llamado tanto por el timer interno como por Cancel() ante una interrupción.
    /// </summary>
    public void EndFlight()
    {
        if (!isFlying) return;

        isFlying  = false;
        isLifting = false;

        movement.SetGravityEnabled(true);
    }

    private void Update()
    {
        if (!isFlying) return;

        // =========================
        // FASE 1: LIFT-OFF
        // =========================
        if (isLifting)
        {
            liftTimer -= Time.deltaTime;

            float t          = 1f - (liftTimer / liftDuration);
            float curveValue = liftCurve.Evaluate(t);

            float targetY = startY + (curveValue * liftForce);
            float deltaY  = targetY - transform.position.y;

            controller.Move(Vector3.up * deltaY);

            if (HasInput() || liftTimer <= 0f)
                isLifting = false;

            return;
        }

        // =========================
        // FASE 2: VUELO NORMAL
        // =========================
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