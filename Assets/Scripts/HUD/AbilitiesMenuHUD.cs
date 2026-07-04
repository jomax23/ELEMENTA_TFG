using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// UI component for the Abilities Menu (pause state).
// Displays selectable elements, abilities, and detailed ability information.
public class AbilitiesMenuHUD : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private List<ElementButtonUI> elementButtons;

    [Header("Abilities")]
    [SerializeField] private List<AbilityButtonUI> abilityButtons;

    [Header("Ability Info")]
    [SerializeField] private Image bigIcon;
    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private TextMeshProUGUI abilityDescription;

    [Header("Data")]
    [SerializeField] private ElementAbilitySet[] elementAbilitySets;

    private ElementType currentElement;

    public void ShowElement(ElementType element)
    {
        currentElement = element;
        foreach (var button in elementButtons)
            button.SetSelected(button.Element == element);
    }

    public void ShowAbilities(ElementType element)
    {
        ElementAbilitySet set = GetSet(element);
        if (set == null) return;

        AbilityData[] abilities = { set.ability1, set.ability2, set.ability3, set.ability4 };
        
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            abilityButtons[i].SetAbility(abilities[i]);
            // Select the first ability by default to populate the info panel
            abilityButtons[i].SetSelected(i == 0); 
        }
        
        ShowAbilityInfo(abilities[0]); 
    }

    public void ShowAbilityInfo(AbilityData ability)
    {
        if (ability == null) return;

        // 1. Calculate actual efficiency. If Master Control is active, bypass affinity penalties.
        float efficiency = 1f;
        if (GameSession.Instance != null && GameSession.Instance.AffinityData != null)
        {
            if (MatchController.Instance != null && MatchController.Instance.ShouldBypassAffinity())
            {
                efficiency = 1f;
            }
            else
            {
                efficiency = GameSession.Instance.AffinityData.GetEfficiency(
                    GameSession.Instance.MainElement, 
                    ability.element
                );
            }
        }

        // 2. Get the description with the real calculated values
        string finalDescription = ability.GetFormattedDescription(efficiency);

        // 3. Update the UI
        bigIcon.sprite = ability.icon;
        abilityName.text = ability.abilityName;
        abilityDescription.text = finalDescription;

        foreach (var btn in abilityButtons)
            btn.SetSelected(btn.Ability == ability);
    }

    private ElementAbilitySet GetSet(ElementType element)
    {
        foreach (var set in elementAbilitySets)
            if (set.element == element) return set;
            
        return null;
    }
}