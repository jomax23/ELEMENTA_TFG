using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyDummy))]
public class EnemyAI : MonoBehaviour, IAbilityUser
{
    // ── Máquina de estados ────────────────────────────────────────────────────
    private enum AIState { Idle, Approaching, BackingOff, MeleeCombat, UsingAbility, Stunned }
    private AIState currentState = AIState.Idle;
    private float   stateTimer;
    private float   decisionTimer;
    
    // ── Componentes ───────────────────────────────────────────────────────────
    private EnemyDummy enemyBody;
    private Health     health;
    
    // ── Target ────────────────────────────────────────────────────────────────
    [Header("Target")]
    [SerializeField] private Transform playerTransform;
    
    // ── Elemento y habilidades ────────────────────────────────────────────────
    [Header("Element & Abilities")]
    [SerializeField] private ElementType         currentElement;
    [SerializeField] private ElementAbilitySet[] elementAbilitySets;
    private AbilityData slot1, slot2, slot3, slot4;
    private AbilityData activeAbility;
    private Dictionary<AbilityData, float> cooldownTimers = new();
    
    // ── Movimiento ────────────────────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float approachSpeed  = 3.5f;
    [SerializeField] private float backOffSpeed   = 2.5f;
    [SerializeField] private float sprintSpeed    = 6.5f;
    [SerializeField] private float sprintDistance = 6f;
    private float currentMoveVelocity;
    
    // ── Rangos de distancia ───────────────────────────────────────────────────
    [Header("Distance Thresholds")]
    [SerializeField] private float detectionRange    = 15f;
    [SerializeField] private float meleeRange        = 1.8f;
    [SerializeField] private float preferredDistance = 1.3f;
    [SerializeField] private float minimumDistance   = 0.7f;
    
    // ── Ataque melee ──────────────────────────────────────────────────────────
    [Header("Melee Attack")]
    [SerializeField] private float meleeDamage         = 10f;
    [SerializeField] private float meleeKnockback      = 5f;
    [SerializeField] private float meleeAttackCooldown = 0.75f;
    [SerializeField] private float meleeHitboxRange    = 2f;
    private float meleeTimer;
    
    // ── Comportamiento ────────────────────────────────────────────────────────
    [Header("AI Behaviour")]
    [Range(0f, 1f)] [SerializeField] private float aggressionLevel        = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float abilityUsageRate       = 0.55f;
    [SerializeField]                 private float minDecisionInterval     = 0.15f;
    [SerializeField]                 private float maxDecisionInterval     = 0.45f;
    [Range(0f, 1f)] [SerializeField] private float retreatHealthThreshold = 0.25f;
    
    // ── Combate ───────────────────────────────────────────────────────────────
    [Header("Combat")]
    [SerializeField] private LayerMask targetLayers;
    
    // ── IAbilityUser ──────────────────────────────────────────────────────────
    public int       FacingDirection { get; private set; } = -1;
    public LayerMask TargetLayers    => targetLayers;
    public void      RunCoroutine(IEnumerator routine) => StartCoroutine(routine);

    // ─────────────────────────────────────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        enemyBody = GetComponent<EnemyDummy>();
        health    = GetComponent<Health>();

        if (playerTransform == null)
        {
            var player = FindFirstObjectByType<PlayerMovement>();
            if (player != null) playerTransform = player.transform;
            else Debug.LogWarning("[EnemyAI] PlayerMovement not found in scene.");
        }

        if (GameSession.Instance != null && GameSession.Instance.EnemyElement != default)
            currentElement = GameSession.Instance.EnemyElement;
        
        InitializeCooldowns();
        LoadAbilitiesForCurrentElement();
        
        enemyBody.OnStunApplied += HandleStunApplied;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (playerTransform == null) return;

        UpdateCooldowns();
        meleeTimer    -= Time.deltaTime;
        stateTimer    -= Time.deltaTime;
        decisionTimer -= Time.deltaTime;

        SyncStunState();

        if (decisionTimer <= 0f)
        {
            Decide();
            decisionTimer = Random.Range(minDecisionInterval, maxDecisionInterval);
        }

