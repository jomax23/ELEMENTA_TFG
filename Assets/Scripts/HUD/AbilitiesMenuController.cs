using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the Abilities Menu (pause menu).
/// Handles menu toggling, time scale pausing, and routes UI selections to the HUD.
/// </summary>
public class AbilitiesMenuController : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private AbilitiesMenuHUD menuHUD;
    [SerializeField] private PlayerAbilities playerAbilities;

    [Header("Input")]
    [SerializeField] private InputActionReference openMenuAction;

    private bool isOpen;
    private ElementType currentElement;
    private AbilityData currentAbility;

    private void OnEnable() => openMenuAction.action.Enable();
    private void OnDisable() => openMenuAction.action.Disable();

    private void Update()
    {
        if (openMenuAction.action.WasPressedThisFrame())
        {
            ToggleMenu();
        }
    }

    /// <summary>
    /// Toggles the menu visibility and pauses/unpauses the game time.
    /// </summary>
    private void ToggleMenu()
    {
        isOpen = !isOpen;
        menuRoot.SetActive(isOpen);

        if (isOpen)
        {
            // Pause the game
            Time.timeScale = 0f;

            // Sync HUD with current player state
            currentElement = playerAbilities.CurrentElement;
            menuHUD.ShowElement(currentElement);
            menuHUD.ShowAbilities(currentElement);
        }
        else
        {
            // Resume the game
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Called by Element UI buttons to change the selected element.
    /// </summary>
    public void SelectElement(ElementType element)
    {
        currentElement = element;
        menuHUD.ShowElement(element);
        menuHUD.ShowAbilities(element);
    }

    /// <summary>
    /// Called by Ability UI buttons to update the detailed info panel.
    /// </summary>
    public void SelectAbility(AbilityData ability)
    {
        currentAbility = ability;
        menuHUD.ShowAbilityInfo(ability);
    }
}