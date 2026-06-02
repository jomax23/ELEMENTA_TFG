using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component that displays detailed information about a specific ability
/// (icon, name, and description) within an ability slot.
/// </summary>
public class AbilitySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    /// <summary>Populates the slot with the provided ability data.</summary>
    public void SetAbility(AbilityData ability)
    {
        if (ability == null)
        {
            ClearSlot();
            return;
        }

        nameText.text = ability.abilityName;
        descriptionText.text = ability.description;

        if (ability.icon != null)
        {
            icon.sprite = ability.icon;
            icon.enabled = true;
        }
        else
        {
            // Hides the icon if the ability has no sprite assigned
            icon.enabled = false;
        }
    }

    /// <summary>Clears all text and hides the icon.</summary>
    private void ClearSlot()
    {
        icon.enabled = false;
        nameText.text = string.Empty;
        descriptionText.text = string.Empty;
    }
}