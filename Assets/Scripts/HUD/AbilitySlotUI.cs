using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Displays detailed info for a specific ability slot (icon, name, description).
public class AbilitySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void SetAbility(AbilityData ability)
    {
        if (ability == null)
        {
            ClearSlot();
            return;
        }

        nameText.text = ability.abilityName;
        descriptionText.text = ability.descriptionTemplate;

        if (ability.icon != null)
        {
            icon.sprite = ability.icon;
            icon.enabled = true;
        }
        else
        {
            icon.enabled = false;
        }
    }

    private void ClearSlot()
    {
        icon.enabled = false;
        nameText.text = string.Empty;
        descriptionText.text = string.Empty;
    }
}