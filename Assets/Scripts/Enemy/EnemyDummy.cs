using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
public class EnemyDummy : MonoBehaviour, IAbilityTarget
{
    [Header("Movement")]
    [SerializeField] private float gravity = -20f;

    [Header("External Effects")]
    [SerializeField] private float impulseDecay = 30f;
    [SerializeField] private float slowRecoverySpeed = 2f;
    [SerializeField] private bool isStunned;

    private float stunTimer;
    private float burnTimer;
    private float burnDps;

    private Health health;
    private CharacterController characterController;
    private Animator animator;

    private Vector3 movement;
    private float verticalVelocity;

    private float externalImpulse;
    private float slowMultiplier = 1f;
    private float slowTimer;

    private float aiVelocity;

    public bool IsStunned => isStunned;

    // ───────── Animator hashes ─────────
    private static readonly int AnimIsMoving    = Animator.StringToHash("IsMoving");
    private static readonly int AnimIsGrounded  = Animator.StringToHash("IsGrounded");
    private static readonly int AnimPunch       = Animator.StringToHash("Punch");
    private static readonly int AnimAbility     = Animator.StringToHash("Ability");
    private static readonly int AnimIsStunned   = Animator.StringToHash("IsStunned");
    private static readonly int AnimSpeed       = Animator.StringToHash("Speed");
    private static readonly int AnimIsSprinting = Animator.StringToHash("IsSprinting");
    // ───────────────────────────────────

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleGravity();
        HandleSlow();
        HandleStun();
        HandleBurn();
        HandleExternalImpulse();
        HandleMovement();
        UpdateAnimator();
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
        movement.x = isStunned ? externalImpulse : aiVelocity + externalImpulse;
        movement.y = verticalVelocity;

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

    // =========================
    // ANIMATIONS
    // =========================

    private void UpdateAnimator()
    {
        float speed = Mathf.Abs(aiVelocity);

        bool isMoving = speed > 0.1f && !isStunned;
        bool isGrounded = characterController.isGrounded;
        bool isSprinting = speed > 4f; // 🔥 umbral de sprint

        animator.SetBool(AnimIsMoving, isMoving);
        animator.SetBool(AnimIsGrounded, isGrounded);
        animator.SetBool(AnimIsStunned, isStunned);
        animator.SetBool(AnimIsSprinting, isSprinting);
        animator.SetFloat(AnimSpeed, speed);
    }

    public void PlayAttack()
    {
        animator.SetTrigger(AnimPunch);
    }

    public void PlayAbility()
    {
        animator.SetTrigger(AnimAbility);
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
        if (stunTimer <= 0f)
            isStunned = false;
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
        externalImpulse += force;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer = duration;
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
    }

    public void ApplyDamage(float damage)
    {
        health.TakeDamage(damage);
    }

    public void ApplyBurn(float damagePerSecond, float duration)
    {
        burnDps = damagePerSecond;
        burnTimer = duration;
    }
}