using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auxiliary component attached to each element button in the selector.
/// Handles the visual state (selected / unselected) by modifying the element icon and text.
/// </summary>
public class ElementSelectorButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image elementIcon;
    [SerializeField] private TextMeshProUGUI labelText;

    private System.Action onClickCallback;
    private Color selectedColor;
    private Color unselectedColor;
    private Button button;

    private void Awake()
    {
        // Cache the Button component to avoid runtime allocation
        button = GetComponent<Button>();
    }

    /// <summary>
    /// Initializes the button with element data, colors, and click callback.
    /// </summary>
    public void Initialize(ElementType element, Color unselected, Color selected, System.Action onClick)
    {
        selectedColor = selected;
        unselectedColor = unselected;
        onClickCallback = onClick;

        if (labelText != null)
            labelText.text = element.ToString().ToUpper();

        if (button != null)
        {
            // Disable default button transition to prevent color tinting conflicts
            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke());
        }

        SetSelected(false);
    }

    /// <summary>
    /// Updates the visual state of the button based on selection.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        Color targetColor = isSelected ? selectedColor : unselectedColor;

        if (elementIcon != null) elementIcon.color = targetColor;
        if (labelText != null) labelText.color = targetColor;
    }
}