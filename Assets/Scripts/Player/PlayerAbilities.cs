using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerAbilities : MonoBehaviour
{
    // Este script centraliza todo lo relacionado con las habilidades del jugador:
    // qué elemento tiene activo, qué habilidades puede usar, sus cooldowns y la HUD.

    // ── Componentes ───────────────────────────────────────────────────────────
    private PlayerMovement playerMovement;

    // ── Elemento ──────────────────────────────────────────────────────────────
    [Header("Element")]
    // Elemento que el jugador tiene seleccionado ahora mismo.
    [SerializeField] private ElementType currentElement;
    public ElementType CurrentElement => currentElement;
    // Elemento principal elegido en la partida. Se usa como base para afinidades.
    private ElementType  mainElement;

    // ── HUD ───────────────────────────────────────────────────────────────────
    [Header("HUD")]
    [SerializeField] private AbilitiesHUD abilitiesHUD;
    [SerializeField] private AffinityHUD  affinityHUD;

    // ── Input ─────────────────────────────────────────────────────────────────
    [Header("Input")]
    [SerializeField] private InputActionReference ability1Action;
    [SerializeField] private InputActionReference ability2Action;
    [SerializeField] private InputActionReference ability3Action;
    [SerializeField] private InputActionReference ability4Action;
    [SerializeField] private InputActionReference changeElementScrollAction;

    // ── Sets de habilidades ───────────────────────────────────────────────────
    [Header("Element Ability Sets")]
    // Cada elemento apunta a un conjunto de 4 habilidades posibles.
    [SerializeField] private ElementAbilitySet[] elementAbilitySets;
    // Referencias a las 4 habilidades visibles/activas del elemento actual.
    private AbilityData currentAbility1;
    private AbilityData currentAbility2;
    private AbilityData currentAbility3;
    private AbilityData currentAbility4;

    // ── Cooldowns ─────────────────────────────────────────────────────────────
    // Guarda el tiempo restante de cooldown de cada habilidad registrada.
    private Dictionary<AbilityData, float> cooldownTimers = new();
    private AbilityData[] cooldownKeys; // cache para evitar alloc por frame

    // ── Habilidad activa ──────────────────────────────────────────────────────
    // Se conserva la corrutina actual para poder interrumpirla por stun o disable.
    private Coroutine   activeAbilityCoroutine;
    private AbilityData activeAbility;

    // ── Afinidad ──────────────────────────────────────────────────────────────
    private AffinityData affinityData;

    // ── Scroll de elemento ────────────────────────────────────────────────────
    [Header("Scroll")]
    // Pequeño bloqueo para que un solo giro de rueda no cambie varios elementos.
    [SerializeField] private float scrollCooldown = 0.15f;
    private float scrollTimer;
    
    
    // =========================
    // INIT
    // =========================

    private void Awake()
    {
        // Cacheamos PlayerMovement porque lo consultamos y usamos constantemente.
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        if (GameSession.Instance != null)
        {
            mainElement  = GameSession.Instance.MainElement;
            affinityData = GameSession.Instance.AffinityData;

            // El elemento inicial debe coincidir con el elegido por el jugador en la sesión.
            currentElement = mainElement;
            
            if (MatchController.Instance != null)
            {
                MatchController.Instance.OnMasterControlStart += RefreshAbilitiesOnStateChange;
                MatchController.Instance.OnMasterControlEnd   += RefreshAbilitiesOnStateChange;
            }
        }
        else
        {
            Debug.LogWarning("[PlayerAbilities] GameSession no encontrado. Sin afinidad.", this);
            mainElement  = currentElement;
            affinityData = null;
        }

        // Primero cargamos qué habilidades corresponden al elemento activo
        // y después registramos todas las habilidades posibles para sus cooldowns.
        LoadAbilitiesForCurrentElement();
        InitializeCooldowns();

        // Sincronizamos la interfaz desde el primer frame.
        abilitiesHUD.SetElement(currentElement);
        abilitiesHUD.SetAbilities(currentAbility1, currentAbility2, currentAbility3, currentAbility4);
        affinityHUD?.Refresh(currentElement);

        // Si el jugador recibe stun, cancelamos la habilidad en curso.
        playerMovement.OnStunApplied += HandleStunInterrupt;
    }

    private void OnDestroy()
    {
        if (playerMovement != null)
            playerMovement.OnStunApplied -= HandleStunInterrupt;
        
        if (MatchController.Instance != null)
        {
            MatchController.Instance.OnMasterControlStart -= RefreshAbilitiesOnStateChange;
            MatchController.Instance.OnMasterControlEnd   -= RefreshAbilitiesOnStateChange;
        }
    }

    private void RefreshAbilitiesOnStateChange()
    {
        LoadAbilitiesForCurrentElement();
        abilitiesHUD?.SetAbilities(currentAbility1, currentAbility2, currentAbility3, currentAbility4);
    }
    
    // =========================
    // INPUT
    // =========================

    private void OnEnable()
    {
        // Las InputActions se activan aquí para que solo escuchen cuando el objeto está habilitado.
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

        // Si el objeto se desactiva en mitad de una habilidad, la cortamos limpiamente.
        ForceInterrupt();
    }

    private void Update()
    {
        // Los cooldowns y el scroll se actualizan siempre, incluso si el jugador está bloqueado.
        UpdateCooldowns();
        HandleElementChangeScroll();

        // Mientras una habilidad mantiene al jugador "ocupado", no dejamos lanzar otra.
        if (playerMovement.IsUsingAbility) return;

        if (ability1Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility1);
        if (ability2Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility2);
        if (ability3Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility3);
        if (ability4Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility4);
    }

    // =========================
    // STUN / INTERRUPCIÓN
    // =========================

    private void HandleStunInterrupt() => ForceInterrupt();

    private void ForceInterrupt()
    {
        if (activeAbilityCoroutine != null)
        {
            // Cancelamos la espera interna de la habilidad (delay o lock animación).
            StopCoroutine(activeAbilityCoroutine);
            activeAbilityCoroutine = null;
        }

        if (activeAbility != null)
        {
            // Permitimos que la propia habilidad limpie efectos persistentes si los tiene.
            activeAbility.Cancel(gameObject);
            activeAbility = null;
        }

        // Dejamos al jugador fuera del estado visual de "lanzando habilidad".
        playerMovement.CancelAbilityAnimation();
    }

    // =========================
    // ACTIVACIÓN
    // =========================

    private void TryUseAbility(AbilityData ability)
    {
        if (ability == null) return;

        // Este controlador no permite activar habilidades en el aire.
        if (!playerMovement.IsGrounded)
        {
            Debug.Log("[PlayerAbilities] No puedes usar habilidades en el aire.");
            return;
        }

        // Si aún queda tiempo, abortamos antes de tocar animación o estado.
        if (cooldownTimers.TryGetValue(ability, out float remaining) && remaining > 0f)
        {
            Debug.Log($"[PlayerAbilities] {ability.abilityName} en cooldown ({remaining:F1}s).");
            return;
        }

        // El cooldown se aplica al iniciar el uso, no cuando termina,
        // para evitar que el jugador pueda spamear durante el delay de activación.
        float cooldownMult      = GetCooldownMultiplierForAbility(ability);
        cooldownTimers[ability] = ability.cooldown * cooldownMult;

        int slotIndex = GetSlotIndex(ability);
        if (slotIndex != -1)
        {
            abilitiesHUD.SetCooldown(slotIndex, true);
            abilitiesHUD.StartCooldown(slotIndex, ability.cooldown * cooldownMult); // ← NUEVO
        }

        if (!string.IsNullOrEmpty(ability.animationStateName))
            playerMovement.PlayAbilityAnimation(ability.animationStateName);

        // Guardamos qué habilidad está viva para poder interrumpirla desde otros eventos.
        activeAbility          = ability;
        activeAbilityCoroutine = StartCoroutine(AbilityLifecycle(ability));
    }

    private IEnumerator AbilityLifecycle(AbilityData ability)
    {
        // Algunas habilidades esperan unos frames/segundos antes de "dispararse" realmente.
        if (ability.activationDelay > 0f)
            yield return new WaitForSeconds(ability.activationDelay);

        // La eficiencia depende de la afinidad entre el elemento principal y el de la habilidad.
        float efficiency = GetEfficiencyForAbility(ability);
        ability.ActivateWithAudio(gameObject, efficiency);

        // Tras activarse, respetamos el resto del tiempo de animación para bloquear acciones.
        float lockRemaining = ability.totalAnimationDuration - ability.activationDelay;
        if (lockRemaining > 0f)
            yield return new WaitForSeconds(lockRemaining);

        // Al terminar, liberamos el estado para que el jugador vuelva a actuar con normalidad.
        activeAbility          = null;
        activeAbilityCoroutine = null;
        playerMovement.CancelAbilityAnimation();
    }

    // =========================
    // AFFINITY
    // =========================

    private float GetEfficiencyForAbility(AbilityData ability)
    {
        if (MatchController.Instance?.ShouldBypassAffinity() == true) return 1f;
        return affinityData?.GetEfficiency(mainElement, ability.element) ?? 1f;
    }

    private float GetCooldownMultiplierForAbility(AbilityData ability)
    {
        if (MatchController.Instance?.ShouldBypassAffinity() == true) return 1f;
        return affinityData?.GetCooldownMultiplier(mainElement, ability.element) ?? 1f;
    }

    // =========================
    // COOLDOWNS — sin alloc por frame
    // =========================

    private void InitializeCooldowns()
    {
        cooldownTimers.Clear();

        // Registramos todas las habilidades de todos los elementos una sola vez.
        // Así sus cooldowns siguen existiendo aunque cambiemos de elemento.
        foreach (ElementAbilitySet set in elementAbilitySets)
        {
            TryRegisterCooldown(set.ability1);
            TryRegisterCooldown(set.ability2);
            TryRegisterCooldown(set.ability3);
            TryRegisterCooldown(set.ability4);
        }

        // Cacheamos las keys para recorrer el diccionario sin generar basura cada frame.
        cooldownKeys = new AbilityData[cooldownTimers.Count];
        cooldownTimers.Keys.CopyTo(cooldownKeys, 0);
    }

    private void TryRegisterCooldown(AbilityData ability)
    {
        if (ability != null && !cooldownTimers.ContainsKey(ability))
            cooldownTimers.Add(ability, 0f);
    }

    private void UpdateCooldowns()
    {
        if (cooldownKeys == null) return;

        for (int i = 0; i < cooldownKeys.Length; i++)
        {
            AbilityData ability = cooldownKeys[i];
            if (cooldownTimers[ability] <= 0f) continue;

            cooldownTimers[ability] -= Time.deltaTime;

            if (cooldownTimers[ability] <= 0f)
                cooldownTimers[ability] = 0f;
        }
    
        // Actualizar HUD cada frame con los tiempos REALES de las habilidades visibles
        RefreshCooldownHUD();
    }

    // =========================
    // ELEMENT CHANGE
    // =========================

    private void HandleElementChangeScroll()
    {
        if (playerMovement.IsUsingAbility) return;
    
        scrollTimer -= Time.deltaTime;
        if (scrollTimer > 0f) return;

        float scrollValue = changeElementScrollAction.action.ReadValue<float>();
        if (Mathf.Abs(scrollValue) < 0.01f) return;

        ChangeElement(scrollValue > 0 ? 1 : -1);
        scrollTimer = scrollCooldown;
    }

    private void ChangeElement(int direction)
    {
        // Hacemos un cambio circular: después del último elemento vuelve al primero.
        int elementCount = System.Enum.GetValues(typeof(ElementType)).Length;
        int newIndex     = ((int)currentElement + direction + elementCount) % elementCount;
        currentElement   = (ElementType)newIndex;

        // Al cambiar de elemento hay que refrescar tanto habilidades como interfaz.
        LoadAbilitiesForCurrentElement();
        abilitiesHUD.SetElement(currentElement);
        abilitiesHUD.SetAbilities(currentAbility1, currentAbility2, currentAbility3, currentAbility4);
        affinityHUD?.Refresh(currentElement);
        RefreshCooldownHUD();
    }

    private void LoadAbilitiesForCurrentElement()
    {
        ElementAbilitySet set = FindAbilitySet(currentElement);
        if (set == null)
        {
            currentAbility1 = currentAbility2 = currentAbility3 = currentAbility4 = null;
            return;
        }

        // Durante Control Maestro: siempre 4 habilidades, sin bloqueo por afinidad
        int availableCount = MatchController.Instance?.ShouldBypassAffinity() == true 
            ? 4 
            : (affinityData?.GetAvailableAbilityCount(mainElement, currentElement) ?? 4);

        currentAbility1 = availableCount >= 1 ? set.ability1 : null;
        currentAbility2 = availableCount >= 2 ? set.ability2 : null;
        currentAbility3 = availableCount >= 3 ? set.ability3 : null;
        currentAbility4 = availableCount >= 4 ? set.ability4 : null;
    }

    private ElementAbilitySet FindAbilitySet(ElementType element)
    {
        // Busca la configuración de habilidades asociada a un elemento concreto.
        foreach (ElementAbilitySet set in elementAbilitySets)
            if (set.element == element) return set;
        return null;
    }

    // =========================
    // HELPERS
    // =========================

    private int GetSlotIndex(AbilityData ability)
    {
        // Convierte una referencia de habilidad en su posición visual dentro de la HUD actual.
        if (ability == currentAbility1) return 0;
        if (ability == currentAbility2) return 1;
        if (ability == currentAbility3) return 2;
        if (ability == currentAbility4) return 3;
        return -1;
    }

    private void RefreshCooldownHUD()
    {
        // Reevalúa cada slot porque al cambiar de elemento pueden verse habilidades distintas.
        RefreshSlot(0, currentAbility1);
        RefreshSlot(1, currentAbility2);
        RefreshSlot(2, currentAbility3);
        RefreshSlot(3, currentAbility4);
    }

    private void RefreshSlot(int index, AbilityData ability)
    {
        if (ability == null)
        {
            abilitiesHUD.UpdateSlotCooldown(index, 0f);
            return;
        }
    
        float remaining = cooldownTimers.TryGetValue(ability, out float time) ? time : 0f;
        abilitiesHUD.UpdateSlotCooldown(index, remaining);
    }
}
