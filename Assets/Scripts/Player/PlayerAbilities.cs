using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Orquesta el uso de habilidades del jugador.
///
/// Flujo de una habilidad:
///   1. Input detectado → cooldown arranca, animación reproduce, lock activo.
///   2. Se espera activationDelay (frame de impacto de la animación).
///   3. Se llama Activate() → el efecto ocurre.
///   4. Se espera el tiempo restante hasta totalAnimationDuration.
///   5. Lock liberado → el jugador recupera el control.
///
/// Interrupción por stun (o cualquier efecto que llame a HandleStunInterrupt):
///   - La coroutine de orquestación se para.
///   - Se llama Cancel() sobre la habilidad activa para limpiar efectos en curso.
///   - El lock se libera inmediatamente (PlayerMovement maneja la animación de stun).
/// </summary>
public class PlayerAbilities : MonoBehaviour
{
    [Header("Element")]
    [SerializeField] private ElementType currentElement;
    public ElementType CurrentElement => currentElement;

    [Header("Abilities")]
    [SerializeField] private AbilitiesHUD abilitiesHUD;

    [Header("Input")]
    [SerializeField] private InputActionReference ability1Action;
    [SerializeField] private InputActionReference ability2Action;
    [SerializeField] private InputActionReference ability3Action;
    [SerializeField] private InputActionReference ability4Action;
    [SerializeField] private InputActionReference changeElementScrollAction;

    [Header("Element Ability Sets")]
    [SerializeField] private ElementAbilitySet[] elementAbilitySets;

    [Header("Scroll")]
    [SerializeField] private float scrollCooldown = 0.15f;

    // ── Referencias ────────────────────────────────────────────────────────────
    private PlayerMovement playerMovement;

    private AbilityData currentAbility1;
    private AbilityData currentAbility2;
    private AbilityData currentAbility3;
    private AbilityData currentAbility4;

    private Dictionary<AbilityData, float> cooldownTimers = new();
    private float scrollTimer;

    // ── Estado de la habilidad activa ──────────────────────────────────────────
    /// <summary>Coroutine del orquestador. Referencia necesaria para poder cancelarla.</summary>
    private Coroutine activeAbilityCoroutine;

    /// <summary>
    /// Habilidad que está ejecutándose ahora mismo.
    /// Necesaria para poder llamar Cancel() si llega una interrupción.
    /// </summary>
    private AbilityData activeAbility;

    // =========================
    // INIT
    // =========================

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        LoadAbilitiesForCurrentElement();
        InitializeCooldowns();

        abilitiesHUD.SetElement(currentElement);
        abilitiesHUD.SetAbilities(currentAbility1, currentAbility2, currentAbility3, currentAbility4);

