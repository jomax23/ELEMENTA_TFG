using UnityEngine;
using UnityEngine.UI;
using TMPro;

// In-game HUD showing affinity penalties for the currently selected element.
// Hides itself entirely if the player is using their main element (no penalties).
public class AffinityHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject affinityPenaltyPanel;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI efficiencyLabel;
    [SerializeField] private TextMeshProUGUI abilitiesLabel;
    [SerializeField] private TextMeshProUGUI cooldownLabel;

    [Header("Colors")]
    [SerializeField] private Color fullEfficiencyColor = new Color(0.3f, 1f, 0.3f); // Green
    [SerializeField] private Color partialColor = new Color(1f, 0.8f, 0.1f);        // Yellow
    [SerializeField] private Color lockedColor = new Color(1f, 0.25f, 0.25f);       // Red

    private void Awake()
    {
        if (affinityPenaltyPanel != null)
            affinityPenaltyPanel.SetActive(false);
    }

    // Called by PlayerAbilities whenever the active element changes
    public void Refresh(ElementType currentElement)
    {
        if (GameSession.Instance?.AffinityData == null)
        {
            Hide();
            return;
        }

        ElementType mainElement = GameSession.Instance.MainElement;
        AffinityInfo info = GameSession.Instance.AffinityData.GetAffinityInfo(mainElement, currentElement);

        // If it's the main element, there are no penalties, so hide the HUD
        if (info.efficiency >= 1f)
        {
            Hide();
            return;
        }

        // Show the panel and color the text based on severity
        if (affinityPenaltyPanel != null)
            affinityPenaltyPanel.SetActive(true);

        bool isLocked = info.availableAbilities == 0;
        Color textColor = isLocked ? lockedColor : partialColor;

        if (efficiencyLabel != null)
        {
            efficiencyLabel.text = isLocked ? "LOCKED" : $"Efficiency: {info.efficiency * 100f:0}%";
            efficiencyLabel.color = textColor;
        }
        
        if (abilitiesLabel != null)
        {
            abilitiesLabel.text = $"Abilities: {info.availableAbilities}/4";
            abilitiesLabel.color = textColor;
        }
        
        if (cooldownLabel != null)
        {
            cooldownLabel.text = info.cooldownMultiplier > 1f ? $"Cooldown: +{(info.cooldownMultiplier - 1f) * 100f:0}%" : "Cooldown: —";
            cooldownLabel.color = textColor;
        }
    }

    private void Hide()
    {
        if (affinityPenaltyPanel != null)
            affinityPenaltyPanel.SetActive(false);
    }
}