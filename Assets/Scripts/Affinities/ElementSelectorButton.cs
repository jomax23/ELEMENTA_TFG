using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente auxiliar adjunto a cada botón de elemento en el selector.
/// Se encarga del estado visual (seleccionado / no seleccionado).
/// Solo modifica el icono del elemento y el texto.
/// </summary>
public class ElementSelectorButton : MonoBehaviour
{
    [SerializeField] private Image elementIcon;
    [SerializeField] private TextMeshProUGUI labelText;

    private System.Action onClickCallback;
    private Color selectedColor;
    private Color unselectedColor;

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

        // Evita cualquier tintado automático del Button sobre gráficos de fondo.
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.transition = Selectable.Transition.None;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onClickCallback?.Invoke());
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        Color c = isSelected ? selectedColor : unselectedColor;

        if (elementIcon != null) elementIcon.color = c;
        if (labelText != null)   labelText.color   = c;
    }
}