using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component representing an Element selection button.
/// Handles click events and visual feedback for the selected state.
/// </summary>
public class ElementButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;

    [Header("Data")]
    [SerializeField] private ElementType element;

    private AbilitiesMenuController controller;

    /// <summary>Gets the element type this button represents.</summary>
    public ElementType Element => element;

    private void Awake()
    {
        // Cache the controller to avoid searching every time the button is clicked
        controller = FindFirstObjectByType<AbilitiesMenuController>();
    }

    /// <summary>Called when the button is clicked.</summary>
    public void OnClick()
    {
        controller?.SelectElement(element);
    }

    /// <summary>Updates the visual state to indicate if this element is currently selected.</summary>
    public void SetSelected(bool selected)
    {
        // Dims the icon alpha when deselected to provide clear visual feedback
        Color c = icon.color;
        c.a = selected ? 1f : 0.1f;
        icon.color = c;
    }
}