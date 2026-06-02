using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Optional HUD component that displays the current element's affinity status to the player.
/// Placed next to the AbilitiesHUD to provide immediate visual feedback when switching to a penalized element.
/// 
/// UNITY SETUP:
/// 1. Add this component to a GameObject in the gameplay HUD Canvas.
/// 2. Assign the fields in the Inspector.
/// 3. Call Refresh(currentElement) from PlayerAbilities whenever the element changes.
/// </summary>
public class AffinityHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject affinityPenaltyPanel;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI efficiencyLabel;
    [SerializeField] private TextMeshProUGUI abilitiesLabel;
    [SerializeField] private TextMeshProUGUI cooldownLabel;

    [Header("Colors")]
    [SerializeField] private Color fullEfficiencyColor = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private Color partialColor = new Color(1f, 0.8f, 0.1f);
    [SerializeField] private Color lockedColor = new Color(1f, 0.25f, 0.25f);

    private void Awake()
    {
        if (affinityPenaltyPanel != null)
            affinityPenaltyPanel.SetActive(false);
    }

    /// <summary>
    /// Refreshes the affinity HUD when the active element changes.
    /// Called from PlayerAbilities.ChangeElement / LoadAbilitiesForCurrentElement.
    /// </summary>
    public void Refresh(ElementType currentElement)
    {
        if (GameSession.Instance?.AffinityData == null)
        {
            Hide();
            return;
        }

        ElementType mainElement = GameSession.Instance.MainElement;
        AffinityInfo info = GameSession.Instance.AffinityData.GetAffinityInfo(mainElement, currentElement);

        // If it's the main element, do not show penalty
        if (info.efficiency >= 1f)
        {
            Hide();
            return;
        }

        // If completely locked or penalized: show panel
        if (affinityPenaltyPanel != null)
            affinityPenaltyPanel.SetActive(true);

        Color textColor = info.availableAbilities == 0 ? lockedColor : partialColor;

        if (efficiencyLabel != null)
        {
            efficiencyLabel.text = info.availableAbilities == 0 ? "LOCKED" : $"Efficiency: {info.efficiency * 100f:0}%";
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