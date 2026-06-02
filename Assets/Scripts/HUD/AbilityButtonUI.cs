using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component representing an Ability selection button.
/// Displays the ability icon and handles click events for the menu controller.
/// </summary>
public class AbilityButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;

    private AbilityData ability;
    private AbilitiesMenuController controller;

    /// <summary>Gets the ability data assigned to this button.</summary>
    public AbilityData Ability => ability;

    private void Awake()
    {
        // Cache the controller to avoid searching every time the button is clicked
        controller = FindFirstObjectByType<AbilitiesMenuController>();
    }

    /// <summary>Assigns ability data to this button and updates the icon.</summary>
    public void SetAbility(AbilityData data)
    {
        ability = data;

        if (data != null)
        {
            icon.sprite = data.icon;
            icon.enabled = true;
        }
        else
        {
            // Hide the icon if no ability is assigned
            icon.enabled = false;
        }
    }

    /// <summary>Called when the button is clicked.</summary>
    public void OnClick()
    {
        if (ability != null)
        {
            controller?.SelectAbility(ability);
        }
    }

    /// <summary>Updates the visual state to indicate if this ability is currently selected.</summary>
    public void SetSelected(bool selected)
    {
        // Dims the icon alpha when deselected to provide clear visual feedback
        Color c = icon.color;
        c.a = selected ? 1f : 0.1f;
        icon.color = c;
    }
}