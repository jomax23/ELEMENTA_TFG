using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
public class EnemyDummy : MonoBehaviour, IAbilityTarget, IArmorUser
{
    // ── Componentes ───────────────────────────────────────────────────────────
    private CharacterController characterController;
    private Animator            animator;
    private Health              health;
    private PlayerArmor           armor;
    private PlayerAudioController audioController;
    private DamageFlash         hitFlash;
    
    // ── Movimiento ────────────────────────────────────────────────────────────
    [Header("Movement")]
    //[SerializeField] private float moveSpeed        = 6f;
    //[SerializeField] private float sprintMultiplier = 100f;
    [SerializeField] private float gravity          = -20f;
    private float fixedZ;
    private float aiVelocity;
    
    // ── Estados especiales ────────────────────────────────────────────────────
    private bool  horizontalMovementEnabled = true;
    private bool  isFlying;
    private bool  isDashing;
    private float armorSpeedMultiplier = 1f;
    
    // ── Efectos externos ──────────────────────────────────────────────────────
    [Header("External Effects")]
    [SerializeField] private float impulseDecay      = 30f;
    [SerializeField] private float slowRecoverySpeed = 2f;
    private float externalImpulse;
    private float slowMultiplier = 1f;
    private float slowTimer;
    private bool  isStunned;
    private float stunTimer;
    private float burnDps;
    private float burnTimer;
    
    // ── Combate / Punch ───────────────────────────────────────────────────────
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private PunchHitbox punchHitbox;
    [SerializeField] private float       punchTime;
    [SerializeField] private float punchHitboxDuration = 0.12f;
    
    // ── Habilidades ───────────────────────────────────────────────────────────
    [Header("Ability Animation")]
    [SerializeField] private float abilityAnimationCrossFade = 0.1f;
    public bool IsIntangible { get; set; }
    
    // ── IAbilityTarget / estado público ──────────────────────────────────────
    //public int       FacingDirection { get; private set; } = 1;
    public LayerMask TargetLayers    => targetLayers;
    public bool      IsUsingAbility  { get; private set; }
    public bool      IsGrounded      => characterController.isGrounded;
    public bool         IsStunned  => isStunned;
    public void      RunCoroutine(IEnumerator routine) => StartCoroutine(routine);
    public event Action OnStunApplied;
    
    // ── Runtime ───────────────────────────────────────────────────────────────
    private Vector3 movement;
    private float   verticalVelocity;
    
