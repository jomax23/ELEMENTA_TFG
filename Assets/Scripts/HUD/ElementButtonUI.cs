using UnityEngine;
using UnityEngine.UI;

// Handles the visual state and click events for an element selection button.
public class ElementButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;

    [Header("Data")]
    [SerializeField] private ElementType element;
    private AbilitiesMenuController controller;

    public ElementType Element => element;

    private void Awake()
    {
        // Cache the controller to avoid searching the scene every time the button is clicked
        controller = FindFirstObjectByType<AbilitiesMenuController>();
    }

    public void OnClick()
    {
        controller?.SelectElement(element);
    }

    // Dims the icon to provide clear visual feedback when deselected
    public void SetSelected(bool selected)
    {
        Color c = icon.color;
        c.a = selected ? 1f : 0.1f;
        icon.color = c;
    }
}