using UnityEngine;
using UnityEngine.InputSystem;

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

    private void OnEnable()
    {
        openMenuAction.action.Enable();
    }

    private void OnDisable()
    {
        openMenuAction.action.Disable();
    }

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
            Time.timeScale = 0f;

            currentElement = playerAbilities.CurrentElement;
            menuHUD.ShowElement(currentElement);
            menuHUD.ShowAbilities(currentElement);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    // 🔥 llamado desde botones de elemento
    public void SelectElement(ElementType element)
    {
        currentElement = element;
        menuHUD.ShowElement(element);
        menuHUD.ShowAbilities(element);
    }

    // ⭐ llamado desde botones de habilidad
    public void SelectAbility(AbilityData ability)
    {
        currentAbility = ability;
        menuHUD.ShowAbilityInfo(ability);
    }
}