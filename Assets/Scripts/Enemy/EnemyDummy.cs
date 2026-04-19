using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
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

    private Vector3 movement;
    private float verticalVelocity;

    // Effects
    private float externalImpulse;
    private float slowMultiplier = 1f;
    private float slowTimer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        health = GetComponent<Health>();
    }

    private void Update()
    {
        HandleGravity();
        HandleSlow();
        HandleStun();
        HandleBurn();
        HandleExternalImpulse();
        HandleMovement();
    }

    // =========================
    // MOVEMENT
    // =========================

    private void HandleMovement()
    {
        movement.x = externalImpulse;
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
    // EFFECTS
    // =========================

    private void HandleExternalImpulse()
    {
        externalImpulse = Mathf.MoveTowards(
            externalImpulse,
            0f,
            impulseDecay * Time.deltaTime
        );
    }

    private void HandleSlow()
    {
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
        }
        else
        {
            slowMultiplier = Mathf.MoveTowards(
                slowMultiplier,
                1f,
                slowRecoverySpeed * Time.deltaTime
            );
        }
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
        if (burnTimer <= 0f)
            return;

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
        Debug.Log($"{name} HP: {health}");
    }
    
    public void ApplyBurn(float damagePerSecond, float duration)
    {
        burnDps = damagePerSecond;
        burnTimer = duration;
    }

}