        ExecuteCurrentState();
        enemyBody.SetMoveVelocity(currentMoveVelocity);
    }

    
    private void OnDestroy()
    {
        if (enemyBody != null)
            enemyBody.OnStunApplied -= HandleStunApplied;
    }
    
    private void HandleStunApplied()
    {
        if (activeAbility != null)
        {
            activeAbility.Cancel(gameObject);
            activeAbility = null;
        }

        StopAllCoroutines();
        enemyBody.CancelAbilityAnimation();
        TransitionTo(AIState.Stunned);
    }
    
    // ─────────────────────────────────────────────────────────────────────────
    // ABILITIES
    // ─────────────────────────────────────────────────────────────────────────

    private void InitializeCooldowns()
    {
        cooldownTimers.Clear();
        foreach (var set in elementAbilitySets)
        {
            TryRegister(set.ability1); TryRegister(set.ability2);
            TryRegister(set.ability3); TryRegister(set.ability4);
        }
    }

    private void TryRegister(AbilityData a)
    {
        if (a != null && !cooldownTimers.ContainsKey(a)) cooldownTimers.Add(a, 0f);
    }

    private void LoadAbilitiesForCurrentElement()
    {
        foreach (var set in elementAbilitySets)
        {
            if (set.element != currentElement) continue;
            slot1 = set.ability1; slot2 = set.ability2;
            slot3 = set.ability3; slot4 = set.ability4;
            return;
        }
        Debug.LogWarning($"[EnemyAI] No ability set found for element {currentElement}.");
    }

    private void UpdateCooldowns()
    {
        var keys = new List<AbilityData>(cooldownTimers.Keys);
        foreach (var a in keys)
            if (cooldownTimers[a] > 0f)
                cooldownTimers[a] = Mathf.Max(0f, cooldownTimers[a] - Time.deltaTime);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DECISION
    // ─────────────────────────────────────────────────────────────────────────

    private void Decide()
    {
        if (currentState is AIState.Stunned or AIState.UsingAbility) return;

        float dist        = DistanceToPlayer();
        float healthRatio = health.health / health.maxHealth;

        if (dist > detectionRange) { TransitionTo(AIState.Idle); return; }
        if (healthRatio < retreatHealthThreshold && Random.value > aggressionLevel) { TransitionTo(AIState.BackingOff, Random.Range(1.2f, 2.5f)); return; }
        if (dist < minimumDistance) { TransitionTo(AIState.BackingOff, 0.4f); return; }

        if (Random.value < abilityUsageRate)
        {
            AbilityData best = PickBestAbility(dist);
            if (best != null) { StartCoroutine(UseAbilityRoutine(best)); return; }
        }

        TransitionTo(dist <= meleeRange ? AIState.MeleeCombat : AIState.Approaching);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STATE EXECUTION
    // ─────────────────────────────────────────────────────────────────────────

    private void ExecuteCurrentState()
    {
        float dist      = DistanceToPlayer();
        int   dir       = DirectionToPlayer();
        float moveSpeed = dist > sprintDistance ? sprintSpeed : approachSpeed;

        switch (currentState)
        {
            case AIState.Idle:
                currentMoveVelocity = 0f;
                break;

            case AIState.Approaching:
                if (dist <= preferredDistance) { TransitionTo(AIState.MeleeCombat); currentMoveVelocity = 0f; }
                else { currentMoveVelocity = dir * moveSpeed; FaceDirection(dir); } break;

            case AIState.BackingOff:
                currentMoveVelocity = -dir * backOffSpeed;
                FaceDirection(dir);
                if (stateTimer <= 0f) TransitionTo(AIState.Approaching);
                break;

            case AIState.MeleeCombat:
                ExecuteMeleeCombat(dist, dir);
                break;

            case AIState.UsingAbility:
                currentMoveVelocity = 0f;
                FaceDirection(dir);
                // No llamar FaceDirection: el enemigo mantiene la dirección
                // que tenía al inicio de la habilidad, igual que el jugador.
                break;

            case AIState.Stunned:
                currentMoveVelocity = 0f;
                
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MELEE
    // ─────────────────────────────────────────────────────────────────────────

    private void ExecuteMeleeCombat(float dist, int dir)
    {
        FaceDirection(dir);

        if (dist > meleeRange * 1.25f) { TransitionTo(AIState.Approaching); currentMoveVelocity = 0f; return; }

        if (dist < minimumDistance)         currentMoveVelocity = -dir * backOffSpeed * 0.6f;
        else if (dist > preferredDistance)  currentMoveVelocity =  dir * approachSpeed * 0.35f;
        else                                currentMoveVelocity = 0f;

        if (meleeTimer <= 0f) { PerformMeleeAttack(dist, dir); meleeTimer = meleeAttackCooldown; }
    }

    private void PerformMeleeAttack(float dist, int dir)
    {
        if (dist > meleeHitboxRange) return;
        var target = playerTransform.GetComponent<IAbilityTarget>();
        if (target == null) return;

        enemyBody.PlayAttack();
        target.ApplyDamage(meleeDamage, DamageType.Punch);
        target.ApplyImpulse(dir * meleeKnockback);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ABILITY ROUTINE — espejo exacto de PlayerAbilities.AbilityLifecycle
    // ─────────────────────────────────────────────────────────────────────────

    private AbilityData PickBestAbility(float dist)
    {
        AbilityData best = null; float bestScore = -1f;
        foreach (AbilityData a in ActiveAbilities())
        {
            if (a == null || !cooldownTimers.ContainsKey(a)) continue;
            if (cooldownTimers[a] > 0f) continue;
            if (dist < a.minRange || dist > a.maxRange)      continue;

            float score = a.aiPriority + Random.Range(-0.25f, 0.25f);
            if (score > bestScore) { bestScore = score; best = a; }
        }
        return best;
    }

    private IEnumerator UseAbilityRoutine(AbilityData ability)
    {
        currentState        = AIState.UsingAbility;
        currentMoveVelocity = 0f;

        // Pequeño delay antes de ejecutar (simula tiempo de reacción)
        yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));

        // 1. Lanzar la animación por nombre de estado, igual que el player
        //    PlayerAbilities hace: playerMovement.PlayAbilityAnimation(ability.animationStateName)
        //    Aquí hacemos lo mismo con enemyBody.
        if (!string.IsNullOrEmpty(ability.animationStateName))
            enemyBody.PlayAbilityAnimation(ability.animationStateName);
        else
            enemyBody.SetUsingAbility(true); // sin animación: bloquear movimiento igualmente

        // 2. Esperar el activationDelay (igual que AbilityLifecycle del player)
        if (ability.activationDelay > 0f)
            yield return new WaitForSeconds(ability.activationDelay);

        // 3. Activar el efecto de la habilidad
        activeAbility = ability;
        ability.Activate(gameObject);
        cooldownTimers[ability] = ability.cooldown;

        // 4. Esperar el tiempo restante de la animación
        float lockRemaining = ability.totalAnimationDuration - ability.activationDelay;
        if (lockRemaining > 0f)
            yield return new WaitForSeconds(lockRemaining);

        // 5. Limpiar — igual que CancelAbilityAnimation en el player
        activeAbility = null;
        enemyBody.CancelAbilityAnimation();

        TransitionTo(AIState.BackingOff, Random.Range(0.4f, 0.9f));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STUN SYNC
    // ─────────────────────────────────────────────────────────────────────────

    private void SyncStunState()
    {
        if (!enemyBody.IsStunned && currentState == AIState.Stunned)
            TransitionTo(AIState.Approaching);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    private void TransitionTo(AIState s, float duration = 0f) { currentState = s; stateTimer = duration; }
    private float DistanceToPlayer() => Mathf.Abs(transform.position.x - playerTransform.position.x);
    private int   DirectionToPlayer() => playerTransform.position.x > transform.position.x ? 1 : -1;

    private void FaceDirection(int dir)
    {
        FacingDirection    = dir;
        transform.rotation = Quaternion.Euler(0f, dir == 1 ? 90f : 270f, 0f);
    }

    private IEnumerable<AbilityData> ActiveAbilities()
    {
        yield return slot1; yield return slot2;
        yield return slot3; yield return slot4;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GIZMOS
    // ─────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;

        Gizmos.color = Color.yellow;        DrawRangeCircle(pos, detectionRange);
        Gizmos.color = Color.red;           DrawRangeCircle(pos, meleeRange);
        Gizmos.color = new Color(1f,.45f,0f); DrawRangeCircle(pos, preferredDistance);
        Gizmos.color = new Color(1f,.1f,.1f,.6f); DrawRangeCircle(pos, minimumDistance);

        AbilityData[] slots = { slot1, slot2, slot3, slot4 };
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            Gizmos.color = new Color(0f,.85f,1f,.35f); DrawRangeCircle(pos, slots[i].minRange);
            Gizmos.color = new Color(.1f,.4f,1f,.35f);  DrawRangeCircle(pos, slots[i].maxRange);
            UnityEditor.Handles.Label(pos + Vector3.right * slots[i].maxRange + Vector3.up * (i * 0.3f), slots[i].abilityName);
        }
    }

    private static void DrawRangeCircle(Vector3 center, float radius, int segments = 32)
    {
        float step = 360f / segments;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float   rad  = i * step * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}