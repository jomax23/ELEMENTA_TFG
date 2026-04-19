using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    // ================= ELEMENTOS =================
    public void ShowElement(ElementType element)
    {
        currentElement = element;

        foreach (var button in elementButtons)
            button.SetSelected(button.Element == element);
    }

    // ================= HABILIDADES =================
    public void ShowAbilities(ElementType element)
    {
        ElementAbilitySet set = GetSet(element);
        if (set == null) return;

        AbilityData[] abilities =
        {
            set.ability1,
            set.ability2,
            set.ability3,
            set.ability4
        };

        for (int i = 0; i < abilityButtons.Count; i++)
        {
            abilityButtons[i].SetAbility(abilities[i]);
            abilityButtons[i].SetSelected(i == 0);
        }

        ShowAbilityInfo(abilities[0]);
    }

    public void ShowAbilityInfo(AbilityData ability)
    {
        if (ability == null) return;

        bigIcon.sprite = ability.icon;
        abilityName.text = ability.abilityName;
        abilityDescription.text = ability.description;

        foreach (var btn in abilityButtons)
            btn.SetSelected(btn.Ability == ability);
    }

    private ElementAbilitySet GetSet(ElementType element)
    {
        foreach (var set in elementAbilitySets)
            if (set.element == element)
                return set;

        return null;
    }
}