        playerMovement.OnStunApplied += HandleStunInterrupt;
    }

    private void OnDestroy()
    {
        if (playerMovement != null)
            playerMovement.OnStunApplied -= HandleStunInterrupt;
    }

    // =========================
    // INPUT
    // =========================

    private void OnEnable()
    {
        ability1Action.action.Enable();
        ability2Action.action.Enable();
        ability3Action.action.Enable();
        ability4Action.action.Enable();
        changeElementScrollAction.action.Enable();
    }

    private void OnDisable()
    {
        ability1Action.action.Disable();
        ability2Action.action.Disable();
        ability3Action.action.Disable();
        ability4Action.action.Disable();
        changeElementScrollAction.action.Disable();

        // Si se desactiva el componente a mitad de una habilidad, limpiar también.
        ForceInterrupt();
    }

    private void Update()
    {
        UpdateCooldowns();
        HandleElementChangeScroll();

        // Input bloqueado mientras hay una habilidad en curso.
        if (playerMovement.IsUsingAbility) return;

        if (ability1Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility1);
        if (ability2Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility2);
        if (ability3Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility3);
        if (ability4Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility4);
    }

    // =========================
    // STUN / INTERRUPCIÓN
    // =========================

    /// <summary>
    /// Suscrito a PlayerMovement.OnStunApplied.
    /// Cancela la habilidad activa limpiando todos sus efectos en curso.
    /// </summary>
    private void HandleStunInterrupt() => ForceInterrupt();

    /// <summary>
    /// Punto único de interrupción forzada (stun, muerte, desactivación del componente…).
    /// </summary>
    private void ForceInterrupt()
    {
        // 1. Parar la coroutine del orquestador.
        if (activeAbilityCoroutine != null)
        {
            StopCoroutine(activeAbilityCoroutine);
            activeAbilityCoroutine = null;
        }

        // 2. Llamar Cancel() en la habilidad activa para que limpie sus efectos
        //    (haces, VFX, healing, bursts en curso…).
        if (activeAbility != null)
        {
            activeAbility.Cancel(gameObject);
            activeAbility = null;
        }

        // 3. Liberar el lock de control para que el stun tome el mando.
        playerMovement.CancelAbilityAnimation();
    }

    // =========================
    // ACTIVACIÓN
    // =========================

    private void TryUseAbility(AbilityData ability)
    {
        if (ability == null) return;

        if (!playerMovement.IsGrounded)
        {
            Debug.Log("[PlayerAbilities] No puedes usar habilidades en el aire.");
            return;
        }

        if (cooldownTimers[ability] > 0f)
        {
            Debug.Log($"[PlayerAbilities] {ability.abilityName} en cooldown.");
            return;
        }

        // Cooldown arranca inmediatamente (anti-spam).
        cooldownTimers[ability] = ability.cooldown;
        int slotIndex = GetSlotIndex(ability);
        if (slotIndex != -1)
            abilitiesHUD.SetCooldown(slotIndex, true);

        // Animación arranca inmediatamente → lock activo desde este momento.
        if (!string.IsNullOrEmpty(ability.animationStateName))
            playerMovement.PlayAbilityAnimation(ability.animationStateName);

        // Orquestador: gestiona el timing de activación y el tiempo de bloqueo total.
        activeAbility           = ability;
        activeAbilityCoroutine  = StartCoroutine(AbilityLifecycle(ability));
    }

    /// <summary>
    /// Coroutine que orquesta el ciclo de vida completo de una habilidad:
    ///   - Fase 1: esperar el frame de impacto (activationDelay).
    ///   - Fase 2: ejecutar el efecto.
    ///   - Fase 3: esperar el tiempo restante de la animación (totalAnimationDuration - activationDelay).
    ///   - Fase 4: liberar el control.
    ///
    /// Si el jugador es stunneado, ForceInterrupt() para esta coroutine externamente
    /// y llama Cancel() sobre la habilidad.
    /// </summary>
    private IEnumerator AbilityLifecycle(AbilityData ability)
    {
        // ── Fase 1: esperar frame de impacto ──────────────────────────────────
        if (ability.activationDelay > 0f)
            yield return new WaitForSeconds(ability.activationDelay);

        // ── Fase 2: efecto ocurre + SFX de activación ────────────────────────
        ability.ActivateWithAudio(gameObject);

        // ── Fase 3: esperar el resto de la animación ──────────────────────────
        float lockRemaining = ability.totalAnimationDuration - ability.activationDelay;
        if (lockRemaining > 0f)
            yield return new WaitForSeconds(lockRemaining);

        // ── Fase 4: limpiar y liberar control ─────────────────────────────────
        activeAbility          = null;
        activeAbilityCoroutine = null;
        playerMovement.CancelAbilityAnimation();
    }

    // =========================
    // COOLDOWNS
    // =========================

    private void InitializeCooldowns()
    {
        cooldownTimers.Clear();
        foreach (var set in elementAbilitySets)
        {
            TryRegisterCooldown(set.ability1);
            TryRegisterCooldown(set.ability2);
            TryRegisterCooldown(set.ability3);
            TryRegisterCooldown(set.ability4);
        }
    }

    private void TryRegisterCooldown(AbilityData ability)
    {
        if (ability != null && !cooldownTimers.ContainsKey(ability))
            cooldownTimers.Add(ability, 0f);
    }

    private void UpdateCooldowns()
    {
        var keys = new List<AbilityData>(cooldownTimers.Keys);
        foreach (var ability in keys)
        {
            if (cooldownTimers[ability] <= 0f) continue;

            cooldownTimers[ability] -= Time.deltaTime;

            if (cooldownTimers[ability] <= 0f)
            {
                cooldownTimers[ability] = 0f;
                int slotIndex = GetSlotIndex(ability);
                if (slotIndex != -1)
                    abilitiesHUD.SetCooldown(slotIndex, false);
            }
        }
    }

    // =========================
    // ELEMENT CHANGE
    // =========================

    private void HandleElementChangeScroll()
    {
        scrollTimer -= Time.deltaTime;
        if (scrollTimer > 0f) return;

        float scrollValue = changeElementScrollAction.action.ReadValue<float>();
        if (Mathf.Abs(scrollValue) < 0.01f) return;

        ChangeElement(scrollValue > 0 ? 1 : -1);
        scrollTimer = scrollCooldown;
    }

    private void ChangeElement(int direction)
    {
        int elementCount = System.Enum.GetValues(typeof(ElementType)).Length;
        int newIndex     = ((int)currentElement + direction + elementCount) % elementCount;
        currentElement   = (ElementType)newIndex;

        LoadAbilitiesForCurrentElement();
        abilitiesHUD.SetElement(currentElement);
        abilitiesHUD.SetAbilities(currentAbility1, currentAbility2, currentAbility3, currentAbility4);
        RefreshCooldownHUD();
    }

    private void LoadAbilitiesForCurrentElement()
    {
        foreach (var set in elementAbilitySets)
        {
            if (set.element != currentElement) continue;
            currentAbility1 = set.ability1;
            currentAbility2 = set.ability2;
            currentAbility3 = set.ability3;
            currentAbility4 = set.ability4;
            return;
        }
        Debug.LogWarning($"[PlayerAbilities] Sin habilidades para {currentElement}.");
    }

    // =========================
    // HELPERS
    // =========================

    private int GetSlotIndex(AbilityData ability)
    {
        if (ability == currentAbility1) return 0;
        if (ability == currentAbility2) return 1;
        if (ability == currentAbility3) return 2;
        if (ability == currentAbility4) return 3;
        return -1;
    }

    private void RefreshCooldownHUD()
    {
        RefreshSlot(0, currentAbility1);
        RefreshSlot(1, currentAbility2);
        RefreshSlot(2, currentAbility3);
        RefreshSlot(3, currentAbility4);
    }

    private void RefreshSlot(int index, AbilityData ability)
    {
        bool onCooldown = ability != null
            && cooldownTimers.ContainsKey(ability)
            && cooldownTimers[ability] > 0f;

        abilitiesHUD.SetCooldown(index, onCooldown);
    }
}