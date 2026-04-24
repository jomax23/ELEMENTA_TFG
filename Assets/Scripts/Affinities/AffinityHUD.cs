using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD opcional que muestra al jugador el estado de afinidad del elemento actual.
///
/// Se coloca junto al AbilitiesHUD para dar feedback visual inmediato
/// cuando el jugador cambia a un elemento penalizado.
///
/// ─── SETUP EN UNITY ────────────────────────────────────────────────────────
/// 1. Añade este componente a un GameObject en el Canvas del HUD de partida.
/// 2. Asigna los campos en el Inspector.
/// 3. Llama a Refresh(currentElement) desde PlayerAbilities cada vez que cambia el elemento.
/// ───────────────────────────────────────────────────────────────────────────
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
    [SerializeField] private Color fullEfficiencyColor  = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private Color partialColor         = new Color(1f, 0.8f, 0.1f);
    [SerializeField] private Color lockedColor          = new Color(1f, 0.25f, 0.25f);

    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (affinityPenaltyPanel != null)
            affinityPenaltyPanel.SetActive(false);
    }

    /// <summary>
    /// Refresca el HUD de afinidad al cambiar de elemento activo.
    /// Llamado desde PlayerAbilities.ChangeElement / LoadAbilitiesForCurrentElement.
    /// </summary>
    public void Refresh(ElementType currentElement)
    {
        if (GameSession.Instance?.AffinityData == null)
        {
            Hide();
            return;
        }

        ElementType mainElement = GameSession.Instance.MainElement;
        AffinityInfo info       = GameSession.Instance.AffinityData.GetAffinityInfo(mainElement, currentElement);

        // Si es el elemento principal no mostrar penalización
        if (info.efficiency >= 1f)
        {
            Hide();
            return;
        }

        // Si está completamente bloqueado o tiene penalización: mostrar panel
        if (affinityPenaltyPanel != null)
            affinityPenaltyPanel.SetActive(true);

        Color c = info.availableAbilities == 0
            ? lockedColor
            : partialColor;

        if (efficiencyLabel != null)
        {
            efficiencyLabel.text  = info.availableAbilities == 0
                ? "BLOQUEADO"
                : $"Eficiencia: {info.efficiency * 100f:0}%";
            efficiencyLabel.color = c;
        }

        if (abilitiesLabel != null)
        {
            abilitiesLabel.text  = $"Habilidades: {info.availableAbilities}/4";
            abilitiesLabel.color = c;
        }

        if (cooldownLabel != null)
        {
            cooldownLabel.text  = info.cooldownMultiplier > 1f
                ? $"Cooldown: +{(info.cooldownMultiplier - 1f) * 100f:0}%"
                : "Cooldown: —";
            cooldownLabel.color = c;
        }
    }

    private void Hide()
    {
        if (affinityPenaltyPanel != null)
            affinityPenaltyPanel.SetActive(false);
    }
}