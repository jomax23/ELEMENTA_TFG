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
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 100f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    private bool gravityEnabled = true;
    private bool horizontalMovementEnabled = true;
    private float fixedZ;
    public bool IsGrounded => characterController.isGrounded;

    [Header("External Effects")]
    [SerializeField] private float impulseDecay = 30f;
    [SerializeField] private float slowRecoverySpeed = 2f;
    private bool isStunned;
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

    [Header("Combat")]
    [SerializeField] private LayerMask targetLayers;

    [Header("Ability Animation")]
    [SerializeField] private float abilityAnimationCrossFade = 0.1f;

    // ── IAbilityUser ──────────────────────────────────────────────────────────
    public int       FacingDirection { get; private set; } = 1;
    public LayerMask TargetLayers    => targetLayers;
    public void RunCoroutine(IEnumerator routine) => StartCoroutine(routine);
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Disparado cuando se aplica un stun al player.
    /// PlayerAbilities se suscribe para cancelar la habilidad en curso.
    /// </summary>
    public event Action OnStunApplied;

    /// <summary>True mientras se reproduce una animación de habilidad.</summary>
    public bool IsUsingAbility { get; private set; }

    private CharacterController characterController;
    private Animator             animator;
    private Health               health;

    private Vector3 movement;
    private float   verticalVelocity;
    private float   externalImpulse;
    private float   slowMultiplier = 1f;
    private float   slowTimer;

    // ── Animator hashes ───────────────────────────────────────────────────────
    private static readonly int AnimIsMoving    = Animator.StringToHash("IsMoving");
    private static readonly int AnimIsSprinting = Animator.StringToHash("IsSprinting");
    private static readonly int AnimIsGrounded  = Animator.StringToHash("IsGrounded");
    private static readonly int AnimPunch       = Animator.StringToHash("Punch");
    private static readonly int AnimIsStunned   = Animator.StringToHash("IsStunned");
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator            = GetComponent<Animator>();
        health              = GetComponent<Health>();
        fixedZ              = transform.position.z;
        armor               = GetComponent<PlayerArmor>();
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
        // Sin movimiento horizontal durante una habilidad o stun
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
        // No girar durante habilidad o stun
        if (IsUsingAbility || isStunned) return;
        if (Mathf.Abs(input) < 0.01f) return;

        FacingDirection = input > 0 ? 1 : -1;
        transform.rotation = Quaternion.Euler(0f, FacingDirection == 1 ? 90f : 270f, 0f);
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

            // Salto bloqueado durante habilidad o stun
            if (!IsUsingAbility && !isStunned && jumpAction.action.WasPressedThisFrame())
                verticalVelocity = jumpForce;
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
        // Puñetazo bloqueado durante habilidad o stun
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

    /// <summary>
    /// Cancela el estado de habilidad activa y devuelve el control al jugador.
    /// Llamado por PlayerAbilities al recibir un stun o al finalizar la animación.
    /// </summary>
    public void CancelAbilityAnimation()
    {
        IsUsingAbility = false;
    }

    private void UpdateAnimator(float input, float inputSprint)
    {
        // Stun tiene máxima prioridad visual
        animator.SetBool(AnimIsStunned, isStunned);

        // Durante habilidad o stun, la locomoción no sobreescribe
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

        // Notifica a PlayerAbilities para que cancele la habilidad en curso
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

    public void SetHorizontalMovementEnabled(bool enabled)
    {
        horizontalMovementEnabled = enabled;
        if (enabled) movement.x = 0f;
    }
}