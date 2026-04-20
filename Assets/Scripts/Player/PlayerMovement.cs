using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
public class PlayerMovement : MonoBehaviour, IAbilityTarget, IAbilityUser
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 100f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    private bool gravityEnabled = true;
    private bool horizontalMovementEnabled = true;
    private float fixedZ;

    [Header("External Effects")]
    [SerializeField] private float impulseDecay = 30f;
    [SerializeField] private float slowRecoverySpeed = 2f;
    [SerializeField] private bool isStunned;
    private float stunTimer;
    private float burnTimer;
    private float burnDps;
    private PlayerArmor armor;
    private float armorSpeedMultiplier = 1f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference punchAction;

    [Header("Health")]
    private Health health;

    [Header("Combat")]
    [Tooltip("Capa(s) que deben recibir daño cuando el player lanza una habilidad. " +
             "Normalmente la capa 'Enemy'.")]
    [SerializeField] private LayerMask targetLayers;

    // ── IAbilityUser ────────────────────────────────────────────────────────────
    public int       FacingDirection { get; private set; } = 1;
    public LayerMask TargetLayers    => targetLayers;
    public void RunCoroutine(IEnumerator routine) => StartCoroutine(routine);
    // ────────────────────────────────────────────────────────────────────────────

    private CharacterController characterController;
    private Animator animator;

    private Vector3 movement;
    private float verticalVelocity;

    private float externalImpulse;
    private float slowMultiplier = 1f;
    private float slowTimer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator            = GetComponent<Animator>();
        fixedZ              = transform.position.z;
        armor               = GetComponent<PlayerArmor>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
        jumpAction.action.Enable();
        punchAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        sprintAction.action.Disable();
        jumpAction.action.Disable();
        punchAction.action.Disable();
        StopAllCoroutines();
    }

    private void Update()
    {
        float input       = moveAction.action.ReadValue<float>();
        float inputSprint = sprintAction.action.ReadValue<float>();

        HandleSlow();
        HandleStun();
        HandleBurn();
        HandleExternalImpulse();
        HandleMovement(input, inputSprint);
        HandleRotation(input);
        HandleJump();
        HandlePunch();
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = fixedZ;
        transform.position = pos;
    }

    // =========================
    // MOVEMENT
    // =========================

    private void HandleMovement(float input, float inputSprint)
    {
        float baseSpeed   = moveSpeed * slowMultiplier * armorSpeedMultiplier;
        float sprintSpeed = sprintMultiplier * inputSprint * slowMultiplier * armorSpeedMultiplier;

        movement.x = horizontalMovementEnabled
            ? input * (baseSpeed + sprintSpeed) + externalImpulse
            : externalImpulse;

        movement.y = verticalVelocity;
        movement.z = 0;

        characterController.Move(movement * Time.deltaTime);
    }

    private void HandleRotation(float input)
    {
        if (Mathf.Abs(input) < 0.01f) return;

        FacingDirection = input > 0 ? 1 : -1;
        float yRotation = FacingDirection == 1 ? 90f : 270f;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    // =========================
    // JUMP
    // =========================

    private void HandleJump()
    {
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (jumpAction.action.WasPressedThisFrame())
            {
                animator.Play("jump");
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            if (gravityEnabled)
                verticalVelocity += gravity * Time.deltaTime;
            else
                verticalVelocity = 0f;
        }
    }

    // =========================
    // PUNCH
    // =========================

    private void HandlePunch()
    {
        if (punchAction.action.WasPressedThisFrame())
            animator.Play("punch");
    }

    // =========================
    // EXTERNAL EFFECTS
    // =========================

    private void HandleExternalImpulse()
    {
        externalImpulse = Mathf.MoveTowards(externalImpulse, 0f, impulseDecay * Time.deltaTime);
    }

    private void HandleSlow()
    {
        if (slowTimer > 0f)
            slowTimer -= Time.deltaTime;
        else
            slowMultiplier = Mathf.MoveTowards(slowMultiplier, 1f, slowRecoverySpeed * Time.deltaTime);
    }

    private void HandleStun()
    {
        if (!isStunned) return;
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f) isStunned = false;
    }

    private void HandleBurn()
    {
        if (burnTimer <= 0f) return;
        burnTimer -= Time.deltaTime;
        ApplyDamage(burnDps * Time.deltaTime);
    }

    // =========================
    // IAbilityTarget
    // =========================

    public void ApplyImpulse(float force)
    {
        float finalForce = armor != null && armor.IsActive ? force * 0.5f : force;
        externalImpulse += finalForce;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (armor != null && armor.IsActive) return;
        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer = duration;
    }

    public void ApplyStun(float duration)
    {
        if (armor != null && armor.IsActive) return;
        isStunned = true;
        stunTimer = duration;
    }

    public void ApplyDamage(float damage)
    {
        float finalDamage = armor != null && armor.IsActive ? armor.AbsorbDamage(damage) : damage;
        
        health.TakeDamage(finalDamage);
        Debug.Log($"{name} HP: {health}");
    }

    public void ApplyBurn(float damagePerSecond, float duration)
    {
        if (armor != null && armor.IsActive) return;
        burnDps   = damagePerSecond;
        burnTimer = duration;
    }

    // =========================
    // EXTERNAL CONTROL
    // =========================

    public void SetGravityEnabled(bool enabled)            => gravityEnabled = enabled;
    public void SetHorizontalMovementEnabled(bool enabled)
    {
        horizontalMovementEnabled = enabled;
        if (enabled) movement.x = 0f;
    }
    public void SetArmorSpeedMultiplier(float multiplier)  => armorSpeedMultiplier = multiplier;
}