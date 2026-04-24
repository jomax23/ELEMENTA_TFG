using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


/// <summary>
/// Componente auxiliar adjunto a cada botón de elemento en el selector.
/// Se encarga del estado visual (seleccionado / no seleccionado).
/// </summary>
public class ElementSelectorButton : MonoBehaviour
{
    [SerializeField] private Image   backgroundImage;
    [SerializeField] private Image   elementIcon;
    [SerializeField] private TextMeshProUGUI labelText;

    private System.Action onClickCallback;
    private Color         selectedColor;
    private Color         unselectedColor;

    public void Initialize(
        ElementType element,
        Color        unselected,
        Color        selected,
        System.Action onClick)
    {
        selectedColor   = selected;
        unselectedColor = unselected;
        onClickCallback = onClick;

        if (labelText != null)
            labelText.text = element.ToString().ToUpper();

        SetSelected(false);

        // Vincular al Button de Unity
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(() => onClickCallback?.Invoke());
    }

    public void SetSelected(bool isSelected)
    {
        Color c = isSelected ? selectedColor : unselectedColor;

        if (backgroundImage != null) backgroundImage.color = c;
        if (elementIcon != null)     elementIcon.color     = c;
        if (labelText != null)       labelText.color       = c;
    }
}

