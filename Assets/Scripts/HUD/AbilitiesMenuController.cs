using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Controls the in-game pause menu. 
// Handles time scale pausing and routes UI selections to the HUD.
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
            ToggleMenu();
    }

    private void ToggleMenu()
    {
        isOpen = !isOpen;
        menuRoot.SetActive(isOpen);
        
        if (isOpen)
        {
            Time.timeScale = 0f; // Pause the game
            
            // Sync HUD with current player state
            currentElement = playerAbilities.CurrentElement;
            menuHUD.ShowElement(currentElement);
            menuHUD.ShowAbilities(currentElement);
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
        }
    }

    public void SelectElement(ElementType element)
    {
        currentElement = element;
        menuHUD.ShowElement(element);
        menuHUD.ShowAbilities(element);
    }

    public void SelectAbility(AbilityData ability)
    {
        currentAbility = ability;
        menuHUD.ShowAbilityInfo(ability);
    }

    // CRITICAL: Resets Time.timeScale to 1f before loading the scene.
    // If we don't do this, the next match will start completely frozen if the 
    // player quits to the main menu while the pause menu is open.
    public void Return()
    {
        Time.timeScale = 1f;
        isOpen = false;
        
        if (menuRoot != null)
            menuRoot.SetActive(false);
            
        SceneManager.LoadScene("Scenes/MainMenu");
    }
}