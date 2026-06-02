using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralizes player abilities logic: active element, available skills, cooldowns, and HUD updates.
/// </summary>
public class PlayerAbilities : MonoBehaviour
{
    // ── Components ─────────────────────────────────────────────────────────
    private PlayerMovement playerMovement;

    // ── Element ────────────────────────────────────────────────────────────
    [Header("Element")]
    [SerializeField] private ElementType currentElement;
    public ElementType CurrentElement => currentElement;
    private ElementType mainElement;

    // ── HUD ────────────────────────────────────────────────────────────────
    [Header("HUD")]
    [SerializeField] private AbilitiesHUD abilitiesHUD;
    [SerializeField] private AffinityHUD affinityHUD;

    // ── Input ──────────────────────────────────────────────────────────────
    [Header("Input")]
    [SerializeField] private InputActionReference ability1Action;
    [SerializeField] private InputActionReference ability2Action;
    [SerializeField] private InputActionReference ability3Action;
    [SerializeField] private InputActionReference ability4Action;
    [SerializeField] private InputActionReference changeElementScrollAction;

    // ── Ability Sets ───────────────────────────────────────────────────────
    [Header("Element Ability Sets")]
    [SerializeField] private ElementAbilitySet[] elementAbilitySets;
    
    private AbilityData currentAbility1;
    private AbilityData currentAbility2;
    private AbilityData currentAbility3;
    private AbilityData currentAbility4;

    // ── Cooldowns ──────────────────────────────────────────────────────────
    private Dictionary<AbilityData, float> cooldownTimers = new();
    private AbilityData[] cooldownKeys; // Cached to prevent allocation per frame

    // ── Active Ability ─────────────────────────────────────────────────────
    private Coroutine activeAbilityCoroutine;
    private AbilityData activeAbility;

    // ── Affinity ───────────────────────────────────────────────────────────
    private AffinityData affinityData;

    // ── Element Scroll ─────────────────────────────────────────────────────
    [Header("Scroll")]
    [SerializeField] private float scrollCooldown = 0.15f;
    private float scrollTimer;

    // =========================================================================
    // INITIALIZATION
    // =========================================================================

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        if (GameSession.Instance != null)
        {
            mainElement = GameSession.Instance.MainElement;
            affinityData = GameSession.Instance.AffinityData;
            currentElement = mainElement;

            if (MatchController.Instance != null)
            {
                MatchController.Instance.OnMasterControlStart += RefreshAbilitiesOnStateChange;
                MatchController.Instance.OnMasterControlEnd += RefreshAbilitiesOnStateChange;
            }
        }
        else
        {
            Debug.LogWarning("[PlayerAbilities] GameSession not found. No affinity applied.", this);
            mainElement = currentElement;
            affinityData = null;
        }

        LoadAbilitiesForCurrentElement();
        InitializeCooldowns();

        abilitiesHUD.SetElement(currentElement);
        abilitiesHUD.SetAbilities(currentAbility1, currentAbility2, currentAbility3, currentAbility4);
        affinityHUD?.Refresh(currentElement);

        playerMovement.OnStunApplied += HandleStunInterrupt;
    }

    private void OnDestroy()
    {
        if (playerMovement != null)
            playerMovement.OnStunApplied -= HandleStunInterrupt;

        if (MatchController.Instance != null)
        {
            MatchController.Instance.OnMasterControlStart -= RefreshAbilitiesOnStateChange;
            MatchController.Instance.OnMasterControlEnd -= RefreshAbilitiesOnStateChange;
        }
    }

    private void RefreshAbilitiesOnStateChange()
    {
        LoadAbilitiesForCurrentElement();
        abilitiesHUD?.SetAbilities(currentAbility1, currentAbility2, currentAbility3, currentAbility4);
    }

    // =========================================================================
    // INPUT & UPDATE
    // =========================================================================

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
        ForceInterrupt();
    }

    private void Update()
    {
        UpdateCooldowns();
        HandleElementChangeScroll();

        if (playerMovement.IsUsingAbility) return;

        if (ability1Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility1);
        if (ability2Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility2);
        if (ability3Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility3);
        if (ability4Action.action.WasPressedThisFrame()) TryUseAbility(currentAbility4);
    }

    // =========================================================================
    // STUN / INTERRUPTION
    // =========================================================================

    private void HandleStunInterrupt() => ForceInterrupt();

    private void ForceInterrupt()
    {
        if (activeAbilityCoroutine != null)
        {
            StopCoroutine(activeAbilityCoroutine);
            activeAbilityCoroutine = null;
        }

        if (activeAbility != null)
        {
            activeAbility.Cancel(gameObject);
            activeAbility = null;
        }

        playerMovement.CancelAbilityAnimation();
    }

    // =========================================================================
    // ACTIVATION
    // =========================================================================

    private void TryUseAbility(AbilityData ability)
    {
        if (ability == null) return;

        if (!playerMovement.IsGrounded) return;

        if (cooldownTimers.TryGetValue(ability, out float remaining) && remaining > 0f) return;

        float cooldownMult = GetCooldownMultiplierForAbility(ability);
        float finalCooldown = ability.cooldown * cooldownMult;
        cooldownTimers[ability] = finalCooldown;

        int slotIndex = GetSlotIndex(ability);
        if (slotIndex != -1)
        {
            abilitiesHUD.SetCooldown(slotIndex, true);
            abilitiesHUD.StartCooldown(slotIndex, finalCooldown);
        }

        if (!string.IsNullOrEmpty(ability.animationStateName))
            playerMovement.PlayAbilityAnimation(ability.animationStateName);

        activeAbility = ability;
        activeAbilityCoroutine = StartCoroutine(AbilityLifecycle(ability));
    }

    private IEnumerator AbilityLifecycle(AbilityData ability)
    {
        if (ability.activationDelay > 0f)
            yield return new WaitForSeconds(ability.activationDelay);

        float efficiency = GetEfficiencyForAbility(ability);
        ability.ActivateWithAudio(gameObject, efficiency);

        float lockRemaining = ability.totalAnimationDuration - ability.activationDelay;
        if (lockRemaining > 0f)
            yield return new WaitForSeconds(lockRemaining);

        activeAbility = null;
        activeAbilityCoroutine = null;
        playerMovement.CancelAbilityAnimation();
    }

    // =========================================================================
    // AFFINITY
    // =========================================================================

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

    // =========================================================================
    // COOLDOWNS (Zero-allocation iteration)
    // =========================================================================

    private void InitializeCooldowns()
    {
        cooldownTimers.Clear();

        foreach (ElementAbilitySet set in elementAbilitySets)
        {
            TryRegisterCooldown(set.ability1);
            TryRegisterCooldown(set.ability2);
            TryRegisterCooldown(set.ability3);
            TryRegisterCooldown(set.ability4);
        }

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
            if (cooldownTimers[ability] < 0f) cooldownTimers[ability] = 0f;
        }

        RefreshCooldownHUD();
    }

    // =========================================================================
    // ELEMENT CHANGE
    // =========================================================================

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
        int elementCount = System.Enum.GetValues(typeof(ElementType)).Length;
        int newIndex = ((int)currentElement + direction + elementCount) % elementCount;
        currentElement = (ElementType)newIndex;

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
        foreach (ElementAbilitySet set in elementAbilitySets)
            if (set.element == element) return set;
        return null;
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

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
        if (ability == null)
        {
            abilitiesHUD.UpdateSlotCooldown(index, 0f);
            return;
        }

        float remaining = cooldownTimers.TryGetValue(ability, out float time) ? time : 0f;
        abilitiesHUD.UpdateSlotCooldown(index, remaining);
    }
}