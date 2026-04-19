using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class PlayerAbilities : MonoBehaviour
{
    [Header("Element")]
    [SerializeField] private ElementType currentElement;
    public ElementType CurrentElement => currentElement;

    [Header("Abilities")]
    [SerializeField] private AbilityData abilitySlot1;
    [SerializeField] private AbilityData abilitySlot2;
    [SerializeField] private AbilityData abilitySlot3;
    [SerializeField] private AbilityData abilitySlot4;
    [SerializeField] private AbilitiesHUD abilitiesHUD;

    [Header("Input")]
    [SerializeField] private InputActionReference ability1Action;
    [SerializeField] private InputActionReference ability2Action;
    [SerializeField] private InputActionReference ability3Action;
    [SerializeField] private InputActionReference ability4Action;
    [SerializeField] private InputActionReference changeElementScrollAction;
    
    [Header("Element Ability Sets")]
    [SerializeField] private ElementAbilitySet[] elementAbilitySets;
    
    
    
    private Dictionary<AbilityData, float> cooldownTimers = new Dictionary<AbilityData, float>();


// Slots activos (runtime)
    private AbilityData currentAbility1;
    private AbilityData currentAbility2;
    private AbilityData currentAbility3;
    private AbilityData currentAbility4;
    
    [SerializeField] private float scrollCooldown = 0.15f;
    private float scrollTimer;
    
    private void Start()
    {
        LoadAbilitiesForCurrentElement();
        
        abilitiesHUD.SetElement(currentElement);

        abilitiesHUD.SetAbilities(
            currentAbility1,
            currentAbility2,
            currentAbility3,
            currentAbility4
        );
        
        InitializeCooldowns();
    }

    
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
        StopAllCoroutines();
    }

    private void Update()
    {
        UpdateCooldowns();
        HandleElementChangeScroll();

        if (ability1Action.action.WasPressedThisFrame())
            TryUseAbility(currentAbility1);

        if (ability2Action.action.WasPressedThisFrame())
            TryUseAbility(currentAbility2);
        
        if (ability3Action.action.WasPressedThisFrame())
            TryUseAbility(currentAbility3);
        
        if (ability4Action.action.WasPressedThisFrame())
            TryUseAbility(currentAbility4);
    }

    private void InitializeCooldowns()
    {
        cooldownTimers.Clear();

        foreach (var set in elementAbilitySets)
        {
            if (set.ability1 != null && !cooldownTimers.ContainsKey(set.ability1))
                cooldownTimers.Add(set.ability1, 0f);

            if (set.ability2 != null && !cooldownTimers.ContainsKey(set.ability2))
                cooldownTimers.Add(set.ability2, 0f);
            
            if (set.ability3 != null && !cooldownTimers.ContainsKey(set.ability3))
                cooldownTimers.Add(set.ability3, 0f);
            
            if (set.ability4 != null && !cooldownTimers.ContainsKey(set.ability4))
                cooldownTimers.Add(set.ability4, 0f);
        }
    }
    
    private void UpdateCooldowns()
    {
        List<AbilityData> keys = new List<AbilityData>(cooldownTimers.Keys);

        foreach (var ability in keys)
        {
            if (cooldownTimers[ability] > 0f)
            {
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
    }



    private void HandleElementChangeScroll()
    {
        scrollTimer -= Time.deltaTime;

        if (scrollTimer > 0f)
            return;

        float scrollValue = changeElementScrollAction.action.ReadValue<float>();

        if (Mathf.Abs(scrollValue) < 0.01f)
            return;

        int direction = scrollValue > 0 ? 1 : -1;
        ChangeElement(direction);

        scrollTimer = scrollCooldown;
    }

    private void ChangeElement(int direction)
    {
        int elementCount = System.Enum.GetValues(typeof(ElementType)).Length;

        int newIndex = ((int)currentElement + direction) % elementCount;
        if (newIndex < 0)
            newIndex += elementCount;

        currentElement = (ElementType)newIndex;

        LoadAbilitiesForCurrentElement();
        
        abilitiesHUD.SetElement(currentElement);

        abilitiesHUD.SetAbilities(
            currentAbility1,
            currentAbility2,
            currentAbility3,
            currentAbility4
        );
        
        RefreshCooldownHUD();

        Debug.Log($"Elemento actual: {currentElement}");
    }


    private void TryUseAbility(AbilityData ability)
    {
        if (ability == null)
            return;

        if (cooldownTimers[ability] > 0f)
        {
            Debug.Log($"Habilidad en cooldown: {ability.abilityName}");
            return;
        }

        ability.Activate(gameObject);
        cooldownTimers[ability] = ability.cooldown;

        int slotIndex = GetSlotIndex(ability);
        if (slotIndex != -1)
            abilitiesHUD.SetCooldown(slotIndex, true);
    }

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
        // Slot 0
        if (currentAbility1 != null &&
            cooldownTimers.ContainsKey(currentAbility1) &&
            cooldownTimers[currentAbility1] > 0f)
        {
            abilitiesHUD.SetCooldown(0, true);
        }
        else
        {
            abilitiesHUD.SetCooldown(0, false);
        }

        // Slot 1
        if (currentAbility2 != null &&
            cooldownTimers.ContainsKey(currentAbility2) &&
            cooldownTimers[currentAbility2] > 0f)
        {
            abilitiesHUD.SetCooldown(1, true);
        }
        else
        {
            abilitiesHUD.SetCooldown(1, false);
        }
        
        // Slot 2
        if (currentAbility3 != null &&
            cooldownTimers.ContainsKey(currentAbility3) &&
            cooldownTimers[currentAbility3] > 0f)
        {
            abilitiesHUD.SetCooldown(2, true);
        }
        else
        {
            abilitiesHUD.SetCooldown(2, false);
        }
        
        // Slot 3
        if (currentAbility4 != null &&
            cooldownTimers.ContainsKey(currentAbility4) &&
            cooldownTimers[currentAbility4] > 0f)
        {
            abilitiesHUD.SetCooldown(3, true);
        }
        else
        {
            abilitiesHUD.SetCooldown(3, false);
        }
    }

    
    private void LoadAbilitiesForCurrentElement()
    {
        foreach (var set in elementAbilitySets)
        {
            if (set.element == currentElement)
            {
                currentAbility1 = set.ability1;
                currentAbility2 = set.ability2;
                currentAbility3 = set.ability3;
                currentAbility4 = set.ability4;
                return;
            }
        }

        Debug.LogWarning(
            $"No hay habilidades asignadas para el elemento {currentElement}"
        );
    }

}