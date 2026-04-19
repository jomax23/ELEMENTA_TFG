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
        movement   = GetComponent<PlayerMovement>();

        if (verticalMoveAction == null)
            Debug.LogError($"[{nameof(PlayerFlight)}] verticalMoveAction no asignado.", this);
    }

    public void StartFlight(float duration)
    {
        if (isFlying) return;

        isFlying = true;

        // 🔥 fase inicial (lift)
        startY = transform.position.y;
        
        isLifting = true;
        liftTimer = liftDuration;

        flightTimer = duration;

        movement.SetGravityEnabled(false);
    }

    private void Update()
    {
        if (!isFlying) return;

        // =========================
        // 🔶 FASE 1: LIFT-OFF
        // =========================
        if (isLifting)
        {
            liftTimer -= Time.deltaTime;

            float t = 1f - (liftTimer / liftDuration); // 0 → 1
            float curveValue = liftCurve.Evaluate(t);

            float targetY = startY + (curveValue * liftForce);
            float deltaY = targetY - transform.position.y;
            
            controller.Move(Vector3.up * deltaY);

            // Si hay input → salir del lift antes
            if (HasInput())
            {
                isLifting = false;
            }

            // Si termina el tiempo → pasar a vuelo
            if (liftTimer <= 0f)
            {
                isLifting = false;
            }

            return;
        }

        // =========================
        // 🔷 FASE 2: VUELO NORMAL
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

        float input = verticalMoveAction.action.ReadValue<float>();
        return Mathf.Abs(input) > 0.1f;
    }

    private void EndFlight()
    {
        isFlying = false;
        isLifting = false;

        movement.SetGravityEnabled(true);
    }
}