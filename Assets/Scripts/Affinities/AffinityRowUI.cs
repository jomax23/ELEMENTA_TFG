using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Componente auxiliar que representa una fila de la tabla de preview de afinidad.
/// Muestra: nombre del elemento, nº de habilidades disponibles, eficiencia y cooldown penalty.
/// </summary>
public class AffinityRowUI : MonoBehaviour
{
    [SerializeField] private Image              elementIcon;
    [SerializeField] private TextMeshProUGUI    elementName;
    [SerializeField] private TextMeshProUGUI    abilitiesText;
    [SerializeField] private TextMeshProUGUI    efficiencyText;
    [SerializeField] private TextMeshProUGUI    cooldownText;

    [Header("Colors")]
    [SerializeField] private Color mainElementColor    = new Color(1f, 0.85f, 0f);
    [SerializeField] private Color normalColor         = Color.white;
    [SerializeField] private Color lockedColor         = new Color(1f, 1f, 1f, 0.3f);

    public void SetData(ElementType element, AffinityInfo info)
    {
        if (elementName != null)
            elementName.text = element.ToString().ToUpper();

        if (abilitiesText != null)
        {
            abilitiesText.text = info.availableAbilities > 0
                ? $"{info.availableAbilities}/4"
                : "BLOQUEADO";
        }

        if (efficiencyText != null)
        {
            efficiencyText.text = info.efficiency >= 1f
                ? "100%"
                : $"{(info.efficiency * 100f):0}%";
        }

        if (cooldownText != null)
        {
            cooldownText.text = info.cooldownMultiplier <= 1f
                ? "— "
                : $"+{((info.cooldownMultiplier - 1f) * 100f):0}%";
        }

        // Color de la fila según estado
        Color rowColor;
        if (info.efficiency >= 1f)
            rowColor = mainElementColor;
        else if (info.availableAbilities == 0)
            rowColor = lockedColor;
        else
            rowColor = normalColor;

        if (elementName != null)    elementName.color    = rowColor;
        if (elementIcon != null)    elementIcon.color    = rowColor;
        if (abilitiesText != null)  abilitiesText.color  = rowColor;
        if (efficiencyText != null) efficiencyText.color = rowColor;
        if (cooldownText != null)   cooldownText.color   = rowColor;
    }
}
