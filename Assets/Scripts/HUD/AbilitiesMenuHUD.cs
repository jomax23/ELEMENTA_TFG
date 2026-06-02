using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI component for the Abilities Menu (pause state).
/// Displays selectable elements, abilities, and detailed ability information.
/// </summary>
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

    /// <summary>
    /// Highlights the selected element button and updates the current element state.
    /// </summary>
    public void ShowElement(ElementType element)
    {
        currentElement = element;

        foreach (var button in elementButtons)
        {
            button.SetSelected(button.Element == element);
        }
    }

    /// <summary>
    /// Populates the ability buttons for the given element and selects the first one by default.
    /// </summary>
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

        // Show details for the first ability
        ShowAbilityInfo(abilities[0]); 
    }

    /// <summary>
    /// Updates the detailed info panel (icon, name, description) and highlights the selected ability button.
    /// </summary>
    public void ShowAbilityInfo(AbilityData ability)
    {
        if (ability == null) return;

        // 1. Calcular la eficiencia actual basada en la afinidad
        float efficiency = 1f;
        if (GameSession.Instance != null && GameSession.Instance.AffinityData != null)
        {
            // Si el Control Maestro está activo, la eficiencia es siempre 100% (1f)
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

        // 2. Obtener la descripción con los valores reales calculados
        string finalDescription = ability.GetFormattedDescription(efficiency);

        // 3. Actualizar la UI
        bigIcon.sprite = ability.icon;
        abilityName.text = ability.abilityName;
        abilityDescription.text = finalDescription; // <-- TEXTO DINÁMICO

        foreach (var btn in abilityButtons)
        {
            btn.SetSelected(btn.Ability == ability);
        }
    }

    /// <summary>
    /// Finds the ability configuration set for a specific element.
    /// </summary>
    private ElementAbilitySet GetSet(ElementType element)
    {
        foreach (var set in elementAbilitySets)
        {
            if (set.element == element) return set;
        }
        return null;
    }
}