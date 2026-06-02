using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles the physical body, movement, animations, and status effects of the enemy.
/// Mirrors PlayerMovement but is driven externally by EnemyAI.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
public class EnemyDummy : MonoBehaviour, IAbilityTarget, IArmorUser
{
    // ── Components ─────────────────────────────────────────────────────────
    private CharacterController characterController;
    private Animator animator;
    private Health health;
    private PlayerArmor armor; // Reused component or enemy-specific armor implementation
    private PlayerAudioController audioController;
    private DamageFlash hitFlash;

    // ── Movement ───────────────────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float gravity = -20f;
    private float fixedZ;
    private float aiVelocity;

    // ── Special States ─────────────────────────────────────────────────────
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

    // ── Combat / Punch ─────────────────────────────────────────────────────
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private PunchHitbox punchHitbox;
    [SerializeField] private float punchTime;
    [SerializeField] private float punchHitboxDuration = 0.12f;

    // ── Abilities ──────────────────────────────────────────────────────────
    [Header("Ability Animation")]
    [SerializeField] private float abilityAnimationCrossFade = 0.1f;
    public bool IsIntangible { get; set; }

    // ── IAbilityTarget / Public State ──────────────────────────────────────
    public LayerMask TargetLayers => targetLayers;
    public bool IsUsingAbility { get; private set; }
    public bool IsGrounded => characterController.isGrounded;
    public bool IsStunned => isStunned;
    public Coroutine RunCoroutine(IEnumerator routine) => StartCoroutine(routine);
    public event Action OnStunApplied;

    // ── Runtime Variables ──────────────────────────────────────────────────
    private Vector3 movement;
    private float verticalVelocity;

    // ── Animator Hashes ────────────────────────────────────────────────────
    private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AnimIsSprinting = Animator.StringToHash("IsSprinting");
    private static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimIsStunned = Animator.StringToHash("IsStunned");
    private static readonly int AnimPunch = Animator.StringToHash("Punch");
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    
    private HashSet<int> validParams = new();

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
        
        BuildValidParams();
    }

    private void Update()
    {
        HandleSlow();
        HandleStun();
        HandleBurn();
        HandleExternalImpulse();
        HandleMovement();
        HandleGravity();
        UpdateAnimator();
    }

    private void LateUpdate()
    {
        // Locks Z-axis movement for 2.5D gameplay
        Vector3 pos = transform.position;
        pos.z = fixedZ;
        transform.position = pos;
    }

    /// <summary>
    /// Caches valid Animator parameters to prevent errors when setting triggers/floats.
    /// </summary>
    private void BuildValidParams()
    {
        validParams.Clear();

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[EnemyDummy] Animator has no Controller assigned.", this);
            return;
        }

        foreach (AnimatorControllerParameter p in animator.parameters)
            validParams.Add(p.nameHash);
    }

    // =========================================================================
    // MOVEMENT & GRAVITY
    // =========================================================================

    /// <summary>Sets the desired velocity from the AI controller.</summary>
    public void SetMoveVelocity(float velocity)
    {
        aiVelocity = velocity * slowMultiplier;
    }

    private void HandleMovement()
    {
        bool canMove = horizontalMovementEnabled && !IsUsingAbility && !isStunned;
        float baseSpeed = slowMultiplier * armorSpeedMultiplier;

        movement.x = canMove ? (aiVelocity * baseSpeed) + externalImpulse : externalImpulse;
        movement.y = verticalVelocity;
        movement.z = 0f;

        characterController.Move(movement * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    // =========================================================================
    // COMBAT (PUNCH)
    // =========================================================================

    public IEnumerator PunchRoutine()
    {
        IsUsingAbility = true;
        SafeSetTrigger(AnimPunch);

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

    public void SetUsingAbility(bool value) => IsUsingAbility = value;

    public void CancelAbilityAnimation() => IsUsingAbility = false;

    public void PlayAbilityAnimation(string stateName)
    {
        if (string.IsNullOrEmpty(stateName)) return;
        IsUsingAbility = true;
        animator.CrossFade(stateName, abilityAnimationCrossFade);
    }

    private void UpdateAnimator()
    {
        animator.SetBool(AnimIsStunned, isStunned);
        
        if (isFlying || isDashing)
        {
            animator.SetBool(AnimIsGrounded, false);
            animator.SetBool(AnimIsMoving, false);
            animator.SetBool(AnimIsSprinting, false);
            return;
        }
        
        if (IsUsingAbility || isStunned) return;
        
        float speed = Mathf.Abs(aiVelocity);
        bool isMoving = speed > 0.1f;
        bool isSprinting = speed > 4f;
        
        SafeSetFloat(AnimSpeed, speed);
        animator.SetBool(AnimIsMoving, isMoving);
        animator.SetBool(AnimIsSprinting, isSprinting);
        animator.SetBool(AnimIsGrounded, characterController.isGrounded);
    }

    public void PlayAttack() => SafeSetTrigger(AnimPunch);

    // ── Safe Animator Wrappers ─────────────────────────────────────────────
    private void SafeSetFloat(int hash, float value)
    {
        if (validParams.Contains(hash)) animator.SetFloat(hash, value);
    }

    private void SafeSetTrigger(int hash)
    {
        if (validParams.Contains(hash))
            animator.SetTrigger(hash);
        else
            Debug.LogWarning($"[EnemyDummy] Trigger hash {hash} does not exist in the Animator Controller.", this);
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

    public void SetArmorSpeedMultiplier(float multiplier) => armorSpeedMultiplier = multiplier;

    public void SetFlying(bool flying)
    {
        isFlying = flying;
        if (!flying) verticalVelocity = 0f;
    }
}