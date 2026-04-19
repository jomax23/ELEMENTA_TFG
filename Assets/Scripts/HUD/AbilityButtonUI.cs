using UnityEngine;
using UnityEngine.UI;

public class AbilityButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    private AbilityData ability;
    private AbilitiesMenuController controller;

    public AbilityData Ability => ability;

    private void Awake()
    {
        controller = FindObjectOfType<AbilitiesMenuController>();
    }

    public void SetAbility(AbilityData data)
    {
        ability = data;
        icon.sprite = data.icon;
        icon.enabled = data != null;
    }

    public void OnClick()
    {
        if (ability != null)
            controller.SelectAbility(ability);
    }

    public void SetSelected(bool selected)
    {
        icon.color = selected
            ? Color.white
            : new Color(1f, 1f, 1f, 0.1f);
    }
}