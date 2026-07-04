using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

// Main controller for the Abilities Information screen.
// Handles element switching, ability selection, and dynamic UI updates.
public class AbilitiesInfo : MonoBehaviour
{
    [Header("UI References - Element Tabs")]
    [SerializeField] private Button fireButton;
    [SerializeField] private Button waterButton;
    [SerializeField] private Button earthButton;
    [SerializeField] private Button airButton;

    [Header("UI References - Ability Slots")]
    [SerializeField] private Button abilityButton1;
    [SerializeField] private Button abilityButton2;
    [SerializeField] private Button abilityButton3;
    [SerializeField] private Button abilityButton4;
    
    [SerializeField] private Image abilityIcon1;
    [SerializeField] private Image abilityIcon2;
    [SerializeField] private Image abilityIcon3;
    [SerializeField] private Image abilityIcon4;

    [Header("UI References - Detail Panel")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private TextMeshProUGUI abilityDescription;
    [SerializeField] private Image backgroundPanel;

    [Header("Backgrounds")]
    [SerializeField] private Sprite defaultBackground;
    [SerializeField] private Sprite fireBackground;
    [SerializeField] private Sprite waterBackground;
    [SerializeField] private Sprite earthBackground;
    [SerializeField] private Sprite airBackground;

    [Header("Data")]
    [SerializeField] private ElementAbilitySet[] abilitySets;

    // Runtime state
    private AbilityData[] _currentAbilities = new AbilityData[4];
    private Button[] _elementButtons;
    private Button[] _abilityButtons;

    private void Awake()
    {
        // Group buttons into arrays for easier iteration
        _elementButtons = new[] { fireButton, waterButton, earthButton, airButton };
        _abilityButtons = new[] { abilityButton1, abilityButton2, abilityButton3, abilityButton4 };
        
        SetupElementButtons();
        SetupAbilityButtons();
        
        // Default to Fire element on load
        OnElementSelected(ElementType.Fire, fireButton);
    }

    private void SetupElementButtons()
    {
        fireButton.onClick.AddListener(() => OnElementSelected(ElementType.Fire, fireButton));
        waterButton.onClick.AddListener(() => OnElementSelected(ElementType.Water, waterButton));
        earthButton.onClick.AddListener(() => OnElementSelected(ElementType.Earth, earthButton));
        airButton.onClick.AddListener(() => OnElementSelected(ElementType.Air, airButton));
    }

    private void SetupAbilityButtons()
    {
        // Pass the array index so we know which ability was clicked
        abilityButton1.onClick.AddListener(() => OnAbilitySelected(0));
        abilityButton2.onClick.AddListener(() => OnAbilitySelected(1));
        abilityButton3.onClick.AddListener(() => OnAbilitySelected(2));
        abilityButton4.onClick.AddListener(() => OnAbilitySelected(3));
    }

    private void OnElementSelected(ElementType element, Button selectedButton)
    {
        var set = abilitySets.FirstOrDefault(s => s.element == element);
        if (set == null) return;

        // Update visual state: highlight selected, dim the rest
        foreach (var btn in _elementButtons)
        {
            SetButtonSelected(btn, btn == selectedButton);
        }

        // Cache the abilities for this element
        _currentAbilities[0] = set.ability1;
        _currentAbilities[1] = set.ability2;
        _currentAbilities[2] = set.ability3;
        _currentAbilities[3] = set.ability4;

        // Populate the 4 ability slot icons
        UpdateAbilityIcon(abilityIcon1, set.ability1);
        UpdateAbilityIcon(abilityIcon2, set.ability2);
        UpdateAbilityIcon(abilityIcon3, set.ability3);
        UpdateAbilityIcon(abilityIcon4, set.ability4);

        // Swap the background art to match the element
        UpdateBackground(element);

        // Auto-select the first ability to populate the detail panel
        if (set.ability1 != null)
        {
            OnAbilitySelected(0);
        }
    }

    private void UpdateAbilityIcon(Image iconImage, AbilityData ability)
    {
        if (iconImage == null) return;
        
        if (ability != null && ability.icon != null)
        {
            iconImage.sprite = ability.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    private void OnAbilitySelected(int index)
    {
        var ability = _currentAbilities[index];
        if (ability == null) return;

        // Highlight the selected ability slot
        for (int i = 0; i < _abilityButtons.Length; i++)
        {
            SetButtonSelected(_abilityButtons[i], i == index);
        }

        // Populate the detail panel (using 1.0f efficiency for the UI preview)
        if (abilityIcon != null) abilityIcon.sprite = ability.icon;
        if (abilityName != null) abilityName.text = ability.abilityName;
        if (abilityDescription != null) abilityDescription.text = ability.GetFormattedDescription(1f);
    }

    // Simple visual feedback: changes the Image alpha to dim unselected buttons
    private void SetButtonSelected(Button button, bool selected)
    {
        if (button == null) return;
        
        var image = button.GetComponent<Image>();
        if (image != null)
        {
            Color c = image.color;
            c.a = selected ? 1f : 0.4f;
            image.color = c;
        }
    }

    private void UpdateBackground(ElementType element)
    {
        if (backgroundPanel == null) return;
        
        // Clean switch expression for background swapping
        backgroundPanel.sprite = element switch
        {
            ElementType.Fire => fireBackground,
            ElementType.Water => waterBackground,
            ElementType.Earth => earthBackground,
            ElementType.Air => airBackground,
            _ => defaultBackground
        };
    }

    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");
}