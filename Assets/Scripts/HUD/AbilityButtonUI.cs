using UnityEngine;
using UnityEngine.UI;

// UI component representing an Ability selection button in the pause menu.
// Displays the ability icon and handles click events for the menu controller.
public class AbilityButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;

    private AbilityData ability;
    private AbilitiesMenuController controller;

    public AbilityData Ability => ability;

    private void Awake()
    {
        // Cache the controller to avoid searching the scene every time the button is clicked
        controller = FindFirstObjectByType<AbilitiesMenuController>();
    }

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
            icon.enabled = false;
        }
    }

    public void OnClick()
    {
        if (ability != null)
            controller?.SelectAbility(ability);
    }

    // Dims the icon alpha when deselected to provide clear visual feedback
    public void SetSelected(bool selected)
    {
        Color c = icon.color;
        c.a = selected ? 1f : 0.1f;
        icon.color = c;
    }
}