using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement, jumping, combat (punches), animator states, 
/// and external status effects (stun, slow, burn, impulse).
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
public class PlayerMovement : MonoBehaviour, IAbilityTarget, IAbilityUser, IArmorUser
{
    // ── Components ─────────────────────────────────────────────────────────
    public CharacterController characterController;
    public Animator animator;
    private Health health;
    private PlayerArmor armor;
    private PlayerAudioController audioController;
    private DamageFlash hitFlash;

    // ── Movement Settings ──────────────────────────────────────────────────
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 100f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    private float fixedZ;

    // ── Special States ─────────────────────────────────────────────────────
    private bool gravityEnabled = true;
    private bool horizontalMovementEnabled = true;
    private bool isFlying;
    private bool isDashing;
    private float armorSpeedMultiplier = 1f;

    // ── External Effects ───────────────────────────────────────────────────
    [Header("External Effects")]
    [SerializeField] private float impulseDecay = 30f;
    [SerializeField] private float slowRecoverySpeed = 2f;
    
    private float externalImpulse;
    private float slowMultiplier = 1f;
    private float slowTimer;
    private bool isStunned;
    private float stunTimer;
    private float burnDps;
    private float burnTimer;

    // ── Input ──────────────────────────────────────────────────────────────
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference punchAction;

    // ── Combat ─────────────────────────────────────────────────────────────
    [Header("Combat")]
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private PunchHitbox punchHitbox;
    [SerializeField] private float punchTime;
    
    [Header("Punch")]
    [SerializeField] private float punchHitboxDuration = 0.12f;

    // ── Abilities ──────────────────────────────────────────────────────────
    [Header("Ability Animation")]
    [SerializeField] private float abilityAnimationCrossFade = 0.1f;
    public bool IsIntangible { get; set; }

    // ── IAbilityUser Implementation ────────────────────────────────────────
    public int FacingDirection { get; private set; } = 1;
    public LayerMask TargetLayers => targetLayers;
    public bool IsUsingAbility { get; private set; }
    public bool IsGrounded => characterController.isGrounded;
    public bool IsStunned => isStunned;
    public void RunCoroutine(IEnumerator routine) => StartCoroutine(routine);
    public event Action OnStunApplied;

    // ── Runtime Variables ──────────────────────────────────────────────────
    private Vector3 movement;
    private float verticalVelocity;

    // ── Animator Hashes ────────────────────────────────────────────────────
    public readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
    public readonly int AnimIsSprinting = Animator.StringToHash("IsSprinting");
    public readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
    public readonly int AnimIsStunned = Animator.StringToHash("IsStunned");
    public readonly int AnimPunch = Animator.StringToHash("Punch");
    public readonly int AnimIsFlying = Animator.StringToHash("IsFlying");
    public readonly int AnimIsDashing = Animator.StringToHash("IsDashing");