    // ── Animator hashes ───────────────────────────────────────────────────────
    private static readonly int AnimIsMoving    = Animator.StringToHash("IsMoving");
    private static readonly int AnimIsSprinting = Animator.StringToHash("IsSprinting");
    private static readonly int AnimIsGrounded  = Animator.StringToHash("IsGrounded");
    private static readonly int AnimIsStunned   = Animator.StringToHash("IsStunned");
    private static readonly int AnimPunch       = Animator.StringToHash("Punch");
    private static readonly int AnimSpeed       = Animator.StringToHash("Speed");
    private HashSet<int> validParams = new();
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator            = GetComponent<Animator>();
        health              = GetComponent<Health>();
        fixedZ              = transform.position.z;
        armor               = GetComponent<PlayerArmor>();
        hitFlash = GetComponent<DamageFlash>();
        audioController     = GetComponent<PlayerAudioController>();
        
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
        Vector3 pos = transform.position;
        pos.z = fixedZ;
        transform.position = pos;
    }
    
    private void BuildValidParams()
    {
        validParams.Clear();

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[EnemyDummy] El Animator no tiene un Controller asignado.", this);
            return;
        }

        foreach (AnimatorControllerParameter p in animator.parameters)
            validParams.Add(p.nameHash);

        var sb = new System.Text.StringBuilder("[EnemyDummy] Parámetros del Animator Controller:\n");
        foreach (AnimatorControllerParameter p in animator.parameters)
            sb.AppendLine($"  · {p.name}  ({p.type})");
        Debug.Log(sb.ToString(), this);
    }

    // =========================
    // MOVEMENT
    // =========================

    public void SetMoveVelocity(float velocity)
    {
        aiVelocity = velocity * slowMultiplier;
    }

    private void HandleMovement()
    {
        bool canMove = horizontalMovementEnabled && !IsUsingAbility && !isStunned;

        float baseSpeed = slowMultiplier * armorSpeedMultiplier;

        movement.x = canMove
            ? (aiVelocity * baseSpeed) + externalImpulse
            : externalImpulse;

        movement.y = verticalVelocity;
        movement.z = 0f;

        characterController.Move(movement * Time.deltaTime);
    }
    
    
    private void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

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
    // =========================
    // ANIMATIONS
    // =========================

    public void SetUsingAbility(bool value)
    {
        IsUsingAbility = value;
    }
    
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
    
    private void UpdateAnimator()
    {
        animator.SetBool(AnimIsStunned, isStunned);
        
        if (isFlying)
        {
            animator.SetBool(AnimIsGrounded,  false);
            animator.SetBool(AnimIsMoving,    false);
            animator.SetBool(AnimIsSprinting, false);
            return;
        }
        
        if (isDashing)
        {
            animator.SetBool(AnimIsGrounded,  false);
            animator.SetBool(AnimIsMoving,    false);
            animator.SetBool(AnimIsSprinting, false);
            return;
        }
        
        if (IsUsingAbility || isStunned) return;
        
        float speed      = Mathf.Abs(aiVelocity);
        bool  isMoving   = speed > 0.1f;
        bool  isSprinting = speed > 4f;
        
        SafeSetFloat(AnimSpeed,       speed);
        animator.SetBool(AnimIsMoving,    isMoving);
        animator.SetBool(AnimIsSprinting, isSprinting);
        animator.SetBool(AnimIsGrounded,  characterController.isGrounded);
    }

    public void PlayAttack()
    {
        SafeSetTrigger(AnimPunch);
    }

    // ── Safe wrappers ──────────────────────────────────────────────────────────

    private void SafeSetFloat(int hash, float value)
    {
        if (validParams.Contains(hash)) animator.SetFloat(hash, value);
    }

    private void SafeSetTrigger(int hash)
    {
        if (validParams.Contains(hash))
            animator.SetTrigger(hash);
        else
            Debug.LogWarning($"[EnemyDummy] Trigger hash {hash} no existe en el Animator Controller.", this);
    }

    // =========================
    // EFFECTS
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
        if (IsIntangible) return;
        
        externalImpulse += armor != null && armor.IsActive ? force * 0.5f : force;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (IsIntangible) return;
        
        if (armor != null && armor.IsActive) return;
        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer      = duration;
    }

    public void ApplyStun(float duration)
    {
        if (IsIntangible) return;
        
        if (armor != null && armor.IsActive) return;
        isStunned = true;
        stunTimer = duration;
        OnStunApplied?.Invoke(); 
    }

    public void ApplyDamage(float damage, DamageType type = DamageType.Generic)
    {
        if (IsIntangible) return;
        
        float finalDamage = armor != null && armor.IsActive
            ? armor.AbsorbDamage(damage)
            : damage;
        
        health.TakeDamage(finalDamage);

        if (type == DamageType.Punch)
        {
            hitFlash?.TriggerFlash();
            HitStop.Trigger(0.05f);
        }
    }

    public void ApplyBurn(float damagePerSecond, float duration)
    {
        if (IsIntangible) return;
        
        if (armor != null && armor.IsActive) return;
        burnDps   = damagePerSecond;
        burnTimer = duration;
    }
    
    // =========================
    // EXTERNAL CONTROL
    // =========================

    public void SetArmorSpeedMultiplier(float multiplier) => armorSpeedMultiplier = multiplier;

    public void SetFlying(bool flying)
    {
        isFlying = flying;
        if (!flying) verticalVelocity = 0f;
    }

}