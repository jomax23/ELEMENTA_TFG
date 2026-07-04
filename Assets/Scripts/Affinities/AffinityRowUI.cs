using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Single row in the affinity preview table. 
// Handles text formatting and color-coding based on the element's affinity state.
public class AffinityRowUI : MonoBehaviour
{
    [SerializeField] private Image elementIcon;
    [SerializeField] private TextMeshProUGUI elementName;
    [SerializeField] private TextMeshProUGUI abilitiesText;
    [SerializeField] private TextMeshProUGUI efficiencyText;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("Colors")]
    [SerializeField] private Color mainElementColor = new Color(1f, 0.85f, 0f); // Gold
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.3f);  // Dimmed

    public void SetData(ElementType element, AffinityInfo info)
    {
        // Format text
        if (elementName != null) elementName.text = element.ToString().ToUpper();
        if (abilitiesText != null) abilitiesText.text = info.availableAbilities > 0 ? $"{info.availableAbilities}/4" : "0/4";
        if (efficiencyText != null) efficiencyText.text = info.efficiency >= 1f ? "100%" : $"{(info.efficiency * 100f):0}%";
        
        // Only show cooldown penalty if it's actually higher than normal (1.0x)
        if (cooldownText != null) 
            cooldownText.text = info.cooldownMultiplier <= 1f ? "—" : $"+{((info.cooldownMultiplier - 1f) * 100f):0}%";

        // Determine row color based on affinity state
        Color rowColor;
        if (info.efficiency >= 1f)
            rowColor = mainElementColor;      // Highlight the player's main element
        else if (info.availableAbilities == 0)
            rowColor = lockedColor;           // Dim completely locked elements
        else
            rowColor = normalColor;           // Standard white for penalized but usable elements

        // Apply color to all text and icons in the row
        if (elementName != null) elementName.color = rowColor;
        if (elementIcon != null) elementIcon.color = rowColor;
        if (abilitiesText != null) abilitiesText.color = rowColor;
        if (efficiencyText != null) efficiencyText.color = rowColor;
        if (cooldownText != null) cooldownText.color = rowColor;
    }
}