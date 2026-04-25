using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
public class PlayerMovement : MonoBehaviour, IAbilityTarget, IAbilityUser
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed        = 6f;
    [SerializeField] private float sprintMultiplier = 100f;
    [SerializeField] private float jumpForce        = 8f;
    [SerializeField] private float gravity          = -20f;
    private bool  gravityEnabled            = true;
    private bool  horizontalMovementEnabled = true;
    private float fixedZ;
    public bool IsGrounded => characterController.isGrounded;

    [Header("External Effects")]
    [SerializeField] private float impulseDecay      = 30f;
    [SerializeField] private float slowRecoverySpeed = 2f;
    private bool  isStunned;
    private float stunTimer;
    private float burnTimer;
    private float burnDps;
    private PlayerArmor armor;
    private float armorSpeedMultiplier = 1f;

    // Flags de estados especiales
    private bool isFlying;
    private bool isDashing;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference punchAction;

    [Header("Combat")]
    [SerializeField] private LayerMask targetLayers;

    [Header("Ability Animation")]
    [SerializeField] private float abilityAnimationCrossFade = 0.1f;

    // ── IAbilityUser ──────────────────────────────────────────────────────────
    public int       FacingDirection { get; private set; } = 1;
    public LayerMask TargetLayers    => targetLayers;
    public void RunCoroutine(IEnumerator routine) => StartCoroutine(routine);

    public event Action OnStunApplied;
    public bool IsUsingAbility { get; private set; }

    public CharacterController characterController;
    public Animator             animator;
    private Health               health;
    private PlayerAudioController audioController;

    private Vector3 movement;
    private float   verticalVelocity;
    private float   externalImpulse;
    private float   slowMultiplier = 1f;
    private float   slowTimer;

    public readonly int AnimIsMoving    = Animator.StringToHash("IsMoving");
    public readonly int AnimIsSprinting = Animator.StringToHash("IsSprinting");
    public readonly int AnimIsGrounded  = Animator.StringToHash("IsGrounded");
    public readonly int AnimPunch       = Animator.StringToHash("Punch");
    public readonly int AnimIsStunned   = Animator.StringToHash("IsStunned");
    public readonly int AnimIsFlying    = Animator.StringToHash("IsFlying");
    public readonly int AnimIsDashing   = Animator.StringToHash("IsDashing");

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator            = GetComponent<Animator>();
        health              = GetComponent<Health>();
        fixedZ              = transform.position.z;
        armor               = GetComponent<PlayerArmor>();
        audioController     = GetComponent<PlayerAudioController>();
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
        UpdateAnimator(input, inputSprint);
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
        bool canMove = horizontalMovementEnabled && !IsUsingAbility && !isStunned;

        float baseSpeed   = moveSpeed * slowMultiplier * armorSpeedMultiplier;
        float sprintSpeed = sprintMultiplier * inputSprint * slowMultiplier * armorSpeedMultiplier;

        movement.x = canMove
            ? input * (baseSpeed + sprintSpeed) + externalImpulse
            : externalImpulse;

        movement.y = verticalVelocity;
        movement.z = 0f;

        characterController.Move(movement * Time.deltaTime);
    }

    private void HandleRotation(float input)
    {
        if (IsUsingAbility || isStunned) return;
        if (Mathf.Abs(input) < 0.01f) return;

        FacingDirection    = input > 0 ? 1 : -1;
        transform.rotation = Quaternion.Euler(0f, FacingDirection == 1 ? 90f : 270f, 0f);
    }

    // =========================
    // JUMP
    // =========================

    private void HandleJump()
    {
        if (isFlying || isDashing) return;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (!IsUsingAbility && !isStunned && jumpAction.action.WasPressedThisFrame())
            {
                verticalVelocity = jumpForce;
                audioController?.PlayJump();
            }
        }
        else
        {
            verticalVelocity += (gravityEnabled ? gravity : 0f) * Time.deltaTime;
        }
    }

    // =========================
    // PUNCH
    // =========================

    private void HandlePunch()
    {
        if (IsUsingAbility || isStunned) return;
        if (punchAction.action.WasPressedThisFrame())
            animator.SetTrigger(AnimPunch);
    }

    // =========================
    // ANIMATIONS
    // =========================

    public void PlayAbilityAnimation(string stateName)
    {
        if (string.IsNullOrEmpty(stateName)) return;
        IsUsingAbility = true;
        animator.CrossFade(stateName, abilityAnimationCrossFade);
    }

    public void CancelAbilityAnimation()
    {
        IsUsingAbility = false;
    }

    private void UpdateAnimator(float input, float inputSprint)
    {
        animator.SetBool(AnimIsStunned, isStunned);

        if (isFlying)
        {
            animator.SetBool(AnimIsFlying,    true);
            animator.SetBool(AnimIsDashing,   false);
            animator.SetBool(AnimIsGrounded,  false);
            animator.SetBool(AnimIsMoving,    false);
            animator.SetBool(AnimIsSprinting, false);
            return;
        }

        if (isDashing)
        {
            animator.SetBool(AnimIsDashing,   true);
            animator.SetBool(AnimIsFlying,    false);
            animator.SetBool(AnimIsGrounded,  false);
            animator.SetBool(AnimIsMoving,    false);
            animator.SetBool(AnimIsSprinting, false);
            return;
        }

        animator.SetBool(AnimIsFlying,  false);
        animator.SetBool(AnimIsDashing, false);

        if (IsUsingAbility || isStunned) return;

        bool isMoving    = Mathf.Abs(input) > 0.01f && horizontalMovementEnabled;
        bool isSprinting = isMoving && inputSprint > 0.01f;

        animator.SetBool(AnimIsMoving,    isMoving);
        animator.SetBool(AnimIsSprinting, isSprinting);
        animator.SetBool(AnimIsGrounded,  characterController.isGrounded);
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
        externalImpulse += armor != null && armor.IsActive ? force * 0.5f : force;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (armor != null && armor.IsActive) return;
        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer      = duration;
    }

    public void ApplyStun(float duration)
    {
        if (armor != null && armor.IsActive) return;
        isStunned = true;
        stunTimer = duration;
        OnStunApplied?.Invoke();
    }

    public void ApplyDamage(float damage)
    {
        float finalDamage = armor != null && armor.IsActive
            ? armor.AbsorbDamage(damage)
            : damage;
        health.TakeDamage(finalDamage);
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

    public void SetGravityEnabled(bool enabled)           => gravityEnabled = enabled;
    public void SetArmorSpeedMultiplier(float multiplier) => armorSpeedMultiplier = multiplier;

    public void SetFlying(bool flying)
    {
        isFlying = flying;
        if (!flying) verticalVelocity = 0f;
    }

    public void SetDashing(bool dashing)
    {
        isDashing = dashing;
        // Resetear siempre: al iniciar evita caída por velocidad acumulada,
        // al terminar evita rebote brusco al recuperar gravedad.
        verticalVelocity = 0f;
    }

    public void SetHorizontalMovementEnabled(bool enabled)
    {
        horizontalMovementEnabled = enabled;
        if (enabled) movement.x = 0f;
    }
}