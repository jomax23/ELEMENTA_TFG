using UnityEngine;
using UnityEngine.UI;

public class ElementButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private ElementType element;

    private AbilitiesMenuController controller;

    public ElementType Element => element;

    private void Awake()
    {
        controller = FindFirstObjectByType<AbilitiesMenuController>();
    }

    public void OnClick()
    {
        controller.SelectElement(element);
    }

    public void SetSelected(bool selected)
    {
        icon.color = selected
            ? Color.white
            : new Color(1f, 1f, 1f, 0.1f);
    }
}