    // =========================================================================
    // INITIALIZATION & LIFECYCLE
    // =========================================================================

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        fixedZ = transform.position.z;
        armor = GetComponent<PlayerArmor>();
        hitFlash = GetComponent<DamageFlash>();
        audioController = GetComponent<PlayerAudioController>();
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
        float input = moveAction.action.ReadValue<float>();
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
        // Locks Z-axis movement for 2.5D gameplay
        Vector3 pos = transform.position;
        pos.z = fixedZ;
        transform.position = pos;
    }

    // =========================================================================
    // MOVEMENT & ROTATION
    // =========================================================================

    private void HandleMovement(float input, float inputSprint)
    {
        bool canMove = horizontalMovementEnabled && !IsUsingAbility && !isStunned;

        float baseSpeed = moveSpeed * slowMultiplier * armorSpeedMultiplier;
        float sprintSpeed = sprintMultiplier * inputSprint * slowMultiplier * armorSpeedMultiplier;

        movement.x = canMove ? input * (baseSpeed + sprintSpeed) + externalImpulse : externalImpulse;
        movement.y = verticalVelocity;
        movement.z = 0f;

        characterController.Move(movement * Time.deltaTime);
    }

    private void HandleRotation(float input)
    {
        if (IsUsingAbility || isStunned) return;
        if (Mathf.Abs(input) < 0.01f) return;

        FacingDirection = input > 0 ? 1 : -1;
        transform.rotation = Quaternion.Euler(0f, FacingDirection == 1 ? 90f : 270f, 0f);
    }

    // =========================================================================
    // JUMP
    // =========================================================================

    private void HandleJump()
    {
        if (isFlying || isDashing) return;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;

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

    // =========================================================================
    // PUNCH
    // =========================================================================

    private void HandlePunch()
    {
        if (IsUsingAbility || isStunned) return;
        if (punchAction.action.WasPressedThisFrame())
        {
            StartCoroutine(PunchRoutine());
        }
    }

    private IEnumerator PunchRoutine()
    {
        IsUsingAbility = true;
        animator.SetTrigger(AnimPunch);

        if (punchHitbox != null) punchHitbox.SetActive(true);
        yield return new WaitForSeconds(punchHitboxDuration);
        if (punchHitbox != null) punchHitbox.SetActive(false);

        float remaining = punchTime - punchHitboxDuration;
        if (remaining > 0f) yield return new WaitForSeconds(remaining);

        IsUsingAbility = false;
    }

    // =========================================================================
    // ANIMATIONS
    // =========================================================================

    public void CancelAbilityAnimation()
    {
        IsUsingAbility = false;
    }

    public void PlayAbilityAnimation(string stateName)
    {
        if (string.IsNullOrEmpty(stateName)) return;
        IsUsingAbility = true;
        animator.CrossFade(stateName, abilityAnimationCrossFade);
    }

    private void UpdateAnimator(float input, float inputSprint)
    {
        animator.SetBool(AnimIsStunned, isStunned);

        if (isFlying)
        {
            SetAnimBools(true, false, false, false, false);
            return;
        }

        if (isDashing)
        {
            SetAnimBools(false, true, false, false, false);
            return;
        }

        animator.SetBool(AnimIsFlying, false);
        animator.SetBool(AnimIsDashing, false);

        if (IsUsingAbility || isStunned) return;

        bool isMoving = Mathf.Abs(input) > 0.01f && horizontalMovementEnabled;
        bool isSprinting = isMoving && inputSprint > 0.01f;

        animator.SetBool(AnimIsMoving, isMoving);
        animator.SetBool(AnimIsSprinting, isSprinting);
        animator.SetBool(AnimIsGrounded, characterController.isGrounded);
    }

    // Helper to reduce repetitive animator calls
    private void SetAnimBools(bool flying, bool dashing, bool grounded, bool moving, bool sprinting)
    {
        animator.SetBool(AnimIsFlying, flying);
        animator.SetBool(AnimIsDashing, dashing);
        animator.SetBool(AnimIsGrounded, grounded);
        animator.SetBool(AnimIsMoving, moving);
        animator.SetBool(AnimIsSprinting, sprinting);
    }

    // =========================================================================
    // EXTERNAL EFFECTS HANDLING
    // =========================================================================

    private void HandleExternalImpulse()
    {
        externalImpulse = Mathf.MoveTowards(externalImpulse, 0f, impulseDecay * Time.deltaTime);
    }

    private void HandleSlow()
    {
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
        }
        else
        {
            slowMultiplier = Mathf.MoveTowards(slowMultiplier, 1f, slowRecoverySpeed * Time.deltaTime);
        }
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

    // =========================================================================
    // IAbilityTarget IMPLEMENTATION
    // =========================================================================

    public void ApplyImpulse(float force)
    {
        if (IsIntangible) return;
        externalImpulse += (armor != null && armor.IsActive) ? force * 0.5f : force;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (IsIntangible || (armor != null && armor.IsActive)) return;
        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer = duration;
    }

    public void ApplyStun(float duration)
    {
        if (IsIntangible || (armor != null && armor.IsActive)) return;
        isStunned = true;
        stunTimer = duration;
        OnStunApplied?.Invoke();
    }

    public void ApplyDamage(float damage, DamageType type = DamageType.Generic)
    {
        if (IsIntangible) return;

        float finalDamage = (armor != null && armor.IsActive) ? armor.AbsorbDamage(damage) : damage;
        health.TakeDamage(finalDamage);

        if (type == DamageType.Punch)
        {
            hitFlash?.TriggerFlash();
            HitStop.Trigger(0.05f);
        }
    }

    public void ApplyBurn(float damagePerSecond, float duration)
    {
        if (IsIntangible || (armor != null && armor.IsActive)) return;
        burnDps = damagePerSecond;
        burnTimer = duration;
    }

    // =========================================================================
    // EXTERNAL CONTROL API
    // =========================================================================

    public void SetGravityEnabled(bool enabled) => gravityEnabled = enabled;
    public void SetArmorSpeedMultiplier(float multiplier) => armorSpeedMultiplier = multiplier;

    public void SetFlying(bool flying)
    {
        isFlying = flying;
        if (!flying) verticalVelocity = 0f;
    }

    public void SetDashing(bool dashing)
    {
        isDashing = dashing;
        verticalVelocity = 0f; // Prevents sudden drops or bounces
    }

    public void SetHorizontalMovementEnabled(bool enabled)
    {
        horizontalMovementEnabled = enabled;
        if (enabled) movement.x = 0f;
    }
}