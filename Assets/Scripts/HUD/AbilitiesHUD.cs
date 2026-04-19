using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AbilitiesHUD : MonoBehaviour
{
    [Header("Ability Slots")]
    [SerializeField] private List<Image> abilitySlots;

    [Header("Element Icon")]
    [SerializeField] private Image elementIcon;

    [Header("Element Icons")]
    [SerializeField] private Sprite fireElementIcon;
    [SerializeField] private Sprite waterElementIcon;
    [SerializeField] private Sprite earthElementIcon;
    [SerializeField] private Sprite airElementIcon;

    private Color normalColor = Color.white;
    private Color cooldownColor = new Color(1f, 1f, 1f, 0.4f);

    // =========================
    // 🔥 HABILIDADES
    // =========================
    public void SetAbilities(
        AbilityData ability1,
        AbilityData ability2,
        AbilityData ability3,
        AbilityData ability4
    )
    {
        SetSlot(0, ability1);
        SetSlot(1, ability2);
        SetSlot(2, ability3);
        SetSlot(3, ability4);
    }

    private void SetSlot(int index, AbilityData ability)
    {
        if (index < 0 || index >= abilitySlots.Count)
            return;

        Image slot = abilitySlots[index];

        if (ability == null || ability.icon == null)
        {
            slot.enabled = false;
            return;
        }

        slot.enabled = true;
        slot.sprite = ability.icon;
        slot.color = normalColor;
    }

    public void SetCooldown(int slotIndex, bool onCooldown)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Count)
            return;

        abilitySlots[slotIndex].color =
            onCooldown ? cooldownColor : normalColor;
    }

    // =========================
    // 🌍 ELEMENTO ACTUAL
    // =========================
    public void SetElement(ElementType element)
    {
        if (elementIcon == null)
            return;

        switch (element)
        {
            case ElementType.Fire:
                elementIcon.sprite = fireElementIcon;
                break;

            case ElementType.Water:
                elementIcon.sprite = waterElementIcon;
                break;

            case ElementType.Earth:
                elementIcon.sprite = earthElementIcon;
                break;

            case ElementType.Air:
                elementIcon.sprite = airElementIcon;
                break;
        }
    }
}
