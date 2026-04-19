using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void SetAbility(AbilityData ability)
    {
        if (ability == null)
        {
            icon.enabled = false;
            nameText.text = "";
            descriptionText.text = "";
            return;
        }

        nameText.text = ability.abilityName;
        descriptionText.text = ability.description;

        if (ability.icon != null)
        {
            icon.enabled = true;
            icon.sprite = ability.icon;
        }
        else
        {
            icon.enabled = false; // o sprite por defecto
        }
    }

}