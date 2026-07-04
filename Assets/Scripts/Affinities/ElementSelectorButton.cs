using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Handles the visual state (selected/unselected) for a single element button.
// We manually control the colors to avoid fighting Unity's default Button transition tinting.
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
        button = GetComponent<Button>();
    }

    public void Initialize(ElementType element, Color unselected, Color selected, System.Action onClick)
    {
        selectedColor = selected;
        unselectedColor = unselected;
        onClickCallback = onClick;

        if (labelText != null)
            labelText.text = element.ToString().ToUpper();

        if (button != null)
        {
            // Disable default color tinting so our manual SetSelected colors don't get overridden
            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke());
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        Color targetColor = isSelected ? selectedColor : unselectedColor;
        
        if (elementIcon != null) elementIcon.color = targetColor;
        if (labelText != null) labelText.color = targetColor;
    }
}